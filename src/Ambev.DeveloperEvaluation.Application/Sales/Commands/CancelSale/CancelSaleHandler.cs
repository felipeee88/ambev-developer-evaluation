using Ambev.DeveloperEvaluation.Application.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSale;

public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, CancelSaleResult>
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public CancelSaleHandler(ISaleRepository repository, IMapper mapper, IMediator mediator)
    {
        _repository = repository;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<CancelSaleResult> Handle(CancelSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("Sale", command.Id);

        sale.Cancel();

        await _repository.UpdateAsync(sale, cancellationToken);

        foreach (var evt in sale.DomainEvents.ToList())
            await _mediator.Publish(evt, cancellationToken);
        sale.ClearDomainEvents();

        return _mapper.Map<CancelSaleResult>(sale);
    }
}
