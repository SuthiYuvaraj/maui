#nullable disable
using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class ReorderableItemsViewDelegator<TItemsView, TViewController> : GroupableItemsViewDelegator<TItemsView, TViewController>
		where TItemsView : ReorderableItemsView
		where TViewController : ReorderableItemsViewController<TItemsView>
	{
		public ReorderableItemsViewDelegator(ItemsViewLayout itemsViewLayout, TViewController itemsViewController)
			: base(itemsViewLayout, itemsViewController)
		{
		}

		public override NSIndexPath GetTargetIndexPathForMove(UICollectionView collectionView, NSIndexPath originalIndexPath, NSIndexPath proposedIndexPath)
		{
			NSIndexPath targetIndexPath;

			var itemsView = ViewController?.ItemsView;
			if (itemsView?.IsGrouped == true)
			{
				if (originalIndexPath.Section == proposedIndexPath.Section || itemsView.CanMixGroups)
				{
					targetIndexPath = proposedIndexPath;
				}
				else
				{
					targetIndexPath = originalIndexPath;
				}

				// When CanMixGroups is true and the proposed path equals the original (no valid target under drag point),
				// try to redirect the drop to a nearby empty group.
				if (originalIndexPath.Equals(proposedIndexPath) && itemsView.CanMixGroups)
				{
					var itemsSource = ViewController?.ItemsSource;
					var emptyGroupTarget = FindNearestEmptyGroup(itemsSource, originalIndexPath);
					if (emptyGroupTarget != null)
					{
						targetIndexPath = emptyGroupTarget;
					}
				}
			}
			else
			{
				targetIndexPath = proposedIndexPath;
			}

			return targetIndexPath;
		}

		static NSIndexPath FindNearestEmptyGroup(IItemsViewSource itemsSource, NSIndexPath currentIndexPath)
		{
			if (itemsSource == null)
				return null;

			var currentSection = (int)currentIndexPath.Section;
			var groupCount = itemsSource.GroupCount;

			for (int distance = 1; distance < groupCount; distance++)
			{
				var sectionAbove = currentSection - distance;
				if (sectionAbove >= 0 && itemsSource.ItemCountInGroup(sectionAbove) == 0)
					return NSIndexPath.Create(sectionAbove, 0);

				var sectionBelow = currentSection + distance;
				if (sectionBelow < groupCount && itemsSource.ItemCountInGroup(sectionBelow) == 0)
					return NSIndexPath.Create(sectionBelow, 0);
			}

			return null;
		}
	}
}
