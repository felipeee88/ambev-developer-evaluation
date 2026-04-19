using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Queries.ListSales;

public class ListSalesHandler : IRequestHandler<ListSalesQuery, ListSalesResult>
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ListSalesResult> Handle(ListSalesQuery query, CancellationToken cancellationToken)
    {
        var filter = new SaleListQuery
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Order = query.Order,
            CustomerId = query.CustomerId,
            BranchId = query.BranchId,
            SaleNumber = query.SaleNumber,
            MinSaleDate = query.MinSaleDate,
            MaxSaleDate = query.MaxSaleDate,
            MinTotalAmount = query.MinTotalAmount,
            MaxTotalAmount = query.MaxTotalAmount,
            Cancelled = query.Cancelled
        };

        var paged = await _repository.ListAsync(filter, cancellationToken);

        return new ListSalesResult
        {
            Data = paged.Data.Select(_mapper.Map<SaleSummary>).ToList(),
            TotalItems = paged.TotalItems,
            CurrentPage = paged.CurrentPage,
            PageSize = paged.PageSize,
            TotalPages = paged.TotalPages
        };
    }
}
