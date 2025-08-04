#nullable disable
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal sealed class HorizontalSupplementaryView : HeightConstrainedTemplatedCell
	{
		public static NSString ReuseId = new NSString("Microsoft.Maui.Controls.HorizontalSupplementaryView");

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public HorizontalSupplementaryView(CGRect frame) : base(frame)
		{
		}

		public override CGSize Measure()
		{
			if (PlatformHandler?.VirtualView == null)
			{
				return CGSize.Empty;
			}

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

			var measure = PlatformHandler.VirtualView.Measure(double.PositiveInfinity, ConstrainedDimension);

			var width = PlatformHandler.VirtualView.Width > 0
				? PlatformHandler.VirtualView.Width : measure.Width;

			return new CGSize(width, ConstrainedDimension);
		}

		protected override bool AttributesConsistentWithConstrainedDimension(UICollectionViewLayoutAttributes attributes)
		{
			return attributes.Frame.Height == ConstrainedDimension;
		}
	}
}