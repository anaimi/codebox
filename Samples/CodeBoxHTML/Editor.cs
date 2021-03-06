﻿using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Resources;
using System.Xml.Linq;
using CodeBox.Core;
using CodeBox.Core.Services;
using CodeBoxHTML.Services;
using CodeBoxHTML.Controls;
using System.Windows.Browser;
using CodeBox.Core.Elements;

namespace CodeBoxHTML
{
	[ScriptableTypeAttribute]
	public class Editor
	{
		#region instance

		private static Editor _instance;
		public static Editor Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new Editor();
				}

				return _instance;
			}
		}
		
		private Editor()
		{

		}
		#endregion
		
		public TooltipBox Tooltip { get; private set; }
		
		public void Initialize()
		{
			// create and add tooltip
			Tooltip = new TooltipBox();
			Controller.Instance.RootCanvas.Children.Add(Tooltip);
			
			// load HTML5 settings
			XDocument html5 = XDocument.Load(GetResourceStream("HTML5.xml"));
			
			// keywords
			Configuration.Keywords = (from element in html5.Root.Element("elements").Descendants()
			                          select element.Attribute("name").Value).ToArray();

			// validation
			Configuration.Validator.IsEnabled = true;
			Configuration.Validator.GetParsingExceptions = Validator.Validate;
			
			// highlighting
			Configuration.SyntaxHighlighter.IsEnabled = true;
			Configuration.SyntaxHighlighter.IdentifierColor = new SolidColorBrush(Color.FromArgb(255, 100, 149, 237));
			Configuration.SyntaxHighlighter.KeywordColor = new SolidColorBrush(Colors.Blue);
			Configuration.SyntaxHighlighter.SymbolColor = new SolidColorBrush(Colors.Gray);
			
			// auto completion
			Configuration.AutoCompleter.IsEnabled = true;
			Configuration.AutoCompleter.ObjectIcon = GetResourceStream("html-tag.png");
			Configuration.AutoCompleter.FunctionIcon = GetResourceStream("html-attribute.png");
			Configuration.AutoCompleter.KeyDown += AutoComplete.KeyDown;
			Configuration.AutoCompleter.OnHighlightedItemChanged = AutoComplete.OnHighlightedItemChanged;
			Configuration.AutoCompleter.OnShow = AutoComplete.OnShow;
			Configuration.AutoCompleter.OnHide = AutoComplete.OnHide;
			AutoComplete.Initialize(html5);			
			
			// initialize services
			Controller.Instance.InitializeServices();

			// do we have a backup? if yes, load it
			Recovery.Instance.LoadBackup();
		}
		
		public string GetText()
		{
			return Controller.Instance.Text;
		}
		
		public void SetText(string text)
		{
			Controller.Instance.Text = text;
		}
		
		#region copy, cut, paste, undo, redo, search
		[ScriptableMemberAttribute]
		public void Copy()
		{
			Controller.Instance.Copy(null);
		}

		[ScriptableMemberAttribute]		
		public void Cut()
		{
			Controller.Instance.Cut(null);
		}

		[ScriptableMemberAttribute]
		public void Paste()
		{
			Controller.Instance.Paste(null);
		}
		
		[ScriptableMemberAttribute]
		public void Undo()
		{
			UndoRedo.Instance.Undo();
		}

		[ScriptableMemberAttribute]
		public void Redo()
		{
			UndoRedo.Instance.Redo();
		}
		
		[ScriptableMemberAttribute]
		public void Search(string query)
		{
			Controller.Instance.Search(query);
		}
		#endregion
		
		private Stream GetResourceStream(string name)
		{
			StreamResourceInfo resource = Application.GetResourceStream(new Uri("/CodeBoxHTML;component/Resources/" + name, UriKind.Relative));
			return resource.Stream;
		}
	}
}
