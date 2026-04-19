using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// External identity for a Branch, denormalized into the Sale aggregate.
/// </summary>
public sealed class BranchInfo
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private BranchInfo()
    {
    }

    public BranchInfo(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new DomainException("Branch Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Branch Name cannot be null or empty.");

        Id = id;
        Name = name;
    }

    public override bool Equals(object? obj)
        => obj is BranchInfo other && other.Id == Id && other.Name == Name;

    public override int GetHashCode() => HashCode.Combine(Id, Name);
}
