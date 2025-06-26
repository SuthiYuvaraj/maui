using System;
using System.Globalization;
using System.Threading;
using Foundation;
using UIKit;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Handlers
{
#if !MACCATALYST
	public partial class DatePickerHandler : ViewHandler<IDatePicker, MauiDatePicker>
	{
		CultureInfo? _lastCulture;
		Timer? _cultureMonitorTimer;

		protected override MauiDatePicker CreatePlatformView()
		{
			MauiDatePicker platformDatePicker = new MauiDatePicker();
			_lastCulture = CultureInfo.CurrentCulture;
			return platformDatePicker;
		}

		internal UIDatePicker? DatePickerDialog { get { return PlatformView?.InputView as UIDatePicker; } }

		internal bool UpdateImmediately { get; set; }

		protected override void ConnectHandler(MauiDatePicker platformView)
		{
			platformView.MauiDatePickerDelegate = new DatePickerDelegate(this);

			if (DatePickerDialog is UIDatePicker picker)
			{
				var date = VirtualView?.Date;
				if (date is DateTime dt)
				{
					picker.Date = dt.ToNSDate();
				}
			}

			StartCultureMonitoring();
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MauiDatePicker platformView)
		{
			platformView.MauiDatePickerDelegate = null;
			StopCultureMonitoring();

			base.DisconnectHandler(platformView);
		}

		public static partial void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler iosHandler)
				iosHandler.CheckCultureChange();
			var picker = (handler as DatePickerHandler)?.DatePickerDialog;
			handler.PlatformView?.UpdateFormat(datePicker, picker);
		}

		public static partial void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler iosHandler)
				iosHandler.CheckCultureChange();
			var picker = (handler as DatePickerHandler)?.DatePickerDialog;
			handler.PlatformView?.UpdateDate(datePicker, picker);
		}

		public static partial void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateMinimumDate(datePicker, platformHandler.DatePickerDialog);
		}

		public static partial void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateMaximumDate(datePicker, platformHandler.DatePickerDialog);
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
			handler.PlatformView?.UpdateTextColor(datePicker);
		}

		public static partial void MapFlowDirection(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateFlowDirection(datePicker);
			handler.PlatformView?.UpdateTextAlignment(datePicker);
		}

		static void OnValueChanged(object? sender)
		{
			if (sender is DatePickerHandler datePickerHandler)
			{
				if (datePickerHandler.UpdateImmediately)  // Platform Specific
					datePickerHandler.SetVirtualViewDate();

				if (datePickerHandler.VirtualView != null)
					datePickerHandler.VirtualView.IsFocused = true;
			}
		}

		static void OnStarted(object? sender)
		{
			if (sender is IDatePickerHandler datePickerHandler && datePickerHandler.VirtualView != null)
				datePickerHandler.VirtualView.IsFocused = true;
		}

		static void OnEnded(object? sender)
		{
			if (sender is IDatePickerHandler datePickerHandler && datePickerHandler.VirtualView != null)
				datePickerHandler.VirtualView.IsFocused = false;
		}

		static void OnDoneClicked(object? sender)
		{
			if (sender is DatePickerHandler handler)
			{
				handler.SetVirtualViewDate();
				handler.PlatformView.ResignFirstResponder();
			}
		}

		void SetVirtualViewDate()
		{
			if (VirtualView == null || DatePickerDialog == null)
				return;

			VirtualView.Date = DatePickerDialog.Date.ToDateTime().Date;
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
					var picker = DatePickerDialog;
					PlatformView.UpdateFormat(VirtualView, picker);
				}
			}
		}

		class DatePickerDelegate : MauiDatePickerDelegate
		{
			readonly WeakReference<IDatePickerHandler> _handler;

			public DatePickerDelegate(IDatePickerHandler handler) =>
				_handler = new WeakReference<IDatePickerHandler>(handler);

			IDatePickerHandler? Handler
			{
				get
				{
					if (_handler?.TryGetTarget(out IDatePickerHandler? target) == true)
						return target;

					return null;
				}
			}

			public override void DatePickerEditingDidBegin()
			{
				DatePickerHandler.OnStarted(Handler);
			}

			public override void DatePickerEditingDidEnd()
			{
				DatePickerHandler.OnEnded(Handler);
			}

			public override void DatePickerValueChanged()
			{
				DatePickerHandler.OnValueChanged(Handler);
			}

			public override void DoneClicked()
			{
				DatePickerHandler.OnDoneClicked(Handler);
			}
		}
	}
#endif
}
