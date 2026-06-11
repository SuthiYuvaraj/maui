using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
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

		// ISO-8601 / RFC-1123 specifiers produce fixed WinUI patterns regardless of locale.
		[Theory(DisplayName = "Invariant Standard Format Specifiers Initialize Correctly")]
		[InlineData("o", "{year.full}-{month.integer(2)}-{day.integer(2)}")]
		[InlineData("O", "{year.full}-{month.integer(2)}-{day.integer(2)}")]
		[InlineData("r", "{dayofweek.abbreviated}, {day.integer} {month.abbreviated} {year.full}")]
		[InlineData("R", "{dayofweek.abbreviated}, {day.integer} {month.abbreviated} {year.full}")]
		[InlineData("s", "{year.full}-{month.integer(2)}-{day.integer(2)}")]
		[InlineData("u", "{year.full}-{month.integer(2)}-{day.integer(2)}")]
		public async Task InvariantFormatSpecifiersInitializeCorrectly(string format, string expectedNativeFormat)
		{
			var datePicker = new DatePickerStub();

			datePicker.Date = new DateTime(2025, 12, 25);
			datePicker.MinimumDate = DateTime.Today.AddDays(-1);
			datePicker.MaximumDate = DateTime.Today.AddDays(1);
			datePicker.Format = format;

			await ValidatePropertyInitValue(datePicker, () => datePicker.Format, GetNativeFormat, format, expectedNativeFormat);
		}

		// Expected strings are hard-coded for en-US to avoid using ToDateFormat() as its own oracle.
		[Theory(DisplayName = "Culture-Sensitive Standard Format Specifiers Produce Correct WinUI Pattern (en-US)")]
		[InlineData("D", "{dayofweek.full}, {month.full} {day.integer}, {year.full}")]
		[InlineData("f", "{dayofweek.full}, {month.full} {day.integer}, {year.full}")]
		[InlineData("F", "{dayofweek.full}, {month.full} {day.integer}, {year.full}")]
		[InlineData("g", "{month.integer(1)}/{day.integer}/{year.full}")]
		[InlineData("G", "{month.integer(1)}/{day.integer}/{year.full}")]
		[InlineData("m", "{month.full} {day.integer}")]
		[InlineData("M", "{month.full} {day.integer}")]
		[InlineData("U", "{dayofweek.full}, {month.full} {day.integer}, {year.full}")]
		[InlineData("y", "{month.full} {year.full}")]
		[InlineData("Y", "{month.full} {year.full}")]
		public void CultureSensitiveFormatSpecifiersProduceCorrectPatternForEnUS(string format, string expectedNativeFormat)
		{
			var savedCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			try
			{
				System.Threading.Thread.CurrentThread.CurrentCulture =
					System.Globalization.CultureInfo.GetCultureInfo("en-US");

				Assert.Equal(expectedNativeFormat, format.ToDateFormat());
			}
			finally
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = savedCulture;
			}
		}

		// Regression guard: locale literals (commas, dots) must survive the conversion.
		[Theory(DisplayName = "Standard Format Specifiers Preserve Locale Literals (de-DE)")]
		[InlineData("D", "{dayofweek.full}, {day.integer}. {month.full} {year.full}")]
		[InlineData("m", "{day.integer}. {month.full}")]
		[InlineData("M", "{day.integer}. {month.full}")]
		[InlineData("g", "{day.integer(2)}.{month.integer(2)}.{year.full}")]
		[InlineData("G", "{day.integer(2)}.{month.integer(2)}.{year.full}")]
		public void StandardFormatSpecifiersPreserveLocaleLiteralsForDeDE(string format, string expectedNativeFormat)
		{
			var savedCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			try
			{
				System.Threading.Thread.CurrentThread.CurrentCulture =
					System.Globalization.CultureInfo.GetCultureInfo("de-DE");

				Assert.Equal(expectedNativeFormat, format.ToDateFormat());
			}
			finally
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = savedCulture;
			}
		}

		CalendarDatePicker GetNativeDatePicker(DatePickerHandler datePickerHandler) =>
			datePickerHandler.PlatformView;

		string GetNativeFormat(DatePickerHandler datePickerHandler)
		{
			var plaformDatePicker = GetNativeDatePicker(datePickerHandler);
			return plaformDatePicker.DateFormat;
		}

		DateTime? GetNativeDate(DatePickerHandler datePickerHandler)
		{
			var plaformDatePicker = GetNativeDatePicker(datePickerHandler);
			var date = plaformDatePicker.Date;

			if (date.HasValue)
				return date.Value.DateTime;

			return null;
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
