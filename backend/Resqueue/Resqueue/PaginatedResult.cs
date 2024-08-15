namespace Resqueue;

public class PaginatedResult<T>
{
    public List<T> Items { get; set; }
    public int PageIndex { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public bool HasPreviousPage => (PageIndex > 1);
    public bool HasNextPage => (PageIndex < TotalPages);
}