﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Text;
using CodeBox.CodeLexer;
using CodeBox.Core.Services.AutoComplete;

namespace CodeBox.Core.Elements
{
	public class Paper : StackPanel
	{
		private StackPanel numbers
		{
			get { return Controller.Instance.NumberPanel; }
		}
		
		public string Text
		{
			get
			{
				var text = new StringBuilder();

				foreach (PaperLine line in Lines)
				{
					foreach (var charachter in line.Characters)
						text.Append(charachter.Char);

					text.Append('\n');
				}

				string str = text.ToString().Substring(0, text.ToString().Length - 1);
				return str;
			}

			set
			{
				Children.Clear();
				Children.Add(new PaperLine());
				_line = 0; _position = 0;

				foreach (char c in value)
				{
					if (c == '\r')
						continue;

					if (c == '\n')
					{
						Children.Insert(Line + 1, new PaperLine());
						UpdateCaret(Line + 1, 0);
						continue;
					}

					// add it
					CurrentLine.Add(new Character(c), Position);
					Position++;
				}
			}
		}
				
		public bool IsMouseDown { get; set; }
		public bool ClickHandledByLine { get; set; }
		
		// caret
		private Rectangle caret;
		private DispatcherTimer caretTimer;
		public bool ShowCaret { get; set; }
		private int _line, _position;
		public int Line
		{
			get { return _line; }
			set
			{
				_line = value;
				
				var line = LineAt(_line);
				
				if (_position > line.LastIndex)
					_position = (line.Children.Count == 0) ? 0 : line.LastIndex + 1;
				
				UpdateCaretPosition();
			}
		}
		public int Position
		{
			get { return _position; }
			set
			{
				_position = value;
				UpdateCaretPosition();
			}
		}
		
		// text highlighting
		public List<Character> HighlightedBlocks;
		public Index HighlightFromIndex { get; private set; }
		public Index HighlightToIndex { get; private set; }
		public bool HaveHighlightedText
		{
			get
			{
				if (HighlightedBlocks == null)
					return false;

				return (HighlightedBlocks.Count != 0);
			}
		}
		public IEnumerable<PaperLine> HighlightedLines
		{
			get
			{
				foreach(var line in Controller.Instance.Paper.HighlightedBlocks.Select(b => b.Line).Distinct())
				{
					yield return LineAt(line);
				}
			}
		}

		public Paper()
		{
			// paper properties
			Background = new SolidColorBrush(Colors.White);
			Cursor = Cursors.IBeam;

			// setup caret
			caret = new Rectangle();
			caret.Fill = new SolidColorBrush(Colors.Black);
			caret.Height = 12;
			caret.Width = 1;
			caret.Margin = new Thickness(-1, 0, 0, 0);

			// setup caret timer
			caretTimer = new DispatcherTimer();
			caretTimer.Interval = TimeSpan.FromSeconds(0.5);
			caretTimer.Tick += delegate { caret.Visibility = (caret.Visibility == Visibility.Visible || !ShowCaret) ? Visibility.Collapsed : Visibility.Visible; };
			caretTimer.Start();
			
			// setup highlighting indicies
			HighlightedBlocks = new List<Character>();
			HighlightFromIndex = new Index(0, 0);
			HighlightToIndex = new Index(0, 0);
			
			// events
			MouseLeftButtonDown += Paper_MouseLeftButtonDown;
			MouseLeftButtonUp += Paper_MouseLeftButtonUp;
		}

		public PaperLine LineAt(int index)
		{
			if (index < 0 || index > Children.Count - 1)
				return null;

			return (PaperLine)Children[index];
		}

		public IEnumerable<PaperLine> Lines
		{
			get
			{
				foreach (var child in Children)
				{
					if (child is PaperLine)
						yield return child as PaperLine;
				}
			}
		}

		public PaperLine CurrentLine
		{
			get
			{
				PaperLine line;

				if (Children.Count == 0)
				{
					line = new PaperLine();
					InsertLine(0, line);
				}
				else
				{
					line = (PaperLine)Children[_line];
				}

				return line;
			}
		}

		public PaperLine NextLine
		{
			get
			{
				return LineAt(_line + 1);
			}
		}

		public PaperLine PrevLine
		{
			get
			{
				return LineAt(_line - 1);
			}
		}

		public bool IsInFirstLine
		{
			get
			{
				if (_line == 0)
					return true;

				return false;
			}
		}

		public bool IsInLastLine
		{
			get
			{
				if (_line == Children.Count - 1)
					return true;

				return false;
			}
		}

		public Character CharacterBeforeCaret
		{
			get
			{
				if (_position == 0)
					return null;

				return CurrentLine.Children[_position - 1] as Character;
			}
		}

