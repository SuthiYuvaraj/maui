using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Handlers.Items;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2;

internal static class LayoutFactory2
{
	public static UICollectionViewLayout CreateList(LinearItemsLayout linearItemsLayout,
		LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo, ItemSizingStrategy itemSizingStrategy = ItemSizingStrategy.MeasureAllItems)
		=> linearItemsLayout.Orientation == ItemsLayoutOrientation.Vertical
			? CreateVerticalList(linearItemsLayout, groupingInfo, headerFooterInfo, itemSizingStrategy)
			: CreateHorizontalList(linearItemsLayout, groupingInfo, headerFooterInfo, itemSizingStrategy);

	public static UICollectionViewLayout CreateGrid(GridItemsLayout gridItemsLayout,
	 LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo, ItemSizingStrategy itemSizingStrategy = ItemSizingStrategy.MeasureAllItems)
		=> gridItemsLayout.Orientation == ItemsLayoutOrientation.Vertical
			? CreateVerticalGrid(gridItemsLayout, groupingInfo, headerFooterInfo, itemSizingStrategy)
			: CreateHorizontalGrid(gridItemsLayout, groupingInfo, headerFooterInfo, itemSizingStrategy);

	static NSCollectionLayoutBoundarySupplementaryItem[] CreateSupplementaryItems(LayoutGroupingInfo? groupingInfo, LayoutHeaderFooterInfo? layoutHeaderFooterInfo,
		UICollectionViewScrollDirection scrollDirection, NSCollectionLayoutDimension width, NSCollectionLayoutDimension height)
	{
		if (groupingInfo is not null && groupingInfo.IsGrouped)
		{
			var items = new List<NSCollectionLayoutBoundarySupplementaryItem>();

			if (groupingInfo.HasHeader)
			{
				items.Add(NSCollectionLayoutBoundarySupplementaryItem.Create(
					NSCollectionLayoutSize.Create(width, height),
					UICollectionElementKindSectionKey.Header.ToString(),
					scrollDirection == UICollectionViewScrollDirection.Vertical
						? NSRectAlignment.Top
						: NSRectAlignment.Leading));
			}

			if (groupingInfo.HasFooter)
			{
				items.Add(NSCollectionLayoutBoundarySupplementaryItem.Create(
					NSCollectionLayoutSize.Create(width, height),
					UICollectionElementKindSectionKey.Footer.ToString(),
					scrollDirection == UICollectionViewScrollDirection.Vertical
						? NSRectAlignment.Bottom
						: NSRectAlignment.Trailing));
			}

			return items.ToArray();
		}

		if (layoutHeaderFooterInfo is not null)
		{
			var items = new List<NSCollectionLayoutBoundarySupplementaryItem>();

			if (layoutHeaderFooterInfo.HasHeader)
			{
				items.Add(NSCollectionLayoutBoundarySupplementaryItem.Create(
					NSCollectionLayoutSize.Create(width, height),
					UICollectionElementKindSectionKey.Header.ToString(),
					scrollDirection == UICollectionViewScrollDirection.Vertical
						? NSRectAlignment.Top
						: NSRectAlignment.Leading));
			}
			;

			if (layoutHeaderFooterInfo.HasFooter)
			{
				items.Add(NSCollectionLayoutBoundarySupplementaryItem.Create(
					NSCollectionLayoutSize.Create(width, height),
					UICollectionElementKindSectionKey.Footer.ToString(),
					scrollDirection == UICollectionViewScrollDirection.Vertical
						? NSRectAlignment.Bottom
						: NSRectAlignment.Trailing));
			}

			return items.ToArray();
		}

		return [];
	}

	static UICollectionViewLayout CreateListLayout(UICollectionViewScrollDirection scrollDirection, LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo layoutHeaderFooterInfo, LayoutSnapInfo snapInfo, NSCollectionLayoutDimension itemWidth, NSCollectionLayoutDimension itemHeight, NSCollectionLayoutDimension groupWidth, NSCollectionLayoutDimension groupHeight, double itemSpacing, Func<Thickness>? peekAreaInsetsFunc, ItemSizingStrategy itemSizingStrategy = ItemSizingStrategy.MeasureAllItems)
	{
		var layoutConfiguration = new UICollectionViewCompositionalLayoutConfiguration();
		layoutConfiguration.ScrollDirection = scrollDirection;

		//create global header and footer
		layoutConfiguration.BoundarySupplementaryItems = CreateSupplementaryItems(null, layoutHeaderFooterInfo, scrollDirection, groupWidth, groupHeight);

		// For MeasureFirstItem, create a custom layout that measures the first item and applies that size to all items
		if (itemSizingStrategy == ItemSizingStrategy.MeasureFirstItem)
		{
			return new MeasureFirstItemCompositionalLayout(snapInfo, scrollDirection, itemSpacing, groupingInfo, layoutHeaderFooterInfo, itemWidth, itemHeight, groupWidth, groupHeight, peekAreaInsetsFunc, layoutConfiguration);
		}

		var layout = new CustomUICollectionViewCompositionalLayout(snapInfo, (sectionIndex, environment) =>
		{
			// Each item has a size
			var itemSize = NSCollectionLayoutSize.Create(itemWidth, itemHeight);
			// Create the item itself from the size
			var item = NSCollectionLayoutItem.Create(layoutSize: itemSize);

			if (peekAreaInsetsFunc?.Invoke() is Thickness peekAreaInsets)
			{
				if (scrollDirection == UICollectionViewScrollDirection.Vertical)
				{
					var newGroupHeight = environment.Container.ContentSize.Height - peekAreaInsets.Top - peekAreaInsets.Bottom;
					groupHeight = NSCollectionLayoutDimension.CreateAbsolute((nfloat)newGroupHeight);
				}
				else
				{
					var newGroupWidth = environment.Container.ContentSize.Width - peekAreaInsets.Left - peekAreaInsets.Right;
					groupWidth = NSCollectionLayoutDimension.CreateAbsolute((nfloat)newGroupWidth);
				}
			}
			// Each group of items (for grouped collections) has a size
			var groupSize = NSCollectionLayoutSize.Create(groupWidth, groupHeight);

			// Create the group
			// If vertical list, we want the group to layout horizontally (eg: grid columns go left to right)
			// for horizontal list, we want to lay grid rows out vertically
			// For simple lists it doesn't matter so much since the items span the entire width or height
			var group = scrollDirection == UICollectionViewScrollDirection.Vertical
				? NSCollectionLayoutGroup.CreateHorizontal(groupSize, item, 1)
				: NSCollectionLayoutGroup.CreateVertical(groupSize, item, 1);

			//group.InterItemSpacing = NSCollectionLayoutSpacing.CreateFixed(new NFloat(itemSpacing));

			// Create our section layout
			var section = NSCollectionLayoutSection.Create(group: group);
			section.InterGroupSpacing = new NFloat(itemSpacing);

			// Create header and footer for group
			section.BoundarySupplementaryItems = CreateSupplementaryItems(
				groupingInfo,
				null,
				scrollDirection,
				groupWidth,
				groupHeight);

			return section;
		}, layoutConfiguration);

		return layout;
	}



