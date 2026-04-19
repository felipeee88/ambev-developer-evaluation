namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public class SaleResult
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public CustomerInfoDto Customer { get; set; } = new();
    public BranchInfoDto Branch { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public bool Cancelled { get; set; }
    public IReadOnlyList<SaleItemResult> Items { get; set; } = Array.Empty<SaleItemResult>();
}
