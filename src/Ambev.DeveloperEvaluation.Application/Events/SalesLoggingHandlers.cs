using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Events;

public sealed class SaleCreatedLoggingHandler : INotificationHandler<SaleCreatedEvent>
{
    private readonly ILogger<SaleCreatedLoggingHandler> _logger;

    public SaleCreatedLoggingHandler(ILogger<SaleCreatedLoggingHandler> logger) => _logger = logger;

    public Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Event {EventName} fired for SaleId {SaleId} (SaleNumber {SaleNumber})",
            nameof(SaleCreatedEvent), notification.SaleId, notification.SaleNumber);
        return Task.CompletedTask;
    }
}

public sealed class SaleModifiedLoggingHandler : INotificationHandler<SaleModifiedEvent>
{
    private readonly ILogger<SaleModifiedLoggingHandler> _logger;

    public SaleModifiedLoggingHandler(ILogger<SaleModifiedLoggingHandler> logger) => _logger = logger;

    public Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Event {EventName} fired for SaleId {SaleId} (SaleNumber {SaleNumber})",
            nameof(SaleModifiedEvent), notification.SaleId, notification.SaleNumber);
        return Task.CompletedTask;
    }
}

public sealed class SaleCancelledLoggingHandler : INotificationHandler<SaleCancelledEvent>
{
    private readonly ILogger<SaleCancelledLoggingHandler> _logger;

    public SaleCancelledLoggingHandler(ILogger<SaleCancelledLoggingHandler> logger) => _logger = logger;

    public Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Event {EventName} fired for SaleId {SaleId} (SaleNumber {SaleNumber})",
            nameof(SaleCancelledEvent), notification.SaleId, notification.SaleNumber);
        return Task.CompletedTask;
    }
}

public sealed class ItemCancelledLoggingHandler : INotificationHandler<ItemCancelledEvent>
{
    private readonly ILogger<ItemCancelledLoggingHandler> _logger;

    public ItemCancelledLoggingHandler(ILogger<ItemCancelledLoggingHandler> logger) => _logger = logger;

    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Event {EventName} fired for SaleId {SaleId} (SaleNumber {SaleNumber}, ItemId {ItemId})",
            nameof(ItemCancelledEvent), notification.SaleId, notification.SaleNumber, notification.ItemId);
        return Task.CompletedTask;
    }
}
