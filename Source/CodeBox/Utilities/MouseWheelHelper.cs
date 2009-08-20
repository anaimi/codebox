using System;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace CodeBox.Core.Utilities
{
	public class MouseWheelHelper
	{
		private static Worker MouseWheelWorker;
		private bool isMouseOver;

		public MouseWheelHelper(UIElement element)
		{
			if (MouseWheelWorker == null)
				MouseWheelWorker = new Worker();

			MouseWheelWorker.Moved += HandleMouseWheel;

			element.MouseEnter += HandleMouseEnter;
			element.MouseLeave += HandleMouseLeave;
			element.MouseMove += HandleMouseMove;
		}

		public event Action<int> Moved;

		private void HandleMouseWheel(int delta)
		{
			if (isMouseOver)
				Moved(delta);
		}

		private void HandleMouseEnter(object sender, EventArgs e)
		{
			isMouseOver = true;
		}

		private void HandleMouseLeave(object sender, EventArgs e)
		{
			isMouseOver = false;
		}

		private void HandleMouseMove(object sender, EventArgs e)
		{

			isMouseOver = true;
		}

		private class Worker
		{
			public Worker()
			{
				if (!HtmlPage.IsEnabled)
					return;
					
				HtmlPage.Window.AttachEvent("DOMMouseScroll", HandleMouseWheel);
				HtmlPage.Window.AttachEvent("onmousewheel", HandleMouseWheel);
				HtmlPage.Document.AttachEvent("onmousewheel", HandleMouseWheel);
			}
			
			public event Action<int> Moved;

			private void HandleMouseWheel(object sender, HtmlEventArgs args)
			{
				double delta = 0;
				var eventObj = args.EventObject;

				if (eventObj.GetProperty("wheelDelta") != null)
				{
					delta = ((double)eventObj.GetProperty("wheelDelta")) / 120;

					if (HtmlPage.Window.GetProperty("opera") != null)
						delta = -delta;
				}

				else if (eventObj.GetProperty("detail") != null)
				{
					delta = -((double)eventObj.GetProperty("detail")) / 3;

					if (HtmlPage.BrowserInformation.UserAgent.IndexOf("Macintosh") != -1)
						delta = delta * 3;
				}

				if (delta == 0 || Moved == null)
					return;

				Moved((int)delta);
			}
		}
	}

	//public class MouseWheelHelper
	//{
	//    public static void Attach(object element)
	//    {
	//        var target = element as UIElement;

	//        target.MouseWheel += MouseWheel;
	//    }

	//    private static void MouseWheel(object sender, MouseWheelEventArgs e)
	//    {
	//        var target = sender as UIElement;
	//        var associatedObj = FrameworkElementAutomationPeer.FromElement(target);
	//        var provider = associatedObj.GetPattern(PatternInterface.Scroll) as IScrollProvider;
			
	//        if (provider == null)
	//            return;
			
	//        int direction = Math.Sign(e.Delta);
	//        var scroll = (direction < 0) ? ScrollAmount.SmallIncrement : ScrollAmount.SmallDecrement;
			
	//        provider.Scroll(ScrollAmount.NoAmount, scroll);
	//    }

	//    private MouseWheelHelper()
	//    {
			
	//    }
	//}
}