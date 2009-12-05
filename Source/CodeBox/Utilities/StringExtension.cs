namespace CodeBox.Core.Utilities
{
	public static class StringExtension
	{
		public static bool IsNullOrEmptyOrWhitespace(this string str)
		{
			if (string.IsNullOrEmpty(str))
				return true;
			
			foreach(var c in str)
				if (!char.IsWhiteSpace(c))
					return false;

			return true;
		}
	}
}
