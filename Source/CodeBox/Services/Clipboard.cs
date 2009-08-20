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
			board = text;
		}

		public string Paste()
		{
			return board;
		}
	}
}
