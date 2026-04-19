using Ambev.DeveloperEvaluation.Application.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Queries.GetSaleById;

public class GetSaleByIdHandler : IRequestHandler<GetSaleByIdQuery, GetSaleByIdResult>
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;

    public GetSaleByIdHandler(ISaleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<GetSaleByIdResult> Handle(GetSaleByIdQuery query, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new NotFoundException("Sale", query.Id);

        return _mapper.Map<GetSaleByIdResult>(sale);
    }
}
