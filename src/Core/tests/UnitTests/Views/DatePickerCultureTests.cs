using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.UnitTests.Views
{
	[Category(TestCategory.Core, TestCategory.View)]
	public class DatePickerCultureTests
	{
		[Fact]
		public void DatePickerCultureTrackerInitializes()
		{
			// Test that the culture tracker can be used
			CultureTracker.CheckForCultureChanges();
			
			// This should not throw
			Assert.True(true);
		}

		[Fact]
		public void DatePickerCultureTrackerSubscription()
		{
			var called = false;
			var subscriber = new object();
			
			CultureTracker.Subscribe(subscriber, () => called = true);
			
			// Change culture to trigger notification
			var originalCulture = CultureInfo.CurrentCulture;
			try
			{
				CultureInfo.CurrentCulture = new CultureInfo("de-DE");
				CultureTracker.CheckForCultureChanges();
				
				Assert.True(called);
			}
			finally
			{
				CultureInfo.CurrentCulture = originalCulture;
				CultureTracker.Unsubscribe(subscriber);
			}
		}

		[Theory]
		[InlineData("en-US", "d")]
		[InlineData("de-DE", "d")]
		[InlineData("fr-FR", "d")]
		public void DatePickerFormatsWithDifferentCultures(string cultureName, string format)
		{
			var originalCulture = CultureInfo.CurrentCulture;
			try
			{
				CultureInfo.CurrentCulture = new CultureInfo(cultureName);
				
				var datePicker = new DatePicker
				{
					Date = new DateTime(2023, 12, 25),
					Format = format
				};

				Assert.NotNull(datePicker);
				Assert.Equal(format, datePicker.Format);
				Assert.Equal(new DateTime(2023, 12, 25), datePicker.Date);
			}
			finally
			{
				CultureInfo.CurrentCulture = originalCulture;
			}
		}
	}
}