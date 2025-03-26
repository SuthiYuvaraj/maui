using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28454 : _IssuesUITest
{
	public Issue28454(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Collectionview cell not expanding on label expanding to wrap in iOS";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewShouldExpandOnLabelTextWrap()
	{
		App.WaitForElement("Issue28454StackLayout");
		App.WaitForElement("Issue28454CollectionView");
		var BeforeRect = App.WaitForElement("Label2").GetRect();
		App.Tap("Issue28454Button");
		var AfterRect = App.WaitForElement("Label2").GetRect();
		Assert.That(BeforeRect.Height, Is.LessThan(AfterRect.Height), "Label height should be increased after button click");
	}
}