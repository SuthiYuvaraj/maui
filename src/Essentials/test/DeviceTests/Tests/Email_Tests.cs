using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Storage;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	// TEST NOTES:
	//   - a human needs to close the email composer window
	[Category("Email")]
	public class Email_Tests
	{
		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public Task Compose_Shows_New_Window()
		{
			return Utils.OnMainThread(() => Email.ComposeAsync());
		}

		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public Task Compose_With_Message_Shows_New_Window()
		{
			return Utils.OnMainThread(() =>
			{
				var email = new EmailMessage
				{
					Subject = "Hello World!",
					Body = "This is a greeting email.",
					To = { "everyone@example.org" }
				};

				return Email.ComposeAsync(email);
			});
		}

		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public Task Compose_With_Message_Shows_New_Window_BlankCC()
		{
			return Utils.OnMainThread(() =>
			{
				var email = new EmailMessage
				{
					Subject = "Hello World!",
					Body = "This is a greeting email.",
					To = { "everyone@example.org" },
					Cc = { string.Empty },
					Bcc = { string.Empty },
				};

				return Email.ComposeAsync(email);
			});
		}

		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public Task Email_Attachments_are_Sent()
		{
			// Save a local cache data directory file
			var file = Path.Combine(FileSystem.AppDataDirectory, "EmailTest.txt");
			File.WriteAllText(file, "Attachment contents goes here...");

			return Utils.OnMainThread(() =>
			{
				var email = new EmailMessage
				{
					Subject = "Hello World!",
					Body =
						"This is a greeting email." + Environment.NewLine +
						"There should be an attachment attached.",
					To = { "everyone@example.org" },
					Attachments = { new EmailAttachment(file) }
				};

				return Email.ComposeAsync(email);
			});
		}

		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public Task Issue33000_Compose_Without_Attachments_Shows_Subject_And_Body()
		{
			// Test for Issue 33000: Email.ComposeAsync - problem with empty body and subject on Spark email application
			// MANUAL VERIFICATION REQUIRED: 
			//   - Verify email composer opens (test PASSES if composer opens)
			//   - Verify Subject field contains: "Test from .NET MAUI - Issue 33000"
			//   - Verify Body field contains: "This email tests that subject and body are correctly passed to Spark and other email apps.\n\nIf you can read this, the fix works!"
			//   - Test in Spark app and other email clients (Gmail, Outlook, etc.)
			//   - Before fix: Subject and Body were EMPTY in Spark app without attachments
			//   - After fix: Subject and Body should be populated correctly
			return Utils.OnMainThread(() =>
			{
				var email = new EmailMessage
				{
					Subject = "Test from .NET MAUI - Issue 33000",
					Body = "This email tests that subject and body are correctly passed to Spark and other email apps.\n\nIf you can read this, the fix works!",
					BodyFormat = EmailBodyFormat.PlainText
				};

				return Email.ComposeAsync(email);
			});
		}

		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public Task Issue33000_Compose_With_Html_Body()
		{
			// Test HTML body conversion for mailto URIs (Issue 33000)
			// MANUAL VERIFICATION REQUIRED:
			//   - Verify email composer opens
			//   - Verify Subject field contains: "HTML Email Test - Issue 33000"
			//   - Verify Body field contains plain text version of HTML (tags stripped, e.g., "HTML Email This tests HTML body conversion...")
			//   - HTML should be converted to plain text for mailto: URIs
			return Utils.OnMainThread(() =>
			{
				var email = new EmailMessage
				{
					Subject = "HTML Email Test - Issue 33000",
					Body = "<h1>HTML Email</h1><p>This tests <strong>HTML body</strong> conversion to plain text for mailto URIs.</p><ul><li>Item 1</li><li>Item 2</li></ul>",
					BodyFormat = EmailBodyFormat.Html
				};

				return Email.ComposeAsync(email);
			});
		}

		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public Task Issue33000_Compose_With_All_Fields()
		{
			// Test complete email with To, Cc, Bcc, Subject, and Body (Issue 33000)
			// MANUAL VERIFICATION REQUIRED:
			//   - Verify email composer opens
			//   - Verify To field contains: "primary@example.com"
			//   - Verify Cc field contains: "cc@example.com"
			//   - Verify Bcc field contains: "bcc@example.com"
			//   - Verify Subject field contains: "Complete Email Test - Issue 33000"
			//   - Verify Body field contains: "This tests that all email fields are correctly included in the mailto URI."
			//   - All fields should be populated in the mailto: URI
			return Utils.OnMainThread(() =>
			{
				var email = new EmailMessage
				{
					Subject = "Complete Email Test - Issue 33000",
					Body = "This tests that all email fields are correctly included in the mailto URI.",
					BodyFormat = EmailBodyFormat.PlainText,
					To = { "primary@example.com" },
					Cc = { "cc@example.com" },
					Bcc = { "bcc@example.com" }
				};

				return Email.ComposeAsync(email);
			});
		}
	}
}
