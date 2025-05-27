namespace UNOPS.PAO.Models;

public class PaginationResponse<T>
{
    public List<T> Records { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}