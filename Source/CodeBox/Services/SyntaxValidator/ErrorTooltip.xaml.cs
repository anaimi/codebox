using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CodeBox.Core.Utilities;
using CodeBox.CodeLexer;
using CodeBox.Core.Elements;

namespace CodeBox.Core.Services.SyntaxValidator
{
	public partial class ErrorTooltip
	{
		private static ErrorTooltip instance;
		private static UIElement mousePositionParent;
		private static Dictionary<Token, string> errors;
		
		public ErrorTooltip()
		{
			InitializeComponent();

			if (instance != null)
				throw new Exception("more than once instance of ErrorTooltip... i thought we agreed on one?");

			instance = this;
			errors = new Dictionary<Token, string>();
		}
		
		public void SetError(string message)
		{
			tbError.Text = message;
		}
		
		public static void AddError(Token token, string message)
		{
			if (errors.ContainsKey(token))
				return;
			
			errors.Add(token, message);
		}
		
		public static void ClearErrors()
		{
			errors.Clear();
		}
		
		public static void SetMousePositionParent(UIElement uie)
		{
			if (mousePositionParent != null)
				throw new Exception("this should be null... why are you setting this twice?");
			
			mousePositionParent = uie;
		}
		
		public static void ShowTooltip(object sender, MouseEventArgs e)
		{
			// arrange
			Character currentChar = ((Character)sender);
			Token token = currentChar.ParentToken;
			TokenChars tc = Controller.Instance.TokenChars.GetTokenCharsByToken(token);
			
			// position
			PaperLine line = (PaperLine)tc.FirstCharacter.Parent;
			double left = Controller.Instance.NumberPanel.ActualWidth + line.PositionFromLeftInPixels(tc.FirstCharacter);
			instance.SetValue(Canvas.LeftProperty, left);
			instance.SetValue(Canvas.TopProperty, Character.CHAR_HEIGHT * (tc.FirstCharacter.Line + 1) + 2);
			
			// set Value & show
			if (errors.ContainsKey(token))
			{
				instance.SetError(errors[token]);
				instance.Visibility = Visibility.Visible;
			}
		}
		
		public static void HideTooltip(object sender, MouseEventArgs e)
		{
			instance.Visibility = Visibility.Collapsed;
		}
	}
}