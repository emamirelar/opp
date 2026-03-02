namespace UNOPS.PAO.Domain.Infrastructure;

public class BaseEntity
{
    public BaseEntity()
    {
    }

    public BaseEntity(string userEmail)
    {
        CreatedDate = DateTime.UtcNow.ToUniversalTime();
        CreatedBy = userEmail;
    }

    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}