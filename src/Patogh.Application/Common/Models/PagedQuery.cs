namespace Patogh.Application.Common.Models;

/// <summary>
/// Base class for all queries that return paginated results.
/// Inherit from this in every Query that returns a list.
/// </summary>
public abstract class PagedQuery
{
    private int _page = 1;
    private int _pageSize = 20;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        // Hard cap at 100 — prevents clients from requesting 10,000 rows
        set => _pageSize = value < 1 ? 20 : value > 100 ? 100 : value;
    }

    // Convenience for EF Core .Skip()
    public int Skip => (Page - 1) * PageSize;
}