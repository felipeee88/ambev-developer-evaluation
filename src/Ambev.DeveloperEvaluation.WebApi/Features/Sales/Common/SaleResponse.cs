namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

public class SaleResponse
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public CustomerDto Customer { get; set; } = new();
    public BranchDto Branch { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public bool Cancelled { get; set; }
    public IReadOnlyList<SaleItemResponse> Items { get; set; } = Array.Empty<SaleItemResponse>();
}
