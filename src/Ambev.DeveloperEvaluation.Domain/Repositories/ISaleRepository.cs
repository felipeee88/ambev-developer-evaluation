using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Paged result container used by <see cref="ISaleRepository.ListAsync"/>.
/// </summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Data { get; init; } = Array.Empty<T>();
    public int TotalItems { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}

/// <summary>
/// Filter/sort/pagination parameters for listing sales.
/// </summary>
public sealed class SaleListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    /// <summary>Ordering expression, e.g. "saleDate desc, totalAmount asc".</summary>
    public string? Order { get; init; }

    public Guid? CustomerId { get; init; }
    public Guid? BranchId { get; init; }
    public string? SaleNumber { get; init; }
    public DateTime? MinSaleDate { get; init; }
    public DateTime? MaxSaleDate { get; init; }
    public decimal? MinTotalAmount { get; init; }
    public decimal? MaxTotalAmount { get; init; }
    public bool? Cancelled { get; init; }
}

/// <summary>
/// Repository contract for the Sale aggregate.
/// </summary>
public interface ISaleRepository
{
    Task<Sale> AddAsync(Sale sale, CancellationToken cancellationToken = default);

    Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default);

    Task<PagedResult<Sale>> ListAsync(SaleListQuery query, CancellationToken cancellationToken = default);
}
