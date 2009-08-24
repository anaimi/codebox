using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Linq;
using CodeBox.Core;
using CodeBox.CodeLexer;
using CodeBox.Core.Services.AutoComplete;

namespace CodeBoxHTML.Services
{
	public class AutoComplete
	{
		private enum AutoCompleteListState
		{
			Elements,
			Attributes
		}
		
		public static List<HTMLElement> HTMLElements { get; private set; }
		public static List<HTMLAttribute> GlobalHTMLAttributes { get; private set; }

		/// <summary>
		/// Equals "Elements" when list is populated with elements
		/// Eduals "Attributes" when list is populated with attributes
		/// Used inorder not to repopulate the list on every keystroke
		/// </summary>
		private static AutoCompleteListState _listState;
		
		public static void Initialize(XDocument html5)
		{
			// read current configuration from XML
			HTMLElements = (from element in html5.Root.Element("elements").Descendants()
							where element.Name.LocalName.Equals("element")
			                select CreateHTMLElement(element)).ToList();

			GlobalHTMLAttributes = (from attribute in html5.Root.Element("attributes").Descendants()
									where attribute.Name.LocalName.Equals("attribute")
									select CreateHTMLAttribute(attribute)).ToList();
			
			// populate list by default with elements
			PopulateHTMLElementList();
		}
		
		private static HTMLElement CreateHTMLElement(XElement node)
		{
			var element = new HTMLElement();

			element.Name = node.Attribute("name").Value;
			element.Description = node.Attribute("description").Value;

			foreach (var attribute in node.Descendants("attribute"))
			{
				element.Attributes.Add(CreateHTMLAttribute(attribute));
			}

			return element;
		}
		
		private static HTMLAttribute CreateHTMLAttribute(XElement node)
		{
			var attribute = new HTMLAttribute();

			attribute.Name = node.Attribute("name").Value;
			attribute.Value = node.Attribute("value").Value;
			attribute.Description = node.Attribute("description").Value;

			return attribute;
		}
		
		private static bool IsInsideIncompleteTagName(Token token, List<Token> tokens)
		{
			return	token != null &&
					(token.Type == TokenType.IDENTIFIER || token.Type == TokenType.KEYWORD) &&
					token.Previous(tokens).Value.Equals("<");
		}
		
		private static bool IsInsideIncompleteAttributeName(Token token, List<Token> tokens)
		{
			Token currentToken;			
			
			if (token == null) // this is true when the character before caret is white-space
				currentToken = Controller.Instance.TokenBeforeCaret;
			else
				currentToken = token;
				
			if (token == null)
				return false;
			
			if (token.Type != TokenType.IDENTIFIER)
				return false;
			
			while (currentToken.Type != TokenType.EOF)
			{
				if (currentToken.Value.Equals("<"))
					return true;
				
				if (currentToken.Value.Equals("/"))
					return false;

				if (currentToken.Value.Equals(">"))
					return false;

				currentToken = currentToken.Previous(tokens);
			}

			return false;
		}

		private static void PopulateHTMLElementList()
		{
			AutoCompleteService.Instance.ClearListboxItems();
			
			foreach (var element in HTMLElements)
			{
				var lItem = new AutoCompleteItem(element.Name, AutoCompleteItemType.Object);
				AutoCompleteService.Instance.AddItem(lItem);
			}

			_listState = AutoCompleteListState.Elements;			
		}

		private static void PopulateHTMLAttributeList(Token token, List<Token> tokens)
		{
			AutoCompleteService.Instance.ClearListboxItems();
			
			// add element-specific attributes
			if (IsInsideIncompleteAttributeName(token, tokens))
			{
				var element = GetElementWhileTypingAttribute(token, tokens);
				
				if (element != null)
					AddHTMLAttributesToList(element.Attributes);
			}
			
			// add global attributes
			AddHTMLAttributesToList(GlobalHTMLAttributes);

			_listState = AutoCompleteListState.Attributes;
		}
		
		private static void AddHTMLAttributesToList(List<HTMLAttribute> attributes)
		{
			foreach (var attribute in attributes)
			{
				var lItem = new AutoCompleteItem(attribute.Name, AutoCompleteItemType.Function);
				lItem.SurroundParamsWithParenthesis = false;

				AutoCompleteService.Instance.AddItem(lItem);
			}
		}
		
