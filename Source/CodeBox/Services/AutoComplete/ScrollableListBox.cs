using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Media;

namespace CodeBox.Core.Services.AutoComplete
{
	/// <summary>
	/// The style of the current AutoComplete listbox is inconsistent with the style of the scroll viewer of the editor
	/// If the listbox style was changed to the one in Resources/DefaultTheme.xaml the method ScrollIntoView will not work
	/// (actually, it doesn't even work well without the stytle)
	/// 
	/// This class was created as an alternative solution - but it does an even worse job
	/// </summary>
	public class ScrollableListBox : ListBox
	{
		private ScrollViewer ScrollViewer;

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			var itemsHost = VisualTreeHelper.GetParent(element) as Panel;

			for (DependencyObject obj = itemsHost; obj != item && obj != null; obj = VisualTreeHelper.GetParent(obj))
			{
				ScrollViewer viewer = obj as ScrollViewer;
				if (viewer != null)
				{
					ScrollViewer = viewer;
					break;
				}
			}

			base.PrepareContainerForItemOverride(element, item);
		}
		
		public void ScrollIntoView(ListBoxItem item)
		{
			if (ScrollViewer == null)
				return;
			
			var index = Items.IndexOf(item);
			var pecentage = ((index + 1) / Items.Count);

			var peer = FrameworkElementAutomationPeer.FromElement(ScrollViewer);
			
			if (peer == null)
				peer = FrameworkElementAutomationPeer.CreatePeerForElement(ScrollViewer);
			
			var provider = peer.GetPattern(PatternInterface.Scroll) as IScrollProvider;

			if (provider == null)
				return;
			
			provider.SetScrollPercent(-1.0, pecentage * 100); // this thing is crap ... doesn't really set the scroll percentage
		}
	}
}
