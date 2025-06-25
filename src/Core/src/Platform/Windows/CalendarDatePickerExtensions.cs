﻿using System;
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
			if (dateFormat.Length == 1)
			{
				return ConvertStandardFormat(dateFormat);
			}

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

		internal static string ConvertStandardFormat(string format)
		{
			switch (format)
			{
				case "D": // Long date pattern
					return "{dayofweek.full} {month.full} {day.integer} {year.full}";
				case "m":
				case "M": // Month day pattern
					return "{month.full} {day.integer}";
				case "y":
				case "Y": // Year month pattern - .NET shows "2023 December"
					return "{year.full} {month.full}";
				case "f": // Full date/time pattern (short time) - show date only for date picker
					return "{dayofweek.full} {month.full} {day.integer} {year.full}";
				case "F": // Full date/time pattern (long time) - show date only for date picker
					return "{dayofweek.full} {month.full} {day.integer} {year.full}";
				case "g": // General date/time pattern (short time) - show date only for date picker
					return "{month.integer}/{day.integer}/{year.full}";
				case "G": // General date/time pattern (long time) - show date only for date picker
					return "{month.integer}/{day.integer}/{year.full}";
				case "U": // Universal full date/time pattern - show date only for date picker
					return "{dayofweek.full} {month.full} {day.integer} {year.full}";
				case "r":
				case "R": // RFC1123 pattern - show date only for date picker
					return "{dayofweek.abbreviated} {day.integer} {month.abbreviated} {year.full}";
				case "s": // Sortable date/time pattern - show date only for date picker
					return "{year.full}-{month.integer(2)}-{day.integer(2)}";
				case "u": // Universal sortable date/time pattern - show date only for date picker
					return "{year.full}-{month.integer(2)}-{day.integer(2)}";
				default:
					// For other standard formats (o, O) that can't be reasonably mapped to date-only patterns,
					// return empty string so that they use the default format
					return string.Empty;
			}
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
