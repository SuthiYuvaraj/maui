using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 15, "CarouselView SnapPointsType not working on Windows", PlatformAffected.Windows)]
    public partial class CarouselViewSnapPointsTest : ContentPage
    {
        public CarouselViewSnapPointsTest()
        {
            InitializeComponent();
            
            // Create test data
            var items = new List<CarouselItemModel>
            {
                new CarouselItemModel { Name = "Item 1", Color = Colors.Red },
                new CarouselItemModel { Name = "Item 2", Color = Colors.Blue },
                new CarouselItemModel { Name = "Item 3", Color = Colors.Green },
                new CarouselItemModel { Name = "Item 4", Color = Colors.Orange },
                new CarouselItemModel { Name = "Item 5", Color = Colors.Purple },
                new CarouselItemModel { Name = "Item 6", Color = Colors.Brown },
                new CarouselItemModel { Name = "Item 7", Color = Colors.Pink },
                new CarouselItemModel { Name = "Item 8", Color = Colors.Teal },
                new CarouselItemModel { Name = "Item 9", Color = Colors.Indigo },
                new CarouselItemModel { Name = "Item 10", Color = Colors.Coral }
            };
            
            TestCarouselView.ItemsSource = items;
        }

        private void OnToggleSnapType(object sender, EventArgs e)
        {
            var currentType = ItemsLayout.SnapPointsType;
            
            switch (currentType)
            {
                case SnapPointsType.None:
                    ItemsLayout.SnapPointsType = SnapPointsType.Mandatory;
                    break;
                case SnapPointsType.Mandatory:
                    ItemsLayout.SnapPointsType = SnapPointsType.MandatorySingle;
                    break;
                case SnapPointsType.MandatorySingle:
                    ItemsLayout.SnapPointsType = SnapPointsType.None;
                    break;
            }
            
            SnapTypeLabel.Text = $"Current Snap Type: {ItemsLayout.SnapPointsType}";
        }

        private void OnToggleSnapAlignment(object sender, EventArgs e)
        {
            var currentAlignment = ItemsLayout.SnapPointsAlignment;
            
            switch (currentAlignment)
            {
                case SnapPointsAlignment.Start:
                    ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Center;
                    break;
                case SnapPointsAlignment.Center:
                    ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.End;
                    break;
                case SnapPointsAlignment.End:
                    ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
                    break;
            }
            
            SnapAlignmentLabel.Text = $"Current Snap Alignment: {ItemsLayout.SnapPointsAlignment}";
        }
    }

    public class CarouselItemModel
    {
        public string Name { get; set; }
        public Color Color { get; set; }
    }
}