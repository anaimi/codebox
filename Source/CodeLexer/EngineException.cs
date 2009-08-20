using System;

namespace CodeBox.CodeLexer
{
	public class EngineException : Exception
	{
		#region ExceptionType enum

		public enum ExceptionType
		{
			Syntax,
			Runtime,
			UnknownSymbol,
			Cast,
			Return,
			Break,
			Exit,
			BadInput,
			Default,
			Continue
		}

		#endregion

		public static Func<int> GetCurrentLine;

		public EngineException(ExceptionType type, string message)
		{
			Type = type;
			Message = message;
			Token = new Token(TokenType.IDENTIFIER, "", null);
			Line = (GetCurrentLine == null) ? 0 : GetCurrentLine();
			Position = 0;
		}

		public EngineException(ExceptionType type, string message, Token token)
		{
			Type = type;
			Message = message;
			Token = token;
			Line = token.Line;
			Position = token.Position;
		}

		public ExceptionType Type { get; set; }
		public Token Token { get; set; }
		public new string Message { get; set; }
		public int Line { get; set; }
		public int Position { get; set; }

		public static void Expect(TokenList tl, TokenType type, string value)
		{
			Token t = tl.Next();

			if ((t.Type == type) && (t.Value == value))
			{
				tl.Consume();
			}
			else
			{
				string expectedType = TokenTypeToString(type);
				string expectedValue = value;
				string foundType = TokenTypeToString(t.Type);
				string foundValue = t.Value;
				ExceptionType exceptionType = (type == TokenType.KEYWORD)
												? ExceptionType.UnknownSymbol
												: ExceptionType.Syntax;

				throw new EngineException(exceptionType,
										  string.Format("Expected '{0}' with Value '{1}' found '{2}' with Value '{3}'", expectedType,
														expectedValue, foundType, foundValue), t);
			}
		}

		public static string Expect(TokenList tl, TokenType type, bool consume)
		{
			Token t = tl.Next();

			if (t.Type == type)
			{
				if (consume)
					tl.Consume();

				return t.Value;
			}

			string expectedType = TokenTypeToString(type);
			string foundType = TokenTypeToString(t.Type);

			throw new EngineException(ExceptionType.Syntax, string.Format("Expected '{0}' found '{1}' with Value '{2}'", expectedType, foundType, tl.Next().Value), t);
		}

		public static string Expect(TokenList tl, TokenType type)
		{
			return Expect(tl, type, true);
		}

		private static string TokenTypeToString(TokenType type)
		{
			switch (type)
			{
				case TokenType.KEYWORD:
					return "KEYWORD";
				case TokenType.OPERATOR:
					return "OPERATOR";
				case TokenType.SYMBOL:
					return "SYMBOL";
				case TokenType.IDENTIFIER:
					return "IDENTIFIER";
				case TokenType.NUMBER:
					return "NUMBER";
				case TokenType.STRING:
					return "STRING";
				case TokenType.EOF:
					return "EOF";
			}

			throw new Exception("unknown type!");
		}
	}
}
