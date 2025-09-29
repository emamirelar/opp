using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Models;

public class UserValueModel
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public UserProfileValueModel? UserProfile { get; set; }
    public string Name
    {
        get
        {
            if (string.IsNullOrEmpty(UserProfile?.Name))
            {
                return Email;
            }

            return UserProfile.Name;
        }
    }
}

public class UserProfileValueModel
{
    public int UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Name { get; set; }
}