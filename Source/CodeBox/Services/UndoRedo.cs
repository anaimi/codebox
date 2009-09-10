using System.Collections.Generic;
using System.Windows.Threading;
using System;
using CodeBox.Core.Services;

namespace CodeBox.Core.Services
{
	public class UndoRedo : IService
	{
		private const double PAUSE_TO_STORE = 1.5;
		
		#region instance
		private static UndoRedo _instance;
		public static UndoRedo Instance
		{
			get
			{
				if (_instance == null)
					_instance = new UndoRedo();

				return _instance;
			}
		}

		private UndoRedo()
		{

		}
		#endregion
		
		private int currentIndex = 0;
		private DispatcherTimer timer;
		private List<State> states = new List<State>();

		private void AddState(object sender, EventArgs e)
		{
			string text = Controller.Instance.Text;

			if (states[currentIndex].Text != text)
			{
				states.RemoveRange(currentIndex + 1, states.Count - currentIndex - 1);

				currentIndex++;
				states.Add(new State(text, Controller.Instance.Paper.Line, Controller.Instance.Paper.Position));
			}

			timer.Stop();
		}

		public void Busy()
		{
			// this method is called with every text update
			// this is to postpone the storage of the new state PAUSE_TO_STORE seconds from now
			// so if the user was typing fast, it will only store once the user pauses
			timer.Stop();
			timer.Start();
		}

		public void Undo()
		{
			if (currentIndex == 0)
				return;

			currentIndex--;
			GoToState(currentIndex);
		}

		public void Redo()
		{
			if (currentIndex == states.Count - 1)
				return;

			currentIndex++;
			GoToState(currentIndex);
		}

		private void GoToState(int index)
		{
			Controller.Instance.Paper.Dispatcher.BeginInvoke(() => Controller.Instance.Text = states[index].Text);
			Controller.Instance.Paper.Dispatcher.BeginInvoke(() => Controller.Instance.Paper.UpdateCaret(states[index].Line, states[index].Position));
		}

		public void Initialize()
		{
			states.Add(new State("", 0, 0));

			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(PAUSE_TO_STORE);
			timer.Tick += AddState;

			Controller.Instance.OnTextChanged += SaveWhenReady;
			
			SaveWhenReady();
		}
		
		public void SaveWhenReady()
		{
			if (timer.IsEnabled)
			{
				timer.Stop();
				timer.Start();
			}
			else
			{
				timer.Start();
			}
		}
	}

	class State
	{
		public string Text { get; set; }
		public int Line { get; set; }
		public int Position { get; set; }

		public State(string text, int line, int pos)
		{
			Text = text;
			Line = line;
			Position = pos;
		}
	}
}