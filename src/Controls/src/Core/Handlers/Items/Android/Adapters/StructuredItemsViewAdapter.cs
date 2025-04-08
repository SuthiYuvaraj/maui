﻿#nullable disable
using System;
using System.ComponentModel;
using Android.Content;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Graphics;
using ViewGroup = Android.Views.ViewGroup;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class StructuredItemsViewAdapter<TItemsView, TItemsViewSource> : ItemsViewAdapter<TItemsView, TItemsViewSource>
		where TItemsView : StructuredItemsView
		where TItemsViewSource : IItemsViewSource
	{
		Size? _size;

		// I'm storing this here because in the children I'm using a weakreference and
		// I don't want this action to get GC'd
		Action<Size> _reportMeasure;
		Func<Size?> _retrieveStaticSize;

		protected internal StructuredItemsViewAdapter(TItemsView itemsView,
			Func<View, Context, ItemContentView> createItemContentView = null) : base(itemsView, createItemContentView)
		{
			_reportMeasure = SetStaticSize;
			_retrieveStaticSize = () => _size ?? null;

			UpdateHasHeader();
			UpdateHasFooter();
		}

		protected override void ItemsViewPropertyChanged(object sender, PropertyChangedEventArgs property)
		{
			base.ItemsViewPropertyChanged(sender, property);

			if (property.Is(Microsoft.Maui.Controls.StructuredItemsView.HeaderProperty) || property.Is(Microsoft.Maui.Controls.StructuredItemsView.HeaderTemplateProperty))
			{
				UpdateHasHeader();
				NotifyDataSetChanged();
			}
			else if (property.Is(Microsoft.Maui.Controls.StructuredItemsView.FooterProperty) || property.Is(Microsoft.Maui.Controls.StructuredItemsView.FooterTemplateProperty))
			{
				UpdateHasFooter();
				NotifyDataSetChanged();
			}
		}

		public override int GetItemViewType(int position)
		{
			if (IsHeader(position))
			{
				return ItemViewType.Header;
			}

			if (IsFooter(position))
			{
				return ItemViewType.Footer;
			}

			return base.GetItemViewType(position);
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var context = parent.Context;

			if (viewType == ItemViewType.Header)
			{
				return CreateHeaderFooterViewHolder(ItemsView.Header, ItemsView.HeaderTemplate, context);
			}

			if (viewType == ItemViewType.Footer)
			{
				return CreateHeaderFooterViewHolder(ItemsView.Footer, ItemsView.FooterTemplate, context);
			}

			return base.OnCreateViewHolder(parent, viewType);
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			if (IsHeader(position))
			{
				if (holder is TemplatedItemViewHolder templatedItemViewHolder)
				{
					BindTemplatedItemViewHolder(templatedItemViewHolder, ItemsView.HeaderTemplate);
				}
				else if (holder is SimpleViewHolder simpleViewHolder)
				{
					// Check if the ItemView is a TextView and update its text
					if (simpleViewHolder.ItemView is Android.Widget.TextView textView && ItemsView.Header is not null)
					{
						textView.Text = ItemsView.Header.ToString(); // Replace "Updated Text" with the desired runtime value
					}
					else
					{
						simpleViewHolder.ItemView = CreateHeaderFooterViewHolder(ItemsView.Header, ItemsView.HeaderTemplate, holder.ItemView.Context).ItemView;
					}
				}
				return;
			}

			if (IsFooter(position))
			{
				if (holder is TemplatedItemViewHolder templatedItemViewHolder)
				{
					BindTemplatedItemViewHolder(templatedItemViewHolder, ItemsView.Footer);
				}
				else if (holder is SimpleViewHolder simpleViewHolder)
				{
					// Check if the ItemView is a TextView and update its text
					if (simpleViewHolder.ItemView is Android.Widget.TextView textView)
					{
						textView.Text = ItemsView.Footer.ToString(); // Replace "Updated Text" with the desired runtime value
					}
					else
					{
						simpleViewHolder.ItemView = CreateHeaderFooterViewHolder(ItemsView.Footer, ItemsView.FooterTemplate, holder.ItemView.Context).ItemView;
					}
				}

				return;
			}

			base.OnBindViewHolder(holder, position);
		}

		protected override void BindTemplatedItemViewHolder(TemplatedItemViewHolder templatedItemViewHolder, object context)
		{
			TemplatedItemViewHolder viewHolder;
			if (ItemsView.ItemSizingStrategy == ItemSizingStrategy.MeasureFirstItem)
			{
				templatedItemViewHolder.Bind(context, ItemsView, _reportMeasure, _size);

				if (templatedItemViewHolder.ItemView is ItemContentView itemContentView)
				{
					itemContentView.RetrieveStaticSize = _retrieveStaticSize;
				}
			}
			else
			{
				if (context == ItemsView.HeaderTemplate || context == ItemsView.FooterTemplate)
				{
					viewHolder = new TemplatedItemViewHolder(templatedItemViewHolder.ItemView as ItemContentView, ItemsView.HeaderTemplate, isSelectionEnabled: false);
				}
				else
				{
					viewHolder = templatedItemViewHolder;
				}


				base.BindTemplatedItemViewHolder(viewHolder, context);
			}
		}

		void UpdateHasHeader()
		{
			ItemsSource.HasHeader = (ItemsView.Header ?? ItemsView.HeaderTemplate) is not null;
		}

		void UpdateHasFooter()
		{
			ItemsSource.HasFooter = (ItemsView.Footer ?? ItemsView.FooterTemplate) is not null;
		}

		bool IsHeader(int position)
		{
			return ItemsSource.IsHeader(position);
		}

		bool IsFooter(int position)
		{
			return ItemsSource.IsFooter(position);
		}

		protected RecyclerView.ViewHolder CreateHeaderFooterViewHolder(object content, DataTemplate template, Context context)
		{
			if (template != null)
			{
				var footerContentView = new ItemContentView(context);
				return new TemplatedItemViewHolder(footerContentView, template, isSelectionEnabled: false);
			}

			if (content is View formsView)
			{
				var viewHolder = SimpleViewHolder.FromFormsView(formsView, context, ItemsView);

				// Propagate the binding context, visual, etc. from the ItemsView to the header/footer
				if (viewHolder.View.Parent != ItemsView)
				{
					ItemsView.AddLogicalChild(viewHolder.View);
				}

				return viewHolder;
			}

			// No template, Footer is not a Forms View, so just display Footer.ToString
			return SimpleViewHolder.FromText(content?.ToString(), context, fill: false);
		}

		void SetStaticSize(Size size)
		{
			_size = size;
		}
	}
}
