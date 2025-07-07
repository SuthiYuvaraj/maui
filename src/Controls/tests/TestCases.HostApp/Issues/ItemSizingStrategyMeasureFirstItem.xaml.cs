using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	public partial class ItemSizingStrategyMeasureFirstItem : ContentPage
	{
		public ObservableCollection<TestItem> Items { get; set; }

		public ItemSizingStrategyMeasureFirstItem()
		{
			InitializeComponent();
			LoadData();
			SetupEventHandlers();
		}

		void LoadData()
		{
			Items = new ObservableCollection<TestItem>
			{
				new TestItem 
				{ 
					Title = "First Item (Large)", 
					Description = "This is the first item with extra content that should define the size for all items when MeasureFirstItem is used.",
					ExtraContent = "Extra line 1\nExtra line 2\nExtra line 3",
					HasExtraContent = true
				},
				new TestItem { Title = "Second Item", Description = "Short description", HasExtraContent = false },
				new TestItem { Title = "Third Item", Description = "Another short description", HasExtraContent = false },
				new TestItem { Title = "Fourth Item", Description = "Yet another short description", HasExtraContent = false },
				new TestItem { Title = "Fifth Item", Description = "One more short description", HasExtraContent = false }
			};

			collectionView.ItemsSource = Items;
		}

		void SetupEventHandlers()
		{
			strategyPicker.SelectedIndexChanged += OnStrategyChanged;
			layoutPicker.SelectedIndexChanged += OnLayoutChanged;
		}

		void OnStrategyChanged(object sender, EventArgs e)
		{
			if (strategyPicker.SelectedIndex == 0)
			{
				collectionView.ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem;
			}
			else
			{
				collectionView.ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems;
			}
		}

		void OnLayoutChanged(object sender, EventArgs e)
		{
			switch (layoutPicker.SelectedIndex)
			{
				case 0: // VerticalList
					collectionView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 5 };
					break;
				case 1: // HorizontalList
					collectionView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal) { ItemSpacing = 5 };
					break;
				case 2: // VerticalGrid
					collectionView.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical) 
					{ 
						VerticalItemSpacing = 5, 
						HorizontalItemSpacing = 5 
					};
					break;
			}
		}
	}

	public class TestItem
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string ExtraContent { get; set; }
		public bool HasExtraContent { get; set; }
	}
}