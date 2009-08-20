using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using CodeBox.Core;
using CodeBox.Core.Utilities;
using CodeBox.Core.Elements;
using CodeBox.Core.Services.AutoComplete;

namespace CodeBox.Core.Services
{
	public class KeyboardShortcuts : IService
	{
		private static KeyboardShortcuts _instance;
		public static KeyboardShortcuts Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new KeyboardShortcuts();
				}

				return _instance;
			}
		}
		
		private KeyboardShortcuts()
		{
			
		}
		
		public void Initialize()
		{
			#region enter
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.Enter,
			                                     () =>
			                                     	{
			                                     		if (!Controller.Instance.IsEditable)
			                                     			return KeyboardBubbling.Continue;

			                                     		Controller.Instance.Paper.RemoveHighlights();

			                                     		if (Controller.Instance.Paper.Position != Controller.Instance.Paper.CurrentLine.LastIndex)
			                                     		{
			                                     			Controller.Instance.Paper.Children.Insert(Controller.Instance.Paper.Line + 1, new PaperLine());
			                                     			PaperLine prevLine = Controller.Instance.Paper.CurrentLine;
			                                     			PaperLine line = Controller.Instance.Paper.NextLine;
			                                     			List<UIElement> chars = new List<UIElement>();
			                                     			for (int i = Controller.Instance.Paper.Position; i < prevLine.Children.Count; i++)
			                                     				chars.Add(prevLine.Children[i]);
			                                     			foreach (UIElement c in chars)
			                                     			{
			                                     				prevLine.Children.Remove(c);
			                                     				line.Children.Add(c);
			                                     			}
			                                     		}
			                                     		else
			                                     		{
			                                     			Controller.Instance.Paper.Children.Insert(Controller.Instance.Paper.Line + 1, new PaperLine());
			                                     		}

			                                     		Controller.Instance.Paper.UpdateCaret(Controller.Instance.Paper.Line + 1, 0);
			                                     		Controller.Instance.UpdatePositions();

			                                     		#region add tabs as much as prev line
			                                     		int numberOfTabs = 0;
			                                     		for (int i = 0; i < Controller.Instance.Paper.PrevLine.Children.Count; i++)
			                                     		{
			                                     			if (((Character)Controller.Instance.Paper.PrevLine.At(i)).IsTab)
			                                     				numberOfTabs++;
			                                     			else
			                                     				break;
			                                     		}
			                                     		while (numberOfTabs > 0)
			                                     		{
			                                     			Controller.Instance.Paper.CurrentLine.Add(new Character('\t', Controller.Instance.Paper.Line, Controller.Instance.Paper.Position), Controller.Instance.Paper.Position);
			                                     			Controller.Instance.Paper.Position++;
			                                     			numberOfTabs--;
			                                     		}
			                                     		#endregion
			                                     		Controller.Instance.TextChanged();
			                                     		return KeyboardBubbling.Stop;
			                                     	});
			#endregion

			#region backspace
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.Back,
			                                     () =>
			                                     	{
			                                     		if (!Controller.Instance.IsEditable)
			                                     			return KeyboardBubbling.Continue;

			                                     		if (Controller.Instance.Paper.HaveHighlightedText)
			                                     		{
			                                     			Controller.Instance.Paper.RemoveHighlights();
			                                     			Controller.Instance.TextChanged();
			                                     			return KeyboardBubbling.Stop;
			                                     		}

			                                     		if (Controller.Instance.Paper.IsInFirstLine && Controller.Instance.Paper.Position == 0)
			                                     			return KeyboardBubbling.Continue;

			                                     		if (Controller.Instance.Paper.Position == 0)
			                                     		{
			                                     			List<UIElement> chars = new List<UIElement>();
			                                     			int pos = Controller.Instance.Paper.LineAt(Controller.Instance.Paper.Line - 1).LastIndex + 1;
			                                     			foreach (UIElement element in Controller.Instance.Paper.CurrentLine.Children)
			                                     				chars.Add(element);
			                                     			foreach (UIElement element in chars)
			                                     			{
			                                     				Controller.Instance.Paper.CurrentLine.Remove(element);
			                                     				Controller.Instance.Paper.PrevLine.Add(element);
			                                     			}

			                                     			Controller.Instance.Paper.RemoveLine(Controller.Instance.Paper.Line);
			                                     			Controller.Instance.Paper.UpdateCaret(Controller.Instance.Paper.Line - 1, pos);
			                                     		}
			                                     		else
			                                     		{
			                                     			if (!Controller.Instance.IsCtrlDown || Controller.Instance.Paper.CharachterBeforeCaret.ParentToken == null)
			                                     			{
			                                     				Controller.Instance.Paper.CurrentLine.Remove(Controller.Instance.Paper.Position - 1);
			                                     				Controller.Instance.Paper.Position--;
			                                     			}
			                                     			else
			                                     			{
			                                     				var charachter = Controller.Instance.Paper.CharachterBeforeCaret.ParentToken;
			                                     				Controller.Instance.Paper.CurrentLine.RemoveCharacters(Controller.Instance.TokenChars.GetTokenCharsByToken(Controller.Instance.Paper.CharachterBeforeCaret.ParentToken).Characters);
			                                     				Controller.Instance.Paper.UpdateCaret(charachter.Line - 1, charachter.Position - 1);
			                                     			}
			                                     		}

			                                     		Controller.Instance.UpdatePositions();
			                                     		Controller.Instance.Paper.UpdateCaretPosition();
			                                     		Controller.Instance.TextChanged();
			                                     		return KeyboardBubbling.Continue;
			                                     	});
			#endregion

			#region delete
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.Delete,
			                                     () =>
			                                     	{
			                                     		if (Controller.Instance.Paper.HaveHighlightedText)
			                                     		{
			                                     			Controller.Instance.Paper.RemoveHighlights();
			                                     			Controller.Instance.TextChanged();
			                                     			return KeyboardBubbling.Continue;
			                                     		}

			                                     		if (Controller.Instance.Paper.IsInLastLine && Controller.Instance.Paper.Position == Controller.Instance.Paper.CurrentLine.LastIndex)
			                                     			return KeyboardBubbling.Continue;

			                                     		if (Controller.Instance.Paper.Position == Controller.Instance.Paper.CurrentLine.LastIndex)
			                                     		{
			                                     			List<UIElement> chars = new List<UIElement>();
			                                     			foreach (UIElement c in Controller.Instance.Paper.NextLine.Children)
			                                     				chars.Add(c);
			                                     			foreach (UIElement c in chars)
			                                     			{
			                                     				Controller.Instance.Paper.NextLine.Children.Remove(c);
			                                     				Controller.Instance.Paper.CurrentLine.Children.Add(c);
			                                     			}

			                                     			Controller.Instance.Paper.RemoveLine(Controller.Instance.Paper.Children.IndexOf(Controller.Instance.Paper.NextLine));
			                                     		}
			                                     		else
			                                     		{
			                                     			Controller.Instance.Paper.CurrentLine.Remove(Controller.Instance.Paper.Position + 1);
			                                     		}

			                                     		Controller.Instance.UpdatePositions();
			                                     		Controller.Instance.Paper.UpdateCaretPosition();
			                                     		Controller.Instance.TextChanged();
			                                     		return KeyboardBubbling.Continue;
			                                     	});
			#endregion

			#region tab
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.Tab,
			                                     () =>
			                                     	{
			                                     		if (!Controller.Instance.IsEditable)
			                                     			return KeyboardBubbling.Continue;

			                                     		if (Controller.Instance.Paper.HaveHighlightedText)
			                                     		{
			                                     			bool isMultiLine = false;

			                                     			int prevLine = Controller.Instance.Paper.HighlightedBlocks.First().Line;
			                                     			foreach (var block in Controller.Instance.Paper.HighlightedBlocks)
			                                     			{
			                                     				if (prevLine != block.Line)
			                                     				{
			                                     					isMultiLine = true;
			                                     					break;
			                                     				}

			                                     				prevLine = block.Line;
			                                     			}

			                                     			if (isMultiLine)
			                                     			{
			                                     				var newBlocks = new List<Character>();
			                                     				foreach (var block in Controller.Instance.Paper.HighlightedBlocks)
			                                     				{
			                                     					if (block.Position == 0)
			                                     					{
			                                     						var line = Controller.Instance.Paper.LineAt(block.Line);
			                                     						var _char = new Character('\t', block.Line, 0);
			                                     						_char.HighlightBlue();
			                                     						line.Add(_char, 0);
			                                     						newBlocks.Add(_char);
			                                     					}
			                                     				}

			                                     				Controller.Instance.Paper.HighlightedBlocks.AddRange(newBlocks);
			                                     				Controller.Instance.UpdatePositions();
			                                     				Controller.Instance.TextChanged();
			                                     			}
			                                     			else
			                                     			{
			                                     				Controller.Instance.AddChar('\t');
			                                     			}
			                                     		}
			                                     		else
			                                     		{
			                                     			Controller.Instance.AddChar('\t');
			                                     		}

			                                     		Controller.Instance.KeepFocus = true;
			                                     		return KeyboardBubbling.Continue;
			                                     	});
			#endregion

			#region up
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.Up,
			                                     () =>
			                                     	{
			                                     		// check if trying to highlight
			                                     		if (Controller.Instance.IsShiftDown)
			                                     		{
			                                     			if (!Controller.Instance.Paper.HaveHighlightedText)
			                                     				Controller.Instance.Paper.HighlightFrom(Controller.Instance.Paper.Line, Controller.Instance.Paper.Position);
			                                     		}
			                                     		else
			                                     		{
			                                     			Controller.Instance.Paper.ClearHighlights();
			                                     		}

			                                     		// move caret
			                                     		if (!Controller.Instance.Paper.IsInFirstLine)
			                                     			Controller.Instance.Paper.Line--;

			                                     		// update if trying to highlight
			                                     		if (Controller.Instance.IsShiftDown)
			                                     			Controller.Instance.Paper.HighlightUpto(Controller.Instance.Paper.Line, Controller.Instance.Paper.Position);

			                                     		return KeyboardBubbling.Stop;
			                                     	});
			#endregion

			#region down
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.Down,
			                                     () =>
			                                     	{
			                                     		// check if trying to highlight
			                                     		if (Controller.Instance.IsShiftDown)
			                                     		{
			                                     			if (!Controller.Instance.Paper.HaveHighlightedText)
			                                     				Controller.Instance.Paper.HighlightFrom(Controller.Instance.Paper.Line, Controller.Instance.Paper.Position);
			                                     		}
			                                     		else
			                                     		{
			                                     			Controller.Instance.Paper.ClearHighlights();
			                                     		}

			                                     		// move caret
			                                     		if (!Controller.Instance.Paper.IsInLastLine)
			                                     			Controller.Instance.Paper.Line++;

			                                     		// update if trying to highlight
			                                     		if (Controller.Instance.IsShiftDown)
			                                     			Controller.Instance.Paper.HighlightUpto(Controller.Instance.Paper.Line, Controller.Instance.Paper.Position);

			                                     		return KeyboardBubbling.Stop;
			                                     	});
			#endregion

			#region left
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.Left,
			                                     () =>
			                                     	{
			                                     		// check if trying to highlight
			                                     		if (Controller.Instance.IsShiftDown)
			                                     		{
			                                     			if (!Controller.Instance.Paper.HaveHighlightedText)
			                                     				Controller.Instance.Paper.HighlightFrom(Controller.Instance.Paper.Line, Controller.Instance.Paper.Position);
			                                     		}
			                                     		else
			                                     		{
			                                     			Controller.Instance.Paper.ClearHighlights();
			                                     		}

			                                     		// move caret
			                                     		if (Controller.Instance.Paper.Position > 0)
			                                     		{
			                                     			if (!Controller.Instance.IsCtrlDown)
			                                     			{
			                                     				Controller.Instance.Paper.Position--;
			                                     			}
			                                     			else
			                                     			{
			                                     				// with control
			                                     				var token = Controller.Instance.Paper.CharachterBeforeCaret.ParentToken;

			                                     				if (token == null) // true when CharachterBeforeCaret is whitespace
			                                     				{
			                                     					var tokens = Controller.Instance.TokenList.Tokens.OrderByDescending(t => t.Line).ThenByDescending(t => t.Position);
			                                     					token = (from t in tokens
			                                     					         where t.Line <= Controller.Instance.Paper.Line + 1 && t.Position + 1 <= Controller.Instance.Paper.Position
			                                     					         select t).First();
			                                     				}

			                                     				var newPosition = token.Indices.First();
			                                     				Controller.Instance.Paper.Line = newPosition.Line - 1;
			                                     				Controller.Instance.Paper.Position = newPosition.Position - 1;
			                                     			}
			                                     		}
			                                     		else if (Controller.Instance.Paper.Position == 0 && !Controller.Instance.Paper.IsInFirstLine)
			                                     		{
			                                     			int line = Controller.Instance.Paper.Line - 1;
			                                     			int pos = Controller.Instance.Paper.LineAt(Controller.Instance.Paper.Line - 1).LastIndex;
			                                     			pos = (pos == 0) ? 0 : pos + 1;
			                                     			Controller.Instance.Paper.UpdateCaret(line, pos);
			                                     		}

			                                     		// update if trying to highlight
			                                     		if (Controller.Instance.IsShiftDown)
			                                     			Controller.Instance.Paper.HighlightUpto(Controller.Instance.Paper.Line, Controller.Instance.Paper.Position);

			                                     		return KeyboardBubbling.Stop;
			                                     	});
			#endregion

			#region right
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.Right,
			                                     () =>
			                                     	{
			                                     		// check if trying to highlight
			                                     		if (Controller.Instance.IsShiftDown)
			                                     		{
			                                     			if (!Controller.Instance.Paper.HaveHighlightedText)
			                                     				Controller.Instance.Paper.HighlightFrom(Controller.Instance.Paper.Line, Controller.Instance.Paper.Position);
			                                     		}
			                                     		else
			                                     		{
			                                     			Controller.Instance.Paper.ClearHighlights();
			                                     		}

			                                     		// move caret
			                                     		if (Controller.Instance.Paper.Position < Controller.Instance.Paper.CurrentLine.LastIndex)
			                                     		{
			                                     			if (!Controller.Instance.IsCtrlDown)
			                                     			{
			                                     				Controller.Instance.Paper.Position++;
			                                     			}
			                                     			else
			                                     			{
			                                     				// with control
			                                     				var token = Controller.Instance.Paper.CharachterAfterCaret.ParentToken;

			                                     				if (token == null) // true when CharachterBeforeCaret is whitespace
			                                     				{
			                                     					token = (from t in Controller.Instance.TokenList.Tokens
			                                     					         where t.Indices.Last().Line >= Controller.Instance.Paper.Line + 1 && t.Indices.Last().Position > Controller.Instance.Paper.Position
			                                     					         select t).First();
			                                     				}

			                                     				var newPosition = token.Indices.Last();
			                                     				Controller.Instance.Paper.Line = newPosition.Line - 1;
			                                     				Controller.Instance.Paper.Position = newPosition.Position;
			                                     			}
			                                     		}
			                                     		else if (!Controller.Instance.Paper.IsInLastLine)
			                                     			Controller.Instance.Paper.UpdateCaret(Controller.Instance.Paper.Line + 1, 0);

			                                     		// update if trying to highlight
			                                     		if (Controller.Instance.IsShiftDown)
			                                     			Controller.Instance.Paper.HighlightUpto(Controller.Instance.Paper.Line, Controller.Instance.Paper.Position);

			                                     		return KeyboardBubbling.Stop;
			                                     	});
			#endregion

			#region ctrl + D (nothing selected)
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.D,
												 () =>
												 {
													 if (!Controller.Instance.IsEditable)
														 return KeyboardBubbling.Continue;

													 if (!Controller.Instance.IsCtrlDown)
														 return KeyboardBubbling.Continue;

													 if (Controller.Instance.Paper.HaveHighlightedText)
														 return KeyboardBubbling.Continue;

													 // get text
													 var text = Controller.Instance.Paper.CurrentLine.GetCharacters(0, Controller.Instance.Paper.CurrentLine.LastIndex).GetText();

													 // create new line
													 Controller.Instance.Paper.Children.Insert(Controller.Instance.Paper.Line, new PaperLine());
													 Controller.Instance.Paper.UpdateCaret(Controller.Instance.Paper.Line, 0);

													 // add text to new line
													 Controller.Instance.AddText(text + "\n");

													 return KeyboardBubbling.Continue;
												 });
			#endregion

			#region ctrl + C (nothing selected)
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.C,
												 () =>
												 {
													 if (!Controller.Instance.IsEditable)
														 return KeyboardBubbling.Continue;

													 if (!Controller.Instance.IsCtrlDown)
														 return KeyboardBubbling.Continue;

													 if (Controller.Instance.Paper.HaveHighlightedText)
														 return KeyboardBubbling.Continue;

													 // get text
													 var text = Controller.Instance.Paper.CurrentLine.GetCharacters(0, Controller.Instance.Paper.CurrentLine.LastIndex).GetText();

													 // add to clipboard
													 Clipboard.Instance.Copy(text + "\n");

													 return KeyboardBubbling.Continue;
												 });
			#endregion

			#region ctrl + X (nothing selected)
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.X,
												 () =>
												 {
													 if (!Controller.Instance.IsEditable)
														 return KeyboardBubbling.Continue;

													 if (!Controller.Instance.IsCtrlDown)
														 return KeyboardBubbling.Continue;

													 if (Controller.Instance.Paper.HaveHighlightedText)
														 return KeyboardBubbling.Continue;

													 // get text
													 var text = Controller.Instance.Paper.CurrentLine.GetCharacters(0, Controller.Instance.Paper.CurrentLine.LastIndex).GetText();
													 
													 if (text == "")
													 	return KeyboardBubbling.Continue;
													 
													 // remove line (if it is not the only line)
													 if (Controller.Instance.Paper.Children.Count > 1)
													 {
														 Controller.Instance.Paper.Children.RemoveAt(Controller.Instance.Paper.Line);
														 Controller.Instance.Paper.UpdateCaretPosition();
													 }

													 // add to clipboard
													 Clipboard.Instance.Copy(text + "\n");

													 return KeyboardBubbling.Continue;
												 });
			#endregion
			
			#region ctrl + A
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.A,
									 () =>
									 {
										 if (!Controller.Instance.IsCtrlDown)
											 return KeyboardBubbling.Continue;
										
										 if (AutoCompleteService.Instance.HasFocus)
											 return KeyboardBubbling.Continue;

										 Controller.Instance.SelectAll();

										 return KeyboardBubbling.Continue;
									 });
			#endregion

			#region ctrl + C
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.C,
									 () =>
									 {
										 if (!Controller.Instance.IsCtrlDown)
											 return KeyboardBubbling.Continue;

										 if (!Controller.Instance.Paper.HaveHighlightedText)
											 return KeyboardBubbling.Continue;

										 if (AutoCompleteService.Instance.HasFocus)
											 return KeyboardBubbling.Continue;

										 Controller.Instance.Copy();

										 return KeyboardBubbling.Continue;
									 });
			#endregion

			#region ctrl + X
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.X,
									 () =>
									 {
										 if (!Controller.Instance.IsEditable)
											 return KeyboardBubbling.Continue;

										 if (!Controller.Instance.IsCtrlDown)
											 return KeyboardBubbling.Continue;

										 if (!Controller.Instance.Paper.HaveHighlightedText)
											 return KeyboardBubbling.Continue;

										 if (AutoCompleteService.Instance.HasFocus)
											 return KeyboardBubbling.Continue;

										 Controller.Instance.Cut();

										 return KeyboardBubbling.Continue;
									 });
			#endregion

			#region ctrl + V
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyUp, Key.V,
									 () =>
									 {
										 if (!Controller.Instance.IsEditable)
											 return KeyboardBubbling.Continue;

										 if (!Controller.Instance.IsCtrlDown)
											 return KeyboardBubbling.Continue;

										 if (AutoCompleteService.Instance.HasFocus)
											 return KeyboardBubbling.Continue;

										 Controller.Instance.Paste();

										 return KeyboardBubbling.Continue;
									 });
			#endregion

			#region ctrl + Z
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyUp, Key.Z,
									 () =>
									 {
										 if (!Controller.Instance.IsEditable)
											 return KeyboardBubbling.Continue;

										 if (!Controller.Instance.IsCtrlDown)
											 return KeyboardBubbling.Continue;

										 if (AutoCompleteService.Instance.HasFocus)
											 return KeyboardBubbling.Continue;

										 UndoRedo.Instance.Undo();

										 return KeyboardBubbling.Continue;
									 });
			#endregion

			#region ctrl + Y
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyUp, Key.Y,
									 () =>
									 {
										 if (!Controller.Instance.IsEditable)
											 return KeyboardBubbling.Continue;

										 if (!Controller.Instance.IsCtrlDown)
											 return KeyboardBubbling.Continue;

										 if (AutoCompleteService.Instance.HasFocus)
											 return KeyboardBubbling.Continue;

										 UndoRedo.Instance.Redo();
										 
										 return KeyboardBubbling.Continue;
									 });
			#endregion
			
			#region HACK: + vs. =
			Controller.Instance.AddKeyboardEvent(KeyboardEventType.KeyDown, Key.Add,
			                                     () =>
			                                     	{
			                                     		if (Controller.Instance.IsShiftDown)
			                                     			return KeyboardBubbling.Continue;

			                                     		Controller.Instance.AddChar('=');
			                                     		return KeyboardBubbling.Continue;
			                                     	});
			#endregion
		}
	}
}