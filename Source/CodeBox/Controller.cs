using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using CodeBox.Core.Services;
using CodeBox.Core.Utilities;
using System.Windows.Threading;
using CodeBox.CodeLexer;
using CodeBox.Core.Elements;
using CodeBox.Core.Services.AutoComplete;
using CodeBox.Core.Services.SyntaxValidator;

namespace CodeBox.Core
{
	public enum KeyboardEventType { KeyDown, KeyUp }
	public enum KeyboardBubbling { Continue, Stop }
	
	public class Controller
	{
		public Canvas RootCanvas { get; private set; }
		public Paper Paper { get; private set; }
		public StackPanel NumberPanel { get; private set; }
		public Token TokenBeforeCaret { get; private set; }
		public TokenList TokenList { get; private set; }
		public List<TokenChars> TokenChars { get; private set; }
		
		public string Text
		{
			get { return Paper.Text; }
			set
			{
				Paper.Text = value;
				TextChanged();
			}
		}
		public bool KeepFocus { get; set; }
		public bool IsShiftDown;
		public bool IsCtrlDown;
		public bool IsEditable = true;
		
		public event Action OnTextChanged;
		public event Action OnMouseDown;
		public event Action OnMouseUp;
		public event Action<double> OnScroll;
		public List<IService> Services;

		private TextBox textBox;
		private DispatcherTimer parsingTimer;
		private List<Character> searchText;
		private Dictionary<Key, List<Func<KeyEventArgs, KeyboardBubbling>>> onKeyDown;
		private Dictionary<Key, List<Func<KeyEventArgs, KeyboardBubbling>>> onKeyUp;
		
		#region instance handling
		private static Controller instance;

		public static Controller Instance
		{
			get { return instance; }
		}

		public static Controller SetInstance(Canvas rootCanvas, Paper paper, StackPanel numberPanel, TextBox textBox)
		{
			instance = new Controller(rootCanvas, paper, numberPanel, textBox);
			return instance;
		}
		
		private Controller(Canvas rootCanvas, Paper paper, StackPanel numberPanel, TextBox textBox)
		{
			RootCanvas = rootCanvas;
			Paper = paper;
			NumberPanel = numberPanel;
			this.textBox = textBox;
						
			Services = new List<IService>();
			instance = this;
			paper.UpdateCaretPosition();

			// set parsing timer
			if (Configuration.DurationBeforeTextChangedCallback > 0)
			{
				parsingTimer = new DispatcherTimer();
				parsingTimer.Interval = TimeSpan.FromSeconds(Configuration.DurationBeforeTextChangedCallback);
				parsingTimer.Tick += (o, e) => ParseAndCallObservers();
			}
			else
			{
				ParseAndCallObservers();
			}
			
			// reset paper
			paper.Children.Clear();
			paper.Children.Add(new PaperLine());
			
			// keyboard events
			onKeyDown = new Dictionary<Key, List<Func<KeyEventArgs, KeyboardBubbling>>>();
			onKeyUp = new Dictionary<Key, List<Func<KeyEventArgs, KeyboardBubbling>>>();
			
			// when MouseUp/MouseDown on number panel, inform Paper
			numberPanel.MouseLeftButtonUp += delegate { paper.IsMouseDown = false; };
			numberPanel.MouseLeftButtonDown += delegate { paper.IsMouseDown = true; };
		}
		#endregion
		
		public void InitializeServices()
		{
			// add services (order matters... lets agree to make Validator always first)
			if (Configuration.Validator.IsEnabled)
				Services.Add(ValidatorService.Instance);
			
			if (Configuration.AutoCompleter.IsEnabled)
				Services.Add(AutoCompleteService.Instance);
			
			Services.Add(KeyboardShortcuts.Instance);
			
			if (Configuration.SyntaxHighlighter.IsEnabled)
				Services.Add(SyntaxHighlighterService.Instance);
			
			Services.Add(Clipboard.Instance);
			Services.Add(Recovery.Instance);
			Services.Add(UndoRedo.Instance);
			
			// initialize services
			foreach (var service in Services)
			{
				service.Initialize();
			}
		}

		public bool HaveService<Service>() where Service : IService
		{
			foreach (var service in Services)
			{
				if (service.GetType() == typeof(Service))
					return true;
			}

			return false;
		}
		
		public void GetFocus()
		{
			textBox.Focus();
		}
		
		public void AddChar(char keyChar)
		{
			if (!IsEditable)
				return;

			// have highlights? replace
			Paper.RemoveHighlights();
			
			// add it
			PaperLine line = Paper.CurrentLine;
			var newChar = new Character(keyChar, Paper.Line, Paper.Position);
			line.Add(newChar, Paper.Position);
			Paper.Position++;

			UpdatePositions();
			TextChanged();
		}

		public void AddText(string text)
		{
			if (!IsEditable)
				return;

			// have highlights? replace
			Paper.RemoveHighlights();

			// arrange
			var line = Paper.Line;
			var position = Paper.Position;
			var pLine = Paper.CurrentLine;
			
			// add text to the current position of the caret (or _line and _position) - DO NOT use CurrentLine since current line keeps changing until Paper.UpdateCaret() is called
			foreach (char c in text)
			{
				if (c == '\n')
				{
					var prevLine = Paper.LineAt(line);
					pLine = new PaperLine();
					
					prevLine.MigrateCharacters(pLine, position + 1, prevLine.LastIndex);
					Paper.Children.Insert(++line, pLine);
					position = 0;
				}
				else if (c == '\r') // becuase nobody like \r
					continue;
				else
				{
					pLine.Add(new Character(c, line, position), position++);
				}
			}

			// update
			Paper.UpdateCaret(line, position);
			TextChanged();
		}

