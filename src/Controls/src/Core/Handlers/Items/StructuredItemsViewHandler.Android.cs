#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> : ItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		protected override IItemsLayout GetItemsLayout() => VirtualView.ItemsLayout;

		protected override StructuredItemsViewAdapter<TItemsView, IItemsViewSource> CreateAdapter() => new(VirtualView);

		public static void MapHeaderTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateHeaderFooterOptimized(true);
		}

		public static void MapFooterTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateHeaderFooterOptimized(false);
		}

		public static void MapItemsLayout(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
			=> (handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateLayoutManager();

		public static void MapItemSizingStrategy(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
			=> (handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateAdapter();

		void UpdateHeaderFooterOptimized(bool isHeader)
		{
			var recyclerView = PlatformView as IMauiRecyclerView<TItemsView>;
			var adapter = (recyclerView as RecyclerView)?.GetAdapter();

			if (recyclerView == null || adapter == null)
				return;

			bool hasHeaderOrFooter = isHeader
				? (VirtualView.Header ?? VirtualView.HeaderTemplate) != null
				: (VirtualView.Footer ?? VirtualView.FooterTemplate) != null;

			bool exists = isHeader
				? DoesHeaderExist(adapter)
				: DoesFooterExist(adapter);

			if (hasHeaderOrFooter && exists)
			{
				if (IsDynamicChange())
				{
					recyclerView.UpdateAdapter();
				}
			}
			else if (hasHeaderOrFooter != exists)
			{
				recyclerView.UpdateAdapter();
			}
		}

		private bool DoesHeaderExist(RecyclerView.Adapter adapter)
		{
			if (adapter.ItemCount == 0)
				return false;

			try
			{
				const int HeaderViewType = 43;
				return adapter.GetItemViewType(0) == HeaderViewType;
			}
			catch
			{
				return false;
			}
		}

		private bool DoesFooterExist(RecyclerView.Adapter adapter)
		{
			var footerPosition = adapter.ItemCount - 1;
			if (footerPosition < 0)
				return false;

			try
			{
				const int FooterViewType = 44;
				return adapter.GetItemViewType(footerPosition) == FooterViewType;
			}
			catch
			{
				return false;
			}
		}

		private bool IsDynamicChange()
		{
			// Simple heuristic: if the RecyclerView has been laid out, it's likely a runtime change
			// During initial loading, the view typically hasn't been measured/laid out yet
			return (PlatformView as RecyclerView)?.IsLaidOut == true;
		}
	}
}