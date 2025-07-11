using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Handlers.ContentView
{
	[Category(TestCategory.ContentView)]
	public partial class ContentViewTests : CoreHandlerTestBase<ContentViewHandler, ContentViewStub>
	{
		[Fact]
		public async Task MeasureMatchesExplicitValues()
		{
			var cv = new ContentViewStub();

			var content = new SliderStub
			{
				DesiredSize = new Size(50, 50)
			};

			cv.Content = content;
			cv.Width = 100;
			cv.Height = 150;

			var contentViewHandler = await CreateHandlerAsync(cv);

			var measure = await InvokeOnMainThreadAsync(() => cv.Measure(double.PositiveInfinity, double.PositiveInfinity));

			Assert.Equal(cv.Width, measure.Width, 0);
			Assert.Equal(cv.Height, measure.Height, 0);
		}

		[Fact]
		public async Task RespectsMinimumValues()
		{
			var cv = new ContentViewStub();

			var content = new SliderStub
			{
				DesiredSize = new Size(50, 50)
			};

			cv.Content = content;
			cv.MinimumWidth = 100;
			cv.MinimumHeight = 150;

			var contentViewHandler = await CreateHandlerAsync(cv);

			var measure = await InvokeOnMainThreadAsync(() => cv.Measure(double.PositiveInfinity, double.PositiveInfinity));

			Assert.Equal(cv.MinimumWidth, measure.Width, 0);
			Assert.Equal(cv.MinimumHeight, measure.Height, 0);
		}

		[Fact]
		public async Task ContentViewWidthAvailableToChildrenDuringLayout()
		{
			var contentView = new ContentViewStub();
			var childView = new TestChildView();

			contentView.Content = childView;
			contentView.WidthRequest = 200;
			contentView.HeightRequest = 100;

			var contentViewHandler = await CreateHandlerAsync(contentView);

			// Simulate a layout pass similar to what happens in CollectionView
			var result = await InvokeOnMainThreadAsync(() =>
			{
				contentView.Measure(200, 100);
				contentView.Arrange(new Graphics.Rect(0, 0, 200, 100));
				return new { ContentViewWidth = contentView.Width, ChildRecordedWidth = childView.RecordedParentWidth };
			});

			// The child should have access to the parent's width during layout
			Assert.True(result.ContentViewWidth > 0, "ContentView Width should be greater than 0");
			Assert.True(result.ChildRecordedWidth > 0, "Child should have recorded a positive parent width during layout");
			Assert.Equal(200, result.ContentViewWidth, 0);
			Assert.Equal(200, result.ChildRecordedWidth, 0);
		}
	}

	public class TestChildView : IView
	{
		public double RecordedParentWidth { get; private set; } = -1;

		public Size Arrange(Rect bounds)
		{
			// Record the parent's width when this child is arranged
			if (this.Parent is IView parent)
			{
				RecordedParentWidth = parent.Width;
			}
			Frame = bounds;
			return bounds.Size;
		}

		public Size Measure(double widthConstraint, double heightConstraint)
		{
			// Record the parent's width when this child is measured
			if (this.Parent is IView parent)
			{
				RecordedParentWidth = parent.Width;
			}
			DesiredSize = new Size(System.Math.Min(50, widthConstraint), System.Math.Min(50, heightConstraint));
			return DesiredSize;
		}

		// Minimal implementation of IView interface
		public IElement? Parent { get; set; }
		public IElementHandler? Handler { get; set; }
		public Rect Frame { get; set; }
		public Size DesiredSize { get; set; }
		public double Width => Frame.Width;
		public double Height => Frame.Height;
		public Thickness Margin => Thickness.Zero;
		public string AutomationId => "";
		public FlowDirection FlowDirection => FlowDirection.LeftToRight;
		public LayoutAlignment HorizontalLayoutAlignment => LayoutAlignment.Fill;
		public LayoutAlignment VerticalLayoutAlignment => LayoutAlignment.Fill;
		public Semantics? Semantics => null;
		public IShape? Clip => null;
		public IShadow? Shadow => null;
		public bool IsEnabled => true;
		public bool IsFocused { get; set; }
		public Visibility Visibility => Visibility.Visible;
		public double Opacity => 1.0;
		public Paint? Background => null;
		public double MinimumWidth => 0;
		public double MaximumWidth => double.PositiveInfinity;
		public double MinimumHeight => 0;
		public double MaximumHeight => double.PositiveInfinity;
		public int ZIndex => 0;
		public void InvalidateMeasure() { }
		public void InvalidateArrange() { }
		IViewHandler? IView.Handler { get; set; }
		public double TranslationX => 0;
		public double TranslationY => 0;
		public double Scale => 1;
		public double ScaleX => 1;
		public double ScaleY => 1;
		public double Rotation => 0;
		public double RotationX => 0;
		public double RotationY => 0;
		public double AnchorX => 0.5;
		public double AnchorY => 0.5;
	}
}