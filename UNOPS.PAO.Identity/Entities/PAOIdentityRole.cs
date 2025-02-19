namespace UNOPS.PAO.Identity.Entities;

using Microsoft.AspNetCore.Identity;

public class PAOIdentityRole : IdentityRole<int>
{ 
    public string? Description { get; set; }
}