		#region keyboard handling
		public void AddKeyboardEvent(KeyboardEventType type, Key key, Func<KeyEventArgs, KeyboardBubbling> callback)
		{
			var dictionary = type == KeyboardEventType.KeyDown ? onKeyDown : onKeyUp;

			if (!dictionary.ContainsKey(key))
			{
				dictionary.Add(key, new List<Func<KeyEventArgs, KeyboardBubbling>>());
			}

			dictionary[key].Add(callback);
		}		
		
		public void KeyDown(object sender, KeyEventArgs e)
		{
			Paper.IsMouseDown = false;
			var keys = Keyboard.Modifiers;
			
			IsShiftDown = (keys & ModifierKeys.Shift) != 0;
			IsCtrlDown = ((keys & ModifierKeys.Control) != 0 || (keys & ModifierKeys.Apple) != 0);
			
			if (!onKeyDown.ContainsKey(e.Key))
				return;
			
			foreach (var f in onKeyDown[e.Key])
			{
				if (f.Invoke(e) == KeyboardBubbling.Stop)
				{
					e.Handled = true;
					break;
				}
			}
		}

		public void KeyUp(object sender, KeyEventArgs e)
		{
			var keys = Keyboard.Modifiers;

			IsShiftDown = (keys & ModifierKeys.Shift) != 0;
			IsCtrlDown = ((keys & ModifierKeys.Control) != 0 || (keys & ModifierKeys.Apple) != 0);
			
			if (!onKeyUp.ContainsKey(e.Key))
				return;
			
			foreach (var f in onKeyUp[e.Key])
			{
				if (f.Invoke(e) == KeyboardBubbling.Stop)
				{
					e.Handled = true;
					break;
				}
			}
		}
		#endregion
		
		#region mouse handling
		public void MouseDown()
		{
			GetFocus();

			if (OnMouseDown != null)
				OnMouseDown();
		}

		public void MouseUp()
		{
			if (OnMouseUp != null)
				OnMouseUp();
		}
		#endregion

		public void UpdatePositions()
		{
			for (int lineNum = 0; lineNum < Paper.Children.Count; lineNum++)
			{
				PaperLine line = (PaperLine)Paper.Children[lineNum];

				for (int charNum = 0; charNum < line.Children.Count; charNum++)
				{
					if (line.Children[charNum].GetType() != typeof(Character))
						continue;

					Character c = (Character)line.Children[charNum];
					c.Line = lineNum;
					c.Position = charNum;
				}
			}
		}
		
		#region TextChanged and parsing
		public void TextChanged()
		{
			if (parsingTimer.IsEnabled)
			{
				parsingTimer.Stop();
			}

			parsingTimer.Start();
		}
		
		public void ParseAndCallObservers()
		{
			if (parsingTimer != null)
				parsingTimer.Stop();
			
			// generate new token list
			TokenChars = new List<TokenChars>();
			TokenList = new TokenList(Text, Configuration.Keywords.ToList());
			TokenBeforeCaret = GeTokenBeforeCaret();

			// populate token chars for each token
			TokenList.Tokens.ForEach(t => TokenChars.Add(new TokenChars(t)));

			// call observers
			if (OnTextChanged != null)
				OnTextChanged();
		}
		
		internal void UpdateParsingSpeed()
		{
			parsingTimer.Interval = TimeSpan.FromSeconds(Configuration.DurationBeforeTextChangedCallback);
		}
		#endregion
		
		#region cut, copy, paste, select all, search
		public void Cut(KeyEventArgs e)
		{
			if (!Paper.HaveHighlightedText)
				return;

			Clipboard.Instance.Copy(Paper.GetHighlightedText(), e);
			Paper.RemoveHighlights();
		}

		public void Copy(KeyEventArgs e)
		{
			if (!Paper.HaveHighlightedText)
				return;

			Clipboard.Instance.Copy(Paper.GetHighlightedText(), e);
		}

		public void Paste(KeyEventArgs e)
		{
			AddText(Clipboard.Instance.Paste(e));
		}

		public void SelectAll()
		{
			Paper.HighlightAll();
		}

		public void Search(string query)
		{
			Paper.ClearHighlights();

			if (searchText == null)
				searchText = new List<Character>();

			if (searchText.Count > 0)
			{
				searchText.ForEach(c => c.Unhighlight());
				searchText.Clear();
			}

			if (string.IsNullOrEmpty(query))
				return;

			var characters = Paper.GetAllCharacters();

			for (int i = 0; i < characters.Count - query.Length + 1; i++)
			{
				var test = characters.GetRange(i, query.Length);

				if (test.GetText().Equals(query))
				{
					searchText.AddRange(test);
					searchText.ForEach(c => c.HighlightYellow());
				}

			}
		}
		#endregion
		
		private Token GeTokenBeforeCaret()
		{
			if (Paper.Position == 0 && Paper.Line == 0)
				return null;

			var tokensBeforeCaret = from token in TokenList.Tokens
									where
										token.Line == Paper.Line + 1 &&
										token.Position <= Paper.Position
									select token;

			if (tokensBeforeCaret.Count() == 0) // true when back-spacing on a line with only white-space
				return null;

			var positionOfLastToken = tokensBeforeCaret.Max(t => t.Position);

			var lastToken = from token in tokensBeforeCaret
							where token.Position == positionOfLastToken
							select token;

			return lastToken.FirstOrDefault();
		}
		
		internal void Scrolled(double newVal)
		{
			if (OnScroll != null)
				OnScroll(newVal);
		}
	}
}
