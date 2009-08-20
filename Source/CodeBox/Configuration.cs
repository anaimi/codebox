using System;
using System.Collections.Generic;
using System.Windows.Media;
using CodeBox.CodeLexer;
using System.Windows.Browser;
using System.Windows;
using CodeBox.Core.Services.AutoComplete;
using System.IO;

namespace CodeBox.Core
{
	public static class Configuration
	{
		public static List<string> Keywords = new List<string>();

		private static Debugger _debugger;
		public static Debugger Debugger
		{
			get
			{
				if (_debugger == null)
				{
					_debugger = new Debugger();
					_debugger.IsEnabled = false;
				}

				return _debugger;
			}
		}
		
		private static SyntaxValidator _validator;
		public static SyntaxValidator Validator
		{
			get
			{
				if (_validator == null)
				{
					_validator = new SyntaxValidator();
					_validator.IsEnabled = false;
				}

				return _validator;
			}
		}

		private static SyntaxHighlighter _highlighter;
		public static SyntaxHighlighter SyntaxHighlighter
		{
			get
			{
				if (_highlighter == null)
				{
					_highlighter = new SyntaxHighlighter();
					_highlighter.IsEnabled = false;
					
					_highlighter.IdentifierColor = new SolidColorBrush(Colors.Black);
					_highlighter.StringColor = new SolidColorBrush(Colors.Red);
					_highlighter.NumberColor = new SolidColorBrush(Colors.Black);
					_highlighter.KeywordColor = new SolidColorBrush(Colors.Blue);
					_highlighter.OperatorColor = new SolidColorBrush(Colors.DarkGray);
					_highlighter.SymbolColor = new SolidColorBrush(Colors.DarkGray);
					_highlighter.CommentColor = new SolidColorBrush(Colors.Green);
				}

				return _highlighter;
			}
		}

		private static AutoCompleter _autoCompleter;
		public static AutoCompleter AutoCompleter
		{
			get
			{
				if (_autoCompleter == null)
				{
					_autoCompleter = new AutoCompleter();
					_autoCompleter.IsEnabled = false;
				}

				return _autoCompleter;
			}
		}
		
		public static Color HoverColor = Color.FromArgb(255, 230, 230, 230);
	}
	
	public class Debugger
	{
		public bool IsEnabled { get; set; }
		public Action<int> OnLineNumberClick { get; set; }
	}

	public class SyntaxValidator
	{
		public bool IsEnabled { get; set; }
		public Func<List<EngineException>> GetParsingExceptions { get; set; }
	}

	public class SyntaxHighlighter
	{
		public bool IsEnabled { get; set; }
		
		public Brush IdentifierColor { get; set; }
		public Brush KeywordColor { get; set; }
		public Brush SymbolColor { get; set; }
		public Brush OperatorColor { get; set; }
		public Brush CommentColor { get; set; }
		public Brush NumberColor { get; set; }
		public Brush StringColor { get; set; }
	}
	
	public class AutoCompleter
	{
		public bool IsEnabled { get; set; }
		public Action KeyDown { get; set; }
		public Action OnShow { get; set; }
		public Action OnHide { get; set; }
		public Action OnHighlightedItemChanged { get; set; }
		
		public Stream ObjectIcon { get; set; }
		public Stream FunctionIcon { get; set; }
	}
}
