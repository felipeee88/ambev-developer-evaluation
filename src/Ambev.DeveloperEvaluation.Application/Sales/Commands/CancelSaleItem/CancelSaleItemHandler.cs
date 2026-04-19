using Ambev.DeveloperEvaluation.Application.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public CancelSaleItemHandler(ISaleRepository repository, IMapper mapper, IMediator mediator)
    {
        _repository = repository;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand command, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(command.SaleId, cancellationToken)
            ?? throw new NotFoundException("Sale", command.SaleId);

        sale.CancelItem(command.ItemId);

        await _repository.UpdateAsync(sale, cancellationToken);

        foreach (var evt in sale.DomainEvents.ToList())
            await _mediator.Publish(evt, cancellationToken);
        sale.ClearDomainEvents();

        return _mapper.Map<CancelSaleItemResult>(sale);
    }
}
