using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 12008, "CollectionView Drag and Drop Reordering Can't Drop in Empty Group", PlatformAffected.iOS)]
public class Issue12008 : ContentPage
{
	public Issue12008()
	{
		var groups = new ObservableCollection<Issue12008Group>
		{
			new Issue12008Group("Group 1")
			{
				new Issue12008Item("Item 1"),
				new Issue12008Item("Item 2"),
			},
			new Issue12008Group("Group 2"),
			new Issue12008Group("Group 3")
			{
				new Issue12008Item("Item 3"),
			},
		};

		var collectionView = new CollectionView
		{
			IsGrouped = true,
			CanReorderItems = true,
			CanMixGroups = true,
			AutomationId = "CollectionViewControl",
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var nameLabel = new Label
				{
					FontAttributes = FontAttributes.Bold,
					Padding = new Thickness(8, 4),
				};
				nameLabel.SetBinding(Label.TextProperty, new Binding("Name"));

				var countLabel = new Label
				{
					Padding = new Thickness(4),
				};
				countLabel.SetBinding(Label.TextProperty, new Binding("Count", stringFormat: "({0} items)"));

				return new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					BackgroundColor = Colors.LightGray,
					Children = { nameLabel, countLabel }
				};
			}),
			ItemTemplate = new DataTemplate(() =>
			{
				var itemLabel = new Label
				{
					Margin = new Thickness(4),
					VerticalTextAlignment = TextAlignment.Center,
				};
				itemLabel.SetBinding(Label.TextProperty, new Binding("Name"));
				itemLabel.SetBinding(Label.AutomationIdProperty, new Binding("Name"));

				return new Border
				{
					Padding = 0,
					Margin = new Thickness(12, 4, 12, 4),
					BackgroundColor = Colors.White,
					StrokeShape = new RoundRectangle { CornerRadius = 5 },
					Shadow = new Shadow { Brush = Brush.Black, Offset = new Point(2, 2), Opacity = 0.3f },
					Content = itemLabel
				};
			}),
			ItemsSource = groups
		};

		Content = collectionView;
	}
}

public class Issue12008Group : ObservableCollection<Issue12008Item>
{
	public string Name { get; }

	public Issue12008Group(string name)
	{
		Name = name;
	}
}

public class Issue12008Item
{
	public string Name { get; set; }

	public Issue12008Item(string name)
	{
		Name = name;
	}
}
