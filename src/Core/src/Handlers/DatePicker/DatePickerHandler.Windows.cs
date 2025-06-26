#nullable enable
using System;
using System.Globalization;
using System.Threading;
using Microsoft.Maui.ApplicationModel;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, CalendarDatePicker>
	{
		CultureInfo? _lastCulture;
		Timer? _cultureMonitorTimer;

		protected override CalendarDatePicker CreatePlatformView()
		{
			_lastCulture = CultureInfo.CurrentCulture;
			return new CalendarDatePicker();
		}

		protected override void ConnectHandler(CalendarDatePicker platformView)
		{
			platformView.DateChanged += DateChanged;
			StartCultureMonitoring();
		}

		protected override void DisconnectHandler(CalendarDatePicker platformView)
		{
			platformView.DateChanged -= DateChanged;
			StopCultureMonitoring();
		}

		public static partial void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler windowsHandler)
				windowsHandler.CheckCultureChange();
			handler.PlatformView.UpdateDate(datePicker);
		}

		public static partial void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler windowsHandler)
				windowsHandler.CheckCultureChange();
			handler.PlatformView.UpdateDate(datePicker);
		}

		public static partial void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateMinimumDate(datePicker);
		}

		public static partial void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateMaximumDate(datePicker);
		}

		public static partial void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateCharacterSpacing(datePicker);
		}

		public static partial void MapFont(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView.UpdateFont(datePicker, fontManager);
		}

		public static partial void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateTextColor(datePicker);
		}

		private void DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
		{
			if (VirtualView == null)
				return;

			if (!args.NewDate.HasValue)
			{
				return;
			}

			// TODO ezhart 2021-06-21 For the moment, IDatePicker requires a date to be selected; once that's fixed, we can uncomment these next lines

			//if (!args.NewDate.HasValue)
			//{
			//	VirtualView.Date = null;
			//	return;
			//}

			//if (VirtualView.Date == null)
			//{
			//	VirtualView.Date = args.NewDate.Value.Date;
			//}

			VirtualView.Date = args.NewDate.Value.Date;
		}

		public static partial void MapBackground(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateBackground(datePicker);
		}

		void StartCultureMonitoring()
		{
			_lastCulture = CultureInfo.CurrentCulture;
			
			// Start a timer to periodically check for culture changes
			_cultureMonitorTimer = new Timer(OnCultureMonitorTick, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
		}

		void StopCultureMonitoring()
		{
			// Stop the culture monitoring timer
			_cultureMonitorTimer?.Dispose();
			_cultureMonitorTimer = null;
			_lastCulture = null;
		}

		void OnCultureMonitorTick(object? state)
		{
			// Check for culture changes on the main thread
			MainThread.BeginInvokeOnMainThread(() =>
			{
				CheckCultureChange();
			});
		}

		void CheckCultureChange()
		{
			var currentCulture = CultureInfo.CurrentCulture;
			if (_lastCulture == null || !_lastCulture.Equals(currentCulture))
			{
				_lastCulture = currentCulture;
				
				// Refresh the DatePicker display to reflect the new culture
				if (VirtualView != null && PlatformView != null)
				{
					PlatformView.UpdateDate(VirtualView);
				}
			}
		}
	}
}