namespace UNOPS.PAO.Identity.Models;

public class GoogleSignInRequest
{
    public string Provider { get; set; } = string.Empty;
    public string? IdToken { get; set; }
}
