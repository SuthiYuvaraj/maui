using System;
using System.Globalization;
using System.Linq;

namespace Microsoft.Maui.Platform
{
	//TODO make this public on NET8
	internal static class CalendarDatePickerExtensions
	{
		public static string ToDateFormat(this string dateFormat)
		{
			// The WinUI CalendarDatePicker DateFormat property use this formatter:
			// https://docs.microsoft.com/en-us/uwp/api/Windows.Globalization.DateTimeFormatting.DateTimeFormatter?redirectedfrom=MSDN&view=winrt-22621#code-snippet-2

			if (string.IsNullOrEmpty(dateFormat) || CheckDateFormat(dateFormat))
				return string.Empty;

			// Handle standard .NET DateTime format strings (single characters)
			// by resolving them to culture-specific patterns first
			if (dateFormat.Length == 1)
			{
				string resolvedPattern = GetResolvedPatternForStandardFormat(dateFormat);
				if (string.IsNullOrEmpty(resolvedPattern))
					return string.Empty;

				// Convert the resolved .NET pattern to WinUI DateTimeFormatter format
				dateFormat = resolvedPattern;
			}

			// Handle custom format strings (or resolved standard formats)
			string result = string.Empty;
			string separator = GetSeparator(dateFormat);

			var parts = dateFormat.Split(separator);

			if (parts.Length > 0)
			{
				for (int i = 0; i < parts.Length; i++)
				{
					if (i < parts.Length - 1)
						result += GetPart(parts[i]) + separator;
					else
						result += GetPart(parts[i]);
				}
			}

			return result;
		}

		internal static string GetResolvedPatternForStandardFormat(string standardFormat)
		{
			// Get culture-specific pattern for the standard format
			// This respects the user's culture settings and is maintainable
			var dtfi = CultureInfo.CurrentCulture.DateTimeFormat;

			switch (standardFormat)
			{
				case "d": // Short date pattern
					return dtfi.ShortDatePattern;
				case "D": // Long date pattern
					return dtfi.LongDatePattern;
				case "M":
				case "m": // Month/day pattern
					return dtfi.MonthDayPattern;
				case "Y":
				case "y": // Year/month pattern
					return dtfi.YearMonthPattern;

				// The following formats include time components in their built-in patterns.
				// For DatePicker (date-only), manually extract only the date portion.
				case "f": // Full date/time (short time) - Extract date part only
					return ExtractDatePart(dtfi.FullDateTimePattern);
				case "F": // Full date/time (long time) - Extract date part only
					return ExtractDatePart(dtfi.FullDateTimePattern);
				case "g": // General date/time (short time) - Extract date part only
					return ExtractDatePart(dtfi.ShortDatePattern + " " + dtfi.ShortTimePattern);
				case "G": // General date/time (long time) - Extract date part only
					return ExtractDatePart(dtfi.ShortDatePattern + " " + dtfi.LongTimePattern);
				case "U": // Universal full date/time - Extract date part only
					return ExtractDatePart(dtfi.FullDateTimePattern);

				// The following formats are not suitable for date-only picker, use default
				case "r":
				case "R": // RFC1123 pattern - invariant culture format
				case "s": // Sortable date/time pattern - invariant culture
				case "u": // Universal sortable pattern - invariant culture
				case "o":
				case "O": // Round-trip date/time pattern - not suitable
					return string.Empty;

				default:
					// For unrecognized formats, return empty string to use the default format
					return string.Empty;
			}
		}

		internal static string ExtractDatePart(string dateTimePattern)
		{
			// Extract the date portion from a date-time pattern by removing time components
			// Time components typically include: h, H, m, s, t, f, F and their variations
			// We'll split by common separators and keep only date-related parts

			if (string.IsNullOrEmpty(dateTimePattern))
				return string.Empty;

			// Find the first occurrence of time-related patterns
			// Common time separators/indicators: space before time, or 'T' separator
			var parts = dateTimePattern.Split(new[] { ' ' }, 2);

			if (parts.Length > 1)
			{
				// Check if the first part contains date components (d, M, y)
				var firstPart = parts[0];
				if (ContainsDateComponents(firstPart))
					return firstPart;
			}

			// If no clear separation, return the full pattern
			// The conversion logic will handle it
			return dateTimePattern;
		}

		internal static bool ContainsDateComponents(string pattern)
		{
			// Check if the pattern contains typical date components
			return pattern.Contains('d', StringComparison.OrdinalIgnoreCase) ||
				   pattern.Contains('M') ||
				   pattern.Contains('y', StringComparison.OrdinalIgnoreCase);
		}
		internal static string GetSeparator(string format)
		{
			string separator;

			if (format.Contains('/', StringComparison.CurrentCultureIgnoreCase))
				separator = "/";
			else if (format.Contains('-', StringComparison.CurrentCultureIgnoreCase))
				separator = "-";
			else if (format.Contains(' ', StringComparison.CurrentCultureIgnoreCase))
				separator = " ";
			else if (format.Contains('.', StringComparison.CurrentCultureIgnoreCase))
				separator = ".";
			else
				separator = string.Empty;

			return separator;
		}

		internal static string GetPart(string format)
		{
			if (IsDay(format))
				return GetDayFormat(format);
			else if (IsMonth(format))
				return GetMonthFormat(format);
			else if (IsYear(format))
				return GetYearFormat(format);
			else
				return string.Empty;
		}

		internal static bool IsDay(string day)
		{
			if (day.Contains('d', StringComparison.OrdinalIgnoreCase))
				return true;

			return false;
		}

		internal static string GetDayFormat(string format)
		{
			if (CheckDateFormat(format))
			{
				return "{day.integer}";
			}
			else if (format.Equals("D", StringComparison.Ordinal))
			{
				return "{dayofweek.full}";
			}
			else
			{
				var day = format.Count(x => x == 'd');

				if (day == 3)
					return "{dayofweek.abbreviated}";
				else if (day == 4)
					return "{dayofweek.full}";
				else
					return $"{{day.integer({day})}}";
			}
		}

		internal static bool IsMonth(string day)
		{
			if (day.Contains('m', StringComparison.OrdinalIgnoreCase))
				return true;

			return false;
		}

		internal static string GetMonthFormat(string format)
		{
			if (CheckDateFormat(format))
			{
				return "{month}";
			}
			else if (format.Equals("D", StringComparison.Ordinal))
			{
				return "{month.full}";
			}
			else
			{
				var month = format.Count(x => string.Equals(new string(new char[] { x }), "M", StringComparison.OrdinalIgnoreCase));

				if (month <= 2)
					return $"{{month.integer({month})}}";
				else if (month == 3)
					return "{month.abbreviated}";
				else
					return "{month.full}";
			}
		}

		internal static bool IsYear(string day)
		{
			if (day.Contains('y', StringComparison.OrdinalIgnoreCase))
				return true;

			return false;
		}

		internal static string GetYearFormat(string format)
		{
			if (CheckDateFormat(format))
			{
				return "{year}";
			}
			else if (format.Equals("D", StringComparison.Ordinal))
			{
				return "{year.full}";
			}
			else
			{
				var year = format.Count(x => x == 'y');

				if (year <= 2)
					return "{year.abbreviated}";
				else
					return "{year.full}";
			}
		}

		internal static bool CheckDateFormat(string format)
		{
			return string.IsNullOrWhiteSpace(format) || format.Equals("d", StringComparison.Ordinal);
		}
	}
}
