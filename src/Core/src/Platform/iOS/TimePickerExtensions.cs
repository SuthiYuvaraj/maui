using System;
using System.Globalization;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class TimePickerExtensions
	{
		public static void UpdateFormat(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			mauiTimePicker.UpdateTime(timePicker, null);
		}

		public static void UpdateFormat(this UIDatePicker picker, ITimePicker timePicker)
		{
			picker.UpdateTime(timePicker);
		}

		public static void UpdateFormat(this MauiTimePicker mauiTimePicker, ITimePicker timePicker, UIDatePicker? picker)
		{
			mauiTimePicker.UpdateTime(timePicker, picker);
		}

		public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			mauiTimePicker.UpdateTime(timePicker, null);
		}

		public static void UpdateTime(this UIDatePicker picker, ITimePicker timePicker)
		{
			if (picker != null)
			{
				// Set the locale of the UIDatePicker based on the current culture
				var cultureInfo = CultureInfo.CurrentCulture;
				NSLocale locale = new NSLocale(cultureInfo.TwoLetterISOLanguageName);
				picker.Locale = locale;

				picker.Date = new DateTime(1, 1, 1).Add(timePicker.Time).ToNSDate();
			}
		}

		public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker, UIDatePicker? picker)
		{
			var cultureInfo = CultureInfo.CurrentCulture;
			var format = timePicker.Format;

			// Set locale based on format before updating the picker's date
			if (picker != null)
			{
				if (string.IsNullOrEmpty(format))
				{
					NSLocale locale = new NSLocale(cultureInfo.TwoLetterISOLanguageName);
					picker.Locale = locale;
				}
				else if (format.Contains('H', StringComparison.Ordinal))
				{
					var ci = new CultureInfo("de-DE");
					NSLocale locale = new NSLocale(ci.TwoLetterISOLanguageName);
					picker.Locale = locale;
				}
				else if (format.Contains('h', StringComparison.Ordinal))
				{
					var ci = new CultureInfo("en-US");
					NSLocale locale = new NSLocale(ci.TwoLetterISOLanguageName);
					picker.Locale = locale;
				}
			}

			picker?.UpdateTime(timePicker);

			var time = timePicker.Time;

			mauiTimePicker.Text = time.ToFormattedString(format, cultureInfo);

			mauiTimePicker.UpdateCharacterSpacing(timePicker);
		}

		public static void UpdateTextAlignment(this MauiTimePicker textField, ITimePicker timePicker)
		{
			// TODO: Update TextAlignment based on the EffectiveFlowDirection property.
		}
	}
}