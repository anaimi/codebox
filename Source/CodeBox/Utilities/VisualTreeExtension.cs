using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

namespace CodeBox.Core.Utilities
{
	// Code stolen from http://miguelmadero.blogspot.com/2008/07/use-visualtreehelper-to-navigate_18.html
	// I added GetAllChildren() - ANaimi
	public static class VisualTreeExtensions
	{
		public static IEnumerable<DependencyObject> GetChildren(this DependencyObject depObject)
		{
			int count = depObject.GetChildrenCount();
			for (int i = 0; i < count; i++)
			{
				yield return VisualTreeHelper.GetChild(depObject, i);
			}
		}

		public static IEnumerable<FrameworkElement> GetAllChildren(this DependencyObject depObject)
		{
			int count = depObject.GetChildrenCount();
			for (int i = 0; i < count; i++)
			{
				var obj = VisualTreeHelper.GetChild(depObject, i);

				foreach (var inner in obj.GetAllChildren())
				{
					yield return inner;
				}

				if (obj is FrameworkElement)
					yield return obj as FrameworkElement;
			}
		}

		public static DependencyObject GetChild(this DependencyObject depObject, int childIndex)
		{
			return VisualTreeHelper.GetChild(depObject, childIndex);
		}

		public static DependencyObject GetChild(this DependencyObject depObject, string name)
		{
			return depObject.GetChild(name, false);
		}

		public static DependencyObject GetChild(this DependencyObject depObject, string name, bool recursive)
		{
			foreach (var child in depObject.GetChildren())
			{
				var element = child as FrameworkElement;
				if (element != null)
				{
					if (element.Name == name)                                           // If its a FrameworkElement check Name
						return element;
					var innerElement = element.FindName(name) as DependencyObject;      // Try to get it using FindByName might be more efficient
					if (innerElement != null)
						return innerElement;
				}
				if (recursive)                                                          // If it's recursive search through its children
				{
					var innerChild = child.GetChild(name, true);
					if (innerChild != null)
						return innerChild;
				}
			}
			return null;
		}

		public static int GetChildrenCount(this DependencyObject depObject)
		{
			return VisualTreeHelper.GetChildrenCount(depObject);
		}

		public static DependencyObject GetParent(this DependencyObject depObject)
		{
			return VisualTreeHelper.GetParent(depObject);
		}
	}
}
