using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class DatePickerHandlerTests
	{
		[Theory(DisplayName = "Native View Bounding Box is not empty")]
		[InlineData(1)]
		[InlineData(100)]
		[InlineData(1000)]
		public override async Task ReturnsNonEmptyNativeBoundingBox(int size)
		{
			var datePicker = new DatePickerStub()
			{
				Height = size,
				Width = size,
				Date = DateTime.Today,
				MinimumDate = DateTime.Today.AddDays(-1),
				MaximumDate = DateTime.Today.AddDays(1)
			};

			var nativeBoundingBox = await GetValueAsync(datePicker, handler => GetBoundingBox(handler));
			Assert.NotEqual(nativeBoundingBox, Rect.Zero);

			var expectedSize = new Size(size, size);
			AssertWithinTolerance(expectedSize, nativeBoundingBox.Size);
		}

		[Fact]
		public override async Task DisconnectHandlerDoesntCrash()
		{
			var datePicker = new DatePickerStub
			{
				MinimumDate = DateTime.Today.AddDays(-1),
				MaximumDate = DateTime.Today.AddDays(1),
				Date = DateTime.Today
			};

			var handler = await CreateHandlerAsync(datePicker) as IPlatformViewHandler;
			await InvokeOnMainThreadAsync(handler.DisconnectHandler);
		}

		[Theory(DisplayName = "Format Initializes Correctly")]
		[InlineData("dd/MM/yyyy", "{day.integer(2)}/{month.integer(2)}/{year.full}")]
		[InlineData("d/M/yy", "{day.integer}/{month.integer(1)}/{year.abbreviated}")]
		[InlineData("ddd/MMM/yyyy", "{dayofweek.abbreviated}/{month.abbreviated}/{year.full}")]
		[InlineData("dddd/MMMM/yyyy", "{dayofweek.full}/{month.full}/{year.full}")]
		public async Task FormatInitializesCorrectly(string format, string nativeFormat)
		{
			var datePicker = new DatePickerStub();

			datePicker.Date = DateTime.Today;
			datePicker.Format = format;

			await ValidatePropertyInitValue(datePicker, () => datePicker.Format, GetNativeFormat, format, nativeFormat);
		}
		[Theory(DisplayName = "Standard Format Strings Initialize Correctly")]
		[InlineData("d")] // Short date pattern (culture-specific)
		[InlineData("D")] // Long date pattern (culture-specific)
		[InlineData("f")] // Full date/time (short time) - extracts date part only
		[InlineData("F")] // Full date/time (long time) - extracts date part only
		[InlineData("m")] // Month/day pattern
		[InlineData("M")] // Month/day pattern
		[InlineData("y")] // Year/month pattern
		[InlineData("Y")] // Year/month pattern
		[InlineData("g")] // General date/time (short time) - extracts date part only
		[InlineData("G")] // General date/time (long time) - extracts date part only
		[InlineData("U")] // Universal full - extracts date part only
		public async Task StandardFormatInitializesCorrectly(string format)
		{
			var datePicker = new DatePickerStub
			{
				Date = new DateTime(2025, 12, 25),
				MinimumDate = DateTime.Today.AddDays(-1),
				MaximumDate = DateTime.Today.AddDays(1),
				Format = format
			};

			// Get the expected native format by resolving the standard format through DateTimeFormatInfo
			// and converting it to WinUI format
			var expectedNativeFormat = format.ToDateFormat();

			await ValidatePropertyInitValue(datePicker, () => datePicker.Format, GetNativeFormat, format, expectedNativeFormat);
		}

		[Theory(DisplayName = "Standard Format Strings That Use Default Initialize Correctly")]
		[InlineData("r")] // RFC1123 - not suitable for date picker, uses default
		[InlineData("R")] // RFC1123 - not suitable for date picker, uses default
		[InlineData("s")] // Sortable - invariant culture, uses default
		[InlineData("u")] // Universal sortable - invariant culture, uses default
		[InlineData("o")] // Round-trip - not suitable for date picker, uses default
		[InlineData("O")] // Round-trip - not suitable for date picker, uses default
		public async Task StandardFormatDefaultInitializesCorrectly(string format)
		{
			var datePicker = new DatePickerStub
			{
				Date = DateTime.Today,
				MinimumDate = DateTime.Today.AddDays(-1),
				MaximumDate = DateTime.Today.AddDays(1),
				Format = format
			};

			// These formats return empty string and use the default platform format
			await ValidatePropertyInitValue(datePicker, () => datePicker.Format, GetNativeFormat, format, string.Empty);
		}
		CalendarDatePicker GetNativeDatePicker(DatePickerHandler datePickerHandler) =>
			datePickerHandler.PlatformView;

		string GetNativeFormat(DatePickerHandler datePickerHandler)
		{
			var plaformDatePicker = GetNativeDatePicker(datePickerHandler);
			return plaformDatePicker.DateFormat;
		}

		DateTime GetNativeDate(DatePickerHandler datePickerHandler)
		{
			var plaformDatePicker = GetNativeDatePicker(datePickerHandler);
			var date = plaformDatePicker.Date;

			if (date.HasValue)
				return date.Value.DateTime;

			return DateTime.MinValue;
		}

		Color GetNativeTextColor(DatePickerHandler datePickerHandler)
		{
			var foreground = GetNativeDatePicker(datePickerHandler).Foreground;

			if (foreground is UI.Xaml.Media.SolidColorBrush solidColorBrush)
				return solidColorBrush.Color.ToColor();

			return null;
		}
	}
}
