using Ambev.DeveloperEvaluation.Application.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.DeleteSale;

public class DeleteSaleHandler : IRequestHandler<DeleteSaleCommand, Unit>
{
    private readonly ISaleRepository _repository;

    public DeleteSaleHandler(ISaleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(DeleteSaleCommand command, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(command.Id, cancellationToken);
        if (!deleted)
            throw new NotFoundException("Sale", command.Id);

        return Unit.Value;
    }
}