	static UICollectionViewLayout CreateGridLayout(UICollectionViewScrollDirection scrollDirection, LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo, LayoutSnapInfo snapInfo, NSCollectionLayoutDimension itemWidth, NSCollectionLayoutDimension itemHeight, NSCollectionLayoutDimension groupWidth, NSCollectionLayoutDimension groupHeight, double verticalItemSpacing, double horizontalItemSpacing, int columns, ItemSizingStrategy itemSizingStrategy = ItemSizingStrategy.MeasureAllItems)
	{
		var layoutConfiguration = new UICollectionViewCompositionalLayoutConfiguration();
		layoutConfiguration.ScrollDirection = scrollDirection;

		// For MeasureFirstItem, create a custom layout that measures the first item and applies that size to all items
		if (itemSizingStrategy == ItemSizingStrategy.MeasureFirstItem)
		{
			return new MeasureFirstItemGridCompositionalLayout(snapInfo, scrollDirection, verticalItemSpacing, horizontalItemSpacing, columns, groupingInfo, headerFooterInfo, itemWidth, itemHeight, groupWidth, groupHeight, layoutConfiguration);
		}

		var layout = new CustomUICollectionViewCompositionalLayout(snapInfo, (sectionIndex, environment) =>
		{
			// Each item has a size
			var itemSize = NSCollectionLayoutSize.Create(itemWidth, itemHeight);
			// Create the item itself from the size
			var item = NSCollectionLayoutItem.Create(layoutSize: itemSize);

			// Each group of items (for grouped collections) has a size
			var groupSize = NSCollectionLayoutSize.Create(groupWidth, groupHeight);

			// Create the group
			// If vertical list, we want the group to layout horizontally (eg: grid columns go left to right)
			// for horizontal list, we want to lay grid rows out vertically
			// For simple lists it doesn't matter so much since the items span the entire width or height
			var group = scrollDirection == UICollectionViewScrollDirection.Vertical
				? NSCollectionLayoutGroup.CreateHorizontal(groupSize, item, columns)
				: NSCollectionLayoutGroup.CreateVertical(groupSize, item, columns);

			if (scrollDirection == UICollectionViewScrollDirection.Vertical)
				group.InterItemSpacing = NSCollectionLayoutSpacing.CreateFixed(new NFloat(horizontalItemSpacing));
			else
				group.InterItemSpacing = NSCollectionLayoutSpacing.CreateFixed(new NFloat(verticalItemSpacing));

			// Create our section layout
			var section = NSCollectionLayoutSection.Create(group: group);

			if (scrollDirection == UICollectionViewScrollDirection.Vertical)
				section.InterGroupSpacing = new NFloat(verticalItemSpacing);
			else
				section.InterGroupSpacing = new NFloat(horizontalItemSpacing);


			section.BoundarySupplementaryItems = CreateSupplementaryItems(
				groupingInfo,
				headerFooterInfo,
				scrollDirection,
				groupWidth,
				groupHeight);

			return section;
		}, layoutConfiguration);

		return layout;
	}

	public static UICollectionViewLayout CreateVerticalList(LinearItemsLayout linearItemsLayout,
		LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo, ItemSizingStrategy itemSizingStrategy = ItemSizingStrategy.MeasureAllItems)
		=> CreateListLayout(UICollectionViewScrollDirection.Vertical,
			groupingInfo,
			headerFooterInfo,
			new LayoutSnapInfo { SnapType = linearItemsLayout.SnapPointsType, SnapAligment = linearItemsLayout.SnapPointsAlignment },
			// Fill the width
			NSCollectionLayoutDimension.CreateFractionalWidth(1f),
			// Dynamic (estimate required)
			NSCollectionLayoutDimension.CreateEstimated(30f),
			NSCollectionLayoutDimension.CreateFractionalWidth(1f),
			NSCollectionLayoutDimension.CreateEstimated(30f),
			linearItemsLayout.ItemSpacing,
			null,
			itemSizingStrategy);


	public static UICollectionViewLayout CreateHorizontalList(LinearItemsLayout linearItemsLayout,
		LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo, ItemSizingStrategy itemSizingStrategy = ItemSizingStrategy.MeasureAllItems)
		=> CreateListLayout(UICollectionViewScrollDirection.Horizontal,
			groupingInfo,
			headerFooterInfo,
			new LayoutSnapInfo { SnapType = linearItemsLayout.SnapPointsType, SnapAligment = linearItemsLayout.SnapPointsAlignment },
			// Dynamic, estimated width
			NSCollectionLayoutDimension.CreateEstimated(30f),
			// Fill the height for horizontal
			NSCollectionLayoutDimension.CreateFractionalHeight(1f),
			NSCollectionLayoutDimension.CreateEstimated(30f),
			NSCollectionLayoutDimension.CreateFractionalHeight(1f),
			linearItemsLayout.ItemSpacing,
			null,
			itemSizingStrategy);

