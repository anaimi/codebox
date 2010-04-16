using System.Collections.Generic;
using System.Text;
using System;

namespace CodeBox.CodeLexer
{
	public class Token : IComparable
	{
		public TokenType Type { get; set; }
		public string Value { get; set; }
		public List<Index> Indices { get; set; }
		public int Line
		{
			get
			{
				if (Indices == null)
					return 0;

				return Indices[0].Line;
			}
		}
		public int Position
		{
			get
			{
				if (Indices == null)
					return 0;

				return Indices[0].Position;
			}
		}

		public Token(TokenType type, string value, List<Index> indices)
		{
			Type = type;
			Value = value;
			Indices = indices;
		}

		public Token Next(List<Token> list)
		{
			int index = Index(list);

			if (index >= list.Count - 1)
				return new Token(TokenType.EOF, "", new List<Index>());

			return list[index + 1];
		}

		public Token Previous(List<Token> list)
		{
			int index = Index(list);

			if (index <= 0)
				return new Token(TokenType.EOF, "", new List<Index>());

			return list[index - 1];
		}

		public bool FallsBefore(List<Token> list, Token target)
		{
			int selfIndex = Index(list);
			int targetIndex = list.IndexOf(target);

			if (selfIndex < targetIndex)
				return true;

			return false;
		}

		public bool FallsAfter(List<Token> list, Token target)
		{
			int selfIndex = Index(list);
			int targetIndex = list.IndexOf(target);

			if (selfIndex > targetIndex)
				return true;

			return false;
		}

		public int Index(List<Token> list)
		{
			return list.IndexOf(this);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			Token other = obj as Token;

			if (other.Value == Value && other.Line == Line && other.Position == Position)
				return true;

			return false;
		}

		public override int GetHashCode()
		{
			return Line * 1000 + Position;
		}

		public int CompareTo(object obj)
		{
			// mainly used in sorting TokenList.Tokens (see Controller.TextChanged())
			// will assume obj is always of type Token (as it should be)
			var other = obj as Token;
			
			if (Line > other.Line) return 1;
			if (Line < other.Line) return -1;
			
			if (Position > other.Position) return 1;
			if (Position < other.Position) return -1;

			return 0;
		}

		public override string ToString()
		{
			return string.Format("{0} [{1}, {2}]", Value, Line, Position);
		}
		
		public void UpdateLineNumber(int offset)
		{
			foreach(var index in Indices)
				index.UpdateLineNumber(offset);
		}
	}

	public class Index
	{
		public int Line { get; private set; }
		public int Position { get; private set; }

		public Index(int line, int pos)
		{
			Line = line;
			Position = pos;
		}
		
		public void UpdateLineNumber(int offset)
		{
			Line += offset;
		}

		public override string ToString()
		{
			return string.Format("[{0}, {1}]", Line, Position);
		}
	}
}
