using System.Collections.Generic;
using CodeBox.Core;
using CodeBox.Core.Utilities;
using CodeBox.Core.Services;

namespace CodeBox.Core.Services
{
	public class SyntaxHighlighterService : IService
	{
		#region instance
		private SyntaxHighlighterService()
		{
			
		}

		private static SyntaxHighlighterService _instance;
		public static SyntaxHighlighterService Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new SyntaxHighlighterService();
				}

				return _instance;
			}
		}
		#endregion
		
		public void Initialize()
		{
			Controller.Instance.OnTextChanged += Highlight;
		}
		
		public void Highlight()
		{
			var tokens = Controller.Instance.TokenChars;

			if (tokens == null)
				return;

			foreach(var t in tokens.Values)
			{
				// apply type to all chars
				foreach (var c in t.Characters)
				{
					c.CharType = t.Token.Type;
				}
			}
		}
	}
}