	public static UICollectionViewLayout CreateVerticalGrid(GridItemsLayout gridItemsLayout,
		LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo, ItemSizingStrategy itemSizingStrategy = ItemSizingStrategy.MeasureAllItems)
		=> CreateGridLayout(UICollectionViewScrollDirection.Vertical,
			groupingInfo,
			headerFooterInfo,
			new LayoutSnapInfo { SnapType = gridItemsLayout.SnapPointsType, SnapAligment = gridItemsLayout.SnapPointsAlignment },
			// Width is the number of columns
			NSCollectionLayoutDimension.CreateFractionalWidth(1f / gridItemsLayout.Span),
			// Height is dynamic, estimated
			NSCollectionLayoutDimension.CreateEstimated(30f),
			// Group spans all columns, full width for vertical
			NSCollectionLayoutDimension.CreateFractionalWidth(1f),
			// Group is dynamic height for vertical
			NSCollectionLayoutDimension.CreateEstimated(30f),
			gridItemsLayout.VerticalItemSpacing,
			gridItemsLayout.HorizontalItemSpacing,
			gridItemsLayout.Span,
			itemSizingStrategy);


	public static UICollectionViewLayout CreateHorizontalGrid(GridItemsLayout gridItemsLayout,
		LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo, ItemSizingStrategy itemSizingStrategy = ItemSizingStrategy.MeasureAllItems)
		=> CreateGridLayout(UICollectionViewScrollDirection.Horizontal,
			groupingInfo,
			headerFooterInfo,
			new LayoutSnapInfo { SnapType = gridItemsLayout.SnapPointsType, SnapAligment = gridItemsLayout.SnapPointsAlignment },
			// Item width is estimated
			NSCollectionLayoutDimension.CreateEstimated(30f),
			// Item height is number of rows
			NSCollectionLayoutDimension.CreateFractionalHeight(1f / gridItemsLayout.Span),
			// Group width is dynamic for horizontal
			NSCollectionLayoutDimension.CreateEstimated(30f),
			// Group spans all rows, full height for horizontal
			NSCollectionLayoutDimension.CreateFractionalHeight(1f),
			gridItemsLayout.VerticalItemSpacing,
			gridItemsLayout.HorizontalItemSpacing,
			gridItemsLayout.Span,
			itemSizingStrategy);


#nullable disable
	public static UICollectionViewLayout CreateCarouselLayout(
		WeakReference<CarouselView> weakItemsView,
		WeakReference<CarouselViewController2> weakController)
	{
		NSCollectionLayoutDimension itemWidth = NSCollectionLayoutDimension.CreateFractionalWidth(1);
		NSCollectionLayoutDimension itemHeight = NSCollectionLayoutDimension.CreateFractionalHeight(1);
		NSCollectionLayoutDimension groupWidth = NSCollectionLayoutDimension.CreateFractionalWidth(1);
		NSCollectionLayoutDimension groupHeight = NSCollectionLayoutDimension.CreateFractionalHeight(1);
		NSCollectionLayoutGroup group = null;

		var layout = new UICollectionViewCompositionalLayout((sectionIndex, environment) =>
		{
			if (!weakItemsView.TryGetTarget(out var itemsView))
			{
				return null;
			}

			bool isHorizontal = itemsView.ItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal;
			var peekAreaInsets = itemsView.PeekAreaInsets;

			double sectionMargin = 0.0;

			if (!isHorizontal)
			{
				sectionMargin = peekAreaInsets.VerticalThickness / 2;
				var newGroupHeight = environment.Container.ContentSize.Height - peekAreaInsets.VerticalThickness;
				groupHeight = NSCollectionLayoutDimension.CreateAbsolute((nfloat)newGroupHeight);
				groupWidth = NSCollectionLayoutDimension.CreateFractionalWidth(1);
			}
			else
			{
				sectionMargin = peekAreaInsets.HorizontalThickness / 2;
				var newGroupWidth = environment.Container.ContentSize.Width - peekAreaInsets.HorizontalThickness;
				groupWidth = NSCollectionLayoutDimension.CreateAbsolute((nfloat)newGroupWidth);
				groupHeight = NSCollectionLayoutDimension.CreateFractionalHeight(1);
			}

			// Each item has a size
			var itemSize = NSCollectionLayoutSize.Create(itemWidth, itemHeight);
			// Create the item itself from the size
			var item = NSCollectionLayoutItem.Create(layoutSize: itemSize);

			//item.ContentInsets = new NSDirectionalEdgeInsets(0, itemInset, 0, 0);

			var groupSize = NSCollectionLayoutSize.Create(groupWidth, groupHeight);

			if (OperatingSystem.IsIOSVersionAtLeast(16))
			{
				group = isHorizontal
					? NSCollectionLayoutGroup.GetHorizontalGroup(groupSize, item, 1)
					: NSCollectionLayoutGroup.GetVerticalGroup(groupSize, item, 1);
			}
			else
			{
				group = isHorizontal
					? NSCollectionLayoutGroup.CreateHorizontal(groupSize, item, 1)
					: NSCollectionLayoutGroup.CreateVertical(groupSize, item, 1);
			}

			var section = NSCollectionLayoutSection.Create(group: group);
			if (itemsView.ItemsLayout is LinearItemsLayout linearItemsLayout)
			{
				section.InterGroupSpacing = (nfloat)linearItemsLayout.ItemSpacing;
			}
			section.OrthogonalScrollingBehavior = isHorizontal
				? UICollectionLayoutSectionOrthogonalScrollingBehavior.GroupPagingCentered
				: UICollectionLayoutSectionOrthogonalScrollingBehavior.None;

			section.VisibleItemsInvalidationHandler = (items, offset, env) =>
			{
				if (!weakItemsView.TryGetTarget(out var itemsView) || !weakController.TryGetTarget(out var cv2Controller))
				{
					return;
				}

				var page = (offset.X + sectionMargin) / env.Container.ContentSize.Width;

				if (Math.Abs(page % 1) > (double.Epsilon * 100) || cv2Controller.ItemsSource.ItemCount <= 0)
				{
					return;
				}

				var pageIndex = (int)page;
				var carouselPosition = pageIndex;

				if (itemsView.Loop && cv2Controller.ItemsSource is ILoopItemsViewSource loopSource)
				{
					var maxIndex = loopSource.LoopCount - 1;

					//To mimic looping, we needed to modify the ItemSource and inserted a new item at the beginning and at the end
					if (pageIndex == maxIndex)
					{
						//When at last item, we need to change to 2nd item, so we can scroll right or left
						pageIndex = 1;
					}
					else if (pageIndex == 0)
					{
						//When at first item, need to change to one before last, so we can scroll right or left
						pageIndex = maxIndex - 1;
					}

					//since we added one item at the beginning of our ItemSource, we need to subtract one
					carouselPosition = pageIndex - 1;

					if (itemsView.Position != carouselPosition)
					{
						//If we are updating the ItemsSource, we don't want to scroll the CollectionView
						if (cv2Controller.IsUpdating())
						{
							return;
						}

						var goToIndexPath = cv2Controller.GetScrollToIndexPath(carouselPosition);

						//This will move the carousel to fake the loop
						cv2Controller.CollectionView.ScrollToItem(
							NSIndexPath.FromItemSection(pageIndex, 0),
							UICollectionViewScrollPosition.Left,
							false);
					}
				}

				//Update the CarouselView position
				cv2Controller?.SetPosition(carouselPosition);
			};

			return section;
		});

		return layout;
	}
#nullable enable
	class CustomUICollectionViewCompositionalLayout : UICollectionViewCompositionalLayout
	{
		LayoutSnapInfo _snapInfo;
		public CustomUICollectionViewCompositionalLayout(LayoutSnapInfo snapInfo, UICollectionViewCompositionalLayoutSectionProvider sectionProvider, UICollectionViewCompositionalLayoutConfiguration configuration) : base(sectionProvider, configuration)
		{
			_snapInfo = snapInfo;
		}

