using Microsoft.AspNetCore.Http;

namespace UNOPS.PAO.MailSender;

public record EmailAttachment
{
    public Stream FileStream { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = "application/octet-stream";
}
