#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		public StructuredItemsViewHandler() : base(StructuredItemsViewMapper)
		{

		}
		public StructuredItemsViewHandler(PropertyMapper mapper = null) : base(mapper ?? StructuredItemsViewMapper)
		{

		}

		public static void MapHeader(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
#if __ANDROID__
			(handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateAdapter();
#elif __IOS__
			(handler.Controller as StructuredItemsViewController<TItemsView>)?.UpdateHeaderView();
#elif WINDOWS
			handler.UpdateHeader();
#elif TIZEN
			(handler.PlatformView as MauiCollectionView<TItemsView>)?.UpdateAdaptor();
#endif
		}

		public static void MapFooter(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
#if __ANDROID__
			(handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateAdapter();
#elif __IOS__
			(handler.Controller as StructuredItemsViewController<TItemsView>)?.UpdateFooterView();
#elif WINDOWS
			handler.UpdateFooter();
#elif TIZEN
			(handler.PlatformView as MauiCollectionView<TItemsView>)?.UpdateAdaptor();
#endif
		}

		public static PropertyMapper<TItemsView, StructuredItemsViewHandler<TItemsView>> StructuredItemsViewMapper = new(ItemsViewMapper)
		{
#if TIZEN
			[StructuredItemsView.HeaderProperty.PropertyName] = MapHeader,
			[StructuredItemsView.FooterProperty.PropertyName] = MapFooter,
			[StructuredItemsView.HeaderTemplateProperty.PropertyName] = MapHeader,
			[StructuredItemsView.FooterTemplateProperty.PropertyName] = MapFooter,
#endif
			[StructuredItemsView.HeaderProperty.PropertyName] = MapHeader,
			[StructuredItemsView.FooterProperty.PropertyName] = MapFooter,
			[StructuredItemsView.HeaderTemplateProperty.PropertyName] = MapHeaderTemplate,
			[StructuredItemsView.FooterTemplateProperty.PropertyName] = MapFooterTemplate,
			[StructuredItemsView.ItemsLayoutProperty.PropertyName] = MapItemsLayout,
			[StructuredItemsView.ItemSizingStrategyProperty.PropertyName] = MapItemSizingStrategy
		};

	}
}
