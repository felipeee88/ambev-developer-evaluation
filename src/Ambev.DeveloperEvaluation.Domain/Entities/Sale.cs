using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Sale aggregate root. Owns its SaleItems and produces domain events for the
/// Application layer to publish after persistence.
/// </summary>
public class Sale : BaseEntity
{
    public string SaleNumber { get; private set; } = string.Empty;
    public DateTime SaleDate { get; private set; }
    public CustomerInfo Customer { get; private set; } = default!;
    public BranchInfo Branch { get; private set; } = default!;
    public decimal TotalAmount { get; private set; }
    public bool Cancelled { get; private set; }

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    protected Sale()
    {
    }

    public Sale(string saleNumber, DateTime saleDate, CustomerInfo customer, BranchInfo branch)
    {
        if (string.IsNullOrWhiteSpace(saleNumber))
            throw new DomainException("SaleNumber is required.");
        if (customer is null)
            throw new DomainException("Customer is required.");
        if (branch is null)
            throw new DomainException("Branch is required.");

        Id = Guid.NewGuid();
        SaleNumber = saleNumber;
        SaleDate = saleDate;
        Customer = customer;
        Branch = branch;
        Cancelled = false;
        TotalAmount = 0m;

        AddDomainEvent(new SaleCreatedEvent(Id, SaleNumber));
    }

    /// <summary>
    /// Replaces the header fields (sale number, date, customer, branch) and
    /// enqueues <see cref="SaleModifiedEvent"/>. Items are not touched by this
    /// method — use dedicated item commands.
    /// </summary>
    public void ChangeHeader(string saleNumber, DateTime saleDate, CustomerInfo customer, BranchInfo branch)
    {
        EnsureNotCancelled();
        if (string.IsNullOrWhiteSpace(saleNumber))
            throw new DomainException("SaleNumber is required.");
        if (customer is null)
            throw new DomainException("Customer is required.");
        if (branch is null)
            throw new DomainException("Branch is required.");

        SaleNumber = saleNumber;
        SaleDate = saleDate;
        Customer = customer;
        Branch = branch;

        AddDomainEvent(new SaleModifiedEvent(Id, SaleNumber));
    }

    /// <summary>
    /// Adds a new item to the sale and recalculates the total.
    /// </summary>
    public SaleItem AddItem(ProductInfo product, int quantity, decimal unitPrice)
    {
        EnsureNotCancelled();
        var item = new SaleItem(product, quantity, unitPrice);
        _items.Add(item);
        Recalculate();
        AddDomainEvent(new SaleModifiedEvent(Id, SaleNumber));
        return item;
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureNotCancelled();
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Item {itemId} not found.");
        _items.Remove(item);
        Recalculate();
        AddDomainEvent(new SaleModifiedEvent(Id, SaleNumber));
    }

    /// <summary>
    /// Cancels the whole sale and, cascading, every non-cancelled item.
    /// </summary>
    public void Cancel()
    {
        if (Cancelled)
            return;

        Cancelled = true;
        foreach (var item in _items.Where(i => !i.Cancelled))
            item.Cancel();

        Recalculate();
        AddDomainEvent(new SaleCancelledEvent(Id, SaleNumber));
    }

    public void CancelItem(Guid itemId)
    {
        EnsureNotCancelled();
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Item {itemId} not found.");
        if (item.Cancelled)
            return;

        item.Cancel();
        Recalculate();
        AddDomainEvent(new ItemCancelledEvent(Id, SaleNumber, item.Id));
    }

    /// <summary>
    /// TotalAmount = sum of non-cancelled items' TotalAmount.
    /// </summary>
    public void Recalculate()
        => TotalAmount = _items.Where(i => !i.Cancelled).Sum(i => i.TotalAmount);

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddDomainEvent(INotification evt) => _domainEvents.Add(evt);

    private void EnsureNotCancelled()
    {
        if (Cancelled)
            throw new DomainException("Cannot modify a cancelled sale.");
    }
}
