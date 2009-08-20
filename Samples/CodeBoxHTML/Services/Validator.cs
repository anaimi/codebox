using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeBox.CodeLexer;
using CodeBox.Core;

namespace CodeBoxHTML.Services
{
	// first thing executed after tokenization (lex-ing) is validation
	public class Validator
	{
		public static List<EngineException> Validate()
		{
			var tokens = Controller.Instance.TokenList.Tokens;
			
			// keywords are html tags, such as: div, b, h1, h2, body, etc
			// the problem is that sometimes in the content or attribute of a tag there is a keyword, example:
			//		<b>ANaimi has a very hot body!</b>
			// the last html code will include three keywords: first <b>, second </b>, and body
			// this is also true for other operators and all type of tokens
			// so, we'll fix it by setting tags' content as symbols (hacky.. i know, but CodeBox was built with a C-like syntax in mind not HTML)
			// TODO: make the tokenizer (TokenList) more extensible, perhaps provide an XML tokenizer?
			SetTagContentAsSymbols(tokens);
			
			// unset keywords that do not occur after a "<" or "</"
			var fakeKeywords = from token in tokens
							   let prev = token.Previous(tokens)
							   where token.Type == TokenType.KEYWORD && !prev.Value.Equals("<") &&
									 (!prev.Value.Equals("/") || !prev.Previous(tokens).Value.Equals("<"))
							   select token;
			foreach(var keyword in fakeKeywords) keyword.Type = TokenType.IDENTIFIER;

			// mark <!-- ... --> as comments
			MarkCommentTagsAsComments(tokens);

			//  ------------------ start basic validation:
			var errors = new List<EngineException>();
						
			// ensure each tag is a proper HTML tag
			// proper HTML element tags were added in Configuration.Keywords, so if there is a non-keyword token
			// with a "<" before it, it is an unknown tag.
			var unknownTags = from token in tokens
			                  let next = token.Next(tokens) 
			                  where
			                  	token.Type == TokenType.OPERATOR && token.Value.Equals("<") &&
			                  	next.Type != TokenType.KEYWORD && !next.Value.Equals("/")
			                  select token;

			foreach (var token in unknownTags)
			{
				var tagName = token.Next(tokens);
				errors.Add(new EngineException(EngineException.ExceptionType.Syntax, "unknown HTML tag", tagName.Type == TokenType.EOF ? token : tagName));
			}
			
			// ensure that each tag is closed, otherwise add an exception to that tag - this is easiest using a stack
			var stack = new Stack<Token>();
			foreach (var token in tokens)
			{
				if (token.Type != TokenType.KEYWORD)
					continue;
				
				if (token.Previous(tokens).Value.Equals("<"))
				{
					stack.Push(token);
					
					#region check for inline closing tag
					var currentToken = token.Next(tokens);
					while (currentToken.Type != TokenType.EOF && !currentToken.Value.Equals("<") && !currentToken.Value.Equals(">"))
					{
						if (currentToken.Value.Equals("/") && currentToken.Next(tokens).Value.Equals(">"))
						{
							stack.Pop();
							break;
						}
						
						currentToken = currentToken.Next(tokens);
					}

					#endregion
				}
				else
				{
					if (stack.Peek().Value != token.Value)
					{
						errors.Add(new EngineException(EngineException.ExceptionType.Syntax, "opening tag not found", token));
					}
					else
					{
						stack.Pop();
					}
				}
			}
			
			// all tags left in the stack have no closing tags
			foreach (var token in stack)
			{
				errors.Add(new EngineException(EngineException.ExceptionType.Syntax, "closing tag not found", token));
			}

			return errors;
		}

		private static void MarkCommentTagsAsComments(List<Token> tokens)
		{
			var insideComment = false;
			foreach (var token in tokens)
			{
				if (token.Value.Equals("<") && token.Index(tokens) <= tokens.Count - 4 && token.Next(tokens).Value.Equals("!"))
				{
					var text = GetText(tokens.GetRange(token.Index(tokens), 4));
					
					if (text == "<!--")
						insideComment = true;
				}
				else if (token.Value.Equals(">") && token.Index(tokens) >= 3 && token.Previous(tokens).Value.Equals("-"))
				{
					var text = GetText(tokens.GetRange(token.Index(tokens) - 2, 3));

					if (text == "-->")
					{
						token.Type = TokenType.COMMENT;
						insideComment = false;
					}
				}
				
				if (insideComment)
					token.Type = TokenType.COMMENT;
			}
		}

		private static void SetTagContentAsSymbols(List<Token> tokens)
		{
			var insideContent = true;
			foreach (var token in tokens)
			{
				if (token.Type == TokenType.OPERATOR && token.Value.Equals("<"))
				{
					insideContent = false;
					continue;
				}
				
				if (token.Type == TokenType.OPERATOR && token.Value.Equals(">"))
				{
					insideContent = true;
					continue;
				}
				
				if (insideContent)
					token.Type = TokenType.SYMBOL;
			}
		}

		private static string GetText(List<Token> tokens)
		{
			var str = new StringBuilder();

			foreach (var token in tokens)
			{
				str.Append(token.Value);
			}

			return str.ToString();
		}
	}
}
