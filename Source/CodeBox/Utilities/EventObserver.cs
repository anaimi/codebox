using System;
using System.Windows.Threading;

namespace CodeBox.Core.Utilities
{
	/// <summary>
	/// Used to inform an observer of an event after a specified delay
	/// If the event occured while waiting (*delaying* a previous event) the timer is reset
	/// </summary>
	public class EventObserver
	{
		public double DelayInSeconds { get; private set; }
		public Action EventHandler { get; private set; }

		private DispatcherTimer timer;

		public EventObserver(double delayInSeconds, Action handler)
		{
			DelayInSeconds = delayInSeconds;			
			EventHandler = handler;
			
			if (delayInSeconds >= 0)
			{
				timer = new DispatcherTimer();
				timer.Tick += (s, e) =>
				{
					EventHandler();
					timer.Stop();
				};
			}
			
		}

		public void EventOccured()
		{
			if (DelayInSeconds <= 0)
			{
				EventHandler();
				return;
			}
			
			if (timer.IsEnabled)
				timer.Stop();

			timer.Interval = TimeSpan.FromSeconds(DelayInSeconds);
			timer.Start();
		}
	}
}
