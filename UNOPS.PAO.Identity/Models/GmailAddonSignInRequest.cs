namespace UNOPS.PAO.Identity.Models;

public class GmailAddonSignInRequest
{
    public string Provider { get; set; }
    public string? IdToken { get; set; }
}
