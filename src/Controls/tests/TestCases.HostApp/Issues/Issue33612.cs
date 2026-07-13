using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33612, "Inconsistent Accessibility Behavior Across Platforms in MAUI", PlatformAffected.All)]
public class Issue33612 : ContentPage
{
	public Issue33612()
	{
		var rootStack = new StackLayout
		{
			Padding = 10,
			Spacing = 10
		};

		// Reproduces the issue: applying SemanticProperties on the parent container
		// (StackLayout) that hosts a BindableLayout collection. According to the issue,
		// this can prevent VoiceOver/child items from being individually focusable.
		SemanticProperties.SetDescription(rootStack, "List of selectable items");
		SemanticProperties.SetHint(rootStack, "Swipe to browse the items");

		var items = new List<Issue33612Item>();
		for (int i = 0; i < 5; i++)
		{
			items.Add(new Issue33612Item
			{
				Id = i,
				Title = $"Item {i}",
				Description = $"Description for item {i}",
				Hint = $"Double tap to select item {i}"
			});
		}

		var resultLabel = new Label
		{
			AutomationId = "ResultLabel",
			Text = "No item activated"
		};

		var itemsStack = new StackLayout
		{
			AutomationId = "ItemsStack",
			Spacing = 8
		};

		BindableLayout.SetItemsSource(itemsStack, items);
		BindableLayout.SetItemTemplate(itemsStack, new DataTemplate(() =>
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, new Binding(nameof(Issue33612Item.Title)));

			var border = new Border
			{
				Padding = 12,
				Content = label
			};

			// Each item exposes its own SemanticProperties so it should be
			// individually focusable/announced by screen readers, and the
			// TapGestureRecognizer should be exposed as an accessible action.
			border.SetBinding(SemanticProperties.DescriptionProperty, new Binding(nameof(Issue33612Item.Description)));
			border.SetBinding(SemanticProperties.HintProperty, new Binding(nameof(Issue33612Item.Hint)));
			border.SetBinding(AutomationProperties.NameProperty, new Binding(nameof(Issue33612Item.Title)));
			border.SetBinding(Element.AutomationIdProperty, new Binding(nameof(Issue33612Item.Id), stringFormat: "BorderItem{0}"));

			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += (sender, e) =>
			{
				if (sender is Border tappedBorder && tappedBorder.BindingContext is Issue33612Item tappedItem)
				{
					resultLabel.Text = $"Activated: {tappedItem.Title}";
				}
			};
			border.GestureRecognizers.Add(tapGestureRecognizer);

			return border;
		}));

		rootStack.Children.Add(new Label
		{
			Text = "BindableLayout accessibility repro (Issue 33612)",
			AutomationId = "HeaderLabel"
		});
		rootStack.Children.Add(itemsStack);
		rootStack.Children.Add(resultLabel);

		Content = new ScrollView
		{
			Content = rootStack
		};
	}

	class Issue33612Item
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Hint { get; set; } = string.Empty;
	}
}
