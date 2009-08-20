using System;
using System.Windows.Threading;
using System.Windows;
using CodeBox.Core.Elements;
using CodeBox.Core.Utilities;

namespace CodeBox.Core
{
	public partial class Main
	{
		private DispatcherTimer textBoxTimer;

		public Main()
		{
			InitializeComponent();

			// textBoxTimer
			textBoxTimer = new DispatcherTimer();
			textBoxTimer.Interval = TimeSpan.FromMilliseconds(50);
			textBoxTimer.Tick += textboxReaderTick;
			textBoxTimer.Start();
			
			// controller and dependencies
			Controller.SetInstance(CanvasRoot, paper, numberPanel, textBox, textBoxTimer);
			
			Loaded += PageLoaded;
		}

		public void PageLoaded(object o, EventArgs e)
		{
			// set focus
			paper.MouseLeftButtonDown += delegate { textBox.Focus(); };
			textBox.LostFocus += textbox_LostFocus;
			textBox.GotFocus += delegate { paper.ShowCaret = true; };
			textBox.Focus();
			textBox.KeyDown += Controller.Instance.KeyDown;
			textBox.KeyUp += Controller.Instance.KeyUp;
			
			// support mouse wheel scrolling
			var scroller = new MouseWheelHelper(ScrollViewerRoot);
			scroller.Moved += (delta) =>
          	{
          		if (delta < 0)
          		{
					if (ScrollViewerRoot.ScrollableHeight > ScrollViewerRoot.VerticalOffset)
						ScrollViewerRoot.ScrollToVerticalOffset(ScrollViewerRoot.VerticalOffset + 40);
          		}
          		else
          		{
					if (0 < ScrollViewerRoot.VerticalOffset)
						ScrollViewerRoot.ScrollToVerticalOffset(ScrollViewerRoot.VerticalOffset - 40);
          		}
          	};
			
			// reset paper
			paper.Children.Clear();
			paper.Children.Add(new PaperLine());
		}

		private void textboxReaderTick(object sender, EventArgs e)
		{
			textBox.Text = textBox.Text.Replace("\r", "");

			if (textBox.Text == "" || textBox.Text == "\n")
				return;

			if (Controller.Instance.IsCtrlDown)
			{
				Controller.Instance.AddText(textBox.Text);
			}
			else
			{
				Controller.Instance.AddChar(textBox.Text[0]);
			}

			textBox.Text = "";
		}

		private void textbox_LostFocus(object sender, RoutedEventArgs e)
		{
			paper.ShowCaret = false;

			if (Controller.Instance.KeepFocus)
			{
				textBox.Focus();
				Controller.Instance.KeepFocus = false;
			}

			System.Diagnostics.Debug.WriteLine("Lost focus");
		}
		
		public void UpdateDimensions(double width, double height)
		{
			Width = width;
			Height = height;

			GridRoot.MinWidth = width - 20;
			GridRoot.MinHeight = height - 20;
		}

		private void GridRoot_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			CanvasRoot.Width = GridRoot.ActualWidth;
			CanvasRoot.Height = GridRoot.ActualHeight;
		}
	}
}
