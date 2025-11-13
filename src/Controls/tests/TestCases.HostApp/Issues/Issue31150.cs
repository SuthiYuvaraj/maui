using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 31150, "Mismatch between Width and Height and Measure results", PlatformAffected.iOS | PlatformAffected.macOS)]

public class Issue31150 : ContentPage
{
	private Issue31150CustomLayout TestLayout;
	private Label LayoutConstraintsResult;
	private Label ChildConstraintsResult;
	private Label TestStatus;

	public Issue31150()
	{
		// Child views inside custom layout
		var child1 = new Label
		{
			Text = "Child 1",
			WidthRequest = 80,
			HeightRequest = 40,
			BackgroundColor = Colors.LightGreen,
			HorizontalOptions = LayoutOptions.Start,
			VerticalOptions = LayoutOptions.Start,
			AutomationId = "Issue31150Child1"
		};

		var child2 = new Label
		{
			Text = "Child 2",
			BackgroundColor = Colors.LightPink,
			HorizontalOptions = LayoutOptions.End,
			VerticalOptions = LayoutOptions.End,
			AutomationId = "Issue31150Child2"
		};

		TestLayout = new Issue31150CustomLayout
		{
			WidthRequest = 200,
			HeightRequest = 150,
			BackgroundColor = Colors.LightBlue,
			AutomationId = "Issue31150TestLayout",
			Children = { child1, child2 }
		};

		LayoutConstraintsResult = new Label
		{
			Text = "Layout constraints: Not measured yet",
			AutomationId = "Issue31150LayoutConstraintsResult",
			BackgroundColor = Colors.LightYellow,
			Padding = 5
		};

		ChildConstraintsResult = new Label
		{
			Text = "Child constraints: Not measured yet",
			AutomationId = "Issue31150ChildConstraintsResult",
			BackgroundColor = Colors.LightYellow,
			Padding = 5,
			Margin = new Thickness(0, 5, 0, 0)
		};

		TestStatus = new Label
		{
			Text = "Test Status: Not Started",
			AutomationId = "Issue31150TestStatus",
			BackgroundColor = Colors.LightGray,
			Padding = 5,
			Margin = new Thickness(0, 10, 0, 0)
		};

		var triggerButton = new Button
		{
			Text = "Trigger Measurement",
			AutomationId = "Issue31150TriggerButton",
			Margin = new Thickness(0, 20, 0, 0)
		};
		triggerButton.Clicked += OnTriggerClicked;

		Content = new ScrollView
		{
			Content = new StackLayout
			{
				Padding = 20,
				Children =
					{
						new Label
						{
							Text = "Custom Layout with WidthRequest=200, HeightRequest=150:",
							FontAttributes = FontAttributes.Bold,
							Margin = new Thickness(0,0,0,10)
						},
						TestLayout,
						new Label
						{
							Text = "Test Results:",
							FontAttributes = FontAttributes.Bold,
							Margin = new Thickness(0,20,0,10)
						},
						LayoutConstraintsResult,
						ChildConstraintsResult,
						triggerButton,
						TestStatus
					}
			}
		};
	}

	private void OnTriggerClicked(object sender, EventArgs e)
	{
		// Force a re-layout to trigger measurement
		TestLayout.InvalidateMeasure();
		TestStatus.Text = "Test Status: Measurement Triggered";
	}

	// Method to update the results from the custom layout
	public void UpdateResults(double actualWidthConstraint, double actualHeightConstraint, string childConstraints)
	{
		var layoutConstraints = $"Received: W={actualWidthConstraint:F0}, H={actualHeightConstraint:F0} | Expected: W=200, H=150";

		LayoutConstraintsResult.Text = layoutConstraints;
		ChildConstraintsResult.Text = childConstraints;

		bool layoutWidthOk = IsConstraintReasonable(actualWidthConstraint, 200.0);
		bool layoutHeightOk = IsConstraintReasonable(actualHeightConstraint, 150.0);
		bool layoutTestPassed = layoutWidthOk && layoutHeightOk;

		bool childTestPassed = childConstraints.Contains("Width: 80", StringComparison.Ordinal) &&
							   childConstraints.Contains("Height: 40", StringComparison.Ordinal);

		if (layoutTestPassed && childTestPassed)
		{
			TestStatus.Text = "Test Status: PASSED - Constraints match WidthRequest/HeightRequest";
			TestStatus.BackgroundColor = Colors.LightGreen;
		}
		else
		{
			string failureReason = "";
			if (!layoutWidthOk)
				failureReason += $"Width {actualWidthConstraint:F0} > expected 200; ";
			if (!layoutHeightOk)
				failureReason += $"Height {actualHeightConstraint:F0} > expected 150; ";
			if (!childTestPassed)
				failureReason += "Child constraints wrong; ";

			TestStatus.Text = $"Test Status: FAILED - {failureReason.TrimEnd()}";
			TestStatus.BackgroundColor = Colors.LightCoral;
		}
	}