		public override CGPoint TargetContentOffset(CGPoint proposedContentOffset, CGPoint scrollingVelocity)
		{
			var snapPointsType = _snapInfo.SnapType;
			var alignment = _snapInfo.SnapAligment;

			if (snapPointsType == SnapPointsType.None)
			{
				// Nothing to do here; fall back to the default
				return base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
			}

			if (snapPointsType == SnapPointsType.MandatorySingle)
			{
				// Mandatory snapping, single element
				return ScrollSingle(alignment, proposedContentOffset, scrollingVelocity);
			}

			// Get the viewport of the UICollectionView at the proposed content offset
			var viewport = new CGRect(proposedContentOffset, CollectionView.Bounds.Size);

			// And find all the elements currently visible in the viewport
			var visibleElements = LayoutAttributesForElementsInRect(viewport);

			if (visibleElements.Length == 0)
			{
				// Nothing to see here; fall back to the default
				return base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
			}

			if (visibleElements.Length == 1)
			{
				// If there is only one item in the viewport,  then we need to align the viewport with it
				return Items.SnapHelpers.AdjustContentOffset(proposedContentOffset, visibleElements[0].Frame, viewport,
					alignment, Configuration.ScrollDirection);
			}

			// If there are multiple items in the viewport, we need to choose the one which is 
			// closest to the relevant part of the viewport while being sufficiently visible

			// Find the spot in the viewport we're trying to align with
			var alignmentTarget = Items.SnapHelpers.FindAlignmentTarget(alignment, proposedContentOffset,
				CollectionView, Configuration.ScrollDirection);

			// Find the closest sufficiently visible candidate
			var bestCandidate = Items.SnapHelpers.FindBestSnapCandidate(visibleElements, viewport, alignmentTarget);

			if (bestCandidate != null)
			{
				return Items.SnapHelpers.AdjustContentOffset(proposedContentOffset, bestCandidate.Frame, viewport, alignment,
					Configuration.ScrollDirection);
			}

			// If we got this far an nothing matched, it means that we have multiple items but somehow
			// none of them fit at least half in the viewport. So just fall back to the first item
			return Items.SnapHelpers.AdjustContentOffset(proposedContentOffset, visibleElements[0].Frame, viewport, alignment,
					Configuration.ScrollDirection);
		}

		CGPoint ScrollSingle(SnapPointsAlignment alignment, CGPoint proposedContentOffset, CGPoint scrollingVelocity)
		{
			// Get the viewport of the UICollectionView at the current content offset
			var contentOffset = CollectionView.ContentOffset;
			var viewport = new CGRect(contentOffset, CollectionView.Bounds.Size);

			// Find the spot in the viewport we're trying to align with
			var alignmentTarget = Items.SnapHelpers.FindAlignmentTarget(alignment, contentOffset, CollectionView, Configuration.ScrollDirection);

			var visibleElements = LayoutAttributesForElementsInRect(viewport);

			// Find the current aligned item
			var currentItem = Items.SnapHelpers.FindBestSnapCandidate(visibleElements, viewport, alignmentTarget);

			if (currentItem == null)
			{
				// Somehow we don't currently have an item in the viewport near the target; fall back to the
				// default behavior
				return base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
			}

			// Determine the index of the current item
			var currentIndex = visibleElements.IndexOf(currentItem);

			// Figure out the step size when jumping to the "next" element 
			var span = 1;
			// if (_itemsLayout is GridItemsLayout gridItemsLayout)
			// {
			// 	span = gridItemsLayout.Span;
			// }

			// Find the next item in the
			currentItem = Items.SnapHelpers.FindNextItem(visibleElements, Configuration.ScrollDirection, span, scrollingVelocity, currentIndex);

			return Items.SnapHelpers.AdjustContentOffset(CollectionView.ContentOffset, currentItem.Frame, viewport, alignment,
				Configuration.ScrollDirection);
		}
	}

