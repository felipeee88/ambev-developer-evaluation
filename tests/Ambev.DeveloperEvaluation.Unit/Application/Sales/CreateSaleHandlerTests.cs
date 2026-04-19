using Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Sales.TestData;
using AutoMapper;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _repo = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();

    private CreateSaleHandler Handler() => new(_repo, _mapper, _mediator);

    [Fact(DisplayName = "Given valid command When handling Then persists sale and returns result")]
    public async Task Handle_ShouldPersistSale_AndReturnResult_WhenCommandIsValid()
    {
        // Given
        var command = SalesFaker.CreateSaleCommand(itemCount: 1);
        _repo.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>()).Returns((Sale?)null);
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult { SaleNumber = command.SaleNumber });

        // When
        var result = await Handler().Handle(command, CancellationToken.None);

        // Then
        result.SaleNumber.Should().Be(command.SaleNumber);
        await _repo.Received(1).AddAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given duplicate SaleNumber When handling Then throws DomainException")]
    public async Task Handle_ShouldThrow_WhenSaleNumberAlreadyExists()
    {
        // Given
        var command = SalesFaker.CreateSaleCommand();
        var existing = SalesFaker.Sale();
        _repo.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>()).Returns(existing);

        // When
        var act = () => Handler().Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage($"SaleNumber '{command.SaleNumber}' already exists.");
        await _repo.DidNotReceive().AddAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given successful save When handling Then publishes SaleCreatedEvent")]
    public async Task Handle_ShouldPublishSaleCreatedEvent_AfterSaveChanges()
    {
        // Given
        var command = SalesFaker.CreateSaleCommand(itemCount: 1);
        _repo.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>()).Returns((Sale?)null);
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult());

        // When
        await Handler().Handle(command, CancellationToken.None);

        // Then
        await _mediator.Received().Publish(Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given successful save When handling Then clears domain events on the aggregate")]
    public async Task Handle_ShouldClearDomainEvents_AfterPublish()
    {
        // Given
        var command = SalesFaker.CreateSaleCommand(itemCount: 1);
        _repo.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        Sale? captured = null;
        _repo.AddAsync(Arg.Do<Sale>(s => captured = s), Arg.Any<CancellationToken>())
             .Returns(ci => ci.Arg<Sale>());
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult());

        // When
        await Handler().Handle(command, CancellationToken.None);

        // Then
        captured.Should().NotBeNull();
        captured!.DomainEvents.Should().BeEmpty();
    }
}
