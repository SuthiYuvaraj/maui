using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
#pragma warning disable RS0016 // Add public types and members to the declared API
    public static class CollectionViewMeasurementCache
#pragma warning restore RS0016 // Add public types and members to the declared API
    {
#pragma warning disable RS0016 // Add public types and members to the declared API
        public static readonly BindableProperty FirstItemMeasuredSizeProperty =
#pragma warning restore RS0016 // Add public types and members to the declared API
            BindableProperty.CreateAttached(
                "FirstItemMeasuredSize",
                typeof(Size),
                typeof(CollectionViewMeasurementCache),
                Size.Zero);

#pragma warning disable RS0016 // Add public types and members to the declared API
        public static void SetFirstItemMeasuredSize(BindableObject element, Size value)
#pragma warning restore RS0016 // Add public types and members to the declared API
            => element.SetValue(FirstItemMeasuredSizeProperty, value);

#pragma warning disable RS0016 // Add public types and members to the declared API
        public static Size GetFirstItemMeasuredSize(BindableObject element)
#pragma warning restore RS0016 // Add public types and members to the declared API
            => (Size)element.GetValue(FirstItemMeasuredSizeProperty);
    }
}
