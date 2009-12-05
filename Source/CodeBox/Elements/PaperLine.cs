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
		public Brush CurrentBrush { get; set; }
		
		public int SelfIndex { get; set; }

		public Number Number
		{
			get
			{
				return (Number)Controller.Instance.NumberPanel.Children[SelfIndex];
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

		public IEnumerable<Character> Characters
		{
			get
			{
				foreach (var child in Children)
				{
					if (child is Character)
						yield return (child as Character);
				}
			}
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
			MouseLeftButtonDown += PaperLineMouseDown;
			MouseMove += PaperLineMouseMove;
			MouseLeftButtonUp += PaperLineMouseUp;
			MouseEnter += PaperLineMouseEnter;
			MouseLeave += PaperLineMouseLeave;
		}

		public void PaperLineMouseLeave(object sender, MouseEventArgs e)
		{
			if (SelfIndex != -1)
			{
				Background = CurrentBrush;
				Number.UnHover();
			}
		}

		public void PaperLineMouseEnter(object sender, MouseEventArgs e)
		{
			if (SelfIndex != -1)
			{
				if (Controller.Instance.Paper.IsMouseDown && !(sender is Number))
				{
					Controller.Instance.Paper.HighlightUpto(SelfIndex, 0);
				}

				Background = new SolidColorBrush(Configuration.HoverColor);
				Number.Hover();
			}
		}

		private void PaperLineMouseDown(object sender, MouseButtonEventArgs e)
		{
			int charIndex = GetCharIndexFromWidth(e.GetPosition(this).X);

			if ((charIndex == LastIndex) && (Controller.Instance.Paper.Line != SelfIndex)) /* HAAACK, and works! (if you click on an unfocused line in an area with no charachters, it sets the cursor before the last char, not after) */
				charIndex++;

			Controller.Instance.Paper.HighlightFrom(SelfIndex, charIndex);
			Controller.Instance.Paper.ClickHandledByLine = true;
		}

		private void PaperLineMouseMove(object sender, MouseEventArgs e)
		{
			if (!Controller.Instance.Paper.IsMouseDown)
				return;

			int charIndex = GetCharIndexFromWidth(e.GetPosition(this).X);
			Controller.Instance.Paper.HighlightUpto(SelfIndex, charIndex);
		}

		private void PaperLineMouseUp(object sender, MouseButtonEventArgs e)
		{

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
			int index = Controller.Instance.Paper.Children.IndexOf(this);

			if (index + 1 >= Controller.Instance.Paper.Children.Count)
				return null;

			return (PaperLine)Controller.Instance.Paper.Children[index + 1];
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

		public IEnumerable<Character> GetCharacters(int from, int to)
		{
			if (from < 0)
				from = 0;
			else if (to >= Children.Count)
				to = Children.Count - 1;

			for (; from <= to; from++)
				if (Children[from] is Character)
					yield return Children[from] as Character;
		}

		public void AddCharacters(IEnumerable<Character> chars, int position)
		{
			foreach (var c in chars)
			{
				Children.Insert(position++, c);
			}
		}

		public void RemoveCharacters(IEnumerable<Character> chars)
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
	}
}