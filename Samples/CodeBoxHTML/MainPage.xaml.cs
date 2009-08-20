using System.Windows;
using System.Windows.Controls;

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
		}
	}
}
