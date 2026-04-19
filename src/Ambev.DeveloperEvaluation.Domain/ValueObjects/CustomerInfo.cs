using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// External identity for a Customer, denormalized into the Sale aggregate.
/// Holds only the data required for display/reporting — no FK to the Customer context.
/// </summary>
public sealed class CustomerInfo
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private CustomerInfo()
    {
    }

    public CustomerInfo(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new DomainException("Customer Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Customer Name cannot be null or empty.");

        Id = id;
        Name = name;
    }

    public override bool Equals(object? obj)
        => obj is CustomerInfo other && other.Id == Id && other.Name == Name;

    public override int GetHashCode() => HashCode.Combine(Id, Name);
}
