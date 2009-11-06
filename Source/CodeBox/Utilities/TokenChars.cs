using System.Collections.Generic;
using CodeBox.CodeLexer;
using CodeBox.Core.Elements;

namespace CodeBox.Core.Utilities
{
	/// <summary>
	/// This class is used as a link between Tokens and Elements.Charachters
	/// </summary>
	public class TokenChars
	{
		public Token Token { get; private set; }
		public List<Character> Characters { get; private set; }

		public TokenChars(Token token)
		{
			Token = token;
			Characters = GetCharsByIndices(token.Indices);

			foreach (Character c in Characters)
			{
				c.ParentToken = token;
			}
		}

		public override string ToString()
		{
			string str = "";

			foreach (var c in Characters)
			{
				str += c.Char.ToString();
			}

			return str;
		}

		public void SetCharacterState(Character.CharState state)
		{
			foreach (var c in Characters)
				c.SetState(state);
		}

		public Character FirstCharacter
		{
			get
			{
				return Characters[0];
			}
		}
		
		public Character LastCharacter
		{
			get
			{
				return Characters[Characters.Count - 1];
			}
		}

		private List<Character> GetCharsByIndices(List<Index> indices)
		{
			Paper paper = Controller.Instance.Paper;

			// Index line and position are off by one...
			List<Character> chars = new List<Character>();
			paper.RemoveCaret();

			foreach (Index index in indices)
			{
				PaperLine pLine = paper.LineAt(index.Line - 1);

				if (pLine.Children.Count > 0)
					chars.Add((Character)pLine.At(index.Position - 1));
			}

			paper.RestoreCaret();

			return chars;
		}
	}
}
