#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID
// https://github.com/dotnet/maui/issues/12008 CollectionView drag-and-drop to empty groups is only implemented on iOS/MacCatalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12008 : _IssuesUITest
{
	public Issue12008(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView Drag and Drop Reordering Can't Drop in Empty Group";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CanDropItemIntoEmptyGroup()
	{
		App.WaitForElement("CollectionViewControl");
		App.DragAndDrop("Item 1", "Item 3");
		VerifyScreenshot();
	}
}
#endif
