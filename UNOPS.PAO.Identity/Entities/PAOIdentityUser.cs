namespace UNOPS.PAO.Identity.Entities;

using Microsoft.AspNetCore.Identity;

public class PAOIdentityUser : IdentityUser<int>
{
    public bool IsInternal { get; set; } = false;
    public bool ActiveUser { get; set; } = true;
    public bool GoogleSignIn { get; set; }
}
