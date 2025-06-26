using System;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.UnitTests.Views
{
	[Category(TestCategory.Core)]
	public class CalendarDatePickerExtensionsTests
	{
		[Theory]
		[InlineData("d", "")] // Default short date format
		[InlineData("D", "{dayofweek.full} {month.full} {day.integer} {year.full}")] // Long date format
		[InlineData("f", "{dayofweek.full} {month.full} {day.integer} {year.full}")] // Full date (time removed for date picker)
		[InlineData("F", "{dayofweek.full} {month.full} {day.integer} {year.full}")] // Full date (time removed for date picker)
		[InlineData("g", "{month.integer}/{day.integer}/{year.full}")] // General date (time removed for date picker)
		[InlineData("G", "{month.integer}/{day.integer}/{year.full}")] // General date (time removed for date picker)
		[InlineData("m", "{month.full} {day.integer}")] // Month day pattern
		[InlineData("M", "{month.full} {day.integer}")] // Month day pattern
		[InlineData("o", "")] // Round-trip date/time - uses default
		[InlineData("O", "")] // Round-trip date/time - uses default
		[InlineData("r", "{dayofweek.abbreviated} {day.integer} {month.abbreviated} {year.full}")] // RFC1123 pattern (time removed for date picker)
		[InlineData("R", "{dayofweek.abbreviated} {day.integer} {month.abbreviated} {year.full}")] // RFC1123 pattern (time removed for date picker)
		[InlineData("s", "{year.full}-{month.integer(2)}-{day.integer(2)}")] // Sortable date pattern (time removed for date picker)
		[InlineData("u", "{year.full}-{month.integer(2)}-{day.integer(2)}")] // Universal sortable date (time removed for date picker)
		[InlineData("U", "{dayofweek.full} {month.full} {day.integer} {year.full}")] // Universal full date (time removed for date picker)
		[InlineData("y", "{year.full} {month.full}")] // Year month pattern
		[InlineData("Y", "{year.full} {month.full}")] // Year month pattern
		public void ToDateFormat_StandardFormats_ReturnExpectedPatterns(string format, string expected)
		{
			// Test that standard format strings are now properly mapped to CalendarDatePicker patterns
			var result = format.ToDateFormat();
			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("dd/MM/yyyy", "{day.integer(2)}/{month.integer(2)}/{year.full}")]
		[InlineData("d/M/yy", "{day.integer}/{month.integer(1)}/{year.abbreviated}")]
		[InlineData("ddd/MMM/yyyy", "{dayofweek.abbreviated}/{month.abbreviated}/{year.full}")]
		[InlineData("dddd/MMMM/yyyy", "{dayofweek.full}/{month.full}/{year.full}")]
		public void ToDateFormat_CustomFormats_WorksCorrectly(string format, string expected)
		{
			// Test that custom formats continue to work correctly
			var result = format.ToDateFormat();
			Assert.Equal(expected, result);
		}
	}
}