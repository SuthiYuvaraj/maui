namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 90955, "RadioButton CharacterSpacing Property Does Not Apply on Windows Platforms", PlatformAffected.UWP)]
public class Issue90955 : ContentPage
{
	public Issue90955()
	{
		var radioContent = "SpacedText";
		var label = new Label();
		var radioButton = new RadioButton { Content = radioContent, CharacterSpacing = 5, AutomationId = "Issue90955RadioButton" };
		label.AutomationId = "Issue90955Label";
		label.Text = $"Length of RadioButton Content: {radioContent.Length}";

		Content = new VerticalStackLayout
		{
			Children =
			{
				label,
				radioButton
			}
		};
	}
}