		private static HTMLElement GetElementWhileTypingAttribute(Token token, List<Token> tokens)
		{
			// this will return the HTMLElement when the user is typing the attributes of that element, i.e.
			//		<img class='rounded' alt='' />
			//								  ^ caret here
			// on the previous example, it will return the element "img"
			// if the caret was not inside the brackets, a null will be returned
			
			var currentToken = token.Previous(tokens);
			while (currentToken.Type != TokenType.EOF)
			{
				if (currentToken.Value.Equals("<") || currentToken.Value.Equals(">"))
					break;

				if (currentToken.Type == TokenType.KEYWORD && currentToken.Previous(tokens).Value.Equals("<"))
					break;

				currentToken = currentToken.Previous(tokens);
			}

			if (currentToken.Type == TokenType.KEYWORD)
			{
				return HTMLElements.SingleOrDefault(e => e.Name.Equals(currentToken.Value));
			}

			return null;
		}
		
		public static void KeyDown()
		{
			var tokens = Controller.Instance.TokenList.Tokens;
			
			// back-spaced to line 0 position 0
			if (Controller.Instance.Paper.CharachterBeforeCaret == null)
				return;
			
			// take the parent of the last charachter
			var token = Controller.Instance.Paper.CharachterBeforeCaret.ParentToken;
			
			// re-sort elements on "<incomplete-tag-name"
			if (IsInsideIncompleteTagName(token, tokens))
			{
				// populate list if not already populated
				if (_listState != AutoCompleteListState.Elements)
				{
					PopulateHTMLElementList();
				}
				
				AutoCompleteService.Instance.ShowList();				
				AutoCompleteService.Instance.SelectClosestItem();
			}
			
			// re-sort attributes on "<tag incomplete-attribute-name"
			else if (IsInsideIncompleteAttributeName(token, tokens))
			{
				// populate list if not already populated
				if (_listState != AutoCompleteListState.Attributes)
				{
					PopulateHTMLAttributeList(token, tokens);
				}

				AutoCompleteService.Instance.ShowList();				
				AutoCompleteService.Instance.SelectClosestItem();
			}
			
			// nothing of the above? hide
			else
			{
				AutoCompleteService.Instance.HideList();
			}
		}
		
		public static void OnHighlightedItemChanged()
		{
			var index = AutoCompleteService.Instance.SelectedListBoxIndex;
			
			if (index < 0)
			{
				OnHide();
				return;
			}
			
			string title = string.Empty;
			string body = string.Empty;
			string footer = string.Empty;
			
			if (_listState == AutoCompleteListState.Elements)
			{
				title = HTMLElements[index - 1].Name;
				body = HTMLElements[index - 1].Description;
			}
			else if (_listState == AutoCompleteListState.Attributes)
			{
				var currentAttributeName = AutoCompleteService.Instance.SelectedListBoxItem.Tag as string;
				var currentElement = GetElementWhileTypingAttribute(Controller.Instance.TokenBeforeCaret, Controller.Instance.TokenList.Tokens);
				
				if (currentElement != null && currentElement.Attributes.Any(a => a.Name.Equals(currentAttributeName)))
				{
					var att = currentElement.Attributes.Single(a => a.Name.Equals(currentAttributeName));

					title = att.Name;
					body = att.Description;
					footer = "Value: " + att.Value;
				}
				else
				{
					var att = GlobalHTMLAttributes.Single(a => a.Name.Equals(currentAttributeName));

					title = att.Name;
					body = att.Description;
					footer = "Value: " + att.Value;
				}
				
			}

			Editor.Instance.Tooltip.SetContent(title, body, footer);
		}
		
		public static void OnShow()
		{
			// position tooltip asynchronously - otherwise ActualWidth will always be zero
			AutoCompleteService.Instance.Dispatcher.BeginInvoke(() => {
				// get coordinates
				var left = (double)AutoCompleteService.Instance.GetValue(Canvas.LeftProperty) + AutoCompleteService.Instance.ActualWidth + 5;
				var top = (double)AutoCompleteService.Instance.GetValue(Canvas.TopProperty) + 2;
				
				// set coordinates
				Editor.Instance.Tooltip.SetValue(Canvas.LeftProperty, left);
				Editor.Instance.Tooltip.SetValue(Canvas.TopProperty, top);
				
				// guess?
				Editor.Instance.Tooltip.Show();
			});
		}
		
		public static void OnHide()
		{
			Editor.Instance.Tooltip.Hide();
		}
	}
	
	public class HTMLElement
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public List<HTMLAttribute> Attributes { get; set; }

		public HTMLElement()
		{
			Attributes = new List<HTMLAttribute>();
		}
	}
	
	public class HTMLAttribute
	{
		public string Name { get; set; }
		public string Value { get; set; }
		public string Description { get; set; }
	}
}
