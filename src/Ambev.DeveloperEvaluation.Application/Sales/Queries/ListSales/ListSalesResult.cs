using Ambev.DeveloperEvaluation.Application.Sales.Common;

namespace Ambev.DeveloperEvaluation.Application.Sales.Queries.ListSales;

public class ListSalesResult
{
    public IReadOnlyList<SaleSummary> Data { get; set; } = Array.Empty<SaleSummary>();
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
