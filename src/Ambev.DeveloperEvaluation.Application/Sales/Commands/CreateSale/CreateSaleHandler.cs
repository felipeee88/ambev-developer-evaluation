using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public CreateSaleHandler(ISaleRepository repository, IMapper mapper, IMediator mediator)
    {
        _repository = repository;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetBySaleNumberAsync(command.SaleNumber, cancellationToken);
        if (existing is not null)
            throw new DomainException($"SaleNumber '{command.SaleNumber}' already exists.");

        var customer = new CustomerInfo(command.CustomerId, command.CustomerName);
        var branch = new BranchInfo(command.BranchId, command.BranchName);

        var saleDate = DateTime.SpecifyKind(command.SaleDate, DateTimeKind.Utc);
        var sale = new Sale(command.SaleNumber, saleDate, customer, branch);

        foreach (var item in command.Items)
        {
            var product = new ProductInfo(item.ProductId, item.ProductName);
            sale.AddItem(product, item.Quantity, item.UnitPrice);
        }

        await _repository.AddAsync(sale, cancellationToken);

        foreach (var evt in sale.DomainEvents.ToList())
            await _mediator.Publish(evt, cancellationToken);
        sale.ClearDomainEvents();

        return _mapper.Map<CreateSaleResult>(sale);
    }
}
