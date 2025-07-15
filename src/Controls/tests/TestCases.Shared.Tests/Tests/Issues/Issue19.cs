using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19 : _IssuesUITest
	{
		public Issue19(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Android - Dynamic Updates to CollectionView Header/Footer and Templates Are Not Displayed";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void DynamicHeaderUpdateShouldBeDisplayed()
		{
			App.WaitForElement("UpdateHeaderButton");
			
			// Initial header should be displayed
			App.WaitForElement("Initial Header");
			
			// Update header and verify it changes
			App.Tap("UpdateHeaderButton");
			App.WaitForElement("Updated Header 1");
			
			// Update again and verify it changes
			App.Tap("UpdateHeaderButton");
			App.WaitForElement("Updated Header 2");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void DynamicFooterUpdateShouldBeDisplayed()
		{
			App.WaitForElement("UpdateFooterButton");
			
			// Initial footer should be displayed
			App.WaitForElement("Initial Footer");
			
			// Update footer and verify it changes
			App.Tap("UpdateFooterButton");
			App.WaitForElement("Updated Footer 1");
			
			// Update again and verify it changes
			App.Tap("UpdateFooterButton");
			App.WaitForElement("Updated Footer 2");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void DynamicHeaderTemplateUpdateShouldBeDisplayed()
		{
			App.WaitForElement("UpdateHeaderTemplateButton");
			
			// Initial header template should be displayed
			App.WaitForElement("HeaderTemplate_Initial_Header_Template");
			
			// Update header template and verify it changes
			App.Tap("UpdateHeaderTemplateButton");
			App.WaitForElement("HeaderTemplate_Updated_Header_Template_1");
			
			// Update again and verify it changes
			App.Tap("UpdateHeaderTemplateButton");
			App.WaitForElement("HeaderTemplate_Updated_Header_Template_2");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void DynamicFooterTemplateUpdateShouldBeDisplayed()
		{
			App.WaitForElement("UpdateFooterTemplateButton");
			
			// Initial footer template should be displayed
			App.WaitForElement("FooterTemplate_Initial_Footer_Template");
			
			// Update footer template and verify it changes
			App.Tap("UpdateFooterTemplateButton");
			App.WaitForElement("FooterTemplate_Updated_Footer_Template_1");
			
			// Update again and verify it changes
			App.Tap("UpdateFooterTemplateButton");
			App.WaitForElement("FooterTemplate_Updated_Footer_Template_2");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void DynamicGroupTemplateUpdateShouldBeDisplayed()
		{
			App.WaitForElement("UpdateGroupTemplatesButton");
			
			// Update group templates
			App.Tap("UpdateGroupTemplatesButton");
			
			// Group header and footer templates should be displayed
			App.WaitForElement("HeaderTemplate_Updated_Group_Header_Template_1");
			App.WaitForElement("FooterTemplate_Updated_Group_Footer_Template_1");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HeaderAndGroupHeaderShouldNotSynchronize()
		{
			App.WaitForElement("UpdateHeaderButton");
			App.WaitForElement("UpdateGroupTemplatesButton");
			
			// Initial state should have normal header
			App.WaitForElement("Initial Header");
			
			// Update to grouped mode with group templates
			App.Tap("UpdateGroupTemplatesButton");
			App.WaitForElement("HeaderTemplate_Updated_Group_Header_Template_1");
			
			// Update the normal header - this should not affect group header
			App.Tap("UpdateHeaderButton");
			App.WaitForElement("Updated Header 1");
			
			// Group header should still be there and unchanged
			App.WaitForElement("HeaderTemplate_Updated_Group_Header_Template_1");
		}
	}
}