namespace UNOPS.PAO.Domain.Entities;

public class PAOUser
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public bool IsInternal { get; set; }
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