	// Custom compositional layout for handling MeasureFirstItem strategy
	class MeasureFirstItemCompositionalLayout : UICollectionViewCompositionalLayout
	{
		LayoutSnapInfo _snapInfo;
		UICollectionViewScrollDirection _scrollDirection;
		double _itemSpacing;
		LayoutGroupingInfo _groupingInfo;
		LayoutHeaderFooterInfo _headerFooterInfo;
		NSCollectionLayoutDimension _originalItemWidth;
		NSCollectionLayoutDimension _originalItemHeight;
		NSCollectionLayoutDimension _groupWidth;
		NSCollectionLayoutDimension _groupHeight;
		Func<Thickness>? _peekAreaInsetsFunc;
		CGSize? _measuredItemSize;
		bool _needsLayoutUpdate;

		public MeasureFirstItemCompositionalLayout(LayoutSnapInfo snapInfo, UICollectionViewScrollDirection scrollDirection, double itemSpacing, LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo, NSCollectionLayoutDimension itemWidth, NSCollectionLayoutDimension itemHeight, NSCollectionLayoutDimension groupWidth, NSCollectionLayoutDimension groupHeight, Func<Thickness>? peekAreaInsetsFunc, UICollectionViewCompositionalLayoutConfiguration configuration) : base(CreateInitialSectionProvider(scrollDirection, itemWidth, itemHeight, groupWidth, groupHeight, itemSpacing, groupingInfo, peekAreaInsetsFunc), configuration)
		{
			_snapInfo = snapInfo;
			_scrollDirection = scrollDirection;
			_itemSpacing = itemSpacing;
			_groupingInfo = groupingInfo;
			_headerFooterInfo = headerFooterInfo;
			_originalItemWidth = itemWidth;
			_originalItemHeight = itemHeight;
			_groupWidth = groupWidth;
			_groupHeight = groupHeight;
			_peekAreaInsetsFunc = peekAreaInsetsFunc;
			_needsLayoutUpdate = true;
		}

		static UICollectionViewCompositionalLayoutSectionProvider CreateInitialSectionProvider(UICollectionViewScrollDirection scrollDirection, NSCollectionLayoutDimension itemWidth, NSCollectionLayoutDimension itemHeight, NSCollectionLayoutDimension groupWidth, NSCollectionLayoutDimension groupHeight, double itemSpacing, LayoutGroupingInfo groupingInfo, Func<Thickness>? peekAreaInsetsFunc)
		{
			return (sectionIndex, environment) =>
			{
				// Initial layout with estimated dimensions - will be updated after first item measurement
				var itemSize = NSCollectionLayoutSize.Create(itemWidth, itemHeight);
				var item = NSCollectionLayoutItem.Create(layoutSize: itemSize);

				var actualGroupWidth = groupWidth;
				var actualGroupHeight = groupHeight;

				if (peekAreaInsetsFunc?.Invoke() is Thickness peekAreaInsets)
				{
					if (scrollDirection == UICollectionViewScrollDirection.Vertical)
					{
						var newGroupHeight = environment.Container.ContentSize.Height - peekAreaInsets.Top - peekAreaInsets.Bottom;
						actualGroupHeight = NSCollectionLayoutDimension.CreateAbsolute((nfloat)newGroupHeight);
					}
					else
					{
						var newGroupWidth = environment.Container.ContentSize.Width - peekAreaInsets.Left - peekAreaInsets.Right;
						actualGroupWidth = NSCollectionLayoutDimension.CreateAbsolute((nfloat)newGroupWidth);
					}
				}

				var groupSize = NSCollectionLayoutSize.Create(actualGroupWidth, actualGroupHeight);
				var group = scrollDirection == UICollectionViewScrollDirection.Vertical
					? NSCollectionLayoutGroup.CreateHorizontal(groupSize, item, 1)
					: NSCollectionLayoutGroup.CreateVertical(groupSize, item, 1);

				var section = NSCollectionLayoutSection.Create(group: group);
				section.InterGroupSpacing = new NFloat(itemSpacing);

				// Create header and footer for group
				section.BoundarySupplementaryItems = CreateSupplementaryItems(
					groupingInfo,
					null,
					scrollDirection,
					actualGroupWidth,
					actualGroupHeight);

				return section;
			};
		}

		public override void PrepareLayout()
		{
			base.PrepareLayout();
			
			// If we haven't measured the first item yet and we have items
			if (_needsLayoutUpdate && CollectionView.NumberOfItemsInSection(0) > 0)
			{
				MeasureFirstItemAndUpdateLayout();
			}
		}

