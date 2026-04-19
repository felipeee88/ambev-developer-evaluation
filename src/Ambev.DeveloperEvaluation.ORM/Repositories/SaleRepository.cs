using System.Linq.Expressions;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _context.Sales.Update(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (sale is null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
        => _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);

    public async Task<PagedResult<Sale>> ListAsync(SaleListQuery query, CancellationToken cancellationToken = default)
    {
        var q = _context.Sales
            .AsNoTracking()
            .Include(s => s.Items)
            .AsQueryable();

        q = ApplyFilters(q, query);

        var totalItems = await q.CountAsync(cancellationToken);

        q = ApplyOrder(q, query.Order);

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

        var data = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Sale>
        {
            Data = data,
            TotalItems = totalItems,
            CurrentPage = page,
            PageSize = pageSize
        };
    }

    private static IQueryable<Sale> ApplyFilters(IQueryable<Sale> q, SaleListQuery f)
    {
        if (f.CustomerId.HasValue)
            q = q.Where(s => s.Customer.Id == f.CustomerId.Value);
        if (f.BranchId.HasValue)
            q = q.Where(s => s.Branch.Id == f.BranchId.Value);
        if (!string.IsNullOrWhiteSpace(f.SaleNumber))
            q = q.Where(s => EF.Functions.ILike(s.SaleNumber, $"%{f.SaleNumber}%"));
        if (f.MinSaleDate.HasValue)
            q = q.Where(s => s.SaleDate >= f.MinSaleDate.Value);
        if (f.MaxSaleDate.HasValue)
            q = q.Where(s => s.SaleDate <= f.MaxSaleDate.Value);
        if (f.MinTotalAmount.HasValue)
            q = q.Where(s => s.TotalAmount >= f.MinTotalAmount.Value);
        if (f.MaxTotalAmount.HasValue)
            q = q.Where(s => s.TotalAmount <= f.MaxTotalAmount.Value);
        if (f.Cancelled.HasValue)
            q = q.Where(s => s.Cancelled == f.Cancelled.Value);
        return q;
    }

    private static IQueryable<Sale> ApplyOrder(IQueryable<Sale> q, string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return q.OrderByDescending(s => s.SaleDate);

        IOrderedQueryable<Sale>? ordered = null;
        var parts = order.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = tokens[0].ToLowerInvariant();
            var desc = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            ordered = field switch
            {
                "salenumber" => AddOrder(q, ordered, s => s.SaleNumber, desc),
                "saledate" => AddOrder(q, ordered, s => s.SaleDate, desc),
                "totalamount" => AddOrder(q, ordered, s => s.TotalAmount, desc),
                "cancelled" => AddOrder(q, ordered, s => s.Cancelled, desc),
                _ => ordered ?? q.OrderBy(s => s.Id)
            };
        }

        return ordered ?? q;
    }

    private static IOrderedQueryable<Sale> AddOrder<TKey>(
        IQueryable<Sale> q,
        IOrderedQueryable<Sale>? current,
        Expression<Func<Sale, TKey>> selector,
        bool desc)
    {
        if (current is null)
            return desc ? q.OrderByDescending(selector) : q.OrderBy(selector);
        return desc ? current.ThenByDescending(selector) : current.ThenBy(selector);
    }
}
