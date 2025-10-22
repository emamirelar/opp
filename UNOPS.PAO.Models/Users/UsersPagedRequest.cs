namespace UNOPS.PAO.Models.Users;

public class UsersPagedRequest
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 50;
    public string? SearchTerm { get; set; }
    public bool ActiveOnly { get; set; } = true;
    public int[]? SelectedUserIds { get; set; }
}
