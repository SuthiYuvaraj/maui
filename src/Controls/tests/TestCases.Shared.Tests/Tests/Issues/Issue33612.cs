using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33612 : _IssuesUITest
{
	public Issue33612(TestDevice device) : base(device) { }

	public override string Issue => "Inconsistent Accessibility Behavior Across Platforms in MAUI";

	[Test]
	[Category(UITestCategories.Accessibility)]
	public void BindableLayoutItemsAreIndividuallyAccessibleWhenParentHasSemantics()
	{
		App.WaitForElement("ItemsStack");

		// Each item rendered by the BindableLayout (Border + Label + TapGestureRecognizer)
		// must be exposed as its own accessible element, with its own accessible name/
		// description, even though the parent StackLayout also has SemanticProperties set.
		for (int i = 0; i < 5; i++)
		{
			var itemId = $"BorderItem{i}";
			App.WaitForElement(itemId);

			var element = App.FindElement(itemId);

#if ANDROID
			var contentDescription = element.GetAttribute<string>("content-desc");
#elif IOS || MACCATALYST
			var contentDescription = element.GetAttribute<string>("label");
#elif WINDOWS
			var contentDescription = element.GetAttribute<string>("Name");
#else
			var contentDescription = element.GetAttribute<string>("name");
#endif

			Assert.That(contentDescription, Does.Contain($"Description for item {i}"),
				$"Item {i} should expose its own accessible description distinct from the parent's semantics.");
		}
	}

	[Test]
	[Category(UITestCategories.Accessibility)]
	public void TappingBindableLayoutItemActivatesItsGestureRecognizer()
	{
		App.WaitForElement("ItemsStack");
		App.WaitForElement("BorderItem2");

		App.Tap("BorderItem2");

		var resultText = App.WaitForElement("ResultLabel").GetText();
		Assert.That(resultText, Is.EqualTo("Activated: Item 2"),
			"Tapping/activating a BindableLayout item's Border should trigger its TapGestureRecognizer.");
	}
}
