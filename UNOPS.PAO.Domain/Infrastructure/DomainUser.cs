namespace UNOPS.PAO.Domain.Infrastructure;

public class DomainUser
{
    public string Id { get; }
    public string Email { get; }
    public string UserName { get; }
    
    public DomainUser(string id, string email, string userName)
    {
        Id = id;
        Email = email;
        UserName = userName;
    }
}