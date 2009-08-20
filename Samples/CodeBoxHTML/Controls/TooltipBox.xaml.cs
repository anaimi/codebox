using System.Windows;
using System.Windows.Controls;

namespace CodeBoxHTML.Controls
{
	public partial class TooltipBox
	{
		public TooltipBox()
		{
			InitializeComponent();

			Visibility = Visibility.Collapsed;
		}
		
		public void SetContent(string title, string body, string footer)
		{
			this.title.Text = title;
			this.body.Text = body;
			
			if (string.IsNullOrEmpty(footer))
				this.footer.Visibility = Visibility.Collapsed;
			else
			{
				this.footer.Text = footer;
				this.footer.Visibility = Visibility.Visible;
			}
		}
		
		public void Show()
		{
			Visibility = Visibility.Visible;
		}
		
		public void Hide()
		{
			Visibility = Visibility.Collapsed;
		}
	}
}
