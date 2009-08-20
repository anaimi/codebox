using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Algorithmatic.Silverlight.RTE.Utilities;

namespace Algorithmatic.Silverlight.RTE.Views
{
	public partial class Editor : UserControl
	{
		private DispatcherTimer textBoxTimer;
		private Controller controller;
		
		
		
		public Editor()
		{
			InitializeComponent();

			// textBoxTimer
			textBoxTimer = new DispatcherTimer();
			textBoxTimer.Interval = TimeSpan.FromMilliseconds(50);
			textBoxTimer.Tick += textboxReaderTick;
			textBoxTimer.Start();
			
			// controller and dependencies
			controller = Controller.SetInstance(Paper, numberPanel, textBox, textBoxTimer);
			//DebuggerTooltip.SetMousePositionParent(CanvasRoot);
			//ErrorTooltip.SetMousePositionParent(CanvasRoot);
			//AutoComplete.SetMousePositionParent(CanvasRoot);

			Loaded += PageLoaded;
		}
		



		public void PageLoaded(object o, EventArgs e)
		{
			// set focus
			Paper.MouseLeftButtonDown += delegate { textBox.Focus(); };
			textBox.LostFocus += textbox_LostFocus;
			textBox.GotFocus += delegate { Paper.ShowCaret = true; System.Diagnostics.Debug.WriteLine("Got focus"); };
			textBox.Focus();
			textBox.KeyDown += controller.KeyDown;
			textBox.KeyUp += controller.KeyUp;

			// reset Paper
			Paper.Children.Clear();
			Paper.Children.Add(new Line());
		}

		private void textboxReaderTick(object sender, EventArgs e)
		{
			textBox.Text = textBox.Text.Replace("\r", "");

			if (textBox.Text == "" || textBox.Text == "\n")
				return;

			if (controller.IsCtrlDown)
			{
				controller.AddText(textBox.Text);
			}
			else
			{
				controller.AddChar(textBox.Text[0]);
			}

			textBox.Text = "";
		}
		
		public void ExecutionBegin()
		{
			//controller.DebugBegin();
		}

		public void ExecutionComplete(bool success)
		{
			//controller.DebugComplete(success);
		}

		private void textbox_LostFocus(object sender, RoutedEventArgs e)
		{
			Paper.ShowCaret = false;

			if (controller.KeepFocus)
			{
				textBox.Focus();
				controller.KeepFocus = false;
			}

			System.Diagnostics.Debug.WriteLine("Lost focus");
		}

		public void SetKeywords(List<string> keywords)
		{
			controller.SetKeywords(keywords);
		}

		public void UpdateCaretPosition(int line, int position)
		{
			controller.Paper.UpdateCaret(line, position);
		}

		public void Cut()
		{
			if (!Controller.Instance.Paper.HaveHighlightedText)
				return;

			Clipboard.Copy(Controller.Instance.Paper.GetHighlightedText());
			Controller.Instance.Paper.RemoveHighlights();
		}

		public void Copy()
		{
			if (!Controller.Instance.Paper.HaveHighlightedText)
				return;

			Clipboard.Copy(Controller.Instance.Paper.GetHighlightedText());
		}

		public void Paste()
		{
			Controller.Instance.AddText(Clipboard.Paste());
		}

		public void SelectAll()
		{
			controller.Paper.HighlightAll();
		}
		
		public void Search(string query)
		{
			controller.Search(query);
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
