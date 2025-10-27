namespace UNOPS.PAO.Models.Shared;

public class PaginationRequest
{
    public PaginationRequest()
    {
    }

    public PaginationRequest(int pageIndex, int pageSize, string? orderBy = null, bool? ascending = null)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        OrderBy = orderBy;
        Ascending = ascending;
        Direction = ascending;
    }

    public string? OrderBy { get; set; }
    public bool? Ascending { get; set; }
    public bool? Direction { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    // Filter toggle state - controls whether global filters are applied
    public bool FilterActive { get; set; } = true;
}

public class GenericPaginationRequest<T> : PaginationRequest where T : PaginationRequest
{
    public GenericPaginationRequest()
    {
    }

    public GenericPaginationRequest(T t)
    {
        PageIndex = t.PageIndex;
        PageSize = t.PageSize;
        OrderBy = t.OrderBy;
        Ascending = t.Ascending ?? t.Direction;
        Direction = t.Direction;
    }

}