		public Character CharacterAfterCaret
		{
			get
			{
				if (_position == CurrentLine.LastIndex)
					return null;

				return CurrentLine.Children[_position + 1] as Character;
			}
		}
		
		public void UpdateLinesIndices(int index)
		{
			for (; index < Children.Count; index++)
			{
				LineAt(index).SelfIndex = index;
			}

			if (Configuration.Debugger.OnLinesIndicesChanged != null)
				Configuration.Debugger.OnLinesIndicesChanged();
		}
		
		#region insert/remove line
		public void InsertLine(int index, PaperLine line)
		{
			Children.Insert(index, line);
			UpdateNumberPanel();
			UpdateLinesIndices(index);			
		}

		public void RemoveLine(int index)
		{
			Children.RemoveAt(index);
			UpdateLinesIndices(index);
		}
		#endregion
		
		#region mouse handling
		private void Paper_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (CurrentLine.Characters.Count() == 0)
			{
				Controller.Instance.GetFocus();
				UpdateCaret(Line, 1);
			}
			
			IsMouseDown = true;
			e.Handled = true;
			
			if (Controller.Instance.HaveService<AutoCompleteService>())
			{
				AutoCompleteService.Instance.HideList();
			}
			
			if (!ClickHandledByLine)
			{
				// user clicked on what is beyond paper lines (i.e. on paper) so Caret must go to end
				int line = Children.Count - 1;
				int pos = LineAt(line).LastIndex + 1;
				RemoveCaret();
				HighlightFrom(line, pos); // will also restore caret
			}

			ClickHandledByLine = false;
			Controller.Instance.MouseDown();
		}

		private void Paper_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			IsMouseDown = false;
			e.Handled = true;
			
