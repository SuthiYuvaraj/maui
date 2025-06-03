using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
    internal static class CollectionViewMeasurementCache
    {
        internal static readonly BindableProperty FirstItemMeasuredSizeProperty =
            BindableProperty.CreateAttached(
                "FirstItemMeasuredSize",
                typeof(Size),
                typeof(CollectionViewMeasurementCache),
                Size.Zero);

        internal static void SetFirstItemMeasuredSize(BindableObject element, Size value)
            => element.SetValue(FirstItemMeasuredSizeProperty, value);

        internal static Size GetFirstItemMeasuredSize(BindableObject element)
            => (Size)element.GetValue(FirstItemMeasuredSizeProperty);
    }
}
