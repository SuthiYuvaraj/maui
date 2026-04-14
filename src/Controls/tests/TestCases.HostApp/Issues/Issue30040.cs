namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30040, "Keyboard navigation does not work with RadioButtons with redefined appearance", PlatformAffected.Android)]
public class Issue30040 : ContentPage
{
	Label _selectedLabel;

	public Issue30040()
	{
		_selectedLabel = new Label
		{
			AutomationId = "SelectedLabel",
			Text = "Selected: None"
		};

		// Use a simple ControlTemplate (just a ContentPresenter wrapped in a Border)
		// to reproduce the issue: keyboard navigation breaks when a ControlTemplate is applied.
		var controlTemplate = new ControlTemplate(() =>
		{
			var border = new Border
			{
				Stroke = Colors.Gray,
				StrokeThickness = 1,
				Padding = new Thickness(8)
			};
			border.Content = new ContentPresenter();
			return border;
		});

		var radio1 = CreateRadioButton("Option 1", "Radio1", controlTemplate);
		var radio2 = CreateRadioButton("Option 2", "Radio2", controlTemplate);
		var radio3 = CreateRadioButton("Option 3", "Radio3", controlTemplate);
		var radio4 = CreateRadioButton("Option 4", "Radio4", controlTemplate);

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 10,
			Children = { _selectedLabel, radio1, radio2, radio3, radio4 }
		};
	}

	RadioButton CreateRadioButton(string content, string automationId, ControlTemplate template)
	{
		var rb = new RadioButton
		{
			Content = content,
			AutomationId = automationId,
			GroupName = "TestGroup",
			ControlTemplate = template
		};
		rb.CheckedChanged += OnCheckedChanged;
		return rb;
	}

	void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && e.Value)
		{
			_selectedLabel.Text = $"Selected: {rb.Content}";
		}
	}
}
