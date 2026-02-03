namespace UNOPS.PAO.MailSender;

public record EmailMessage
{
    public required string TemplateName { get; init; }
    public required string Title { get; init; }
    public string[] EmailReceivers { get; init; } = Array.Empty<string>();
    /// <summary>
    /// CC recipients for the email. Used for workflow notifications to include
    /// stakeholders like Opportunity Manager, workflow initiator, and Director/Manager.
    /// </summary>
    public string[] CcReceivers { get; init; } = Array.Empty<string>();
    public List<EmailAttachment>? Attachments { get; init; }
}
