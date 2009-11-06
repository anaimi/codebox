using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using CodeBox.Core.Elements;
using CodeBox.Core.Utilities;
using System.Threading;

namespace CodeBox.Core
{
	public partial class Main
	{
		public Main()
		{
			InitializeComponent();

			textBox.TextChanged += TextChanged;

			Controller.SetInstance(CanvasRoot, paper, numberPanel, textBox);
			
			Loaded += PageLoaded;
		}

		public void PageLoaded(object o, EventArgs e)
		{
			// wires the scrollbar event - see WireScrollBarEvent to know how MSFT is making everybody's life difficult by sealing controls
			LayoutUpdated += WireScrollBarEvent;
			
			// set focus
			paper.MouseLeftButtonDown += (s, ea) => textBox.Focus();
			textBox.GotFocus += (s, ea) => { paper.ShowCaret = true; };			
			textBox.LostFocus += TextBoxLostFocus;
			textBox.Focus();
			textBox.KeyDown += Controller.Instance.KeyDown;
			textBox.KeyUp += Controller.Instance.KeyUp;
			
			// support mouse wheel scrolling
			var scroller = new MouseWheelHelper(ScrollViewerRoot);
			scroller.Moved += MouseWheelScrolled;
			
			// reset paper
			paper.Children.Clear();
			paper.Children.Add(new PaperLine());
		}
		
		private void TextChanged(object sender, EventArgs e)
		{
			if (Controller.Instance.IsCtrlDown)
			{
				textBox.Text = "";
				return;
			}
			
			textBox.Text = textBox.Text.Replace("\r", "");

			if (textBox.Text == "" || textBox.Text == "\n")
				return;

			Controller.Instance.AddText(textBox.Text);

			textBox.Text = "";
		}

		private void TextBoxLostFocus(object sender, RoutedEventArgs e)
		{
			paper.ShowCaret = false;

			if (Controller.Instance.KeepFocus)
			{
				textBox.Focus();
				Controller.Instance.KeepFocus = false;
			}
		}

		#region scrolling
		private void MouseWheelScrolled(int delta)
		{
			if (delta < 0)
			{
				if (ScrollViewerRoot.ScrollableHeight > ScrollViewerRoot.VerticalOffset)
				{
					var offset = ScrollViewerRoot.VerticalOffset + 40;
					if (offset > ScrollViewerRoot.ScrollableHeight)
						offset = ScrollViewerRoot.ScrollableHeight;

					Controller.Instance.Scrolled(offset);
					// ScrollToVerticalOffset is Asynch, thats why we need to call Scrolled and compute values individualy
					ScrollViewerRoot.ScrollToVerticalOffset(offset);
				}
			}
			else
			{
				if (0 < ScrollViewerRoot.VerticalOffset)
				{
					var offset = ScrollViewerRoot.VerticalOffset - 40;
					if (offset < 0)
						offset = 0;

					Controller.Instance.Scrolled(offset);
					ScrollViewerRoot.ScrollToVerticalOffset(offset);
				}
			}
		}

		private void WireScrollBarEvent(object sender, EventArgs e)
		{
			// Q: What is this?
			// A: This is to create an OnScroll event. Especially useful when drawing stuff into the editor. (like debug tooltip boxes)

			// Q: Why inside LayoutUpdated? why not directly?
			// A: Because the visual tree of the editor will not be populated on PageLoad. I know. Not intuitive. See http://blogs.msdn.com/silverlight_sdk/archive/2008/10/24/loaded-event-timing-in-silverlight.aspx

			// Q: Why are you getting the ScrollBar using a VisualTree?
			// A: Because the freaking thing is sealed!

			// Q: The code below doesn't look like it will work. Are you sure this works? Can you guarantee this will be compatible with later version of SL?
			// A: It works on my machine! (at least for now). I'll put it in a try-catch clause just in case.

			try
			{
				// get scroll bar via magic
				var allChildren = ScrollViewerRoot.GetAllChildren().ToList(); // could this be less efficient?
				var scrollbarsOnly = allChildren.Where(o1 => o1 is ScrollBar).ToList();
				var verticalScrollbar = (scrollbarsOnly[2] as ScrollBar);

				// assign event and remove self
				verticalScrollbar.Scroll += (s, se) => Controller.Instance.Scrolled(se.NewValue);
			}
			catch
			{
				// send email to ANaimi
			}

			LayoutUpdated -= WireScrollBarEvent;
		}
		#endregion
		
		#region dimensions
		public void UpdateDimensions(double width, double height)
		{
			Width = width;
			Height = height;

			GridRoot.MinWidth = width - 20;
			GridRoot.MinHeight = height - 20;
		}

		private void GridRootSizeChanged(object sender, SizeChangedEventArgs e)
		{
			CanvasRoot.Width = GridRoot.ActualWidth;
			CanvasRoot.Height = GridRoot.ActualHeight;
		}
		#endregion
	}
}
