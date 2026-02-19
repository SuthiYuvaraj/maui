using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
    public partial class CarouselViewSnapPointsTests : ControlsHandlerTestBase
    {
        void SetupBuilder()
        {
            EnsureHandlerCreated(builder =>
            {
                builder.ConfigureMauiHandlers(handlers =>
                {
                    handlers.AddHandler<CarouselView, CarouselViewHandler>();
                    handlers.AddHandler<IContentView, ContentViewHandler>();
                    handlers.AddHandler<Grid, LayoutHandler>();
                    handlers.AddHandler<Label, LabelHandler>();
                });
            });
        }

        [Fact]
        public async Task CarouselViewSnapPointsTypeCanBeUpdatedAtRuntime()
        {
            SetupBuilder();

            var carouselView = new CarouselView
            {
                ItemsSource = new[] { "Item 1", "Item 2", "Item 3" },
                ItemTemplate = new DataTemplate(() => new Label())
            };

            var handler = await CreateHandlerAsync(carouselView);
            
            // Check initial state
            Assert.Equal(SnapPointsType.MandatorySingle, carouselView.ItemsLayout.SnapPointsType);
            
            // Change the snap points type
            carouselView.ItemsLayout.SnapPointsType = SnapPointsType.Mandatory;
            
            // Wait for the property change to propagate
            await Task.Delay(100);
            
            // Verify the change was applied
            Assert.Equal(SnapPointsType.Mandatory, carouselView.ItemsLayout.SnapPointsType);
            
            // Change to None
            carouselView.ItemsLayout.SnapPointsType = SnapPointsType.None;
            
            // Wait for the property change to propagate
            await Task.Delay(100);
            
            // Verify the change was applied
            Assert.Equal(SnapPointsType.None, carouselView.ItemsLayout.SnapPointsType);
        }

        [Fact]
        public async Task CarouselViewSnapPointsAlignmentCanBeUpdatedAtRuntime()
        {
            SetupBuilder();

            var carouselView = new CarouselView
            {
                ItemsSource = new[] { "Item 1", "Item 2", "Item 3" },
                ItemTemplate = new DataTemplate(() => new Label())
            };

            var handler = await CreateHandlerAsync(carouselView);
            
            // Check initial state
            Assert.Equal(SnapPointsAlignment.Center, carouselView.ItemsLayout.SnapPointsAlignment);
            
            // Change the snap points alignment
            carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
            
            // Wait for the property change to propagate
            await Task.Delay(100);
            
            // Verify the change was applied
            Assert.Equal(SnapPointsAlignment.Start, carouselView.ItemsLayout.SnapPointsAlignment);
            
            // Change to End
            carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.End;
            
            // Wait for the property change to propagate
            await Task.Delay(100);
            
            // Verify the change was applied
            Assert.Equal(SnapPointsAlignment.End, carouselView.ItemsLayout.SnapPointsAlignment);
        }

        [Fact]
        public async Task CarouselViewSnapPointsHandlerShouldNotCrashWhenUpdatingProperties()
        {
            SetupBuilder();

            var carouselView = new CarouselView
            {
                ItemsSource = new[] { "Item 1", "Item 2", "Item 3" },
                ItemTemplate = new DataTemplate(() => new Label())
            };

            var handler = await CreateHandlerAsync(carouselView);
            
            // This should not crash
            carouselView.ItemsLayout.SnapPointsType = SnapPointsType.Mandatory;
            carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
            
            // Wait for any background processing
            await Task.Delay(100);
            
            // Rapid changes should also not crash
            for (int i = 0; i < 10; i++)
            {
                carouselView.ItemsLayout.SnapPointsType = (SnapPointsType)(i % 3);
                carouselView.ItemsLayout.SnapPointsAlignment = (SnapPointsAlignment)(i % 3);
                await Task.Delay(10);
            }
        }
    }
}