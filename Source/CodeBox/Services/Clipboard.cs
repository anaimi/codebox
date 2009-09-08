using System.Linq;

namespace CodeBox.Core.Services
{
	public class Clipboard : IService
	{
		private string board = "";
				
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
		
		private Clipboard()
		{
			
		}
		
		public void Initialize()
		{
			
		}
		
		public void Copy(string text)
		{
			//// Because we do not have a newline charachter, we must check if the last virtual \n is selected.
			//// Meaning, if the current text is (equal-sign border not included):
			//// ===============================================================
			//// This is a line
			//// This is another line
			//// 
			//// ===============================================================
			//// Then the user selects the above three lines and attemps to copy.
			//// Paper.HighlightedBlocks will only include visible charachters. So the last line's \n will
			//// not be included. To fix this, we take the current line of the caret, and compare it to the lines of the HighlightedBlocks
			//// If no highlighted block shared the same line with the caret & the caret line is less than all of them, prepend \n
			//// If no highlighted block shared the same line with the caret & the caret line is grater than all of them, append \n
			//// Clunky? i know.

			//var currentLine = Controller.Instance.Paper.Line;
			//var minLine = Controller.Instance.Paper.HighlightedBlocks.Min(c => c.Line);
			//var maxLine = Controller.Instance.Paper.HighlightedBlocks.Max(c => c.Line);
			
			//if (currentLine < minLine) text = "\n" + text;
			//else if (currentLine > maxLine) text = "\n" + text;
			
			board = text;
		}

		public string Paste()
		{
			return board;
		}
	}
}
