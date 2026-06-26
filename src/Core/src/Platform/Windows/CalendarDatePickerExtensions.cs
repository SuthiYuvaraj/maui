using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Maui.Platform
{
	internal static class CalendarDatePickerExtensions
	{
		public static string ToDateFormat(this string dateFormat)
		{
			// The WinUI CalendarDatePicker DateFormat property use this formatter:
			// https://docs.microsoft.com/en-us/uwp/api/Windows.Globalization.DateTimeFormatting.DateTimeFormatter?redirectedfrom=MSDN&view=winrt-22621#code-snippet-2

			if (string.IsNullOrEmpty(dateFormat))
				return string.Empty;

			// Standard .NET date-only format specifiers are mapped to WinUI templates.
			// Time-containing standard specifiers are not supported by date-only CalendarDatePicker.
			if (TryConvertStandardDateFormat(dateFormat, out var standardDateFormat))
				return standardDateFormat;

			if (CheckDateFormat(dateFormat))
				return string.Empty;

			// Handle custom format strings using the tokenizer that preserves locale literals.
			// This properly handles patterns like "dddd, MMMM d, yyyy" (en-AU) or
			// "d. MMMM" (de-DE) where punctuation and spacing are significant.
			return ConvertNetPatternToWinUI(dateFormat);
		}

		internal static bool TryConvertStandardDateFormat(string format, out string convertedFormat)
		{
			convertedFormat = null;

			if (format.Length != 1)
				return false;

			switch (format)
			{
				case "d":
					convertedFormat = "shortdate";
					return true;
				case "D":
					convertedFormat = "longdate";
					return true;
				case "m":
				case "M":
					convertedFormat = "month day";
					return true;
				case "y":
				case "Y":
					convertedFormat = "month year";
					return true;

				// DatePicker is date-only, so time-containing standard DateTime formats
				// intentionally fall back to the native default date format.
				case "f":
				case "F":
				case "g":
				case "G":
				case "o":
				case "O":
				case "r":
				case "R":
				case "s":
				case "t":
				case "T":
				case "u":
				case "U":
					convertedFormat = string.Empty;
					return true;
				default:
					return false;
			}
		}

		// Converts a .NET date-pattern string (e.g. from DateTimeFormatInfo) to a WinUI
		// DateTimeFormatter pattern string.  Unlike the separator-based GetSeparator/GetPart
		// path, this tokenizer preserves every literal character — including commas, dots, and
		// spaces — so locale patterns such as "dddd, d MMMM yyyy" (en-AU long date) or
		// "d. MMMM" (de-DE month/day) round-trip correctly.
		internal static string ConvertNetPatternToWinUI(string netPattern)
		{
			var sb = new StringBuilder();
			int i = 0;

			while (i < netPattern.Length)
			{
				char c = netPattern[i];

				if (c == '\'' && i + 1 < netPattern.Length) // Single-quoted literal
				{
					i++; // skip opening quote
					while (i < netPattern.Length && netPattern[i] != '\'')
						sb.Append(netPattern[i++]);
					if (i < netPattern.Length) i++; // skip closing quote
				}
				else if (c == '\\' && i + 1 < netPattern.Length) // Escape sequence
				{
					sb.Append(netPattern[i + 1]);
					i += 2;
				}
				else if (c == 'd' || c == 'M' || c == 'y')
				{
					int start = i;
					while (i < netPattern.Length && netPattern[i] == c) i++;
					sb.Append(GetDateToken(c, i - start));
				}
				else
				{
					// Preserve all other characters as literals (commas, spaces, slashes, dots,
					// dashes, etc.).  Time-related characters that appear in full date/time patterns
					// are also emitted as literals; they are harmless for CalendarDatePicker since
					// it is a date-only control.
					sb.Append(c);
					i++;
				}
			}

			return sb.ToString();
		}

		// Returns the WinUI DateTimeFormatter token for a given .NET date-pattern character and run length.
		internal static string GetDateToken(char specifier, int count) =>
			(specifier, count) switch
			{
				('d', 1) => "{day.integer}",
				('d', 2) => "{day.integer(2)}",
				('d', 3) => "{dayofweek.abbreviated}",
				('d', _) => "{dayofweek.full}",
				('M', 1) => "{month.integer(1)}",
				('M', 2) => "{month.integer(2)}",
				('M', 3) => "{month.abbreviated}",
				('M', _) => "{month.full}",
				('y', 1) or ('y', 2) => "{year.abbreviated}",
				_ => "{year.full}"
			};


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
