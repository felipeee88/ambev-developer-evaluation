using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// External identity for a Product, denormalized into the SaleItem.
/// </summary>
public sealed class ProductInfo
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private ProductInfo()
    {
    }

    public ProductInfo(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new DomainException("Product Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product Name cannot be null or empty.");

        Id = id;
        Name = name;
    }

    public override bool Equals(object? obj)
        => obj is ProductInfo other && other.Id == Id && other.Name == Name;

    public override int GetHashCode() => HashCode.Combine(Id, Name);
}
