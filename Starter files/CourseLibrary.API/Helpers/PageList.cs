using Microsoft.EntityFrameworkCore;

namespace CourseLibrary.API.Helpers;

public class PageList<T> : List<T>
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public PageList(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        TotalCount = totalCount;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        AddRange(items);
    }

    public static async Task<PageList<T>> CreateAsync(IQueryable<T> sourceQuery, int pageNumber, int pageSize)
    {
        var count = sourceQuery.Count();
        var items = await sourceQuery.Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return new PageList<T>(items, count, pageNumber, pageSize);
    }
}