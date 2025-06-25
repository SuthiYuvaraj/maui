using System;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.UnitTests.Views
{
	[Category(TestCategory.Core)]
	public class CalendarDatePickerExtensionsTests
	{
		[Theory]
		[InlineData("d", "")]  // Currently returns empty, but should handle standard format
		[InlineData("D", "")]  // Currently returns empty, but should handle standard format
		[InlineData("f", "")]  // Currently returns empty, but should handle standard format
		[InlineData("F", "")]  // Currently returns empty, but should handle standard format
		[InlineData("g", "")]  // Currently returns empty, but should handle standard format
		[InlineData("G", "")]  // Currently returns empty, but should handle standard format
		[InlineData("m", "")]  // Currently returns empty, but should handle standard format
		[InlineData("M", "")]  // Currently returns empty, but should handle standard format
		[InlineData("y", "")]  // Currently returns empty, but should handle standard format
		[InlineData("Y", "")]  // Currently returns empty, but should handle standard format
		public void ToDateFormat_StandardFormats_ReturnEmptyCurrently(string format, string expected)
		{
			// Test current behavior - this shows the issue
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
			// Test that custom formats work correctly
			var result = format.ToDateFormat();
			Assert.Equal(expected, result);
		}
	}
}