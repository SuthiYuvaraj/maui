#if TEST_FAILS_ON_WINDOWS //ItemSizingStrategy not working in Windows, Issue:https://github.com/dotnet/maui/issues/29130
using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue29130 : _IssuesUITest
	{
		public override string Issue => "CollectionView2 ItemSizingStrategy should work for MeasureFirstItem";
		public Issue29130(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ItemSizeShouldRespondForItemSizingStrategy()
		{
			// Is a iOS issue; see https://github.com/dotnet/maui/issues/29130
			App.WaitForElement("29130Grid");
			var cv = App.WaitForElement("29130CollectionView");
			var cvHeight = cv.GetRect();
			App.WaitForElement("29130MeasureAllItemsButton");
			App.Tap("29130MeasureAllItemsButton");
			var cv2Height = cv.GetRect();
			Assert.That(cv2Height.Height > cvHeight.Height, "CollectionView height should be greater after changing ItemSizingStrategy to MeasureAllItems");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ItemTemplateShouldUpdateDynamicallyWithCacheClear()
		{
			// Test that changing ItemTemplate dynamically works correctly with caching
			App.WaitForElement("29130Grid");
			var cv = App.WaitForElement("29130CollectionView");
			
			// Start with MeasureFirstItem to enable caching
			App.WaitForElement("29130MeasureFirstItemButton");
			App.Tap("29130MeasureFirstItemButton");
			
			// Get initial dimensions with simple template
			var initialRect = cv.GetRect();
			
			// Change to alternate template (which should be larger due to border/padding)
			App.WaitForElement("29130ChangeTemplateButton");
			App.Tap("29130ChangeTemplateButton");
			
			// Verify the CollectionView layout updated after template change
			var afterTemplateChangeRect = cv.GetRect();
			Assert.That(afterTemplateChangeRect.Height > initialRect.Height, 
				"CollectionView height should increase after changing to bordered template");
			
			// Change back to original template
			App.Tap("29130ChangeTemplateButton");
			
			// Verify it shrinks back to approximate original size (allowing some tolerance)
			var backToOriginalRect = cv.GetRect();
			Assert.That(Math.Abs(backToOriginalRect.Height - initialRect.Height) < 20, 
				"CollectionView height should return close to original after changing back to simple template");
		}
	}
}
#endif
