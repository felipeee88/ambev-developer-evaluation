using Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Sales.TestData;

/// <summary>
/// Bogus builders that always produce domain-valid instances.
/// </summary>
public static class SalesFaker
{
    private static readonly Faker _f = new("en");

    public static CustomerInfo Customer() =>
        new(Guid.NewGuid(), _f.Company.CompanyName());

    public static BranchInfo Branch() =>
        new(Guid.NewGuid(), _f.Address.City());

    public static ProductInfo Product() =>
        new(Guid.NewGuid(), _f.Commerce.ProductName());

    public static SaleItem SaleItem(int quantity = 5, decimal unitPrice = 10m) =>
        new(Product(), quantity, unitPrice);

    public static Sale Sale(int itemCount = 2)
    {
        var sale = new Sale($"S-{Guid.NewGuid():N}".Substring(0, 12),
            DateTime.UtcNow, Customer(), Branch());
        for (var i = 0; i < itemCount; i++)
            sale.AddItem(Product(), 5, 10m);
        return sale;
    }

    public static CreateSaleCommand CreateSaleCommand(int itemCount = 1)
    {
        var items = new List<CreateSaleItemInput>();
        for (var i = 0; i < itemCount; i++)
            items.Add(new CreateSaleItemInput
            {
                ProductId = Guid.NewGuid(),
                ProductName = _f.Commerce.ProductName(),
                Quantity = _f.Random.Int(1, 20),
                UnitPrice = _f.Random.Decimal(1m, 100m)
            });

        return new CreateSaleCommand
        {
            SaleNumber = $"S-{Guid.NewGuid():N}".Substring(0, 12),
            SaleDate = DateTime.UtcNow.AddHours(-1),
            CustomerId = Guid.NewGuid(),
            CustomerName = _f.Company.CompanyName(),
            BranchId = Guid.NewGuid(),
            BranchName = _f.Address.City(),
            Items = items
        };
    }
}
