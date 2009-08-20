using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CodeBox.Core.Utilities;
using System.Windows.Threading;
using CodeBox.CodeLexer;
using CodeBox.Core.Elements;

namespace CodeBox.Core.Services.AutoComplete
{
	public partial class AutoCompleteService : IService
	{
		#region instance
		private AutoCompleteService instance;
		
		private static AutoCompleteService _instance;
		public static AutoCompleteService Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new AutoCompleteService();
				}

				return _instance;
			}
		}
		#endregion

		public ListBoxItem SelectedListBoxItem
		{
			get
			{
				return listBox.SelectedItem as ListBoxItem;
			}
		}

		public int SelectedListBoxIndex
		{
			get
			{
				return listBox.SelectedIndex;
			}
		}
		
		public bool HasFocus
		{
			get
			{
				if (instance.Visibility == Visibility.Collapsed)
					return false;

				if (instance.listBox.SelectedItem == null)
					return false;

				return true;
			}
		}		
				
		private AutoCompleteService()
		{
			InitializeComponent();
			
			if (instance != null)
				throw new Exception("more than one instance of AutoComplete... i thought we agreed on one?");

			Grid.SetRow(this, 1);
			Grid.SetColumn(this, 1);
			Visibility = Visibility.Collapsed;
			Controller.Instance.RootCanvas.Children.Add(this);

			instance = this;
		}

		public void Initialize()
		{
			Controller.Instance.OnTextChanged += Configuration.AutoCompleter.KeyDown;
			Controller.Instance.Paper.MouseLeftButtonDown += (e, s) => { HideList(); Controller.Instance.GetFocus(); };
			
			// on key Escape
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.Escape, () => { HideList(); return KeyboardBubbling.Stop; });
			
			// on key Enter
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.Enter,
			                                     () =>
			                                     	{
			                                     		if (HasFocus)
			                                     		{
			                                     			Select();
			                                     			return KeyboardBubbling.Stop;
			                                     		}

			                                     		return KeyboardBubbling.Continue;
			                                     	});
			// on key Down
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.Down,
												 () =>
												 {
													 if (HasFocus)
													 {
														 SelectNext();
														 return KeyboardBubbling.Stop;
													 }

													 return KeyboardBubbling.Continue;
												 });

			// on key Up
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.Up,
												 () =>
												 {
													 if (HasFocus)
													 {
														 SelectPrevious();
														 return KeyboardBubbling.Stop;
													 }

													 return KeyboardBubbling.Continue;
												 });
			
		}
		
		#region selections (current, next, prev)
		public void Select()
		{
			// nothing selected? hide
			if (instance.listBox.SelectedItem == null || instance.Visibility == Visibility.Collapsed || ((ListBoxItem)instance.listBox.SelectedItem).Tag.ToString() == "Keep")
			{
				HideList();
				return;
			}

			// get token & line
			var token = Controller.Instance.Paper.CharachterBeforeCaret.ParentToken;
			PaperLine pLine = Controller.Instance.Paper.LineAt(token.Line - 1);
			
			// remember current charachter (before removing it)
			var currentChar = Controller.Instance.Paper.CharachterBeforeCaret;
			
			// remove token chars
			Controller.Instance.Paper.RemoveCaret();
			List<Character> charsToRemove = new List<Character>();
			if (pLine.Children.Count != 0)
				foreach (var index in token.Indices)
					charsToRemove.Add((Character)pLine.At(index.Position - 1));
			pLine.RemoveCharacters(charsToRemove);
			Controller.Instance.Paper.UpdateCaret(Controller.Instance.Paper.Line, token.Position - 1); // also restores caret
			
			// add new identifier
			var newToken = ((ListBoxItem) instance.listBox.SelectedItem).Tag.ToString();
			if (currentChar.ParentToken.Value == ".")
				newToken = "." + newToken;
			Controller.Instance.Paper.UpdateCaret(token.Line - 1, token.Position - 1);
			Controller.Instance.AddText(newToken);
			
			HideList();
		}
		
		public void SelectNext()
		{
			if (instance.listBox.SelectedIndex == -1)
				instance.listBox.SelectedIndex = 0;
			else
			{
				if (instance.listBox.Items.Count > instance.listBox.SelectedIndex + 1)
					instance.listBox.SelectedIndex++;
			}

			if (Configuration.AutoCompleter.OnHighlightedItemChanged != null)
				Configuration.AutoCompleter.OnHighlightedItemChanged();
		}
		
		public void SelectPrevious()
		{
			if (instance.listBox.SelectedIndex == -1)
				instance.listBox.SelectedIndex = instance.listBox.Items.Count - 1;
			else
			{
				if (instance.listBox.SelectedIndex > 0)
					instance.listBox.SelectedIndex--;
			}

			if (Configuration.AutoCompleter.OnHighlightedItemChanged != null)
				Configuration.AutoCompleter.OnHighlightedItemChanged();
		}

		public void SelectClosestItem()
		{
			var token = Controller.Instance.Paper.CharachterBeforeCaret.ParentToken;
			
			if (token == null)
				return;
			
			var term = token.Value;

			foreach (ListBoxItem item in instance.listBox.Items)
			{
				if (!item.Tag.ToString().ToLower().StartsWith(term.ToLower()))
					continue;

				instance.listBox.SelectedItem = item;
				instance.listBox.UpdateLayout();
				instance.listBox.ScrollIntoView(item);

				break;
			}

			if (instance.listBox.SelectedItem == null)
			{
				instance.listBox.SelectedIndex = 0;
				instance.listBox.UpdateLayout();
			}

			if (Configuration.AutoCompleter.OnHighlightedItemChanged != null)
				Configuration.AutoCompleter.OnHighlightedItemChanged();
		}
		#endregion
		
		#region show/hide list
		public void ShowList()
		{
			SelectClosestItem();
			
			// arrange
			Character currentChar = Controller.Instance.Paper.CharachterBeforeCaret;
			Token token = currentChar.ParentToken;
			TokenChars tc = Controller.Instance.TokenChars.GetTokenCharsByToken(token);

			// position
			PaperLine line = (PaperLine)tc.FirstCharacter.Parent;
			double left = Controller.Instance.NumberPanel.ActualWidth + line.PositionFromLeftInPixels(tc.FirstCharacter);
			instance.SetValue(Canvas.LeftProperty, left);
			instance.SetValue(Canvas.TopProperty, Character.CHAR_HEIGHT * (tc.FirstCharacter.Line + 1));
			
			instance.Visibility = Visibility.Visible;
			
			if (Configuration.AutoCompleter.OnShow != null)
				Configuration.AutoCompleter.OnShow();
		}

		public void HideList()
		{
			instance.Visibility = Visibility.Collapsed;		
			
			if (Configuration.AutoCompleter.OnHide != null)
				Configuration.AutoCompleter.OnHide();
		}
		#endregion
		
		#region show/hide loading/error
		private DispatcherTimer loadingTimer;
		public void ShowLoading()
		{
			instance.loadingIndicator.Visibility = Visibility.Visible;
			instance.tbLoadingText.Text = "loading";
			instance.tbLoadingDots.Text = ".";

			bool increaseDots = true;
			loadingTimer = new DispatcherTimer();
			loadingTimer.Interval = TimeSpan.FromMilliseconds(300);
			loadingTimer.Tick += delegate
			                     	{
										var length = instance.tbLoadingDots.Text.Length;

										if (length == 5)
											increaseDots = false;
										else if (length == 1)
											increaseDots = true;
										
         								if (increaseDots)
											instance.tbLoadingDots.Dispatcher.BeginInvoke(() => instance.tbLoadingDots.Text += ".");
										else
											instance.tbLoadingDots.Dispatcher.BeginInvoke(() => instance.tbLoadingDots.Text = instance.tbLoadingDots.Text.Substring(0, length - 1));
			                     	};
         	
			loadingTimer.Start();
		}
		
		public void HideLoading()
		{
			instance.loadingIndicator.Visibility = Visibility.Collapsed;
			
			if (loadingTimer != null)
				loadingTimer.Stop();
		}

		public void ShowError(string title, string description)
		{
			loadingTimer.Stop();
			instance.tbLoadingText.Text = title;
			instance.tbLoadingDots.Text = description;
		}
		#endregion
		
		public void AddItem(AutoCompleteItem item)
		{
			listBox.Items.Add(CreateListBoxItem(item));
		}

		public void ClearListboxItems()
		{
			var itemsToRemove = new List<ListBoxItem>();

			foreach (ListBoxItem item in instance.listBox.Items)
				if ((string)item.Tag != "Keep")
					itemsToRemove.Add(item);

			foreach (var item in itemsToRemove)
				instance.listBox.Items.Remove(item);
		}
		
		private ListBoxItem CreateListBoxItem(AutoCompleteItem item)
		{
			ListBoxItem lbItem = new ListBoxItem();
			lbItem.Height = 21;
			lbItem.Tag = item.Name;
			
			// content
			StackPanel content = new StackPanel();
			content.Orientation = Orientation.Horizontal;
			lbItem.Content = content;

			// attach double-click handler (MouseButtonDown doesn't work with ListBoxItem)
			content.Background = new SolidColorBrush(Colors.Transparent); // it will only stretch if it has a background
			content.Width = 3000; // to cover all area
			content.Loaded += (sender, e) => DoubleClickHelper.Attach(sender, (s, me) => Select());
			
			if (Configuration.AutoCompleter.ObjectIcon != null && Configuration.AutoCompleter.FunctionIcon != null)
			{
				// icon
				Image icon = new Image();
				icon.Width = icon.Height = 12;
				icon.Margin = new Thickness(0, 0, 5, 0);
				icon.Source = new BitmapImage();
				
				content.Children.Add(icon);
				
				switch (item.Type)
				{
					case AutoCompleteItemType.Object:
						(icon.Source as BitmapImage).SetSource(Configuration.AutoCompleter.ObjectIcon);
						break;

					case AutoCompleteItemType.Function:
						(icon.Source as BitmapImage).SetSource(Configuration.AutoCompleter.FunctionIcon);
						break;
				}
			}

			// text
			TextBlock tb = new TextBlock();
			tb.Text = item.Name;
			tb.FontFamily = new FontFamily("Courier New");
			tb.FontWeight = FontWeights.Light;
			tb.Margin = new Thickness(0, 2, 0, 0);
			content.Children.Add(tb);
			
			if (item.Info != "")
			{
				// info
				TextBlock info = new TextBlock();
				info.Text = item.Info;
				info.FontFamily = new FontFamily("Courier New");
				info.FontWeight = FontWeights.Light;
				info.Foreground = new SolidColorBrush(Colors.LightGray);
				info.HorizontalAlignment = HorizontalAlignment.Right;
				info.Margin = new Thickness(5, 2, 0, 0);
				content.Children.Add(info);
			}
			
			// parameters (if applicable)
			if (item.Type == AutoCompleteItemType.Function)
			{
				string parameters = "";
				
				if (item.Parameters != null)
					parameters = string.Join(", ", item.Parameters);

				TextBlock tbArgs = new TextBlock();
				tbArgs.Text = item.SurroundParamsWithParenthesis ? "(" + parameters + ")" : parameters;
				tbArgs.FontFamily = new FontFamily("Courier New");
				tbArgs.FontWeight = FontWeights.Light;
				tbArgs.Margin = new Thickness(0, 2, 0, 0);
				tbArgs.Foreground = new SolidColorBrush(Colors.Gray);
				content.Children.Add(tbArgs);
			}
			
			return lbItem;
		}
	}

	public class AutoCompleteItem : IComparable
	{
		public string Name { get; set; }
		public string Info { get; set; }
		public string[] Parameters { get; set; }
		public AutoCompleteItemType Type { get; set; }
		public bool SurroundParamsWithParenthesis { get; set; }

		public AutoCompleteItem(string name, AutoCompleteItemType type)
		{
			Name = name;
			Type = type;

			SurroundParamsWithParenthesis = true;
		}

		public AutoCompleteItem(string name, string info, AutoCompleteItemType type) : this(name, type)
		{
			Info = info;
		}
		
		public AutoCompleteItem(string name, string[] parameters, AutoCompleteItemType type) : this(name, type)
		{
			Parameters = parameters;
		}

		public int CompareTo(object obj)
		{
			AutoCompleteItem item = obj as AutoCompleteItem;

			return Name.CompareTo(item.Name);
		}
	}

	public enum AutoCompleteItemType
	{
		Object,
		Function
	}
}