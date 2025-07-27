#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using WASDKApp = Microsoft.UI.Xaml.Application;
using WListView = Microsoft.UI.Xaml.Controls.ListView;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using WSetter = Microsoft.UI.Xaml.Setter;
using WStyle = Microsoft.UI.Xaml.Style;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> : ItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		View _currentHeader;
		View _currentFooter;
		WeakNotifyPropertyChangedProxy _layoutPropertyChangedProxy;
		PropertyChangedEventHandler _layoutPropertyChanged;

		~StructuredItemsViewHandler() => _layoutPropertyChangedProxy?.Unsubscribe();

		protected override IItemsLayout Layout { get => ItemsView?.ItemsLayout; }

		protected override void ConnectHandler(ListViewBase platformView)
		{
			base.ConnectHandler(platformView);

			if (Layout is not null)
			{
				_layoutPropertyChanged ??= LayoutPropertyChanged;
				_layoutPropertyChangedProxy = new WeakNotifyPropertyChangedProxy(Layout, _layoutPropertyChanged);
			}
			else if (_layoutPropertyChangedProxy is not null)
			{
				_layoutPropertyChangedProxy.Unsubscribe();
				_layoutPropertyChangedProxy = null;
			}
		}

		protected override void DisconnectHandler(ListViewBase platformView)
		{
			base.DisconnectHandler(platformView);

			if (_layoutPropertyChangedProxy is not null)
			{
				_layoutPropertyChangedProxy.Unsubscribe();
				_layoutPropertyChangedProxy = null;
			}
		}

		void LayoutPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == GridItemsLayout.SpanProperty.PropertyName)
				UpdateItemsLayoutSpan();
			else if (e.PropertyName == GridItemsLayout.HorizontalItemSpacingProperty.PropertyName || e.PropertyName == GridItemsLayout.VerticalItemSpacingProperty.PropertyName)
				UpdateItemsLayoutItemSpacing();
			else if (e.PropertyName == LinearItemsLayout.ItemSpacingProperty.PropertyName)
				UpdateItemsLayoutItemSpacing();
		}

		public static void MapHeaderTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateHeader();
		}

		public static void MapFooterTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateFooter();
		}

		public static void MapItemsLayout(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateItemsLayout();
		}

		public static void MapItemSizingStrategy(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateItemSizingStrategy();
		}

		protected override ListViewBase SelectListViewBase()
		{
			switch (VirtualView.ItemsLayout)
			{
				case GridItemsLayout gridItemsLayout:
					return CreateGridView(gridItemsLayout);
				case LinearItemsLayout listItemsLayout when listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical:
					return CreateVerticalListView(listItemsLayout);
				case LinearItemsLayout listItemsLayout when listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal:
					return CreateHorizontalListView(listItemsLayout);
			}

			throw new NotImplementedException("The layout is not implemented");
		}

		protected virtual void UpdateHeader()
		{
			if (ListViewBase == null)
			{
				return;
			}

			if (_currentHeader != null)
			{
				Element.RemoveLogicalChild(_currentHeader);
				_currentHeader = null;
			}

			var header = ItemsView.Header ?? ItemsView.HeaderTemplate;

			switch (header)
			{
				case null:
					ListViewBase.Header = null;
					break;

				case string text:
					ListViewBase.HeaderTemplate = null;
					ListViewBase.Header = new TextBlock { Text = text };
					break;

				case View view:
					ListViewBase.HeaderTemplate = ViewTemplate;
					_currentHeader = view;
					Element.AddLogicalChild(_currentHeader);
					ListViewBase.Header = view;
					break;

				default:
					var headerTemplate = ItemsView.HeaderTemplate;
					if (headerTemplate != null)
					{
						ListViewBase.HeaderTemplate = ItemsViewTemplate;
						ListViewBase.Header = new ItemTemplateContext(headerTemplate, header, Element, mauiContext: MauiContext);
					}
					else
					{
						ListViewBase.HeaderTemplate = null;
						ListViewBase.Header = null;
					}
					break;
			}
		}

		protected virtual void UpdateFooter()
		{
			if (ListViewBase == null)
			{
				return;
			}

			if (_currentFooter != null)
			{
				Element.RemoveLogicalChild(_currentFooter);
				_currentFooter = null;
			}

			var footer = ItemsView.Footer ?? ItemsView.FooterTemplate;

			switch (footer)
			{
				case null:
					ListViewBase.Footer = null;
					break;

				case string text:
					ListViewBase.FooterTemplate = null;
					ListViewBase.Footer = new TextBlock { Text = text };
					break;

				case View view:
					ListViewBase.FooterTemplate = ViewTemplate;
					_currentFooter = view;
					Element.AddLogicalChild(_currentFooter);
					ListViewBase.Footer = view;
					break;

				default:
					var footerTemplate = ItemsView.FooterTemplate;
					if (footerTemplate != null)
					{
						ListViewBase.FooterTemplate = ItemsViewTemplate;
						ListViewBase.Footer = new ItemTemplateContext(footerTemplate, footer, Element, mauiContext: MauiContext);
					}
					else
					{
						ListViewBase.FooterTemplate = null;
						ListViewBase.Footer = null;
					}
					break;
			}
		}

		static ListViewBase CreateGridView(GridItemsLayout gridItemsLayout)
		{
			var gridView = new FormsGridView
			{
				Orientation = gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
					? Orientation.Horizontal
					: Orientation.Vertical,

				Span = gridItemsLayout.Span,
				ItemContainerStyle = GetItemContainerStyle(gridItemsLayout)
			};

			if (gridView.Orientation == Orientation.Horizontal)
			{
				ScrollViewer.SetVerticalScrollMode(gridView, WScrollMode.Disabled);
				ScrollViewer.SetHorizontalScrollMode(gridView, WScrollMode.Enabled);
			}

			return gridView;
		}

		static ListViewBase CreateVerticalListView(LinearItemsLayout listItemsLayout)
		{
			return new FormsListView()
			{
				ItemContainerStyle = GetVerticalItemContainerStyle(listItemsLayout)
			};
		}

		static ListViewBase CreateHorizontalListView(LinearItemsLayout listItemsLayout)
		{
			var horizontalListView = new FormsListView()
			{
				ItemsPanel = (ItemsPanelTemplate)WASDKApp.Current.Resources["HorizontalListItemsPanel"],
				ItemContainerStyle = GetHorizontalItemContainerStyle(listItemsLayout)
			};
			ScrollViewer.SetVerticalScrollBarVisibility(horizontalListView, Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Hidden);
			ScrollViewer.SetVerticalScrollMode(horizontalListView, WScrollMode.Disabled);
			ScrollViewer.SetHorizontalScrollMode(horizontalListView, WScrollMode.Auto);
			ScrollViewer.SetHorizontalScrollBarVisibility(horizontalListView, Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Auto);

			return horizontalListView;
		}

		static WStyle GetItemContainerStyle(GridItemsLayout layout)
		{
			var h = layout?.HorizontalItemSpacing ?? 0;
			var v = layout?.VerticalItemSpacing ?? 0;
			var margin = WinUIHelpers.CreateThickness(h, v, h, v);

			var style = new WStyle(typeof(GridViewItem));

			style.Setters.Add(new WSetter(FrameworkElement.MarginProperty, margin));
			style.Setters.Add(new WSetter(Control.PaddingProperty, WinUIHelpers.CreateThickness(0)));
			style.Setters.Add(new WSetter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));

			return style;
		}

		static WStyle GetVerticalItemContainerStyle(LinearItemsLayout layout)
		{
			var v = layout?.ItemSpacing ?? 0;
			var margin = WinUIHelpers.CreateThickness(0, v, 0, v);

			var style = new WStyle(typeof(ListViewItem));

			style.Setters.Add(new WSetter(FrameworkElement.MinHeightProperty, 0));
			style.Setters.Add(new WSetter(FrameworkElement.MarginProperty, margin));
			style.Setters.Add(new WSetter(Control.PaddingProperty, WinUIHelpers.CreateThickness(0)));
			style.Setters.Add(new WSetter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));

			return style;
		}

		static WStyle GetHorizontalItemContainerStyle(LinearItemsLayout layout)
		{
			var h = layout?.ItemSpacing ?? 0;
			var padding = WinUIHelpers.CreateThickness(h, 0, h, 0);

			var style = new WStyle(typeof(ListViewItem));

			style.Setters.Add(new WSetter(FrameworkElement.MinWidthProperty, 0));
			style.Setters.Add(new WSetter(Control.PaddingProperty, padding));
			style.Setters.Add(new WSetter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch));

			return style;
		}

		void UpdateItemsLayoutSpan()
		{
			if (ListViewBase is FormsGridView formsGridView)
			{
				formsGridView.Span = ((GridItemsLayout)Layout).Span;
			}
		}

		void UpdateItemsLayoutItemSpacing()
		{
			if (ListViewBase is FormsGridView formsGridView && Layout is GridItemsLayout gridLayout)
			{
				formsGridView.ItemContainerStyle = GetItemContainerStyle(gridLayout);
			}

			if (Layout is LinearItemsLayout linearItemsLayout)
			{
				switch (ListViewBase)
				{
					case FormsListView formsListView:
						formsListView.ItemContainerStyle = GetVerticalItemContainerStyle(linearItemsLayout);
						break;
					case WListView listView:
						listView.ItemContainerStyle = GetHorizontalItemContainerStyle(linearItemsLayout);
						break;
				}
			}
		}

		void UpdateItemSizingStrategy()
		{
			if (ListViewBase == null || ItemsView == null)
			{
				return;
			}

			// For Windows, ItemSizingStrategy.MeasureFirstItem needs special handling
			// We need to ensure all items have the same size based on the first item
			if (ItemsView.ItemSizingStrategy == ItemSizingStrategy.MeasureFirstItem)
			{
				// Force a uniform item container style that will make all items the same size
				SetUniformItemSizing();
			}
			else
			{
				// Allow items to size individually
				ClearUniformItemSizing();
			}
		}

		void SetUniformItemSizing()
		{
			if (ListViewBase == null || ItemsView?.ItemsLayout == null)
			{
				return;
			}

			// Apply uniform sizing based on the item layout type
			switch (ItemsView.ItemsLayout)
			{
				case GridItemsLayout gridLayout:
					if (ListViewBase is FormsGridView gridView)
					{
						var uniformStyle = GetUniformGridItemContainerStyle(gridLayout);
						gridView.ItemContainerStyle = uniformStyle;
					}
					break;

				case LinearItemsLayout linearLayout when linearLayout.Orientation == ItemsLayoutOrientation.Vertical:
					if (ListViewBase is FormsListView listView)
					{
						var uniformStyle = GetUniformVerticalItemContainerStyle(linearLayout);
						listView.ItemContainerStyle = uniformStyle;
					}
					break;

				case LinearItemsLayout linearLayout when linearLayout.Orientation == ItemsLayoutOrientation.Horizontal:
					var uniformHorizontalStyle = GetUniformHorizontalItemContainerStyle(linearLayout);
					ListViewBase.ItemContainerStyle = uniformHorizontalStyle;
					break;
			}
		}

		void ClearUniformItemSizing()
		{
			// Restore the default item container styles
			UpdateItemsLayoutItemSpacing();
		}

		(double width, double height) MeasureFirstItem()
		{
			// Default fallback sizes
			double defaultWidth = 0;
			double defaultHeight = 0;

			// Get the first item from the items source
			var itemsSource = ItemsView.ItemsSource;
			if (itemsSource == null)
			{
				return (defaultWidth, defaultHeight);
			}

			if (itemsSource is System.Collections.IEnumerable enumerable)
			{
				var enumerator = enumerable.GetEnumerator();
				if (enumerator.MoveNext())
				{
					var firstItem = enumerator.Current;

					if (ItemsView.ItemTemplate is not null)
					{                       // Create the actual view from the template
						var templateView = ItemsView.ItemTemplate.CreateContent() as View;
						if (templateView != null)
						{
							// Set the binding context
							templateView.BindingContext = firstItem;

							// Ensure the view has a handler by converting to platform
							var mauiContext = MauiContext ?? ItemsView.FindMauiContext();
							if (mauiContext != null)
							{
								var platformView = templateView.ToPlatform(mauiContext);

								// Measure the view using the MAUI measurement system
								var measuredSize = templateView.Measure(double.PositiveInfinity, double.PositiveInfinity);

								var measuredWidth = measuredSize.Width;
								var measuredHeight = measuredSize.Height;

								// Clean up the temporary view
								templateView.Handler?.DisconnectHandler();

								// Use measured size if valid, otherwise use defaults
								return (
									measuredWidth > 0 ? measuredWidth : defaultWidth,
									measuredHeight > 0 ? measuredHeight : defaultHeight
								);
							}
						}
					}
					else
					{
						// No item template - measure based on string representation
						var itemText = firstItem?.ToString() ?? string.Empty;

						// Create a temporary TextBlock to measure the item's string representation
						var textBlock = new TextBlock { Text = itemText };

						// Measure the TextBlock
						textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

						var measuredWidth = textBlock.DesiredSize.Width;
						var measuredHeight = textBlock.DesiredSize.Height;

						// Return measured size if valid, otherwise use defaults
						return (
							measuredWidth > 0 ? measuredWidth : defaultWidth,
							measuredHeight > 0 ? measuredHeight : defaultHeight
						);
					
				}
				}
			}

			return (defaultWidth, defaultHeight);
		}

		FrameworkElement CreateTempItemContainer()
		{
			switch (ItemsView.ItemsLayout)
			{
				case GridItemsLayout:
					return new GridViewItem();
				case LinearItemsLayout:
					return new ListViewItem();
				default:
					return null;
			}
		}

		WStyle GetUniformGridItemContainerStyle(GridItemsLayout layout)
		{
			var h = layout?.HorizontalItemSpacing ?? 0;
			var v = layout?.VerticalItemSpacing ?? 0;
			var margin = WinUIHelpers.CreateThickness(h, v, h, v);

			var style = new WStyle(typeof(GridViewItem));

			// Measure the first item to get uniform size
			var (measuredWidth, measuredHeight) = MeasureFirstItem();

			style.Setters.Add(new WSetter(FrameworkElement.WidthProperty, measuredWidth));
			style.Setters.Add(new WSetter(FrameworkElement.HeightProperty, measuredHeight));
			style.Setters.Add(new WSetter(FrameworkElement.MarginProperty, margin));
			style.Setters.Add(new WSetter(Control.PaddingProperty, WinUIHelpers.CreateThickness(0)));
			style.Setters.Add(new WSetter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
			style.Setters.Add(new WSetter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch));

			return style;
		}

		WStyle GetUniformVerticalItemContainerStyle(LinearItemsLayout layout)
		{
			var v = layout?.ItemSpacing ?? 0;
			var margin = WinUIHelpers.CreateThickness(0, v, 0, v);

			var style = new WStyle(typeof(ListViewItem));

			// Measure the first item to get uniform height
			var (_, measuredHeight) = MeasureFirstItem();

			style.Setters.Add(new WSetter(FrameworkElement.HeightProperty, measuredHeight));
			style.Setters.Add(new WSetter(FrameworkElement.MinHeightProperty, measuredHeight));
			style.Setters.Add(new WSetter(FrameworkElement.MarginProperty, margin));
			style.Setters.Add(new WSetter(Control.PaddingProperty, WinUIHelpers.CreateThickness(0)));
			style.Setters.Add(new WSetter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
			style.Setters.Add(new WSetter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch));

			return style;
		}

		WStyle GetUniformHorizontalItemContainerStyle(LinearItemsLayout layout)
		{
			var h = layout?.ItemSpacing ?? 0;
			var padding = WinUIHelpers.CreateThickness(h, 0, h, 0);

			var style = new WStyle(typeof(ListViewItem));

			// Measure the first item to get uniform width
			var (measuredWidth, _) = MeasureFirstItem();

			style.Setters.Add(new WSetter(FrameworkElement.WidthProperty, measuredWidth));
			style.Setters.Add(new WSetter(FrameworkElement.MinWidthProperty, measuredWidth));
			style.Setters.Add(new WSetter(Control.PaddingProperty, padding));
			style.Setters.Add(new WSetter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch));
			style.Setters.Add(new WSetter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));

			return style;
		}
	}
}