		void MeasureFirstItemAndUpdateLayout()
		{
			if (CollectionView?.NumberOfItemsInSection(0) == 0) return;

			// Try to get the first visible cell to measure
			var indexPath = NSIndexPath.FromItemSection(0, 0);
			var cell = CollectionView.CellForItem(indexPath);
			
			if (cell != null && cell.Frame.Size.Width > 0 && cell.Frame.Size.Height > 0)
			{
				// Use the size of the first visible cell
				_measuredItemSize = cell.Frame.Size;
				_needsLayoutUpdate = false;
				UpdateLayoutWithMeasuredSize();
			}
			else if (!_measuredItemSize.HasValue)
			{
				// If no visible cell is available, try to measure a prototype cell
				// This follows the pattern from ItemsViewLayout where we create a prototype cell
				var prototype = CollectionView.DequeueReusableCell(CollectionView.RegisterClassForCell(typeof(UICollectionViewCell), "prototype"), indexPath);
				if (prototype != null)
				{
					// Size the prototype cell to get an accurate measurement
					var targetSize = new CGSize(
						_scrollDirection == UICollectionViewScrollDirection.Vertical ? CollectionView.Bounds.Width : 1000,
						_scrollDirection == UICollectionViewScrollDirection.Horizontal ? CollectionView.Bounds.Height : 1000
					);

					var measuredSize = prototype.SystemLayoutSizeFittingSize(targetSize);
					if (measuredSize.Width > 0 && measuredSize.Height > 0)
					{
						_measuredItemSize = measuredSize;
						_needsLayoutUpdate = false;
						UpdateLayoutWithMeasuredSize();
					}
				}
			}
		}

		void UpdateLayoutWithMeasuredSize()
		{
			if (!_measuredItemSize.HasValue) return;

			var measuredSize = _measuredItemSize.Value;
			
			// Create absolute dimensions based on measured size
			var itemWidth = NSCollectionLayoutDimension.CreateAbsolute(measuredSize.Width);
			var itemHeight = NSCollectionLayoutDimension.CreateAbsolute(measuredSize.Height);

			// Update the layout with the new section provider that uses absolute dimensions
			var newSectionProvider = CreateSectionProviderWithAbsoluteDimensions(itemWidth, itemHeight);
			
			// We need to create a new layout with the updated section provider
			// Unfortunately, UICollectionViewCompositionalLayout doesn't allow updating the section provider directly
			// So we'll need to invalidate and recreate the layout
			InvalidateLayout();
		}

		UICollectionViewCompositionalLayoutSectionProvider CreateSectionProviderWithAbsoluteDimensions(NSCollectionLayoutDimension itemWidth, NSCollectionLayoutDimension itemHeight)
		{
			return (sectionIndex, environment) =>
			{
				// Create item with absolute dimensions
				var itemSize = NSCollectionLayoutSize.Create(itemWidth, itemHeight);
				var item = NSCollectionLayoutItem.Create(layoutSize: itemSize);

				var actualGroupWidth = _groupWidth;
				var actualGroupHeight = _groupHeight;

				if (_peekAreaInsetsFunc?.Invoke() is Thickness peekAreaInsets)
				{
					if (_scrollDirection == UICollectionViewScrollDirection.Vertical)
					{
						var newGroupHeight = environment.Container.ContentSize.Height - peekAreaInsets.Top - peekAreaInsets.Bottom;
						actualGroupHeight = NSCollectionLayoutDimension.CreateAbsolute((nfloat)newGroupHeight);
					}
					else
					{
						var newGroupWidth = environment.Container.ContentSize.Width - peekAreaInsets.Left - peekAreaInsets.Right;
						actualGroupWidth = NSCollectionLayoutDimension.CreateAbsolute((nfloat)newGroupWidth);
					}
				}

				var groupSize = NSCollectionLayoutSize.Create(actualGroupWidth, actualGroupHeight);
				var group = _scrollDirection == UICollectionViewScrollDirection.Vertical
					? NSCollectionLayoutGroup.CreateHorizontal(groupSize, item, 1)
					: NSCollectionLayoutGroup.CreateVertical(groupSize, item, 1);

				var section = NSCollectionLayoutSection.Create(group: group);
				section.InterGroupSpacing = new NFloat(_itemSpacing);

				// Create header and footer for group
				section.BoundarySupplementaryItems = CreateSupplementaryItems(
					_groupingInfo,
					null,
					_scrollDirection,
					actualGroupWidth,
					actualGroupHeight);

				return section;
			};
		}

		public override CGPoint TargetContentOffset(CGPoint proposedContentOffset, CGPoint scrollingVelocity)
		{
			var snapPointsType = _snapInfo.SnapType;
			var alignment = _snapInfo.SnapAligment;

			if (snapPointsType == SnapPointsType.None)
			{
				return base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
			}

			if (snapPointsType == SnapPointsType.MandatorySingle)
			{
				return ScrollSingle(alignment, proposedContentOffset, scrollingVelocity);
			}

			// Get the viewport of the UICollectionView at the proposed content offset
			var viewport = new CGRect(proposedContentOffset, CollectionView.Bounds.Size);
			var visibleElements = LayoutAttributesForElementsInRect(viewport);

			if (visibleElements.Length == 0)
			{
				return base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
			}

			if (visibleElements.Length == 1)
			{
				return Items.SnapHelpers.AdjustContentOffset(proposedContentOffset, visibleElements[0].Frame, viewport,
					alignment, _scrollDirection);
			}

			var alignmentTarget = Items.SnapHelpers.FindAlignmentTarget(alignment, proposedContentOffset,
				CollectionView, _scrollDirection);

			var bestCandidate = Items.SnapHelpers.FindBestSnapCandidate(visibleElements, viewport, alignmentTarget);

			if (bestCandidate != null)
			{
				return Items.SnapHelpers.AdjustContentOffset(proposedContentOffset, bestCandidate.Frame, viewport, alignment,
					_scrollDirection);
			}

			return Items.SnapHelpers.AdjustContentOffset(proposedContentOffset, visibleElements[0].Frame, viewport, alignment,
				_scrollDirection);
		}