			Controller.Instance.MouseUp();
		}
		#endregion
		
		#region highlighting methods
		public string GetHighlightedText()
		{
			var upperLine = Math.Min(HighlightFromIndex.Line, HighlightToIndex.Line);
			var lowerLine = Math.Max(HighlightFromIndex.Line, HighlightToIndex.Line);
			
			var text = new StringBuilder("");

			for (var i = upperLine; i <= lowerLine; i++)
			{
				var characters = HighlightedBlocks.Where(b => b.Line == i).ToList();
				characters.ForEach(c => text.Append(c.Char));

				if (i != lowerLine)
					text.Append("\r\n");
			}
			
			return text.ToString();
		}

		public void HighlightAll()
		{
			RemoveCaret();
			int lastLineChildrenCount = LineAt(Children.Count - 1).Children.Count;

			if (lastLineChildrenCount == 0)
				lastLineChildrenCount++;

			HighlightFrom(0, 0);
			RemoveCaret();
			HighlightUpto(Children.Count - 1, lastLineChildrenCount - 1);

			UpdateCaret(Children.Count - 1, lastLineChildrenCount);
		}

		public void HighlightFrom(int line, int pos)
		{
			ClearHighlights();
			UpdateCaret(line, pos);
			HighlightFromIndex = new Index(line, pos);
		}

		public void HighlightUpto(int line, int pos)
		{
			var newBlocks = new List<Character>();

			if (HighlightToIndex != null && HighlightToIndex.Line == line && HighlightToIndex.Position == pos)
				return;

			// get blocks
			int fromLine = 0, fromPos = 0, toLine = 0, toPos = 0;
			PaperLine pLine;
			if (HighlightFromIndex.Line == line)
			{
				pLine = (PaperLine)Children[line];
				fromPos = Math.Min(HighlightFromIndex.Position, pos);
				toPos = Math.Max(HighlightFromIndex.Position, pos);

				newBlocks.AddRange(pLine.GetCharacters(fromPos, toPos));
			}
			else if (HighlightFromIndex.Line > line)
			{
				fromLine = line;
				toLine = HighlightFromIndex.Line;
				
				for (; fromLine <= toLine; fromLine++)
				{
					pLine = (PaperLine)Children[fromLine];

					fromPos = (fromLine == line) ? pos : 0;
					toPos = (fromLine == HighlightFromIndex.Line) ? HighlightFromIndex.Position - 1 : pLine.LastIndex;

					newBlocks.AddRange(pLine.GetCharacters(fromPos, toPos));
				}
			}
			else if (HighlightFromIndex.Line < line)
			{
				fromLine = HighlightFromIndex.Line;
				toLine = line;

				for (; fromLine <= toLine; fromLine++)
				{
					pLine = (PaperLine)Children[fromLine];

					fromPos = (fromLine == HighlightFromIndex.Line) ? HighlightFromIndex.Position : 0;
					toPos = (fromLine == line) ? pos : pLine.LastIndex;

					newBlocks.AddRange(pLine.GetCharacters(fromPos, toPos));
				}
			}

			// set CaretPosition
			UpdateCaret(line, pos);

			// the blocks that are already highlighted, but now need to be unhighlighted
			HighlightedBlocks.Except(newBlocks).ToList().ForEach(b => b.Unhighlight());
			
			// highlight the rest
			HighlightedBlocks = newBlocks;
			foreach (var block in HighlightedBlocks)
			{
				if (block.CurrentBackgroundState != Character.BackgroundState.Blue)
					block.HighlightBlue();
			}

			// keep track to be used in RemoveHighlights
			HighlightToIndex = new Index(line, pos);
		}

		public void ClearHighlights()
		{
			HighlightedBlocks.ForEach(b => b.Unhighlight());
			HighlightedBlocks.Clear();
		}

		public void RemoveHighlights()
		{
			if (HighlightedBlocks == null || HighlightedBlocks.Count == 0)
				return;

			// lines
			var firstLine = LineAt(Math.Min(HighlightFromIndex.Line, HighlightToIndex.Line));
			var lastLine = LineAt(Math.Max(HighlightFromIndex.Line, HighlightToIndex.Line));

			// setup endCharIndex
			int endCharIndex = 0;
			if (firstLine == lastLine)
			{
				endCharIndex = Math.Min(HighlightFromIndex.Position, HighlightToIndex.Position);
			}
			else if (firstLine == LineAt(HighlightFromIndex.Line))
			{
				endCharIndex = HighlightFromIndex.Position;
			}
			else // firstLine is actually highlightLineTo
			{
				endCharIndex = HighlightToIndex.Position;
			}

			// remove the blocks
			foreach (var c in HighlightedBlocks)
			{
				var l = (PaperLine)c.Parent;
				l.Remove(c);
			}

			// remove the lines (between the first and last)
			var linesToRemove = new List<PaperLine>();
			for (int i = firstLine.SelfIndex + 1; i < lastLine.SelfIndex; i++)
				linesToRemove.Add(LineAt(i));
			for (int i = 0; i < linesToRemove.Count; i++)
				Children.Remove(linesToRemove[i]); // do not use RemoveLine since indices will be updated on every iteration - instead update at the end of this method

			// glue the remaining of the last line with the remaining of the first line
			if (firstLine != lastLine)
			{
				var index = (firstLine.LastIndex == 0) ? 0 : firstLine.LastIndex + 1;
				var chars = lastLine.GetCharacters(0, lastLine.LastIndex).ToList();
				lastLine.RemoveCharacters(chars);
				firstLine.AddCharacters(chars, index);

				Children.Remove(lastLine);
			}

			// reset and set
			HighlightedBlocks = new List<Character>();
			UpdateLinesIndices(firstLine.SelfIndex);
			UpdateCaret(Children.IndexOf(firstLine), endCharIndex);
			Controller.Instance.TextCompletelyChanged();
		}
		#endregion

		#region caret related methods
		public void UpdateCaretPosition()
		{
			PaperLine parent = caret.Parent as PaperLine;
			caret.Visibility = Visibility.Visible;
			PaperLine line = CurrentLine;

			if (parent != null)
			{
				parent.Children.Remove(caret);
			}

			if (_position > line.Children.Count) //ehm
				_position = line.Children.Count;

			line.Children.Insert(_position, caret);
			UpdateNumberPanel();
		}

		public void UpdateCaret(int line, int position)
		{
			_line = line;
			_position = position;
			UpdateCaretPosition();
		}

		public void RemoveCaret()
		{
			CurrentLine.Remove(caret);
		}

		public void RestoreCaret()
		{
			CurrentLine.Children.Insert(_position, caret);
		}
		#endregion
		
		public List<Character> GetAllCharacters()
		{
			var chars = new List<Character>();

			foreach (PaperLine line in Children)
			{
				chars.AddRange(line.Characters);
			}
			
			return chars;
		}

		public void UpdateNumberPanel()
		{
			int countOfNumbers = numbers.Children.Count;
			int countOfLines = Children.Count;

			if (countOfNumbers == countOfLines)
			{
				return;
			}
			else if (countOfNumbers > countOfLines)
			{
				for (; countOfNumbers > countOfLines; countOfNumbers--)
					numbers.Children.RemoveAt(countOfNumbers - 1);
			}
			else if (countOfNumbers < countOfLines)
			{
				for (; countOfNumbers < countOfLines; countOfNumbers++)
					numbers.Children.Add(Number.New(countOfNumbers));
			}
		}
	}
}