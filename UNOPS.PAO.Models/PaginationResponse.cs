namespace UNOPS.PAO.Models;

public class PaginationResponse<T>
{
    public List<T> Records { get; set; }
    public int TotalCount { get; set; }
}