		CGPoint ScrollSingle(SnapPointsAlignment alignment, CGPoint proposedContentOffset, CGPoint scrollingVelocity)
		{
			var contentOffset = CollectionView.ContentOffset;
			var viewport = new CGRect(contentOffset, CollectionView.Bounds.Size);
			var alignmentTarget = Items.SnapHelpers.FindAlignmentTarget(alignment, contentOffset, CollectionView, _scrollDirection);
			var visibleElements = LayoutAttributesForElementsInRect(viewport);
			var currentItem = Items.SnapHelpers.FindBestSnapCandidate(visibleElements, viewport, alignmentTarget);

			if (currentItem == null)
			{
				return base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
			}

			var currentIndex = visibleElements.IndexOf(currentItem);
			var span = 1;
			currentItem = Items.SnapHelpers.FindNextItem(visibleElements, _scrollDirection, span, scrollingVelocity, currentIndex);

			return Items.SnapHelpers.AdjustContentOffset(CollectionView.ContentOffset, currentItem.Frame, viewport, alignment,
				_scrollDirection);
		}
	}

	// Custom compositional layout for handling MeasureFirstItem strategy in grid layouts
	class MeasureFirstItemGridCompositionalLayout : UICollectionViewCompositionalLayout
	{
		LayoutSnapInfo _snapInfo;
		UICollectionViewScrollDirection _scrollDirection;
		double _verticalItemSpacing;
		double _horizontalItemSpacing;
		int _columns;
		LayoutGroupingInfo _groupingInfo;
		LayoutHeaderFooterInfo _headerFooterInfo;
		NSCollectionLayoutDimension _originalItemWidth;
		NSCollectionLayoutDimension _originalItemHeight;
		NSCollectionLayoutDimension _groupWidth;
		NSCollectionLayoutDimension _groupHeight;
		CGSize? _measuredItemSize;
		bool _needsLayoutUpdate;

		public MeasureFirstItemGridCompositionalLayout(LayoutSnapInfo snapInfo, UICollectionViewScrollDirection scrollDirection, double verticalItemSpacing, double horizontalItemSpacing, int columns, LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo, NSCollectionLayoutDimension itemWidth, NSCollectionLayoutDimension itemHeight, NSCollectionLayoutDimension groupWidth, NSCollectionLayoutDimension groupHeight, UICollectionViewCompositionalLayoutConfiguration configuration) : base(CreateInitialGridSectionProvider(scrollDirection, itemWidth, itemHeight, groupWidth, groupHeight, verticalItemSpacing, horizontalItemSpacing, columns, groupingInfo, headerFooterInfo), configuration)
		{
			_snapInfo = snapInfo;
			_scrollDirection = scrollDirection;
			_verticalItemSpacing = verticalItemSpacing;
			_horizontalItemSpacing = horizontalItemSpacing;
			_columns = columns;
			_groupingInfo = groupingInfo;
			_headerFooterInfo = headerFooterInfo;
			_originalItemWidth = itemWidth;
			_originalItemHeight = itemHeight;
			_groupWidth = groupWidth;
			_groupHeight = groupHeight;
			_needsLayoutUpdate = true;
		}

		static UICollectionViewCompositionalLayoutSectionProvider CreateInitialGridSectionProvider(UICollectionViewScrollDirection scrollDirection, NSCollectionLayoutDimension itemWidth, NSCollectionLayoutDimension itemHeight, NSCollectionLayoutDimension groupWidth, NSCollectionLayoutDimension groupHeight, double verticalItemSpacing, double horizontalItemSpacing, int columns, LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo)
		{
			return (sectionIndex, environment) =>
			{
				// Initial layout with estimated dimensions - will be updated after first item measurement
				var itemSize = NSCollectionLayoutSize.Create(itemWidth, itemHeight);
				var item = NSCollectionLayoutItem.Create(layoutSize: itemSize);

				var groupSize = NSCollectionLayoutSize.Create(groupWidth, groupHeight);
				var group = scrollDirection == UICollectionViewScrollDirection.Vertical
					? NSCollectionLayoutGroup.CreateHorizontal(groupSize, item, columns)
					: NSCollectionLayoutGroup.CreateVertical(groupSize, item, columns);

				if (scrollDirection == UICollectionViewScrollDirection.Vertical)
					group.InterItemSpacing = NSCollectionLayoutSpacing.CreateFixed(new NFloat(horizontalItemSpacing));
				else
					group.InterItemSpacing = NSCollectionLayoutSpacing.CreateFixed(new NFloat(verticalItemSpacing));

				var section = NSCollectionLayoutSection.Create(group: group);

				if (scrollDirection == UICollectionViewScrollDirection.Vertical)
					section.InterGroupSpacing = new NFloat(verticalItemSpacing);
				else
					section.InterGroupSpacing = new NFloat(horizontalItemSpacing);

				section.BoundarySupplementaryItems = CreateSupplementaryItems(
					groupingInfo,
					headerFooterInfo,
					scrollDirection,
					groupWidth,
					groupHeight);

				return section;
			};
		}

		public override void PrepareLayout()
		{
			base.PrepareLayout();
			
			// If we haven't measured the first item yet and we have items
			if (_needsLayoutUpdate && CollectionView.NumberOfItemsInSection(0) > 0)
			{
				MeasureFirstItemAndUpdateLayout();
			}
		}

		void MeasureFirstItemAndUpdateLayout()
		{
			if (CollectionView?.NumberOfItemsInSection(0) == 0) return;

			// Try to get the first visible cell to measure
			var indexPath = NSIndexPath.FromItemSection(0, 0);
			var cell = CollectionView.CellForItem(indexPath);
			
			if (cell != null && cell.Frame.Size.Width > 0 && cell.Frame.Size.Height > 0)
			{
				// Use the size of the first visible cell
				_measuredItemSize = cell.Frame.Size;
				_needsLayoutUpdate = false;
				UpdateLayoutWithMeasuredSize();
			}
			else if (!_measuredItemSize.HasValue)
			{
				// If no visible cell is available, try to measure a prototype cell
				var prototype = CollectionView.DequeueReusableCell(CollectionView.RegisterClassForCell(typeof(UICollectionViewCell), "prototype"), indexPath);
				if (prototype != null)
				{
					// Size the prototype cell to get an accurate measurement
					var targetSize = new CGSize(
						_scrollDirection == UICollectionViewScrollDirection.Vertical ? CollectionView.Bounds.Width / _columns : 1000,
						_scrollDirection == UICollectionViewScrollDirection.Horizontal ? CollectionView.Bounds.Height / _columns : 1000
					);

					var measuredSize = prototype.SystemLayoutSizeFittingSize(targetSize);
					if (measuredSize.Width > 0 && measuredSize.Height > 0)
					{
						_measuredItemSize = measuredSize;
						_needsLayoutUpdate = false;
						UpdateLayoutWithMeasuredSize();
					}
				}
			}
		}

