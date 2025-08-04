#nullable disable
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal sealed class VerticalCell : WidthConstrainedTemplatedCell
	{
		public static NSString ReuseId = new NSString("Microsoft.Maui.Controls.VerticalCell");

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public VerticalCell(CGRect frame) : base(frame)
		{
		}

		public override CGSize Measure()
		{
			var measure = PlatformHandler.VirtualView.Measure(ConstrainedDimension, double.PositiveInfinity);

			// Check if the content is invisible and should collapse to zero
			if (PlatformHandler?.VirtualView is VisualElement visualElement && !visualElement.IsVisible)
			{
				return CGSize.Empty;
			}

			// Also check if the content has maximum constraints set to zero (from our VisualElement enhancement)
			if (PlatformHandler?.VirtualView is VisualElement ve && ve.MaximumHeightRequest == 0 && ve.MaximumWidthRequest == 0)
			{
				return CGSize.Empty;
			}

			return new CGSize(ConstrainedDimension, measure.Height);
		}

		protected override bool AttributesConsistentWithConstrainedDimension(UICollectionViewLayoutAttributes attributes)
		{
			return attributes.Frame.Width == ConstrainedDimension;
		}
	}
}