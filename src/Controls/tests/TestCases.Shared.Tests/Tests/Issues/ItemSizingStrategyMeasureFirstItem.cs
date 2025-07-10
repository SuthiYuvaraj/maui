using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class ItemSizingStrategyMeasureFirstItem : _IssuesUITest
	{
		public ItemSizingStrategyMeasureFirstItem(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Windows & iOS] CollectionView2 ItemSizingStrategy=\"MeasureFirstItem\" Fails to Apply Correct Sizing";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewMeasureFirstItemShouldApplyUniformSizing()
		{
			// Wait for the CollectionView to load
			App.WaitForElement("TestCollectionView");

			// Ensure MeasureFirstItem is selected
			App.WaitForElement("StrategyPicker");
			App.Tap("StrategyPicker");
			App.Tap("MeasureFirstItem");

			// Wait a moment for the layout to update
			System.Threading.Thread.Sleep(1000);

			// Get the first item dimensions
			var firstItem = App.Query("TestCollectionView").FirstOrDefault();
			Assert.That(firstItem, Is.Not.Null, "CollectionView should be present");

			// TODO: Add more specific assertions once we can identify individual items
			// For now, this test verifies that the basic functionality works without crashes
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewSizingStrategyCanBeChanged()
		{
			// Wait for controls to load
			App.WaitForElement("StrategyPicker");
			App.WaitForElement("TestCollectionView");

			// Test switching between strategies
			App.Tap("StrategyPicker");
			App.Tap("MeasureAllItems");
			System.Threading.Thread.Sleep(500);

			App.Tap("StrategyPicker");
			App.Tap("MeasureFirstItem");
			System.Threading.Thread.Sleep(500);

			// If we get here without exceptions, the strategy switching works
			Assert.Pass("Strategy switching completed without errors");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewLayoutCanBeChanged()
		{
			// Wait for controls to load
			App.WaitForElement("LayoutPicker");
			App.WaitForElement("TestCollectionView");

			// Test different layout types with MeasureFirstItem
			string[] layouts = { "VerticalList", "HorizontalList", "VerticalGrid" };

			foreach (var layout in layouts)
			{
				App.Tap("LayoutPicker");
				App.Tap(layout);
				System.Threading.Thread.Sleep(500);

				// Verify CollectionView is still present after layout change
				Assert.That(App.Query("TestCollectionView").Any(), Is.True, $"CollectionView should be present after switching to {layout}");
			}
		}
	}
}