using Microsoft.Extensions.Configuration;
using Resend;

namespace Tools;

/// <summary>
/// Defines how batch emails are dispatched to multiple recipients.
/// </summary>
public enum BatchSendMode
{
    /// <summary>
    /// All recipients are placed in the <c>To</c> field of a single email,
    /// so every recipient can see the full list.
    /// </summary>
    SingleEmailAllRecipients,

    /// <summary>
    /// One individual email is sent per recipient.
    /// Each recipient only sees themselves in the <c>To</c> field.
    /// </summary>
    OneEmailPerRecipient,
}

/// <summary>
/// Provides helper methods for sending transactional emails via Resend.
/// </summary>
public class EmailHelper
{
    private readonly IResend _resend;
    private readonly string _defaultFrom;

    /// <summary>
    /// Initialises a new instance of <see cref="EmailHelper"/>.
    /// </summary>
    /// <param name="resend">The Resend client used to dispatch emails.</param>
    /// <param name="configuration">
    /// Application configuration. The value at <c>Resend:From</c> is used as the
    /// default sender address; falls back to <c>no-reply@projet-cyna.fr</c>.
    /// </param>
    public EmailHelper(IResend resend, IConfiguration configuration)
    {
        _resend = resend;
        _defaultFrom = configuration["Resend:From"] ?? "no-reply@projet-cyna.fr";
    }

    /// <summary>
    /// Sends an email with file attachments to multiple recipients.
    /// </summary>
    /// <param name="recipients">The list of recipient email addresses.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="htmlBody">The HTML content of the email body.</param>
    /// <param name="attachments">
    /// A collection of <c>(Filename, Data)</c> tuples representing the files to attach.
    /// </param>
    /// <param name="mode">
    /// Controls dispatch strategy:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <see cref="BatchSendMode.SingleEmailAllRecipients"/> — one email with all recipients
    ///       and attachments (one API call).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="BatchSendMode.OneEmailPerRecipient"/> — the same attachments are sent
    ///       individually to each recipient (one API call per recipient).
    ///     </description>
    ///   </item>
    /// </list>
    /// Defaults to <see cref="BatchSendMode.SingleEmailAllRecipients"/>.
    /// </param>
    /// <param name="from">
    /// Optional sender address. Defaults to the value configured at <c>Resend:From</c>.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// When <paramref name="mode"/> is <see cref="BatchSendMode.SingleEmailAllRecipients"/>,
    /// the single message ID. When <see cref="BatchSendMode.OneEmailPerRecipient"/>, the ID of
    /// the last successfully sent message.
    /// </returns>
    public async Task<List<Guid>?> SendBatchAsync(
        IEnumerable<string> recipients,
        string subject,
        string htmlBody,
        IEnumerable<(string Filename, byte[] Data)>? attachments,
        BatchSendMode mode = BatchSendMode.SingleEmailAllRecipients,
        string? from = null,
        CancellationToken ct = default)
    {
        var sender          = from ?? _defaultFrom;

        if (mode == BatchSendMode.OneEmailPerRecipient)
        {
            List<Guid>? lastId = null;
            foreach (var recipient in recipients)
            {
                var msg = new EmailMessage
                {
                    From     = sender,
                    Subject  = subject,
                    HtmlBody = htmlBody,
                };
                msg.To.Add(recipient);

                if (attachments != null)
                {
                    foreach (var (filename, data) in attachments)
                    {
                        msg.Attachments.Add(new EmailAttachment { Filename = filename, Content = data, });
                    }
                }

                var res = await _resend.EmailSendAsync(msg, ct);
                lastId.Add(res.Content);
            }
            return lastId;
        }

        // SingleEmailAllRecipients (default)
        var message = new EmailMessage
        {
            From     = sender,
            Subject  = subject,
            HtmlBody = htmlBody,
        };

        foreach (var r in recipients)
            message.To.Add(r);

        if (attachments != null)
        {
            foreach (var (filename, data) in attachments)
            {
                message.Attachments.Add(new EmailAttachment { Filename = filename, Content = data, });
            }
        }

        var response = await _resend.EmailSendAsync(message, ct);
        return [response.Content];
    }

    /// <summary>
    /// Sends an email with file attachments to a single recipient.
    /// </summary>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="htmlBody">The HTML content of the email body.</param>
    /// <param name="attachments">
    /// A collection of <c>(Filename, Data)</c> tuples representing the files to attach.
    /// </param>
    /// <param name="from">
    /// Optional sender address. Defaults to the value configured at <c>Resend:From</c>.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The unique identifier of the sent message returned by Resend.</returns>
    public async Task<Guid> SendAsync(
        string to,
        string subject,
        string htmlBody,
        IEnumerable<(string Filename, byte[] Data)>? attachments,
        string? from = null,
        CancellationToken ct = default)
    {
        var message = new EmailMessage
        {
            From     = from ?? _defaultFrom,
            Subject  = subject,
            HtmlBody = htmlBody,
        };
        message.To.Add(to);

        if (attachments != null)
        {
            foreach (var (filename, data) in attachments)
            {
                message.Attachments.Add(new EmailAttachment { Filename = filename, Content = data, });
            }
        }

        var response = await _resend.EmailSendAsync(message, ct);
        return response.Content;
    }
}