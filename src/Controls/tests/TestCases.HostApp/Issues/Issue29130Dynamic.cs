using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 0, "CollectionView ItemSizingStrategy.MeasureFirstItem should work when items are added dynamically to empty collection", PlatformAffected.UWP)]
public class Issue29130Dynamic : ContentPage
{
    CollectionView collectionView;
    Issue29130DynamicViewModel viewModel;

    public Issue29130Dynamic()
    {
        viewModel = new Issue29130DynamicViewModel();
        this.BindingContext = viewModel;

        var addItemButton = new Button
        {
            Text = "Add Item",
            AutomationId = "29130DynamicAddButton"
        };
        addItemButton.Clicked += OnAddItemButtonClicked;

        var clearItemsButton = new Button
        {
            Text = "Clear Items",
            AutomationId = "29130DynamicClearButton"
        };
        clearItemsButton.Clicked += OnClearItemsButtonClicked;

        var toggleSizingButton = new Button
        {
            Text = "Toggle Sizing Strategy",
            AutomationId = "29130DynamicToggleSizingButton"
        };
        toggleSizingButton.Clicked += OnToggleSizingButtonClicked;

        var statusLabel = new Label
        {
            AutomationId = "29130DynamicStatusLabel",
            Text = "Items: 0, Strategy: MeasureFirstItem"
        };
        statusLabel.SetBinding(Label.TextProperty, "StatusText");

        var buttonGrid = new Grid
        {
            AutomationId = "29130DynamicButtonGrid",
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        Grid.SetColumn(addItemButton, 0);
        Grid.SetColumn(clearItemsButton, 1);
        Grid.SetColumn(toggleSizingButton, 2);

        buttonGrid.Children.Add(addItemButton);
        buttonGrid.Children.Add(clearItemsButton);
        buttonGrid.Children.Add(toggleSizingButton);

        // Start with empty CollectionView using MeasureFirstItem strategy
        collectionView = new CollectionView
        {
            Margin = new Thickness(0, 20, 0, 0),
            AutomationId = "29130DynamicCollectionView",
            ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem,
            BackgroundColor = Colors.LightGray, // Make it visible even when empty
            MinimumHeightRequest = 200,
            ItemTemplate = new DataTemplate(() =>
            {
                var frame = new Frame
                {
                    BackgroundColor = Colors.White,
                    BorderColor = Colors.Blue,
                    CornerRadius = 5,
                    Margin = new Thickness(5),
                    Padding = new Thickness(10)
                };

                var stackLayout = new StackLayout();

                var titleLabel = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 16
                };
                titleLabel.SetBinding(Label.TextProperty, "Title");
                titleLabel.SetBinding(Label.AutomationIdProperty, "TitleAutomationId");

                var descriptionLabel = new Label
                {
                    FontSize = 12,
                    TextColor = Colors.Gray
                };
                descriptionLabel.SetBinding(Label.TextProperty, "Description");
                descriptionLabel.SetBinding(Label.FontSizeProperty, "FontSize");
                descriptionLabel.SetBinding(Label.AutomationIdProperty, "DescriptionAutomationId");

                stackLayout.Children.Add(titleLabel);
                stackLayout.Children.Add(descriptionLabel);
                frame.Content = stackLayout;

                return frame;
            })
        };
        collectionView.SetBinding(ItemsView.ItemsSourceProperty, "Items");

        var mainStackLayout = new StackLayout
        {
            Padding = 10,
            Spacing = 10,
            Children =
            {
                statusLabel,
                buttonGrid,
                collectionView
            }
        };

        Content = mainStackLayout;
    }

    void OnAddItemButtonClicked(object sender, EventArgs e)
    {
        viewModel.AddItem();
    }

    void OnClearItemsButtonClicked(object sender, EventArgs e)
    {
        viewModel.ClearItems();
    }

    void OnToggleSizingButtonClicked(object sender, EventArgs e)
    {
        if (collectionView.ItemSizingStrategy == ItemSizingStrategy.MeasureFirstItem)
        {
            collectionView.ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems;
            viewModel.UpdateSizingStrategy("MeasureAllItems");
        }
        else
        {
            collectionView.ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem;
            viewModel.UpdateSizingStrategy("MeasureFirstItem");
        }
    }
}

public class Issue29130DynamicViewModel : ViewModelBase
{
    private readonly string[] _sampleTitles = {
        "Short Title",
        "Medium Length Title Here",
        "This is a Very Long Title That Should Test the Sizing Strategy Properly",
        "Another Title",
        "Test Item with Different Content"
    };

    private readonly string[] _sampleDescriptions = {
        "Short description.",
        "This is a medium length description that has more content to display.",
        "This is a very long description that contains multiple sentences and should test how the sizing strategy handles different content lengths. It includes various words and phrases to create a realistic test scenario for measuring items in the collection view.",
        "Simple desc.",
        "Different content here with various sizes and lengths to test the measure first item strategy."
    };

    private readonly double[] _fontSizes = { 12, 14, 16, 18, 20 };

    private int _itemCounter = 0;
    private string _sizingStrategy = "MeasureFirstItem";

    public ObservableCollection<Issue29130DynamicItemModel> Items { get; } = new();

    public string StatusText => $"Items: {Items.Count}, Strategy: {_sizingStrategy}";

    public void AddItem()
    {
        var titleIndex = _itemCounter % _sampleTitles.Length;
        var descIndex = _itemCounter % _sampleDescriptions.Length;
        var fontIndex = _itemCounter % _fontSizes.Length;

        var item = new Issue29130DynamicItemModel(
            title: _sampleTitles[titleIndex],
            description: _sampleDescriptions[descIndex],
            fontSize: _fontSizes[fontIndex],
            titleAutomationId: $"29130DynamicTitle{_itemCounter}",
            descriptionAutomationId: $"29130DynamicDesc{_itemCounter}"
        );

        Items.Add(item);
        _itemCounter++;
        OnPropertyChanged(nameof(StatusText));
    }

    public void ClearItems()
    {
        Items.Clear();
        _itemCounter = 0;
        OnPropertyChanged(nameof(StatusText));
    }

    public void UpdateSizingStrategy(string strategy)
    {
        _sizingStrategy = strategy;
        OnPropertyChanged(nameof(StatusText));
    }
}

public class Issue29130DynamicItemModel
{
    public string Title { get; }
    public string Description { get; }
    public double FontSize { get; }
    public string TitleAutomationId { get; }
    public string DescriptionAutomationId { get; }

    public Issue29130DynamicItemModel(string title, string description, double fontSize, string titleAutomationId, string descriptionAutomationId)
    {
        Title = title;
        Description = description;
        FontSize = fontSize;
        TitleAutomationId = titleAutomationId;
        DescriptionAutomationId = descriptionAutomationId;
    }
}
