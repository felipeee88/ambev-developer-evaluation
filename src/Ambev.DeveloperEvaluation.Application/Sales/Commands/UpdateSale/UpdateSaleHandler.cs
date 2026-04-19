using Ambev.DeveloperEvaluation.Application.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public UpdateSaleHandler(ISaleRepository repository, IMapper mapper, IMediator mediator)
    {
        _repository = repository;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("Sale", command.Id);

        var customer = new CustomerInfo(command.CustomerId, command.CustomerName);
        var branch = new BranchInfo(command.BranchId, command.BranchName);
        var saleDate = DateTime.SpecifyKind(command.SaleDate, DateTimeKind.Utc);

        sale.ChangeHeader(command.SaleNumber, saleDate, customer, branch);

        await _repository.UpdateAsync(sale, cancellationToken);

        foreach (var evt in sale.DomainEvents.ToList())
            await _mediator.Publish(evt, cancellationToken);
        sale.ClearDomainEvents();

        return _mapper.Map<UpdateSaleResult>(sale);
    }
}
