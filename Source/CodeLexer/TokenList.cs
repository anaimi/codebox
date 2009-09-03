using System;
using System.Collections.Generic;

namespace CodeBox.CodeLexer
{
	public class TokenList
	{
		public List<Token> Tokens
		{
			get
			{
				return tokens;
			}
		}

		private List<Token> tokens = new List<Token>();
		private List<string> keywords;
		private int currentTokenIndex;
		
		public TokenList()
		{
			
		}
		
		/// <summary>
		/// Creates a new token list by cloning outerTokens with a specified range (range is exclusive, not inclusive)
		/// </summary>
		public TokenList(TokenList outerTokens, int fromToken, int toToken)
		{
			// do copy
			for (var i = fromToken; fromToken < toToken; fromToken++)
			{
				tokens.Add(outerTokens.tokens[fromToken]);
			}
		}

		public TokenList(string code, List<string> keywords)
		{
			this.keywords = keywords;
			GenerateList(code);
		}

		public Token Next()
		{
			// return next token, skip if token is comment, return EOF on end
			while (tokens.Count > currentTokenIndex)
			{
				if (tokens[currentTokenIndex].Type == TokenType.COMMENT)
				{
					currentTokenIndex++;
					continue;
				}

				return tokens[currentTokenIndex];
			}

			return new Token(TokenType.EOF, "EOF", null);
		}

		public Token Next(int ahead)
		{
			while (tokens.Count > currentTokenIndex + ahead && 0 <= currentTokenIndex + ahead)
			{
				if (tokens[currentTokenIndex + ahead].Type == TokenType.COMMENT)
				{
					currentTokenIndex++;
					continue;
				}

				return tokens[currentTokenIndex + ahead];
			}

			return new Token(TokenType.EOF, "EOF", null);
		}
		
		public Token Previous()
		{
			while(currentTokenIndex >= 0 && tokens.Count > 0)
			{
				if (tokens[currentTokenIndex].Type == TokenType.COMMENT)
				{
					currentTokenIndex--;
					continue;
				}

				return tokens[currentTokenIndex];
			}

			return new Token(TokenType.EOF, "EOF", null);
		}

		public void Backtrack()
		{
			currentTokenIndex--;
		}

		public int CurrentIndex()
		{
			return currentTokenIndex;
		}
		
		public void GotoToken(Token token)
		{
			currentTokenIndex = tokens.IndexOf(token);
			
			if (currentTokenIndex == -1)
				currentTokenIndex = 0;
		}

		public void Consume()
		{
			currentTokenIndex++;
		}

		public void ConsumeBlockBackward()
		{
			if (Previous().Value != "}")
				throw new Exception("CurrentIndex is not pointing to a block-tail.");

			int braces = 0;
			Token token;

			while (true)
			{
				if (!HasPrevious())
					break;

				token = Previous();
				Backtrack();

				if (token.Value == "}")
				{
					braces++;
				}
				else if (token.Value == "{")
				{
					braces--;
				}

				if (braces == 0)
					break;
			}
		}

		public void ConsumeBlockForward()
		{
			if (Next().Value != "{")
				throw new Exception("CurrentIndex is not pointing to a block-head.");

			int braces = 0;
			Token token;

			while (true)
			{
				if (!HasNext())
					break;

				token = Next();
				Consume();

				if (token.Value == "{")
				{
					braces++;
				}
				else if (token.Value == "}")
				{
					braces--;
				}

				if (braces == 0)
					break;
			}
		}

		public void Add(int index, TokenType type, string value)
		{
			tokens.Insert(index, new Token(type, value, null));
		}

		public bool HasNext()
		{
			return (tokens.Count > currentTokenIndex);
		}
		
		public bool HasPrevious()
		{
			return (currentTokenIndex > 0);
		}

		private void GenerateList(string code)
		{
			Position position = new Position(code);

			while (true)
			{
				// completed?
				if (position.CurrentIndex > code.Length - 1)
					break;

				// new indices
				List<Index> indices = new List<Index>();

				// parse comments
				ParseComments(code, position, true);

				// add tokens
				if (code.Length > position.CurrentIndex)
					switch (code[position.CurrentIndex])
					{
						case '+':
						case '-':
						case '*':
						case '/':
						case '\\':
						case '%':
						case '^':
						case '<':
						case '>':
						case '&':
						case '|':
						case '!':
						case '?':
						case '=':
							indices.Add(new Index(position.Line, position.CharPosition));
							tokens.Add(new Token(TokenType.OPERATOR, code[position.CurrentIndex].ToString(), indices));
							position.CurrentIndex++;
							break;

						case '(':
						case ')':
						case ';':
						case '{':
						case '}':
						case '[':
						case ']':
						case '\'':
						case '`':
						case '_':
						case ',':
						case '.':
						case ':':
						case '$':
						case '#':
						case '@':
						case '~':
							indices.Add(new Index(position.Line, position.CharPosition));
							tokens.Add(new Token(TokenType.SYMBOL, code[position.CurrentIndex].ToString(), indices));
							position.CurrentIndex++;
							break;

						case '0':
						case '1':
						case '2':
						case '3':
						case '4':
						case '5':
						case '6':
						case '7':
						case '8':
						case '9':
							string num = ParseNumricConstant(code, position, indices);
							tokens.Add(new Token(TokenType.NUMBER, num, indices));
							break;

						case '"':
							string str = ParseStringConstant(code, position, indices);
							tokens.Add(new Token(TokenType.STRING, str, indices));
							break;

						default:
							// must be identifier
							if (!Char.IsLetter(code[position.CurrentIndex]))
								throw new EngineException(EngineException.ExceptionType.BadInput, "bad/unexpected input of Value '" + code[position.CurrentIndex] + "'", new Token(TokenType.SYMBOL, code[position.CurrentIndex].ToString(), indices));

							string identifier = string.Empty;

							while ((code.Length > position.CurrentIndex) && (Char.IsLetterOrDigit(code[position.CurrentIndex])))
							{
								indices.Add(new Index(position.Line, position.CharPosition));
								identifier += code[position.CurrentIndex];
								position.CurrentIndex++;
							}

							if (IsKeyword(identifier))
								tokens.Add(new Token(TokenType.KEYWORD, identifier, indices));
							else
								tokens.Add(new Token(TokenType.IDENTIFIER, identifier, indices));

							break;
					}
			}
		}

