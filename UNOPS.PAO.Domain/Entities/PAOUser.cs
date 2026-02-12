namespace UNOPS.PAO.Domain.Entities;

public class PAOUser
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public bool IsInternal { get; set; }
    /// <summary>Soft delete: false means user is deactivated (e.g. no longer in ERP).</summary>
    public bool ActiveUser { get; set; } = true;
    public UserProfile? UserProfile { get; set; }
    public string Name
    {
        get
        {
            if (string.IsNullOrEmpty(UserProfile?.Name))
            {
                return string.Empty;
            }

            return UserProfile.Name;
        }
    }
}
