#nullable disable
using System;
using Android.Content;
using AndroidX.RecyclerView.Widget;
using ALayoutDirection = Android.Views.LayoutDirection;
using ARect = Android.Graphics.Rect;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class SpacingItemDecoration : RecyclerView.ItemDecoration
	{
		public int HorizontalOffset { get; }

		public int VerticalOffset { get; }

		ItemsLayoutOrientation _orientation;

		public SpacingItemDecoration(Context context, IItemsLayout itemsLayout)
		{
			if (itemsLayout == null)
			{
				throw new ArgumentNullException(nameof(itemsLayout));
			}

			double horizontalOffset;
			double verticalOffset;

			switch (itemsLayout)
			{
				case GridItemsLayout gridItemsLayout:
					horizontalOffset = gridItemsLayout.HorizontalItemSpacing / 2.0;
					verticalOffset = gridItemsLayout.VerticalItemSpacing / 2.0;
					_orientation = gridItemsLayout.Orientation;
					break;
				case LinearItemsLayout listItemsLayout:
					if (listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
					{
						horizontalOffset = listItemsLayout.ItemSpacing / 2.0;
						verticalOffset = 0;
					}
					else
					{
						horizontalOffset = 0;
						verticalOffset = listItemsLayout.ItemSpacing / 2.0;
					}
					_orientation = listItemsLayout.Orientation;
					break;
				default:
					horizontalOffset = 0;
					verticalOffset = 0;
					_orientation = ItemsLayoutOrientation.Vertical;
					break;
			}

			HorizontalOffset = (int)context.ToPixels(horizontalOffset);
			VerticalOffset = (int)context.ToPixels(verticalOffset);
		}

		public override void GetItemOffsets(ARect outRect, AView view, RecyclerView parent, RecyclerView.State state)
		{
			base.GetItemOffsets(outRect, view, parent, state);

			// Nothing to apply; skip the row computations below entirely.
			if (HorizontalOffset == 0 && VerticalOffset == 0)
			{
				return;
			}

			var adapter = parent.GetAdapter();
			if (adapter is null)
			{
				return;
			}

			int position = parent.GetChildAdapterPosition(view);
			int itemCount = adapter.ItemCount;

			if (position == RecyclerView.NoPosition || position < 0 || position >= itemCount)
			{
				return;
			}

			outRect.Left = HorizontalOffset;
			outRect.Right = HorizontalOffset;
			outRect.Bottom = VerticalOffset;
			outRect.Top = VerticalOffset;

			// Remove spacing on the outer edges so spacing only appears between items.
			bool isInFirstRowCol;
			bool isInLastRowCol;

			if (parent.GetLayoutManager() is GridLayoutManager gridLayoutManager)
			{
				// Full-span items (group headers, footers, etc.) are accounted for via the SpanSizeLookup.
				// Only the items that can share a row with `position` are inspected (at most spanCount of them);
				// SpanSizeLookup.GetSpanGroupIndex(itemCount - 1) would walk every position in the adapter
				// for every cell on every layout pass.
				var spanSizeLookup = gridLayoutManager.GetSpanSizeLookup();
				int spanCount = gridLayoutManager.SpanCount;
				int spanIndex = spanSizeLookup.GetSpanIndex(position, spanCount);
				int spanSize = spanSizeLookup.GetSpanSize(position);
				rowCol = spanSizeLookup.GetSpanGroupIndex(position, spanCount);
				lastRowCol = spanSizeLookup.GetSpanGroupIndex(itemCount - 1, spanCount);

				if (_orientation == ItemsLayoutOrientation.Vertical)
				{
					GetCrossAxisOffsets(HorizontalOffset * 2, spanIndex, spanSize, spanCount, out int start, out int end);

					if (parent.LayoutDirection == ALayoutDirection.Rtl)
					{
						outRect.Left = end;
						outRect.Right = start;
					}
					else
					{
						outRect.Left = start;
						outRect.Right = end;
					}
				}
				else
				{
					GetCrossAxisOffsets(VerticalOffset * 2, spanIndex, spanSize, spanCount, out int top, out int bottom);
					outRect.Top = top;
					outRect.Bottom = bottom;
				}
			 }
			else
			{
				// Linear layout: each item occupies exactly one row/column.
				isInFirstRowCol = position == 0;
				isInLastRowCol = position == itemCount - 1;
			}

			if (_orientation == ItemsLayoutOrientation.Vertical)
			{
				if (isInFirstRowCol)
					outRect.Top = 0;
				if (isInLastRowCol)
					outRect.Bottom = 0;
			}
			else
			{
				if (isInFirstRowCol)
					outRect.Left = 0;
				if (isInLastRowCol)
					outRect.Right = 0;
			}
		}

		internal static void GetCrossAxisOffsets(int spacing, int spanIndex, int spanSize, int spanCount, out int start, out int end)
		{
			start = (int)((long)spacing * spanIndex / spanCount);
			end = spacing - (int)((long)spacing * (spanIndex + spanSize) / spanCount);
		}
	}
}