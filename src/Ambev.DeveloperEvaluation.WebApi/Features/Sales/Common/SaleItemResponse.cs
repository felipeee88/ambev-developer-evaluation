namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

public class SaleItemResponse
{
    public Guid Id { get; set; }
    public ProductDto Product { get; set; } = new();
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public bool Cancelled { get; set; }
}
