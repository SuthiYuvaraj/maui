namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31627, "CollectionView throws LayoutCycleException on Windows", PlatformAffected.UWP)]
public class Issue31627 : ContentPage
{
	// These constants mirror the public repro (jrlaff/LayoutCycleExceptionRepro1):
	// several "screens" (Grids), each containing many ContentViews, each of which
	// hosts a CollectionView with a handful of items. High element counts combined
	// with CollectionView layout are required to trigger the layout cycle on Windows.
	const int GridCount = 5;
	const int ContentViewsPerGrid = 40;
	const int ItemsPerCollectionView = 6;

	public Issue31627()
	{
		var resultLabel = new Label
		{
			Text = "Waiting",
			AutomationId = "ResultLabel"
		};

		var rootGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			}
		};

		var screensLayout = new VerticalStackLayout();

		for (int screen = 0; screen < GridCount; screen++)
		{
			var screenGrid = new Grid
			{
				RowDefinitions = CreateAutoRows(ContentViewsPerGrid)
			};

			for (int i = 0; i < ContentViewsPerGrid; i++)
			{
				var items = new List<string>();
				for (int itemIndex = 0; itemIndex < ItemsPerCollectionView; itemIndex++)
				{
					items.Add($"Screen {screen} Item {itemIndex}");
				}

				var collectionView = new CollectionView
				{
					ItemsSource = items,
					ItemTemplate = new DataTemplate(() =>
					{
						var label = new Label();
						label.SetBinding(Label.TextProperty, ".");
						return label;
					}),
					HeightRequest = 120
				};

				var contentView = new ContentView
				{
					Content = collectionView
				};

				Grid.SetRow(contentView, i);
				screenGrid.Children.Add(contentView);
			}

			screensLayout.Children.Add(screenGrid);
		}

		var scrollView = new ScrollView { Content = screensLayout };

		rootGrid.Children.Add(resultLabel);
		rootGrid.Children.Add(scrollView);
		Grid.SetRow(scrollView, 1);

		Content = rootGrid;

		// If a LayoutCycleException is thrown during layout on Windows, the app will
		// crash before this handler ever runs. Reaching "Success" proves the page
		// rendered without triggering the layout cycle.
		Loaded += (s, e) => resultLabel.Text = "Success";
	}

	static IList<RowDefinition> CreateAutoRows(int count)
	{
		var rows = new List<RowDefinition>();
		for (int i = 0; i < count; i++)
		{
			rows.Add(new RowDefinition(GridLength.Auto));
		}

		return rows;
	}
}
