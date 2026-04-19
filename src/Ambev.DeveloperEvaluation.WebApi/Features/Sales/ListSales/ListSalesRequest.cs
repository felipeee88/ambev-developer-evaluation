using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;

public class ListSalesRequest
{
    [FromQuery(Name = "_page")] public int Page { get; set; } = 1;
    [FromQuery(Name = "_size")] public int Size { get; set; } = 10;
    [FromQuery(Name = "_order")] public string? Order { get; set; }

    [FromQuery(Name = "customerId")] public Guid? CustomerId { get; set; }
    [FromQuery(Name = "branchId")] public Guid? BranchId { get; set; }
    [FromQuery(Name = "saleNumber")] public string? SaleNumber { get; set; }
    [FromQuery(Name = "cancelled")] public bool? Cancelled { get; set; }
    [FromQuery(Name = "_minSaleDate")] public DateTime? MinSaleDate { get; set; }
    [FromQuery(Name = "_maxSaleDate")] public DateTime? MaxSaleDate { get; set; }
    [FromQuery(Name = "_minTotalAmount")] public decimal? MinTotalAmount { get; set; }
    [FromQuery(Name = "_maxTotalAmount")] public decimal? MaxTotalAmount { get; set; }
}
