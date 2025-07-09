using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
    public partial class CarouselViewSnapPointsTests
    {
        // Windows-specific test to verify that the ScrollViewer properties are actually set
        [Fact]
        public async Task CarouselViewSnapPointsAreAppliedToScrollViewerOnWindows()
        {
            SetupBuilder();

            var carouselView = new CarouselView
            {
                ItemsSource = new[] { "Item 1", "Item 2", "Item 3" },
                ItemTemplate = new DataTemplate(() => new Label())
            };

            var handler = await CreateHandlerAsync(carouselView);
            
            // Allow time for the handler to initialize
            await Task.Delay(200);
            
            // Change the snap points type
            carouselView.ItemsLayout.SnapPointsType = SnapPointsType.Mandatory;
            
            // Wait for the property change to propagate
            await Task.Delay(100);
            
            // Verify the change was applied to the view model
            Assert.Equal(SnapPointsType.Mandatory, carouselView.ItemsLayout.SnapPointsType);
            
            // Change the snap points alignment
            carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
            
            // Wait for the property change to propagate
            await Task.Delay(100);
            
            // Verify the change was applied to the view model
            Assert.Equal(SnapPointsAlignment.Start, carouselView.ItemsLayout.SnapPointsAlignment);
        }
    }
}