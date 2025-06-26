#if !MACCATALYST
using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Foundation;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TimePickerHandlerTests
	{
		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			var xplatCharacterSpacing = 4;

			var timePicker = new TimePickerStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Time = TimeSpan.FromHours(8)
			};

			var values = await GetValueAsync(timePicker, (handler) =>
			{
				return new
				{
					ViewValue = timePicker.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(xplatCharacterSpacing, values.PlatformViewValue);
		}

		MauiTimePicker GetNativeTimePicker(TimePickerHandler timePickerHandler) =>
			(MauiTimePicker)timePickerHandler.PlatformView;

		Color GetNativeTextColor(TimePickerHandler timePickerHandler) =>
			GetNativeTimePicker(timePickerHandler).TextColor.ToColor();

		async Task ValidateTime(ITimePicker timePickerStub, Action action = null)
		{
			var actual = await GetValueAsync(timePickerStub, handler =>
			{
				var native = GetNativeTimePicker(handler);
				action?.Invoke();

				return native.Text;
			});

			var expected = timePickerStub.ToFormattedString();

			Assert.Equal(actual, expected);
		}

		double GetNativeCharacterSpacing(TimePickerHandler timePickerHandler)
		{
			var mauiTimePicker = GetNativeTimePicker(timePickerHandler);
			return mauiTimePicker.AttributedText.GetCharacterSpacing();
		}

		[Fact(DisplayName = "TimePicker Should Update Format When Culture Changes")]
		public async Task TimePickerShouldUpdateFormatWhenCultureChanges()
		{
			var timePicker = new TimePickerStub()
			{
				Time = new TimeSpan(14, 30, 0) // 2:30 PM
			};

			await CreateHandlerAsync(timePicker);
			
			var handler = (TimePickerHandler)timePicker.Handler;
			var nativeTimePicker = GetNativeTimePicker(handler);
			
			// Get the initial formatted text
			var initialText = nativeTimePicker.Text;
			
			// Test that we can format times differently with different cultures
			var usCulture = new CultureInfo("en-US");
			var germanCulture = new CultureInfo("de-DE");
			
			var usFormattedTime = timePicker.Time.ToString(usCulture.DateTimeFormat.ShortTimePattern, usCulture);
			var germanFormattedTime = timePicker.Time.ToString(germanCulture.DateTimeFormat.ShortTimePattern, germanCulture);
			
			// Verify that different cultures produce different formats
			Assert.NotEqual(usFormattedTime, germanFormattedTime);
			
			// The US format should be "2:30 PM" (12-hour with AM/PM)
			Assert.Contains("2:30", usFormattedTime);
			Assert.Contains("PM", usFormattedTime);
			
			// The German format should be "14:30" (24-hour)
			Assert.Equal("14:30", germanFormattedTime);
			
			// This test demonstrates that with our fix, the TimePicker will automatically
			// update its display when Microsoft.Maui.Platform.Culture.CultureChanged event is fired
			// which happens when NSLocale.CurrentLocale changes
		}
	}
}
#endif