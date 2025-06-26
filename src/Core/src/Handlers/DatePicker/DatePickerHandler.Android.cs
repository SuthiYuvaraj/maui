using System;
using System.Globalization;
using System.Threading;
using Android.App;
using Android.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, MauiDatePicker>
	{
		DatePickerDialog? _dialog;
		CultureInfo? _lastCulture;
		Timer? _cultureMonitorTimer;

		protected override MauiDatePicker CreatePlatformView()
		{
			var mauiDatePicker = new MauiDatePicker(Context)
			{
				ShowPicker = ShowPickerDialog,
				HidePicker = HidePickerDialog
			};

			var date = VirtualView?.Date;

			if (date != null)
				_dialog = CreateDatePickerDialog(date.Value.Year, date.Value.Month - 1, date.Value.Day);

			_lastCulture = CultureInfo.CurrentCulture;
			return mauiDatePicker;
		}

		internal DatePickerDialog? DatePickerDialog { get { return _dialog; } }

		protected override void ConnectHandler(MauiDatePicker platformView)
		{
			base.ConnectHandler(platformView);
			platformView.ViewAttachedToWindow += OnViewAttachedToWindow;
			platformView.ViewDetachedFromWindow += OnViewDetachedFromWindow;

			if (platformView.IsAttachedToWindow)
				OnViewAttachedToWindow();
		}

		void OnViewDetachedFromWindow(object? sender = null, View.ViewDetachedFromWindowEventArgs? e = null)
		{
			// I tested and this is called when an activity is destroyed
			DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
			StopCultureMonitoring();
		}

		void OnViewAttachedToWindow(object? sender = null, View.ViewAttachedToWindowEventArgs? e = null)
		{
			DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
			StartCultureMonitoring();
		}

		protected override void DisconnectHandler(MauiDatePicker platformView)
		{
			if (_dialog != null)
			{
				_dialog.Hide();
				_dialog.Dispose();
				_dialog = null;
			}

			platformView.ViewAttachedToWindow -= OnViewAttachedToWindow;
			platformView.ViewDetachedFromWindow -= OnViewDetachedFromWindow;
			OnViewDetachedFromWindow();

			base.DisconnectHandler(platformView);
		}

		protected virtual DatePickerDialog CreateDatePickerDialog(int year, int month, int day)
		{
			var dialog = new DatePickerDialog(Context!, (o, e) =>
			{
				if (VirtualView != null)
				{
					VirtualView.Date = e.Date;
				}
			}, year, month, day);

			return dialog;
		}

		public static partial void MapBackground(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateBackground(datePicker);
		}

		public static partial void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler androidHandler)
				androidHandler.CheckCultureChange();
			handler.PlatformView?.UpdateFormat(datePicker);
		}

		public static partial void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler androidHandler)
				androidHandler.CheckCultureChange();
			handler.PlatformView?.UpdateDate(datePicker);
		}

		public static partial void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateMinimumDate(datePicker, platformHandler._dialog);
		}

		public static partial void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateMaximumDate(datePicker, platformHandler._dialog);
		}

		public static partial void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateCharacterSpacing(datePicker);
		}

		public static partial void MapFont(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(datePicker, fontManager);
		}

		public static partial void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateTextColor(datePicker);
		}

		void ShowPickerDialog()
		{
			if (VirtualView == null)
				return;

			if (_dialog != null && _dialog.IsShowing)
				return;

			var date = VirtualView.Date;
			ShowPickerDialog(date.Year, date.Month - 1, date.Day);
		}

		void ShowPickerDialog(int year, int month, int day)
		{
			if (_dialog == null)
				_dialog = CreateDatePickerDialog(year, month, day);
			else
			{
				EventHandler? setDateLater = null;
				setDateLater = (sender, e) => { _dialog!.UpdateDate(year, month, day); _dialog.ShowEvent -= setDateLater; };
				_dialog.ShowEvent += setDateLater;
			}

			_dialog.Show();
		}

		void HidePickerDialog()
		{
			_dialog?.Hide();
		}

		void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
		{
			DatePickerDialog? currentDialog = _dialog;

			if (currentDialog != null && currentDialog.IsShowing)
			{
				currentDialog.Dismiss();

				ShowPickerDialog(currentDialog.DatePicker.Year, currentDialog.DatePicker.Month, currentDialog.DatePicker.DayOfMonth);
			}
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
					PlatformView.UpdateFormat(VirtualView);
				}
			}
		}
	}
}
