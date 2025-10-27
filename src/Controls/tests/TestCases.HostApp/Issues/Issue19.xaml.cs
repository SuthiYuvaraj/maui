using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 19, "Android - Dynamic Updates to CollectionView Header/Footer and Templates Are Not Displayed",
		PlatformAffected.Android)]
	public partial class Issue19 : ContentPage
	{
		private int _headerUpdateCount = 1;
		private int _footerUpdateCount = 1;
		private int _headerTemplateUpdateCount = 1;
		private int _footerTemplateUpdateCount = 1;
		private int _groupTemplateUpdateCount = 1;

		public Issue19()
		{
			InitializeComponent();
			
			// Set up initial data
			TestCollectionView.ItemsSource = new ObservableCollection<string>
			{
				"Item 1", "Item 2", "Item 3", "Item 4", "Item 5"
			};

			// Set initial header and footer
			TestCollectionView.Header = "Initial Header";
			TestCollectionView.Footer = "Initial Footer";
			
			// Set initial templates
			TestCollectionView.HeaderTemplate = CreateHeaderTemplate("Initial Header Template");
			TestCollectionView.FooterTemplate = CreateFooterTemplate("Initial Footer Template");

			// Wire up button events
			UpdateHeaderButton.Clicked += OnUpdateHeaderClicked;
			UpdateFooterButton.Clicked += OnUpdateFooterClicked;
			UpdateHeaderTemplateButton.Clicked += OnUpdateHeaderTemplateClicked;
			UpdateFooterTemplateButton.Clicked += OnUpdateFooterTemplateClicked;
			UpdateGroupTemplatesButton.Clicked += OnUpdateGroupTemplatesClicked;
		}

		private void OnUpdateHeaderClicked(object sender, EventArgs e)
		{
			TestCollectionView.Header = $"Updated Header {_headerUpdateCount++}";
		}

		private void OnUpdateFooterClicked(object sender, EventArgs e)
		{
			TestCollectionView.Footer = $"Updated Footer {_footerUpdateCount++}";
		}

		private void OnUpdateHeaderTemplateClicked(object sender, EventArgs e)
		{
			TestCollectionView.HeaderTemplate = CreateHeaderTemplate($"Updated Header Template {_headerTemplateUpdateCount++}");
		}

		private void OnUpdateFooterTemplateClicked(object sender, EventArgs e)
		{
			TestCollectionView.FooterTemplate = CreateFooterTemplate($"Updated Footer Template {_footerTemplateUpdateCount++}");
		}

		private void OnUpdateGroupTemplatesClicked(object sender, EventArgs e)
		{
			// Make it a grouped view and update group templates
			TestCollectionView.IsGrouped = true;
			TestCollectionView.GroupHeaderTemplate = CreateHeaderTemplate($"Updated Group Header Template {_groupTemplateUpdateCount}");
			TestCollectionView.GroupFooterTemplate = CreateFooterTemplate($"Updated Group Footer Template {_groupTemplateUpdateCount++}");
			
			// Update the items source to be grouped
			var groupedItems = new ObservableCollection<GroupedItem>
			{
				new GroupedItem("Group 1", new[] { "Group 1 Item 1", "Group 1 Item 2" }),
				new GroupedItem("Group 2", new[] { "Group 2 Item 1", "Group 2 Item 2" })
			};
			TestCollectionView.ItemsSource = groupedItems;
		}

		private DataTemplate CreateHeaderTemplate(string text)
		{
			return new DataTemplate(() =>
			{
				var label = new Label
				{
					Text = text,
					BackgroundColor = Colors.LightBlue,
					Padding = 10,
					AutomationId = $"HeaderTemplate_{text.Replace(" ", "_")}"
				};
				return label;
			});
		}

		private DataTemplate CreateFooterTemplate(string text)
		{
			return new DataTemplate(() =>
			{
				var label = new Label
				{
					Text = text,
					BackgroundColor = Colors.LightGreen,
					Padding = 10,
					AutomationId = $"FooterTemplate_{text.Replace(" ", "_")}"
				};
				return label;
			});
		}
	}

	public class GroupedItem : ObservableCollection<string>
	{
		public string Name { get; }

		public GroupedItem(string name, IEnumerable<string> items) : base(items)
		{
			Name = name;
		}
	}
}