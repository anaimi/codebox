using System;
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

namespace CodeBox.Core.Elements
{
	public class Number : Canvas
	{
		public static Number New(int number)
		{
			Number n = new Number();
			n.SetNumber(number);
			return n;
		}

		public Ellipse EllipseLarge;
		public Ellipse EllipseSmall;

		private Rectangle rectBG;
		private TextBlock tbNumber;

		public Number()
		{
			Height = Character.CHAR_HEIGHT;
			Width = 30;
			
			#region default children
			rectBG = new Rectangle();
			rectBG.Fill = new SolidColorBrush(Colors.Transparent);
			rectBG.Width = 30;
			rectBG.Height = 15;
			Children.Add(rectBG);

			EllipseLarge = new Ellipse();
			EllipseLarge.Width = 11;
			EllipseLarge.Height = 11;
			EllipseLarge.Margin = new Thickness(2, 2, 0, 0);
			EllipseLarge.Fill = new SolidColorBrush(Colors.Red);
			EllipseLarge.Visibility = Visibility.Collapsed;
			Children.Add(EllipseLarge);
			
			EllipseSmall = new Ellipse();
			EllipseSmall.Width = 9;
			EllipseSmall.Height = 9;
			EllipseSmall.Margin = new Thickness(3, 3, 0, 0);
			EllipseSmall.Fill = new SolidColorBrush(Colors.Yellow);
			EllipseSmall.Visibility = Visibility.Collapsed;
			Children.Add(EllipseSmall);
			
			tbNumber = new TextBlock();
			tbNumber.Text = "1";
			tbNumber.Margin = new Thickness(0, 0, 0, 0);
			tbNumber.Foreground = new SolidColorBrush(Color.FromArgb(255, 124, 150, 93));
			tbNumber.TextAlignment = TextAlignment.Right;
			tbNumber.FontFamily = new FontFamily("Verdana");
			tbNumber.Width = 28;
			Children.Add(tbNumber);
			#endregion
			
			if (Configuration.Debugger.IsEnabled)
				tbNumber.Cursor = Cursors.Hand;

			MouseLeftButtonDown += (s, e) => { if (Configuration.Debugger.IsEnabled) Configuration.Debugger.OnLineNumberClick(int.Parse(tbNumber.Text)); };
		}

		public void SetNumber(int number)
		{
			tbNumber.Text = (number + 1).ToString();
		}
		
		public void Hover()
		{
			rectBG.Fill = new SolidColorBrush(Configuration.HoverColor);
		}
		
		public void UnHover()
		{
			rectBG.Fill = new SolidColorBrush(Colors.Transparent);
		}

		public void ResetState()
		{
			rectBG.Fill = new SolidColorBrush(Colors.Transparent);
			EllipseLarge.Visibility = Visibility.Collapsed;
			EllipseSmall.Visibility = Visibility.Collapsed;
		}
	}
}