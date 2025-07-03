using System;
using System.Globalization;
using System.Linq;
using Foundation;

namespace Microsoft.Maui.Platform
{
	public static class Culture
	{
		static NSLocale? s_locale;
		static CultureInfo? s_currentCulture;

		public static event EventHandler? CultureChanged;

		public static CultureInfo CurrentCulture
		{
			get
			{
				if (s_locale == null || s_currentCulture == null || s_locale != NSLocale.CurrentLocale)
				{
					var oldCulture = s_currentCulture;
					s_locale = NSLocale.CurrentLocale;
					string countryCode = s_locale.CountryCode;
					var cultureInfo = CultureInfo.GetCultures(CultureTypes.AllCultures)
						.Where(c => c.Name.EndsWith("-" + countryCode)).FirstOrDefault();

					if (cultureInfo == null)
						cultureInfo = CultureInfo.InvariantCulture;

					s_currentCulture = cultureInfo;

					// Notify if culture actually changed (not just first initialization)
					if (oldCulture != null && !oldCulture.Equals(s_currentCulture))
					{
						CultureChanged?.Invoke(null, EventArgs.Empty);
					}
				}

				return s_currentCulture;
			}
		}

		/// <summary>
		/// Forces a refresh of the current culture and notifies listeners if it has changed.
		/// This method is useful for testing or when you know the system locale has changed.
		/// </summary>
		public static void RefreshCurrentCulture()
		{
			// Clear cached values to force refresh on next access
			s_locale = null;
			s_currentCulture = null;
			
			// Access CurrentCulture to trigger refresh and potential notification
			_ = CurrentCulture;
		}
	}
}