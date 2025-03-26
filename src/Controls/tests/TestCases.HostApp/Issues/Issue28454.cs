using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28454, "Collectionview cell not expanding on label expanding to wrap in iOS",
		PlatformAffected.iOS)]
public class Issue28454 : ContentPage
{
	StackLayout stackLayout;
	CollectionView collectionView;
	public Issue28454()
	{
		stackLayout = new StackLayout();
		stackLayout.AutomationId = "Issue28454StackLayout";

		// Create the button
		var button = new Button
		{
			Text = "Click Me",
			AutomationId = "Issue28454Button",
		};
		button.Clicked += OnButtonClicked;

		collectionView = new CollectionView();
		collectionView.AutomationId = "Issue28454CollectionView";
		collectionView.SetBinding(ItemsView.ItemsSourceProperty, "Data");
		collectionView.ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid();
				grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
				grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

				var labelOne = new Label();
				labelOne.SetBinding(Label.TextProperty, "LineOne");

				var labelTwo = new Label { LineBreakMode = LineBreakMode.WordWrap };
				labelTwo.SetBinding(Label.TextProperty, "LineTwo");
				labelTwo.SetBinding(Label.AutomationIdProperty, "LabelId");
				Grid.SetRow(labelTwo, 1);

				grid.Children.Add(labelOne);
				grid.Children.Add(labelTwo);

				return grid;
			});

		stackLayout.Children.Add(button);
		stackLayout.Children.Add(collectionView);
		this.Content = stackLayout;
		this.BindingContext = new Issue28454ViewModel();
	}


	private void OnButtonClicked(object sender, EventArgs e)
	{
		var vm = this.BindingContext as Issue28454ViewModel;
		if (vm == null || vm.Data == null)
			return;
		// Handle the button click event
		vm.Data[1].LineTwo = "Second line item 1 XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
		vm.Data[0].LineTwo = "Second line item 2 ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ";

	}
}

public class Issue28454Model : INotifyPropertyChanged
{
	#region DataViewModel Members

	private string mLineOne;
	public string LineOne
	{
		get => mLineOne;
		set
		{
			if (mLineOne != value)
			{
				mLineOne = value;
				OnPropertyChanged(nameof(this.LineOne));
			}
		}
	}

	private string mLineTwo;
	public string LineTwo
	{
		get => mLineTwo;
		set
		{
			if (mLineTwo != value)
			{
				mLineTwo = value;
				OnPropertyChanged(nameof(this.LineTwo));
			}
		}
	}

	private string labelid;
	public string LabelId
	{
		get => labelid;
		set
		{
			if (labelid != value)
			{
				labelid = value;
				OnPropertyChanged(nameof(this.LabelId));
			}
		}
	}



	#endregion DataViewModel Members

	#region Ëvents

	public event PropertyChangedEventHandler PropertyChanged;

	#endregion Ëvents

	#region Protected Members

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	#endregion Protected Members

}
public class Issue28454ViewModel : INotifyPropertyChanged
{
	#region Constructors

	public Issue28454ViewModel()
	{
		Data = new ObservableCollection<Issue28454Model>();
		Data.Add(new Issue28454Model
		{
			LineOne = "First line item 1",
			LineTwo = "Second line item 1",
			LabelId = "Label1"
		});
		Data.Insert(0, new Issue28454Model
		{
			LineOne = "First line item 2",
			LineTwo = "Second line item 2",
			LabelId = "Label2"
		});
	}

	#endregion Constructors

	#region MainViewModel Members

	public ObservableCollection<Issue28454Model> Data
	{
		get;
		set;
	}

	#endregion MainViewModel Members

	#region Ëvents

	public event PropertyChangedEventHandler PropertyChanged;

	#endregion Ëvents

	#region Protected Members

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	#endregion Protected Members

}
