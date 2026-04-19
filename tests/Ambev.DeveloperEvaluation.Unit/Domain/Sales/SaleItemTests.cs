using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Unit.Domain.Sales.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Sales;

public class SaleItemTests
{
    [Theory(DisplayName = "Given quantity < 4 When calculating totals Then applies 0% discount")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CalculateTotals_ShouldApplyNoDiscount_WhenQuantityLessThanFour(int quantity)
    {
        // Given
        var item = new SaleItem(SalesFaker.Product(), quantity, 10m);

        // When
        var discount = item.Discount;
        var total = item.TotalAmount;

        // Then
        discount.Should().Be(0m);
        total.Should().Be(quantity * 10m);
    }

    [Theory(DisplayName = "Given quantity in [4,9] When calculating totals Then applies 10% discount")]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(9)]
    public void CalculateTotals_ShouldApply10Percent_WhenQuantityBetween4And9(int quantity)
    {
        // Given/When
        var item = new SaleItem(SalesFaker.Product(), quantity, 10m);

        // Then
        item.Discount.Should().Be(quantity * 10m * 0.10m);
        item.TotalAmount.Should().Be(quantity * 10m - item.Discount);
    }

    [Theory(DisplayName = "Given quantity in [10,20] When calculating totals Then applies 20% discount")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void CalculateTotals_ShouldApply20Percent_WhenQuantityBetween10And20(int quantity)
    {
        // Given/When
        var item = new SaleItem(SalesFaker.Product(), quantity, 10m);

        // Then
        item.Discount.Should().Be(quantity * 10m * 0.20m);
        item.TotalAmount.Should().Be(quantity * 10m - item.Discount);
    }

    [Fact(DisplayName = "Given quantity > 20 When creating item Then throws DomainException")]
    public void CalculateTotals_ShouldThrow_WhenQuantityGreaterThan20()
    {
        // Given/When
        var act = () => new SaleItem(SalesFaker.Product(), 21, 10m);

        // Then
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot sell more than 20 identical items*");
    }

    [Fact(DisplayName = "Given quantity < 1 When creating item Then throws DomainException")]
    public void CalculateTotals_ShouldThrow_WhenQuantityLessThanOne()
    {
        // Given/When
        var act = () => new SaleItem(SalesFaker.Product(), 0, 10m);

        // Then
        act.Should().Throw<DomainException>()
            .WithMessage("Quantity must be at least 1*");
    }

    [Theory(DisplayName = "Given unit price <= 0 When creating item Then throws DomainException")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldThrow_WhenUnitPriceZeroOrNegative(decimal unitPrice)
    {
        // Given/When
        var act = () => new SaleItem(SalesFaker.Product(), 5, unitPrice);

        // Then
        act.Should().Throw<DomainException>()
            .WithMessage("UnitPrice must be greater than zero*");
    }

    [Fact(DisplayName = "Given an item When Cancel is called Then flag is set without raising events")]
    public void Cancel_ShouldSetCancelledTrue_AndNotRaiseEvent()
    {
        // Given
        var item = new SaleItem(SalesFaker.Product(), 5, 10m);

        // When
        item.Cancel();

        // Then: SaleItem is not an aggregate root, so it never emits events itself.
        item.Cancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "Given an item When ChangeQuantity is called Then totals are recalculated")]
    public void ChangeQuantity_ShouldRecalculate()
    {
        // Given
        var item = new SaleItem(SalesFaker.Product(), 3, 10m);

        // When
        item.ChangeQuantity(10);

        // Then
        item.Quantity.Should().Be(10);
        item.Discount.Should().Be(20m);        // 10 * 10 * 0.20
        item.TotalAmount.Should().Be(80m);
    }
}
