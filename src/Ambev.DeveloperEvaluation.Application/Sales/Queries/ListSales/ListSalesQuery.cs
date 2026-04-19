using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Queries.ListSales;

public class ListSalesQuery : IRequest<ListSalesResult>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Order { get; set; }

    public Guid? CustomerId { get; set; }
    public Guid? BranchId { get; set; }
    public string? SaleNumber { get; set; }
    public DateTime? MinSaleDate { get; set; }
    public DateTime? MaxSaleDate { get; set; }
    public decimal? MinTotalAmount { get; set; }
    public decimal? MaxTotalAmount { get; set; }
    public bool? Cancelled { get; set; }
}
