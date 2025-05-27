namespace UNOPS.PAO.Identity.Models;

public class GoogleSignInRequest
{
    public string Provider { get; set; }
    public string? IdToken { get; set; }
    public string? Email { get; set; }
}
