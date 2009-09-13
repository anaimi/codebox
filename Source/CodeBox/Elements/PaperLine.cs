using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System;
using CodeBox.Core;

namespace CodeBox.Core.Elements
{
	public class PaperLine : StackPanel
	{
		public Brush CurrentBrush;
		
		private Paper paper
		{
			get { return Controller.Instance.Paper; }
		}
		private int selfIndex
		{
			get { return paper.Children.IndexOf(this); }
		}

		public PaperLine()
		{
			// settings
			Orientation = Orientation.Horizontal;
			HorizontalAlignment = HorizontalAlignment.Stretch;
			Margin = new Thickness(0, 0, 0, 0);
			Height = 15;
			CurrentBrush = Background = new SolidColorBrush(Colors.White);
			
			// events
			Loaded += PaperLine_Loaded;
			MouseLeftButtonDown += PaperLineMouseDown;
			MouseMove += PaperLineMouseMove;
			MouseLeftButtonUp += PaperLineMouseUp;
			MouseEnter += PaperLineMouseEnter;
			MouseLeave += PaperLineMouseLeave;
		}

		private void PaperLine_Loaded(object sender, RoutedEventArgs e)
		{
			// handling number
			Number.MouseEnter += PaperLineMouseEnter;
			Number.MouseLeave += PaperLineMouseLeave;
		}

		public void PaperLineMouseLeave(object sender, MouseEventArgs e)
		{
			if (selfIndex != -1)
			{
				Background = CurrentBrush;
				Number.UnHover();
			}
		}

		private void PaperLineMouseEnter(object sender, MouseEventArgs e)
		{
			if (selfIndex != -1)
			{
				if (Controller.Instance.Paper.IsMouseDown && !(sender is Number))
				{
					Controller.Instance.Paper.HighlightUpto(selfIndex, 0);
				}

				Background = new SolidColorBrush(Configuration.HoverColor);
				Number.Hover();
			}
		}

		private void PaperLineMouseDown(object sender, MouseButtonEventArgs e)
		{
			int charIndex = GetCharIndexFromWidth(e.GetPosition(this).X);

			if ((charIndex == LastIndex) && (paper.Line != selfIndex)) /* HAAACK, and works! (if you click on an unfocused line in an area with no charachters, it sets the cursor before the last char, not after) */
				charIndex++;

			paper.HighlightFrom(selfIndex, charIndex);
			paper.ClickHandledByLine = true;
		}

		private void PaperLineMouseMove(object sender, MouseEventArgs e)
		{
			if (!paper.IsMouseDown)
				return;

			int charIndex = GetCharIndexFromWidth(e.GetPosition(this).X);
			paper.HighlightUpto(selfIndex, charIndex);
		}

		private void PaperLineMouseUp(object sender, MouseButtonEventArgs e)
		{
			
		}

		private int GetCharIndexFromWidth(double widthInPixels)
		{
			double actual = 0;
			int index = 0;

			foreach (UIElement uie in Children)
			{
				if (uie.GetType() != typeof(Character))
					continue;

				Character c = (Character)uie;

				index = (widthInPixels >= (actual + c.ActualWidth / 2)) ? index + 1 : index;
				actual += c.ActualWidth;

				if (actual >= widthInPixels)
					break;
			}

			return index;
		}

		public Number Number
		{
			get
			{
				return (Number)Controller.Instance.NumberPanel.Children[selfIndex];
			}
		}		
		
		public Character LastCharacter
		{
			get
			{
				if (Children.Count == 0)
					return null;

				return (Character)Children[Children.Count - 1];
			}
		}

		public int LastIndex
		{
			get { return (Children.Count == 0) ? 0 : Children.Count - 1; }
		}

		public List<Character> Characters
		{
			get
			{
				List<Character> chars = new List<Character>();

				foreach (UIElement child in Children)
				{
					if (child.GetType() == typeof(Character))
						chars.Add((Character)child);
				}

				return chars;
			}
		}

		public void Add(UIElement c)
		{
			Children.Add(c);
		}

		public void Add(UIElement c, int index)
		{
			Children.Insert(index, c);
		}

		public void Remove(UIElement c)
		{
			Children.Remove(c);
		}

		public void Remove(int index)
		{
			Children.RemoveAt(index);
		}

		public PaperLine NextLine()
		{
			int index = paper.Children.IndexOf(this);

			if (index + 1 >= paper.Children.Count)
				return null;

			return (PaperLine)paper.Children[index + 1];
		}

		public UIElement At(int index)
		{
			if (index >= Children.Count && index > 0)//ehm
				index = Children.Count - 1;

			return Children[index];
		}

		public double PositionFromLeftInPixels(UIElement uie)
		{
			double position = 0;
			int index = Children.IndexOf(uie);

			for (int i = 0; i < index; i++)
			{
				if (Children[i].GetType() == typeof(Character))
					position += ((Character)Children[i]).ActualWidth;
				else if (Children[i].GetType() == typeof(Rectangle))
					position += ((Rectangle)Children[i]).ActualWidth;
			}

			return position;
		}

		public List<Character> GetCharacters(int from, int to)
		{
			List<Character> c = new List<Character>();

			if (from < 0)
				from = 0;
			else if (to >= Children.Count)
				to = Children.Count - 1;

			for (; from <= to; from++)
				if (Children[from].GetType() == typeof(Character))
					c.Add((Character)Children[from]);

			return c;
		}

		public void AddCharacters(List<Character> chars, int position)
		{
			foreach (var c in chars)
			{
				Children.Insert(position++, c);
			}
		}

		public void RemoveCharacters(List<Character> chars)
		{
			foreach (var c in chars)
				Children.Remove(c);
		}
		
		public void MigrateCharacters(PaperLine newLine, int fromIndex, int toIndex)
		{
			if (Children.Count == 0)
				return;
			
			var chars = new List<UIElement>();
			
			for (; fromIndex <= toIndex; fromIndex++)
				chars.Add(Children[fromIndex]);
				
			foreach (UIElement c in chars)
			{
				Children.Remove(c);
				newLine.Children.Add(c);
			}
		}
	}
}