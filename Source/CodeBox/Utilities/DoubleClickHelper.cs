using System;
using System.Windows;
using System.Windows.Input;

namespace CodeBox.Core.Utilities
{
	public class DoubleClickHelper
	{
		private long lastClickedTime;
		private DoubleClickHandler callback;

		public delegate void DoubleClickHandler(object sender, MouseButtonEventArgs e);

		public static void Attach(object doubleClickTarget, DoubleClickHandler function)
		{
			var instance = new DoubleClickHelper();
			var target = doubleClickTarget as UIElement;

			target.MouseLeftButtonUp += instance.MouseUp;
			target.MouseLeftButtonDown += instance.MouseDown;

			instance.callback = function;
		}
		
		private DoubleClickHelper()
		{
			
		}

		private void MouseUp(object sender, MouseButtonEventArgs e)
		{
			lastClickedTime = DateTime.Now.Ticks / 10000;
		}

		private void MouseDown(object sender, MouseButtonEventArgs e)
		{
			long currentMillis = DateTime.Now.Ticks / 10000;

			if (currentMillis - lastClickedTime < 100 && currentMillis - lastClickedTime > 0)
			{
				callback(sender, e);
			}

		}
	}
}