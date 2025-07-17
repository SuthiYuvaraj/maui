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
			// Only call UpdateAdapter if adapter is not yet set up, otherwise the adapter's property listener handles it
			var recyclerView = handler.PlatformView as IMauiRecyclerView<TItemsView>;
			if (recyclerView != null)
			{
				// Check if this is a RecyclerView and if the adapter is already set
				if (handler.PlatformView is RecyclerView androidRecyclerView && androidRecyclerView.GetAdapter() != null)
				{
					// Adapter is already set, the adapter's property change listener will handle this more efficiently
					return;
				}
				// Adapter not set yet, need to use UpdateAdapter for initial setup
				recyclerView.UpdateAdapter();
			}
		}

		public static void MapFooterTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			// Only call UpdateAdapter if adapter is not yet set up, otherwise the adapter's property listener handles it
			var recyclerView = handler.PlatformView as IMauiRecyclerView<TItemsView>;
			if (recyclerView != null)
			{
				// Check if this is a RecyclerView and if the adapter is already set
				if (handler.PlatformView is RecyclerView androidRecyclerView && androidRecyclerView.GetAdapter() != null)
				{
					// Adapter is already set, the adapter's property change listener will handle this more efficiently
					return;
				}
				// Adapter not set yet, need to use UpdateAdapter for initial setup
				recyclerView.UpdateAdapter();
			}
		}

		public static void MapItemsLayout(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
			=> (handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateLayoutManager();

		public static void MapItemSizingStrategy(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
			=> (handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateAdapter();
	}
}
