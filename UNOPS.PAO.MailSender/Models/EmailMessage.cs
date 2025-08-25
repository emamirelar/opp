namespace UNOPS.PAO.MailSender;

public record EmailMessage
{
    public required string TemplateName { get; init; }
    public required string Title { get; init; }
    public string[] EmailReceivers { get; init; } = Array.Empty<string>();
    public List<EmailAttachment>? Attachments { get; init; }
}
