using Microsoft.Extensions.Configuration;
using Resend;

namespace Tools;

public class EmailHelper
{
    private readonly IResend _resend;
    private readonly string _defaultFrom;

    public EmailHelper(IResend resend, IConfiguration configuration)
    {
        _resend = resend;
        _defaultFrom = configuration["Resend:From"] ?? "no-reply@projet-cyna.fr";
    }

    public async Task<Guid> SendAsync(
        string to,
        string subject,
        string htmlBody,
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

        var response = await _resend.EmailSendAsync(message, ct);
        return response.Content;
    }

    public async Task<Guid> SendBatchAsync(
        IEnumerable<string> recipients,
        string subject,
        string htmlBody,
        string? from = null,
        CancellationToken ct = default)
    {
        var message = new EmailMessage
        {
            From     = from ?? _defaultFrom,
            Subject  = subject,
            HtmlBody = htmlBody,
        };

        foreach (var r in recipients)
            message.To.Add(r);

        var response = await _resend.EmailSendAsync(message, ct);
        return response.Content;
    }

    public async Task<Guid> SendWithAttachmentAsync(
        string to,
        string subject,
        string htmlBody,
        IEnumerable<(string Filename, byte[] Data)> attachments,
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

        foreach (var (filename, data) in attachments)
        {
            message.Attachments.Add(new EmailAttachment
            {
                Filename = filename,
                Content  = data,
            });
        }

        var response = await _resend.EmailSendAsync(message, ct);
        return response.Content;
    }
}