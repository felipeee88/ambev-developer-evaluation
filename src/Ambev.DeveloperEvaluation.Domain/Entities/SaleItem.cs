using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Line item of a Sale. Encapsulates the discount-tier rules per product quantity.
/// </summary>
public class SaleItem : BaseEntity
{
    public ProductInfo Product { get; private set; } = default!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool Cancelled { get; private set; }

    protected SaleItem()
    {
    }

    public SaleItem(ProductInfo product, int quantity, decimal unitPrice)
    {
        if (product is null)
            throw new DomainException("Product is required.");
        if (unitPrice <= 0)
            throw new DomainException("UnitPrice must be greater than zero.");

        Id = Guid.NewGuid();
        Product = product;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Cancelled = false;

        CalculateTotals();
    }

    /// <summary>
    /// Applies the discount tier based on quantity and recomputes Discount and TotalAmount.
    /// Rules: Q&lt;4 → 0%, 4≤Q≤9 → 10%, 10≤Q≤20 → 20%, Q&gt;20 → DomainException.
    /// </summary>
    public void CalculateTotals()
    {
        if (Quantity < 1)
            throw new DomainException("Quantity must be at least 1.");
        if (Quantity > 20)
            throw new DomainException("Cannot sell more than 20 identical items.");

        var rate = Quantity switch
        {
            < 4 => 0m,
            <= 9 => 0.10m,
            _ => 0.20m
        };

        var gross = Quantity * UnitPrice;
        Discount = gross * rate;
        TotalAmount = gross - Discount;
    }

    /// <summary>
    /// Marks the item as cancelled. Event raising is orchestrated by the Sale aggregate.
    /// </summary>
    public void Cancel() => Cancelled = true;

    public void ChangeQuantity(int quantity)
    {
        Quantity = quantity;
        CalculateTotals();
    }

    public void ChangeUnitPrice(decimal unitPrice)
    {
        if (unitPrice <= 0)
            throw new DomainException("UnitPrice must be greater than zero.");
        UnitPrice = unitPrice;
        CalculateTotals();
    }
}