		void UpdateLayoutWithMeasuredSize()
		{
			if (!_measuredItemSize.HasValue) return;

			var measuredSize = _measuredItemSize.Value;
			
			// Create absolute dimensions based on measured size
			var itemWidth = NSCollectionLayoutDimension.CreateAbsolute(measuredSize.Width);
			var itemHeight = NSCollectionLayoutDimension.CreateAbsolute(measuredSize.Height);

			// Update the layout with the new section provider that uses absolute dimensions
			var newSectionProvider = CreateGridSectionProviderWithAbsoluteDimensions(itemWidth, itemHeight);
			
			// We need to create a new layout with the updated section provider
			// Unfortunately, UICollectionViewCompositionalLayout doesn't allow updating the section provider directly
			// So we'll need to invalidate and recreate the layout
			InvalidateLayout();
		}

		UICollectionViewCompositionalLayoutSectionProvider CreateGridSectionProviderWithAbsoluteDimensions(NSCollectionLayoutDimension itemWidth, NSCollectionLayoutDimension itemHeight)
		{
			return (sectionIndex, environment) =>
			{
				// Create item with absolute dimensions
				var itemSize = NSCollectionLayoutSize.Create(itemWidth, itemHeight);
				var item = NSCollectionLayoutItem.Create(layoutSize: itemSize);

				var groupSize = NSCollectionLayoutSize.Create(_groupWidth, _groupHeight);
				var group = _scrollDirection == UICollectionViewScrollDirection.Vertical
					? NSCollectionLayoutGroup.CreateHorizontal(groupSize, item, _columns)
					: NSCollectionLayoutGroup.CreateVertical(groupSize, item, _columns);

				if (_scrollDirection == UICollectionViewScrollDirection.Vertical)
					group.InterItemSpacing = NSCollectionLayoutSpacing.CreateFixed(new NFloat(_horizontalItemSpacing));
				else
					group.InterItemSpacing = NSCollectionLayoutSpacing.CreateFixed(new NFloat(_verticalItemSpacing));

				var section = NSCollectionLayoutSection.Create(group: group);

				if (_scrollDirection == UICollectionViewScrollDirection.Vertical)
					section.InterGroupSpacing = new NFloat(_verticalItemSpacing);
				else
					section.InterGroupSpacing = new NFloat(_horizontalItemSpacing);

				section.BoundarySupplementaryItems = CreateSupplementaryItems(
					_groupingInfo,
					_headerFooterInfo,
					_scrollDirection,
					_groupWidth,
					_groupHeight);

				return section;
			};
		}

		public override CGPoint TargetContentOffset(CGPoint proposedContentOffset, CGPoint scrollingVelocity)
		{
			var snapPointsType = _snapInfo.SnapType;
			var alignment = _snapInfo.SnapAligment;

			if (snapPointsType == SnapPointsType.None)
			{
				return base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
			}

			if (snapPointsType == SnapPointsType.MandatorySingle)
			{
				return ScrollSingle(alignment, proposedContentOffset, scrollingVelocity);
			}

			// Get the viewport of the UICollectionView at the proposed content offset
			var viewport = new CGRect(proposedContentOffset, CollectionView.Bounds.Size);
			var visibleElements = LayoutAttributesForElementsInRect(viewport);

			if (visibleElements.Length == 0)
			{
				return base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
			}

			if (visibleElements.Length == 1)
			{
				return Items.SnapHelpers.AdjustContentOffset(proposedContentOffset, visibleElements[0].Frame, viewport,
					alignment, _scrollDirection);
			}

			var alignmentTarget = Items.SnapHelpers.FindAlignmentTarget(alignment, proposedContentOffset,
				CollectionView, _scrollDirection);

			var bestCandidate = Items.SnapHelpers.FindBestSnapCandidate(visibleElements, viewport, alignmentTarget);

			if (bestCandidate != null)
			{
				return Items.SnapHelpers.AdjustContentOffset(proposedContentOffset, bestCandidate.Frame, viewport, alignment,
					_scrollDirection);
			}

			return Items.SnapHelpers.AdjustContentOffset(proposedContentOffset, visibleElements[0].Frame, viewport, alignment,
				_scrollDirection);
		}

		CGPoint ScrollSingle(SnapPointsAlignment alignment, CGPoint proposedContentOffset, CGPoint scrollingVelocity)
		{
			var contentOffset = CollectionView.ContentOffset;
			var viewport = new CGRect(contentOffset, CollectionView.Bounds.Size);
			var alignmentTarget = Items.SnapHelpers.FindAlignmentTarget(alignment, contentOffset, CollectionView, _scrollDirection);
			var visibleElements = LayoutAttributesForElementsInRect(viewport);
			var currentItem = Items.SnapHelpers.FindBestSnapCandidate(visibleElements, viewport, alignmentTarget);

			if (currentItem == null)
			{
				return base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
			}

			var currentIndex = visibleElements.IndexOf(currentItem);
			var span = 1;
			currentItem = Items.SnapHelpers.FindNextItem(visibleElements, _scrollDirection, span, scrollingVelocity, currentIndex);

			return Items.SnapHelpers.AdjustContentOffset(CollectionView.ContentOffset, currentItem.Frame, viewport, alignment,
				_scrollDirection);
		}
	}

}