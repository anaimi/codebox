using System.Windows;
using System.Windows.Controls;
using System.Windows.Browser;

namespace CodeBoxHTML
{
	public partial class MainPage : UserControl
	{
		public MainPage()
		{
			InitializeComponent();
			
			Loaded += PageLoaded;
		}

		private void PageLoaded(object sender, RoutedEventArgs e)
		{
			Editor.Instance.Initialize();
			
			HtmlPage.RegisterScriptableObject("Editor", Editor.Instance);
		}
	}
}
