using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace CodeBox.Core.Services
{
	public class Clipboard : IService
	{
		//private string board = "";
				
		#region instance
		private static Clipboard _instance;
		public static Clipboard Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new Clipboard();
				}

				return _instance;
			}
		}
		#endregion

		private ClipboardHelper board;
		
		private Clipboard()
		{
			board = new ClipboardHelper();
		}
		
		public void Initialize()
		{
			
		}

		public void Copy(string text, KeyEventArgs e)
		{
			board.Text = text;
			board.SelectAll();
			board.ProcessKeyDown(e);
			//board = text;
		}

		public string Paste(KeyEventArgs e)
		{
			board.Text = "";
			board.ProcessKeyDown(e);
			return board.Text;
		}
		
		private class ClipboardHelper : TextBox
		{
			internal ClipboardHelper()
			{
				AcceptsReturn = true;
			}

			internal void ProcessKeyDown(KeyEventArgs e)
			{
				OnKeyDown(e);
			}
		}
		
		private class ClipboardEventArgs : EventArgs
		{
			
		}
	}
}
