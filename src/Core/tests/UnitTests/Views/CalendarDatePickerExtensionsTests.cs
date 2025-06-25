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
		[InlineData("f", "{dayofweek.full} {month.full} {day.integer} {year.full}")] // Full date (short time) - maps to long date
		[InlineData("F", "{dayofweek.full} {month.full} {day.integer} {year.full}")] // Full date (long time) - maps to long date
		[InlineData("g", "")] // General date (short time) - uses default
		[InlineData("G", "")] // General date (long time) - uses default
		[InlineData("m", "{month.full} {day.integer}")] // Month day pattern
		[InlineData("M", "{month.full} {day.integer}")] // Month day pattern
		[InlineData("o", "")] // Round-trip date/time - uses default
		[InlineData("O", "")] // Round-trip date/time - uses default
		[InlineData("r", "{dayofweek.abbreviated} {day.integer} {month.abbreviated} {year.full}")] // RFC1123 pattern approximation
		[InlineData("R", "{dayofweek.abbreviated} {day.integer} {month.abbreviated} {year.full}")] // RFC1123 pattern approximation
		[InlineData("s", "{year.full}-{month.integer(2)}-{day.integer(2)}")] // Sortable date pattern
		[InlineData("u", "")] // Universal sortable - uses default
		[InlineData("U", "{dayofweek.full} {month.full} {day.integer} {year.full}")] // Universal full - maps to long date
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