using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Microsoft.Maui.Storage;
using Uri = Android.Net.Uri;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class EmailImplementation : IEmail
	{
		static EmailMessage testEmail =>
			new("Testing Microsoft.Maui.Essentials", "This is a test email.", "Microsoft.Maui.Essentials@example.org");

		public bool IsComposeSupported
			=> PlatformUtils.IsIntentSupported(CreateIntent(testEmail));

		Task PlatformComposeAsync(EmailMessage message)
		{
			var intent = CreateIntent(message);
			var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
			intent.SetFlags(flags);

			Application.Context.StartActivity(intent);

			return Task.FromResult(true);
		}

		static Intent CreateIntent(EmailMessage message)
		{
			var action = (message?.Attachments?.Count ?? 0) switch
			{
				0 => Intent.ActionSendto,
				1 => Intent.ActionSend,
				_ => Intent.ActionSendMultiple
			};
			var intent = new Intent(action);

			if (action == Intent.ActionSendto)
			{
				// For ACTION_SENDTO, data must be in the mailto: URI (PutExtra is ignored)
				// Build RFC 6068 compliant mailto URI
				intent.SetData(Uri.Parse(BuildMailtoUri(message)));
			}
			else
			{
				intent.SetType(FileMimeTypes.EmailMessage);

				if (!string.IsNullOrEmpty(message?.Body))
				{
					if (message.BodyFormat == EmailBodyFormat.Html)
					{
						ISpanned html;
#if __ANDROID_24__
						if (OperatingSystem.IsAndroidVersionAtLeast(24))
						{
							html = Html.FromHtml(message.Body, FromHtmlOptions.ModeLegacy);
						}
						else
#endif
						{
#pragma warning disable CS0618 // Type or member is obsolete
							html = Html.FromHtml(message.Body);
#pragma warning restore CS0618 // Type or member is obsolete
						}
						intent.PutExtra(Intent.ExtraText, html);
					}
					else
					{
						intent.PutExtra(Intent.ExtraText, message.Body);
					}
				}
				if (!string.IsNullOrEmpty(message?.Subject))
					intent.PutExtra(Intent.ExtraSubject, message.Subject);
				if (message?.To?.Count > 0)
					intent.PutExtra(Intent.ExtraEmail, message.To.ToArray());
				if (message?.Cc?.Count > 0)
					intent.PutExtra(Intent.ExtraCc, message.Cc.ToArray());
				if (message?.Bcc?.Count > 0)
					intent.PutExtra(Intent.ExtraBcc, message.Bcc.ToArray());
			}

			if (message?.Attachments?.Count > 0)
			{
				var uris = new List<IParcelable>();
				foreach (var attachment in message.Attachments)
				{
					uris.Add(FileSystemUtils.GetShareableFileUri(attachment));
				}

				if (uris.Count > 1)
					intent.PutParcelableArrayListExtra(Intent.ExtraStream, uris);
				else
					intent.PutExtra(Intent.ExtraStream, uris[0]);

				intent.AddFlags(ActivityFlags.GrantReadUriPermission);
			}

			return intent;
		}

		/// <summary>
		/// Builds a RFC 6068 compliant mailto URI with all email parameters
		/// </summary>
		static string BuildMailtoUri(EmailMessage message)
		{
			var builder = new StringBuilder("mailto:");

			// Email addresses in path (unescaped per RFC 6068)
			if (message?.To?.Count > 0)
				builder.Append(string.Join(",", message.To));

			// Query parameters (URL-escaped)
			var queryParams = new List<string>();

			if (!string.IsNullOrEmpty(message?.Subject))
				queryParams.Add($"subject={System.Uri.EscapeDataString(message.Subject)}");

			if (!string.IsNullOrEmpty(message?.Body))
			{
				// mailto URIs don't support HTML, so strip tags if HTML format
				var body = message.BodyFormat == EmailBodyFormat.Html
					? StripHtmlTags(message.Body)
					: message.Body;
				queryParams.Add($"body={System.Uri.EscapeDataString(body)}");
			}

			if (message?.Cc?.Count > 0)
				queryParams.Add($"cc={System.Uri.EscapeDataString(string.Join(",", message.Cc))}");

			if (message?.Bcc?.Count > 0)
				queryParams.Add($"bcc={System.Uri.EscapeDataString(string.Join(",", message.Bcc))}");

			if (queryParams.Count > 0)
				builder.Append("?").Append(string.Join("&", queryParams));

			return builder.ToString();
		}

		/// <summary>
		/// Strips HTML tags from content for plain text mailto URIs
		/// </summary>
		static string StripHtmlTags(string html)
		{
			if (string.IsNullOrEmpty(html))
				return html;

			// Remove HTML tags
			var stripped = Regex.Replace(html, "<[^>]*>", string.Empty);
			// Decode HTML entities
			stripped = System.Net.WebUtility.HtmlDecode(stripped);
			return stripped;
		}
	}
}
