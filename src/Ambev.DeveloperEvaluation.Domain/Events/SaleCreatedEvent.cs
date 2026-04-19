using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed class SaleCreatedEvent : INotification
{
    public Guid SaleId { get; }
    public string SaleNumber { get; }
    public DateTime OccurredOn { get; }

    public SaleCreatedEvent(Guid saleId, string saleNumber)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        OccurredOn = DateTime.UtcNow;
    }
}