		private void EatWhitespace(string code, Position position)
		{
			// eat whitespace
			while ((code.Length > position.CurrentIndex) && (Char.IsWhiteSpace(code[position.CurrentIndex])))
			{
				position.CurrentIndex++;
			}
		}

		private int ParseComments(string code, Position position, bool recurse)
		{
			List<Index> indices = new List<Index>();

			if ((code.Length > position.CurrentIndex + 1) && (code[position.CurrentIndex] == '/'))
			{
				if (code[position.CurrentIndex + 1] == '/')
				{
					while ((code.Length > position.CurrentIndex) && (code[position.CurrentIndex] != '\n'))
					{
						indices.Add(new Index(position.Line, position.CharPosition));
						position.CurrentIndex++;
					}

					position.CurrentIndex++;
				}
				else if (code[position.CurrentIndex + 1] == '*')
				{
					while (code.Length > position.CurrentIndex)
					{
						if (code.Length > position.CurrentIndex + 1 && code[position.CurrentIndex] == '*' && code[position.CurrentIndex + 1] == '/')
						{
							indices.Add(new Index(position.Line, position.CharPosition));
							indices.Add(new Index(position.Line, position.CharPosition + 1));
							position.CurrentIndex += 2;
							break;
						}
						else if (code.Length > position.CurrentIndex && code[position.CurrentIndex] != '\n')
						{
							indices.Add(new Index(position.Line, position.CharPosition));
						}

						position.CurrentIndex++;
					}
				}
			}

			EatWhitespace(code, position);

			// add token only if comments were found
			if (indices.Count > 0)
			{
				tokens.Add(new Token(TokenType.COMMENT, "", indices));
			}

			// check if next is another comment...
			if (recurse)
			{
				int oldPosition = position.CurrentIndex;
				if (ParseComments(code, position, false) != oldPosition)
					return ParseComments(code, position, true);
			}

			return position.CurrentIndex;
		}

		private string ParseNumricConstant(string code, Position position, List<Index> indices)
		{
			double mantissa = 0;
			double fraction = 0;

			// Look for the integral part
			while ((code.Length > position.CurrentIndex) && (Char.IsDigit(code[position.CurrentIndex])))
			{
				mantissa = (mantissa * 10.0) + (code[position.CurrentIndex] - '0');
				indices.Add(new Index(position.Line, position.CharPosition));
				position.CurrentIndex++;
			}

			// Now look for the fractional part
			if ((code.Length > position.CurrentIndex) && (code[position.CurrentIndex] == '.'))
			{
				indices.Add(new Index(position.Line, position.CharPosition));
				position.CurrentIndex++;
				double t = .1;

				while ((code.Length > position.CurrentIndex) && (Char.IsDigit(code[position.CurrentIndex])))
				{
					indices.Add(new Index(position.Line, position.CharPosition));
					fraction = fraction + (t * (code[position.CurrentIndex] - '0'));
					position.CurrentIndex++;
					t = t / 10.0;
				}
			}

			mantissa = (mantissa + fraction);

			return mantissa.ToString();
		}

		private string ParseStringConstant(string code, Position position, List<Index> indices)
		{
			string result = "";

			indices.Add(new Index(position.Line, position.CharPosition));
			position.CurrentIndex++;

			while ((code.Length > position.CurrentIndex) && (code[position.CurrentIndex] != '"'))
			{
				result += code[position.CurrentIndex];
				indices.Add(new Index(position.Line, position.CharPosition));
				position.CurrentIndex++;
			}

			if (code.Length > position.CurrentIndex) // record last (")
			{
				indices.Add(new Index(position.Line, position.CharPosition));
				position.CurrentIndex++;
			}

			return result;
		}

		private bool IsKeyword(string identifier)
		{
			foreach (string s in keywords)
				if (s == identifier)
					return true;

			return false;
		}

		private class Position
		{
			public Position(string code)
			{
				this.code = code;
			}

			private string code;
			private int _line, _pos;

			public int CurrentIndex;
			public int CharPosition
			{
				get
				{
					CalculatePosition();
					return _pos;
				}
			}
			public int Line
			{
				get
				{
					CalculatePosition();
					return _line;
				}
			}

			private void CalculatePosition()
			{
				_line = 1;
				_pos = 1;

				for (int i = 0; i < CurrentIndex; i++)
				{
					if (code[i] == '\n')
					{
						_line++;
						_pos = 1;
					}
					else
					{
						_pos++;
					}
				}
			}
		}
	}
}
