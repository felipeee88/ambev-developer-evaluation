using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Unit.Domain.Sales.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Sales;

public class SaleTests
{
    [Fact(DisplayName = "Given valid arguments When constructing sale Then enqueues SaleCreatedEvent")]
    public void Constructor_ShouldEnqueueSaleCreatedEvent()
    {
        // Given/When
        var sale = new Sale("S-1", DateTime.UtcNow, SalesFaker.Customer(), SalesFaker.Branch());

        // Then
        sale.DomainEvents.Should().ContainSingle(e => e is SaleCreatedEvent);
    }

    [Fact(DisplayName = "Given a sale When AddItem is called Then enqueues SaleModifiedEvent and recalculates total")]
    public void AddItem_ShouldEnqueueSaleModifiedEvent_AndRecalculateTotal()
    {
        // Given
        var sale = new Sale("S-1", DateTime.UtcNow, SalesFaker.Customer(), SalesFaker.Branch());
        sale.ClearDomainEvents();

        // When
        sale.AddItem(SalesFaker.Product(), 5, 10m);

        // Then
        sale.Items.Should().HaveCount(1);
        sale.TotalAmount.Should().Be(45m);                     // 5*10 - 10%
        sale.DomainEvents.Should().ContainSingle(e => e is SaleModifiedEvent);
    }

    [Fact(DisplayName = "Given a sale with items When Cancel is called Then every item is cancelled and SaleCancelledEvent enqueued")]
    public void Cancel_ShouldCancelAllItems_AndEnqueueSaleCancelledEvent()
    {
        // Given
        var sale = SalesFaker.Sale(itemCount: 2);

        // When
        sale.Cancel();

        // Then
        sale.Cancelled.Should().BeTrue();
        sale.Items.Should().OnlyContain(i => i.Cancelled);
        sale.DomainEvents.Should().ContainSingle(e => e is SaleCancelledEvent);
    }

    [Fact(DisplayName = "Given a cancelled sale When Cancel is called again Then no new event is enqueued (idempotent)")]
    public void Cancel_ShouldBeIdempotent()
    {
        // Given
        var sale = SalesFaker.Sale(itemCount: 1);
        sale.Cancel();
        sale.ClearDomainEvents();

        // When
        sale.Cancel();

        // Then
        sale.DomainEvents.Should().BeEmpty();
    }

    [Fact(DisplayName = "Given a sale When CancelItem is called Then item is cancelled, sale is not cancelled, ItemCancelledEvent enqueued")]
    public void CancelItem_ShouldNotCancelSale_ButEnqueueItemCancelledEvent()
    {
        // Given
        var sale = SalesFaker.Sale(itemCount: 2);
        sale.ClearDomainEvents();
        var itemId = sale.Items.First().Id;

        // When
        sale.CancelItem(itemId);

        // Then
        sale.Cancelled.Should().BeFalse();
        sale.Items.Single(i => i.Id == itemId).Cancelled.Should().BeTrue();
        sale.DomainEvents.Should().ContainSingle(e => e is ItemCancelledEvent);
    }

    [Fact(DisplayName = "Given a sale When CancelItem is called with unknown id Then throws DomainException")]
    public void CancelItem_ShouldThrow_WhenItemNotFound()
    {
        // Given
        var sale = SalesFaker.Sale(itemCount: 1);

        // When
        var act = () => sale.CancelItem(Guid.NewGuid());

        // Then
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Given a sale with cancelled item When Recalculate Then excludes it from TotalAmount")]
    public void Recalculate_ShouldIgnoreCancelledItems()
    {
        // Given
        var sale = new Sale("S-1", DateTime.UtcNow, SalesFaker.Customer(), SalesFaker.Branch());
        sale.AddItem(SalesFaker.Product(), 5, 10m);     // 45
        sale.AddItem(SalesFaker.Product(), 3, 10m);     // 30
        var firstItemId = sale.Items.First().Id;

        // When
        sale.CancelItem(firstItemId);

        // Then
        sale.TotalAmount.Should().Be(30m);
    }

    [Fact(DisplayName = "Given a cancelled sale When AddItem is called Then throws DomainException")]
    public void AddItem_ShouldThrow_WhenSaleCancelled()
    {
        // Given
        var sale = SalesFaker.Sale(itemCount: 1);
        sale.Cancel();

        // When
        var act = () => sale.AddItem(SalesFaker.Product(), 1, 10m);

        // Then
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Given a sale When ChangeHeader is called Then header is updated and SaleModifiedEvent enqueued")]
    public void ChangeHeader_ShouldUpdateFields_AndEnqueueSaleModifiedEvent()
    {
        // Given
        var sale = SalesFaker.Sale(itemCount: 1);
        sale.ClearDomainEvents();
        var newCustomer = SalesFaker.Customer();

        // When
        sale.ChangeHeader("S-NEW", DateTime.UtcNow, newCustomer, SalesFaker.Branch());

        // Then
        sale.SaleNumber.Should().Be("S-NEW");
        sale.Customer.Should().Be(newCustomer);
        sale.DomainEvents.Should().ContainSingle(e => e is SaleModifiedEvent);
    }

    [Fact(DisplayName = "Given a cancelled sale When ChangeHeader is called Then throws DomainException")]
    public void ChangeHeader_ShouldThrow_WhenSaleCancelled()
    {
        // Given
        var sale = SalesFaker.Sale(itemCount: 1);
        sale.Cancel();

        // When
        var act = () => sale.ChangeHeader("X", DateTime.UtcNow, SalesFaker.Customer(), SalesFaker.Branch());

        // Then
        act.Should().Throw<DomainException>();
    }
}
