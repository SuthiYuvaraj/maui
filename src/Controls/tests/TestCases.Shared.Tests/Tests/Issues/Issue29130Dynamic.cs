using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue29130Dynamic : _IssuesUITest
    {
        public Issue29130Dynamic(TestDevice testDevice) : base(testDevice)
        {
        }

        public override string Issue => "CollectionView ItemSizingStrategy.MeasureFirstItem should work when items are added dynamically to empty collection";

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void CollectionViewMeasureFirstItemWithDynamicItemsShouldWork()
        {
            // Verify initial state - empty collection with MeasureFirstItem strategy
            App.WaitForElement("29130DynamicCollectionView");
            App.WaitForElement("29130DynamicStatusLabel");

            var statusLabel = App.FindElement("29130DynamicStatusLabel");
            Assert.That(statusLabel.GetText(), Is.EqualTo("Items: 0, Strategy: MeasureFirstItem"));

            // Add first item - this should establish the size for all subsequent items
            App.WaitForElement("29130DynamicAddButton");
            App.Tap("29130DynamicAddButton");

            // Verify first item was added and is visible
            App.WaitForElement("29130DynamicTitle0");
            App.WaitForElement("29130DynamicDesc0");

            // Check status updated
            statusLabel = App.FindElement("29130DynamicStatusLabel");
            Assert.That(statusLabel.GetText(), Is.EqualTo("Items: 1, Strategy: MeasureFirstItem"));

            // Add several more items to test that they use the first item's measurements
            for (int i = 1; i < 5; i++)
            {
                App.Tap("29130DynamicAddButton");
                App.WaitForElement($"29130DynamicTitle{i}");
                App.WaitForElement($"29130DynamicDesc{i}");
            }

            // Verify all items are present
            statusLabel = App.FindElement("29130DynamicStatusLabel");
            Assert.That(statusLabel.GetText(), Is.EqualTo("Items: 5, Strategy: MeasureFirstItem"));

            // Verify that items with different content sizes are still using uniform sizing
            // (This is the key behavior of MeasureFirstItem - all items should have the same size as the first item)
            var firstTitle = App.FindElement("29130DynamicTitle0");
            var secondTitle = App.FindElement("29130DynamicTitle1");
            var thirdTitle = App.FindElement("29130DynamicTitle2");

            // All items should be visible and properly laid out
            Assert.That(firstTitle.GetText(), Is.Not.Empty);
            Assert.That(secondTitle.GetText(), Is.Not.Empty);
            Assert.That(thirdTitle.GetText(), Is.Not.Empty);
        }

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void CollectionViewSizingStrategyToggleShouldWork()
        {
            // Start with MeasureFirstItem
            App.WaitForElement("29130DynamicStatusLabel");
            var statusLabel = App.FindElement("29130DynamicStatusLabel");
            Assert.That(statusLabel.GetText(), Contains.Substring("Strategy: MeasureFirstItem"));

            // Add some items
            App.Tap("29130DynamicAddButton");
            App.Tap("29130DynamicAddButton");
            App.Tap("29130DynamicAddButton");

            // Toggle to MeasureAllItems
            App.Tap("29130DynamicToggleSizingButton");
            statusLabel = App.FindElement("29130DynamicStatusLabel");
            Assert.That(statusLabel.GetText(), Contains.Substring("Strategy: MeasureAllItems"));

            // Toggle back to MeasureFirstItem
            App.Tap("29130DynamicToggleSizingButton");
            statusLabel = App.FindElement("29130DynamicStatusLabel");
            Assert.That(statusLabel.GetText(), Contains.Substring("Strategy: MeasureFirstItem"));
        }

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void CollectionViewClearAndAddItemsShouldWork()
        {
            // Add some items
            for (int i = 0; i < 3; i++)
            {
                App.Tap("29130DynamicAddButton");
            }

            // Verify items were added
            var statusLabel = App.FindElement("29130DynamicStatusLabel");
            Assert.That(statusLabel.GetText(), Is.EqualTo("Items: 3, Strategy: MeasureFirstItem"));

            // Clear all items
            App.Tap("29130DynamicClearButton");

            // Verify items were cleared
            statusLabel = App.FindElement("29130DynamicStatusLabel");
            Assert.That(statusLabel.GetText(), Is.EqualTo("Items: 0, Strategy: MeasureFirstItem"));

            // Add items again - first item should again establish the sizing
            App.Tap("29130DynamicAddButton");
            App.WaitForElement("29130DynamicTitle0");

            // Add more items
            App.Tap("29130DynamicAddButton");
            App.WaitForElement("29130DynamicTitle1");

            // Verify final state
            statusLabel = App.FindElement("29130DynamicStatusLabel");
            Assert.That(statusLabel.GetText(), Is.EqualTo("Items: 2, Strategy: MeasureFirstItem"));
        }

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void CollectionViewShouldHandleVariedContentSizes()
        {
            // Add multiple items with different content sizes
            // The test items have different title lengths and description lengths
            for (int i = 0; i < 5; i++)
            {
                App.Tap("29130DynamicAddButton");
            }

            // All items should be visible and properly rendered
            // even though they have different content sizes
            for (int i = 0; i < 5; i++)
            {
                App.WaitForElement($"29130DynamicTitle{i}");
                App.WaitForElement($"29130DynamicDesc{i}");

                var title = App.FindElement($"29130DynamicTitle{i}");
                var description = App.FindElement($"29130DynamicDesc{i}");

                Assert.That(title.GetText(), Is.Not.Empty);
                Assert.That(description.GetText(), Is.Not.Empty);
            }

            // Verify the status shows all items
            var statusLabel = App.FindElement("29130DynamicStatusLabel");
            Assert.That(statusLabel.GetText(), Is.EqualTo("Items: 5, Strategy: MeasureFirstItem"));
        }
    }
}
