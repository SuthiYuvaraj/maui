using Microsoft.Maui.ApplicationModel.Communication;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33000, "Email.ComposeAsync - problem with empty body and subject on Spark email application", PlatformAffected.Android)]
public partial class Issue33000 : ContentPage
{
    public Issue33000()
    {
        InitializeComponent();
        CheckEmailSupport();
    }

    private async void CheckEmailSupport()
    {
        try
        {
            var isSupported = Email.Default.IsComposeSupported;
            Console.WriteLine($"[Issue33000] Email.IsComposeSupported: {isSupported}");

            if (!isSupported)
            {
                StatusLabel.Text = "⚠️ Email not supported on this device";
                StatusLabel.TextColor = Colors.Orange;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Issue33000] Error checking email support: {ex.Message}");
            StatusLabel.Text = $"Error: {ex.Message}";
            StatusLabel.TextColor = Colors.Red;
        }
    }

    private async void OnComposeBasicEmail(object sender, EventArgs e)
    {
        try
        {
            Console.WriteLine("[Issue33000] Testing basic email (Subject + Body)");

            var message = new EmailMessage
            {
                Subject = "Test from .NET MAUI - Issue 33000",
                Body = "This email tests that subject and body are correctly passed to Spark and other email apps.\n\nIf you can read this, the fix works!",
                BodyFormat = EmailBodyFormat.PlainText
            };

            await Email.Default.ComposeAsync(message);

            StatusLabel.Text = "✅ Email composer opened";
            StatusLabel.TextColor = Colors.Green;
            Console.WriteLine("[Issue33000] Basic email composer opened successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Issue33000] Error composing basic email: {ex.Message}");
            StatusLabel.Text = $"❌ Error: {ex.Message}";
            StatusLabel.TextColor = Colors.Red;
        }
    }

    private async void OnComposeFullEmail(object sender, EventArgs e)
    {
        try
        {
            Console.WriteLine("[Issue33000] Testing full email (To + Subject + Body)");

            var message = new EmailMessage
            {
                Subject = "Complete Email Test - Issue 33000",
                Body = "This tests email with recipient, subject, and body all populated.",
                BodyFormat = EmailBodyFormat.PlainText,
                To = new List<string> { "test@example.com" }
            };

            await Email.Default.ComposeAsync(message);

            StatusLabel.Text = "✅ Full email composer opened";
            StatusLabel.TextColor = Colors.Green;
            Console.WriteLine("[Issue33000] Full email composer opened successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Issue33000] Error composing full email: {ex.Message}");
            StatusLabel.Text = $"❌ Error: {ex.Message}";
            StatusLabel.TextColor = Colors.Red;
        }
    }

    private async void OnComposeHtmlEmail(object sender, EventArgs e)
    {
        try
        {
            Console.WriteLine("[Issue33000] Testing HTML email");

            var message = new EmailMessage
            {
                Subject = "HTML Email Test - Issue 33000",
                Body = "<h1>HTML Email</h1><p>This tests <strong>HTML body</strong> conversion to plain text for mailto URIs.</p><ul><li>Item 1</li><li>Item 2</li></ul>",
                BodyFormat = EmailBodyFormat.Html
            };

            await Email.Default.ComposeAsync(message);

            StatusLabel.Text = "✅ HTML email composer opened";
            StatusLabel.TextColor = Colors.Green;
            Console.WriteLine("[Issue33000] HTML email composer opened successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Issue33000] Error composing HTML email: {ex.Message}");
            StatusLabel.Text = $"❌ Error: {ex.Message}";
            StatusLabel.TextColor = Colors.Red;
        }
    }

    private async void OnComposeEmailWithCcBcc(object sender, EventArgs e)
    {
        try
        {
            Console.WriteLine("[Issue33000] Testing email with Cc/Bcc");

            var message = new EmailMessage
            {
                Subject = "Email with Cc/Bcc - Issue 33000",
                Body = "This tests that Cc and Bcc recipients are correctly included in the mailto URI.",
                BodyFormat = EmailBodyFormat.PlainText,
                To = new List<string> { "primary@example.com" },
                Cc = new List<string> { "cc@example.com" },
                Bcc = new List<string> { "bcc@example.com" }
            };

            await Email.Default.ComposeAsync(message);

            StatusLabel.Text = "✅ Email with Cc/Bcc opened";
            StatusLabel.TextColor = Colors.Green;
            Console.WriteLine("[Issue33000] Email with Cc/Bcc opened successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Issue33000] Error composing email with Cc/Bcc: {ex.Message}");
            StatusLabel.Text = $"❌ Error: {ex.Message}";
            StatusLabel.TextColor = Colors.Red;
        }
    }
}
