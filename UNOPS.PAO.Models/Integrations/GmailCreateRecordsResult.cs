namespace UNOPS.PAO.Models.Integrations;

public class GmailCreateRecordsResult
{
    public int CreatedContacts { get; set; }
    public int CreatedPartners { get; set; }
    public List<string> FailedEmails { get; set; } = new List<string>();
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
