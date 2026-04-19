using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.DeleteSale;

public class DeleteSaleCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}
