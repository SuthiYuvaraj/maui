using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 11, "For pages used in FlyoutItem/Tab/ShellContent the Page.Loaded event is called erratically", PlatformAffected.All)]
	public class ShellPageLoadedIssue : Shell
	{
		public static Dictionary<string, int> PageLoadedCounts = new Dictionary<string, int>();
		public static Dictionary<string, List<string>> LoadedPageSequence = new Dictionary<string, List<string>>();
		public static string CurrentTestName = "";

		public ShellPageLoadedIssue()
		{
			var tab = new Tab { Title = "The Pages", Route = "ViewCaseViewModel" };

			tab.Items.Add(new ShellContent
			{
				Title = "Main Page",
				ContentTemplate = new DataTemplate(() => new TestPage1()),
				Route = "Route1"
			});

			tab.Items.Add(new ShellContent
			{
				Title = "Other Page", 
				ContentTemplate = new DataTemplate(() => new TestPage2()),
				Route = "Route2"
			});

			tab.Items.Add(new ShellContent
			{
				Title = "Another Page",
				ContentTemplate = new DataTemplate(() => new TestPage3()),
				Route = "Route3"
			});

			var flyoutItem = new FlyoutItem
			{
				FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems
			};
			flyoutItem.Items.Add(tab);

			this.Items.Add(flyoutItem);

			// Add single shell contents
			this.Items.Add(new ShellContent
			{
				Title = "Single Main",
				ContentTemplate = new DataTemplate(() => new TestPage1()),
				Route = "MainPage"
			});

			this.Items.Add(new ShellContent
			{
				Title = "Single Other",
				ContentTemplate = new DataTemplate(() => new TestPage2()),
				Route = "OtherPage"
			});
		}

		public static void ResetCounters(string testName)
		{
			CurrentTestName = testName;
			PageLoadedCounts.Clear();
			if (!LoadedPageSequence.ContainsKey(testName))
				LoadedPageSequence[testName] = new List<string>();
			else
				LoadedPageSequence[testName].Clear();
		}

		public static void RecordPageLoaded(string pageName)
		{
			if (!PageLoadedCounts.ContainsKey(pageName))
				PageLoadedCounts[pageName] = 0;
			
			PageLoadedCounts[pageName]++;

			if (!string.IsNullOrEmpty(CurrentTestName))
			{
				if (!LoadedPageSequence.ContainsKey(CurrentTestName))
					LoadedPageSequence[CurrentTestName] = new List<string>();
				LoadedPageSequence[CurrentTestName].Add($"{pageName}:{PageLoadedCounts[pageName]}");
			}
		}
	}

	public class TestPage1 : ContentPage
	{
		public TestPage1()
		{
			Title = "Main Page";
			AutomationId = "TestPage1";
			Content = new StackLayout
			{
				Children =
				{
					new Label { Text = "This is Test Page 1", AutomationId = "Page1Label" },
					new Button 
					{ 
						Text = "Navigate to Other Page", 
						AutomationId = "NavigateToOtherButton",
						Command = new Command(async () => await Shell.Current.GoToAsync("//Route2"))
					},
					new Button 
					{ 
						Text = "Navigate to Another Page", 
						AutomationId = "NavigateToAnotherButton",
						Command = new Command(async () => await Shell.Current.GoToAsync("//Route3"))
					},
					new Label { Text = GetLoadedCountText(), AutomationId = "LoadedCountLabel" }
				}
			};

			this.Loaded += (s, e) => 
			{
				ShellPageLoadedIssue.RecordPageLoaded("TestPage1");
				if (Content is StackLayout stack && stack.Children.Last() is Label label)
				{
					label.Text = GetLoadedCountText();
				}
			};
		}

		private string GetLoadedCountText()
		{
			return $"Page1 Loaded Count: {(ShellPageLoadedIssue.PageLoadedCounts.ContainsKey("TestPage1") ? ShellPageLoadedIssue.PageLoadedCounts["TestPage1"] : 0)}";
		}
	}

	public class TestPage2 : ContentPage
	{
		public TestPage2()
		{
			Title = "Other Page";
			AutomationId = "TestPage2";
			Content = new StackLayout
			{
				Children =
				{
					new Label { Text = "This is Test Page 2", AutomationId = "Page2Label" },
					new Button 
					{ 
						Text = "Navigate to Main Page", 
						AutomationId = "NavigateToMainButton",
						Command = new Command(async () => await Shell.Current.GoToAsync("//Route1"))
					},
					new Button 
					{ 
						Text = "Navigate to Another Page", 
						AutomationId = "NavigateToAnotherButton",
						Command = new Command(async () => await Shell.Current.GoToAsync("//Route3"))
					},
					new Label { Text = GetLoadedCountText(), AutomationId = "LoadedCountLabel" }
				}
			};

			this.Loaded += (s, e) => 
			{
				ShellPageLoadedIssue.RecordPageLoaded("TestPage2");
				if (Content is StackLayout stack && stack.Children.Last() is Label label)
				{
					label.Text = GetLoadedCountText();
				}
			};
		}

		private string GetLoadedCountText()
		{
			return $"Page2 Loaded Count: {(ShellPageLoadedIssue.PageLoadedCounts.ContainsKey("TestPage2") ? ShellPageLoadedIssue.PageLoadedCounts["TestPage2"] : 0)}";
		}
	}

	public class TestPage3 : ContentPage
	{
		public TestPage3()
		{
			Title = "Another Page";
			AutomationId = "TestPage3";
			Content = new StackLayout
			{
				Children =
				{
					new Label { Text = "This is Test Page 3", AutomationId = "Page3Label" },
					new Button 
					{ 
						Text = "Navigate to Main Page", 
						AutomationId = "NavigateToMainButton",
						Command = new Command(async () => await Shell.Current.GoToAsync("//Route1"))
					},
					new Button 
					{ 
						Text = "Navigate to Other Page", 
						AutomationId = "NavigateToOtherButton",
						Command = new Command(async () => await Shell.Current.GoToAsync("//Route2"))
					},
					new Label { Text = GetLoadedCountText(), AutomationId = "LoadedCountLabel" }
				}
			};

			this.Loaded += (s, e) => 
			{
				ShellPageLoadedIssue.RecordPageLoaded("TestPage3");
				if (Content is StackLayout stack && stack.Children.Last() is Label label)
				{
					label.Text = GetLoadedCountText();
				}
			};
		}

		private string GetLoadedCountText()
		{
			return $"Page3 Loaded Count: {(ShellPageLoadedIssue.PageLoadedCounts.ContainsKey("TestPage3") ? ShellPageLoadedIssue.PageLoadedCounts["TestPage3"] : 0)}";
		}
	}
}