#nullable disable
using System;
using Android.Content;
using Android.Views;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class ItemContentView : ViewGroup
	{
		Size? _pixelSize;
		WeakReference _reportMeasure;
		WeakReference _retrieveStaticSize;
		int _previousPixelWidth = -1;
		int _previousPixelHeight = -1;

		Action<Size> ReportMeasure
		{
			get => _reportMeasure?.Target as Action<Size>;
		}

		protected IPlatformViewHandler Content;
		internal IView View => Content?.VirtualView;

		public ItemContentView(Context context) : base(context)
		{
		}

		internal void ClickOn() => CallOnClick();

		AView PlatformView => Content?.ContainerView ?? Content?.PlatformView;

		internal Func<Size?> RetrieveStaticSize
		{
			get => _retrieveStaticSize?.Target as Func<Size?>;
			set => _retrieveStaticSize = new WeakReference(value);
		}

		internal void RealizeContent(View view, ItemsView itemsView)
		{
			Content = CreateHandler(view, itemsView);
			var platformView = PlatformView;

			//make sure we don't belong to a previous Holder
			platformView.RemoveFromParent();
			AddView(platformView);

			if (View is VisualElement visualElement)
			{
				visualElement.MeasureInvalidated += ElementMeasureInvalidated;
			}
		}

		internal void Recycle()
		{
			if (View is VisualElement visualElement)
			{
				visualElement.MeasureInvalidated -= ElementMeasureInvalidated;
			}

			var platformView = PlatformView;

			if (platformView != null)
			{
				RemoveView(platformView);
			}

			Content = null;
			_pixelSize = null;
			_reportMeasure = null;
			_previousPixelWidth = -1;
			_previousPixelHeight = -1;
		}

		internal void HandleItemSizingStrategy(Action<Size> reportMeasure, Size? size)
		{
			_reportMeasure = new WeakReference(reportMeasure);
			_pixelSize = size;
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (Content == null)
			{
				return;
			}

			if (View?.Handler is IPlatformViewHandler handler)
			{
				handler.LayoutVirtualView(l, t, r, b);
			}

		void ForceRemeasureHierarchy(IView view)
		{
			if (view is VisualElement visualElement)
			{
				// Force invalidate and batch the remeasure
				visualElement.InvalidateMeasure();

				// Use dispatcher to ensure measure happens after current layout cycle
				visualElement.Dispatcher.Dispatch(() =>
				{
					visualElement.InvalidateMeasure();
				});

				// Handle ContentView with nested content
				if (visualElement is ContentView contentView && contentView.Content != null)
				{
					ForceRemeasureHierarchy(contentView.Content);
				}

				// Handle Layout with children
				if (visualElement is Layout layout)
				{
					foreach (var child in layout.Children)
					{
						if (child is IView childView)
						{
							ForceRemeasureHierarchy(childView);
						}
					}
				}

				// Handle Grid specifically (since your progress bar uses Grid)
				if (visualElement is Grid grid)
				{
					foreach (var child in grid.Children)
					{
						if (child is IView childView)
						{
							ForceRemeasureHierarchy(childView);
						}
					}
				}
			}
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (Content == null)
			{
				SetMeasuredDimension(0, 0);
				return;
			}

			int pixelWidth = MeasureSpec.GetSize(widthMeasureSpec);
			int pixelHeight = MeasureSpec.GetSize(heightMeasureSpec);
			var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
			var heightMode = MeasureSpec.GetMode(heightMeasureSpec);

			// This checks to see if a static size has been set already on the adapter
			// The problem is that HandleItemSizingStrategy(Action<Size> reportMeasure, Size? size) 
			// is called during "Bind" and then measure is called on all visible cells after that
			// the result of this is that every single cell does an individual measure opposed to just using
			// the first cell to set the size.

			var possibleNewSize = RetrieveStaticSize?.Invoke();

			// This means a different cell has already set our new size
			// so let's just use that instead of perform our own speculative measure
			if (possibleNewSize is not null &&
				_pixelSize is not null &&
				!_pixelSize.Equals(possibleNewSize))
			{
				_pixelSize = possibleNewSize;
			}
			else
			{
				_pixelSize = _pixelSize ?? possibleNewSize;

				// If the measure changes significantly, we need to invalidate the pixel size
				// This will happen if the user rotates the device or even just changes the height/width
				// on the CollectionView itself. 
				// The Abs comparison I think is currently just a workaround for this
				// https://github.com/dotnet/maui/issues/22271
				// Once we fix 22271 I don't think we need this
				// I'm also curious if we would still need this https://github.com/dotnet/maui/pull/21140
				if (pixelWidth != 0 && Math.Abs(_previousPixelWidth - pixelWidth) > 1)
				{
					// We only need to worry about clearing pixel size if we've
					// already made a first pass
					if (_previousPixelWidth != -1)
					{
						_pixelSize = null;
					}
				}


				if (pixelHeight != 0 && Math.Abs(_previousPixelHeight - pixelHeight) > 1)
				{
					// We only need to worry about clearing pixel size if we've
					// already made a first pass
					if (_previousPixelHeight != -1)
					{
						_pixelSize = null;
					}
				}
			}

			if (pixelWidth != 0)
			{
				_previousPixelWidth = pixelWidth;
			}

			if (pixelHeight != 0)
			{
				_previousPixelHeight = pixelHeight;
			}

			// If we're using ItemSizingStrategy.MeasureFirstItem and now we have a set size, use that
			// Even though we already know the size we still need to pass the measure through to the children.

			// Replace the _pixelSize handling section:

			if (_pixelSize is not null)
			{
				var pixelWidthValue = _pixelSize.Value.Width;
				var pixelHeightValue = _pixelSize.Value.Height;

				// CRITICAL: Set the frame BEFORE any measurement to ensure children have access to parent size
				var newCurrentFrame = View.Frame;
				var newFrameCurrent = new Graphics.Rect(newCurrentFrame.X, newCurrentFrame.Y, pixelWidthValue, pixelHeightValue);
				if (newCurrentFrame != newFrameCurrent)
				{
					View.Frame = newFrameCurrent;
				}

				// Perform a full measure without constraints to allow children to recalculate
				var measureSpec = View.Measure(pixelWidthValue, pixelHeightValue);

				// Force remeasurement of the entire hierarchy
				//ForceRemeasureHierarchy(View);

				// Also call the platform handler measure to ensure platform-specific updates
				_ = (View.Handler as IPlatformViewHandler)?.MeasureVirtualView(
						MeasureSpec.MakeMeasureSpec((int)pixelWidthValue, MeasureSpecMode.Exactly),
						MeasureSpec.MakeMeasureSpec((int)pixelHeightValue, MeasureSpecMode.Exactly)
					);

				SetMeasuredDimension((int)pixelWidthValue, (int)pixelHeightValue);
				return;
			}

			var width = widthMode == MeasureSpecMode.Unspecified
				? double.PositiveInfinity
				: this.FromPixels(pixelWidth);

			var height = heightMode == MeasureSpecMode.Unspecified
				? double.PositiveInfinity
				: this.FromPixels(pixelHeight);

			var measure = View.Measure(width, height);

			if (pixelWidth == 0)
			{
				pixelWidth = (int)this.ToPixels(measure.Width);
			}

			if (pixelHeight == 0)
			{
				pixelHeight = (int)this.ToPixels(measure.Height);
			}

			ReportMeasure?.Invoke(new Size(pixelWidth, pixelHeight));

			SetMeasuredDimension(pixelWidth, pixelHeight);
		}

		void ElementMeasureInvalidated(object sender, EventArgs e)
		{
			if (this.IsAlive())
			{
				PlatformInterop.RequestLayoutIfNeeded(this);
			}
			else if (sender is VisualElement ve)
			{
				ve.MeasureInvalidated -= ElementMeasureInvalidated;
			}
		}

		static IPlatformViewHandler CreateHandler(View view, ItemsView itemsView) =>
			TemplateHelpers.GetHandler(view, itemsView.FindMauiContext());
	}
}