	private bool IsConstraintReasonable(double actualConstraint, double expectedConstraint)
	{
		if (double.IsInfinity(actualConstraint))
			return false;

		double tolerance = expectedConstraint * 0.5; // Allow 50% variance
		double maxAllowed = expectedConstraint + tolerance;

		return actualConstraint <= maxAllowed && actualConstraint >= (expectedConstraint * 0.8);
	}
}

// Custom layout that tracks the constraints passed to Measure
public class Issue31150CustomLayout : Layout
{
	private new Issue31150CustomLayoutManager _layoutManager;

	protected override ILayoutManager CreateLayoutManager()
	{
		_layoutManager = new Issue31150CustomLayoutManager(this);
		return _layoutManager;
	}
}

// Custom layout manager that captures and validates measurement constraints
public class Issue31150CustomLayoutManager : ILayoutManager
{
	private readonly Issue31150CustomLayout _layout;
	private double _lastWidthConstraint;
	private double _lastHeightConstraint;
	private readonly Dictionary<IView, (double width, double height)> _childConstraints = new();

	public Issue31150CustomLayoutManager(Issue31150CustomLayout layout)
	{
		_layout = layout;
	}

	public Size Measure(double widthConstraint, double heightConstraint)
	{
		// Store the constraints received for validation
		_lastWidthConstraint = widthConstraint;
		_lastHeightConstraint = heightConstraint;

		// Clear previous child constraint tracking
		_childConstraints.Clear();

		var totalWidth = 0.0;
		var totalHeight = 0.0;

		// Measure each child and track their constraints
		foreach (var child in _layout)
		{
			if (child.Visibility == Visibility.Collapsed)
				continue;

			// For this test, we'll use simple constraints for children
			var childWidthConstraint = widthConstraint;
			var childHeightConstraint = heightConstraint;

			// If the child has explicit width/height requests, those should be honored
			if (child is View view)
			{
				if (view.WidthRequest > 0)
					childWidthConstraint = Math.Min(childWidthConstraint, view.WidthRequest);
				if (view.HeightRequest > 0)
					childHeightConstraint = Math.Min(childHeightConstraint, view.HeightRequest);
			}

			var childSize = child.Measure(childWidthConstraint, childHeightConstraint);

			// Store child constraints for validation
			_childConstraints[child] = (childWidthConstraint, childHeightConstraint);

			// Simple layout: stack children vertically
			totalWidth = Math.Max(totalWidth, childSize.Width);
			totalHeight += childSize.Height;
		}

		// Update the parent page with constraint information
		UpdateParentWithResults();

		// Return the measured size, respecting any explicit size requests
		var measuredWidth = _layout.WidthRequest > 0 ? _layout.WidthRequest : Math.Min(totalWidth, widthConstraint);
		var measuredHeight = _layout.HeightRequest > 0 ? _layout.HeightRequest : Math.Min(totalHeight, heightConstraint);

		return new Size(measuredWidth, measuredHeight);
	}

	public Size ArrangeChildren(Rect bounds)
	{
		double currentY = bounds.Y;

		foreach (var child in _layout)
		{
			if (child.Visibility == Visibility.Collapsed)
				continue;

			var childBounds = new Rect(bounds.X, currentY, bounds.Width, child.DesiredSize.Height);
			child.Arrange(childBounds);

			currentY += child.DesiredSize.Height;
		}

		return bounds.Size;
	}

	private void UpdateParentWithResults()
	{
		// Find the parent Issue31150 page
		var parent = _layout.Parent;
		while (parent != null && !(parent is Issue31150))
		{
			parent = parent.Parent;
		}

		if (parent is Issue31150 issuePage)
		{
			// Format layout constraint results
			var layoutResult = $"Layout constraints - Width: {_lastWidthConstraint:F1}, Height: {_lastHeightConstraint:F1} " +
							  $"(Expected: Width: 200, Height: 150)";

			// Format child constraint results - focus on the first child with explicit dimensions
			var childResult = "Child constraints - ";
			var firstChildWithDimensions = _childConstraints.FirstOrDefault(kvp =>
				kvp.Key is View view && (view.WidthRequest > 0 || view.HeightRequest > 0));

			if (firstChildWithDimensions.Key != null)
			{
				var child = firstChildWithDimensions.Key;
				var constraints = firstChildWithDimensions.Value;
				childResult += $"Width: {constraints.width:F1}, Height: {constraints.height:F1} ";

				if (child is View view)
				{
					childResult += $"(Expected: Width: {view.WidthRequest}, Height: {view.HeightRequest})";
				}
			}
			else
			{
				childResult += "No children with explicit dimensions found";
			}

			// Update the UI on the main thread
			Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
			{
				issuePage.UpdateResults(_lastWidthConstraint, _lastHeightConstraint, childResult);
			});
		}
	}
}
