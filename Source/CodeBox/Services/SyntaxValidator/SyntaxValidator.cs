using System.Collections.Generic;
using CodeBox.Core.Utilities;
using CodeBox.Core.Elements;

namespace CodeBox.Core.Services.SyntaxValidator
{
	public class ValidatorService : IService
	{
		#region instance
		private static ValidatorService _instance;
		public static ValidatorService Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new ValidatorService();
				}

				return _instance;
			}
		}
		#endregion
		
		private List<TokenChars> underlinedTokenChars;
		
		public void Initialize()
		{
			ErrorTooltip.SetMousePositionParent(Controller.Instance.RootCanvas);
			
			Controller.Instance.OnTextChanged += Validate;
		}
		
		private void Validate()
		{
			List<TokenChars> tokenChars = Controller.Instance.TokenChars;

			if (underlinedTokenChars == null)
				underlinedTokenChars = new List<TokenChars>();
			
			// remove previousely underlined charachters with their event handlers
			foreach (TokenChars tc in underlinedTokenChars)
			{
				tc.SetCharacterState(Character.CharState.Normal);

				foreach (Character character in tc.Characters)
				{
					character.MouseEnter -= ErrorTooltip.ShowTooltip;
					character.MouseLeave -= ErrorTooltip.HideTooltip;
				}
			}
			
			underlinedTokenChars.Clear();
			ErrorTooltip.ClearErrors();
			
			// underline errors & assign event handlers
			var exceptions = Configuration.Validator.GetParsingExceptions();
			foreach (var exception in exceptions)
			{
				if (exception.Token != null)
				{
					var token = tokenChars.GetTokenCharsByToken(exception.Token);
					
					if (token == null)
						continue;
					
					token.SetCharacterState(Character.CharState.UnderlinedRed);
					underlinedTokenChars.Add(token);
					ErrorTooltip.AddError(token.Token, exception.Message);

					foreach (Character character in token.Characters)
					{
						character.MouseEnter += ErrorTooltip.ShowTooltip;
						character.MouseLeave += ErrorTooltip.HideTooltip;
					}
				}
			}
		}
	}
}