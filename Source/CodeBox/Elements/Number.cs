using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CodeBox.Core.Elements
{
	public class Number : Canvas
	{
		public static Number New(int number)
		{
			var n = new Number();
			n.Initialize();
			n.SetNumber(number);
			
			return n;
		}

		public Ellipse EllipseLarge;
		public Ellipse EllipseSmall;

		private Rectangle rectBG;
		private TextBlock tbNumber;
		private PaperLine Line
		{
			get { return Controller.Instance.Paper.LineAt(LineIndex); }
		}
		private int LineIndex
		{
			get { return int.Parse(tbNumber.Text) - 1; }
		}
		
		public Number()
		{
			Height = Character.CHAR_HEIGHT;
			Width = 40;
		}
		
		public void Initialize()
		{
			Cursor = Cursors.IBeam;
			Children.Clear();
			
			#region default children
			rectBG = new Rectangle();
			rectBG.Fill = new SolidColorBrush(Colors.Transparent);
			rectBG.Width = Width;
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
			tbNumber.Margin = new Thickness(15, 0, 0, 0);
			tbNumber.Foreground = new SolidColorBrush(Color.FromArgb(255, 124, 150, 93));
			tbNumber.TextAlignment = TextAlignment.Right;
			tbNumber.FontFamily = new FontFamily("Verdana");
			tbNumber.Width = 23;
			Children.Add(tbNumber);
			#endregion
			
			rectBG.MouseLeftButtonDown += MouseDown;
			EllipseLarge.MouseLeftButtonDown += MouseDown;
			EllipseSmall.MouseLeftButtonDown += MouseDown;
			
			// enable debugging?
			if (Configuration.Debugger.IsEnabled)
			{
				rectBG.Cursor = Cursors.Hand;
				EllipseLarge.Cursor = Cursors.Hand;
				EllipseSmall.Cursor = Cursors.Hand;
			}
			
			// enable highlighting when mouse is over Number
			tbNumber.MouseLeftButtonDown += OnNumberDown;
			tbNumber.MouseEnter += OnNumberMouseEnter;
			
			// hover effect
			MouseEnter += (s, e) => Line.PaperLineMouseEnter(s, e);
			MouseLeave += (s, e) => Line.PaperLineMouseLeave(s, e);
		}

		private void MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (Configuration.Debugger.IsEnabled && Configuration.Debugger.OnLineNumberClick != null)
				Configuration.Debugger.OnLineNumberClick(int.Parse(tbNumber.Text));
		}

		private void OnNumberDown(object sender, MouseButtonEventArgs e)
		{
			Controller.Instance.Paper.HighlightFrom(LineIndex, 0);
			Controller.Instance.Paper.HighlightUpto(LineIndex, Line.LastIndex);
		}

		private void OnNumberMouseEnter(object sender, MouseEventArgs e)
		{
			if (!Controller.Instance.Paper.IsMouseDown)
				return;

			var pos = Controller.Instance.Paper.HighlightFromIndex.Line > LineIndex ? 0 : Line.LastIndex;
			Controller.Instance.Paper.HighlightUpto(LineIndex, pos);
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