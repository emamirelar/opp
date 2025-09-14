namespace UNOPS.PAO.MailSender;

public record EmailConfiguration
{
    public const string SectionName = "SmtpSettings";
    public string SmtpServer { get; init; } = string.Empty;
    public string SmtpEmail { get; init; } = string.Empty;
    public string SmtpEmailDisplayName { get; init; } = string.Empty;
    public string? LocalDomain { get; init; }
    public int SmtpPort { get; init; } = 587;
    public string? Username { get; init; } = string.Empty;
    public string? Password { get; init; } = string.Empty;
    public bool EnableSsl { get; init; } = true;
}
