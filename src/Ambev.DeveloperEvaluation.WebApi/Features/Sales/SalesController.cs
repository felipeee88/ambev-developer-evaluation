using Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Queries.GetSaleById;
using Ambev.DeveloperEvaluation.Application.Sales.Queries.ListSales;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

/// <summary>
/// CRUD endpoints for Sales, discount rules and cancellation flows.
/// </summary>
[Authorize]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>Creates a new sale with its items and discount tiers.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request, CancellationToken ct)
    {
        var command = _mapper.Map<CreateSaleCommand>(request);
        var result = await _mediator.Send(command, ct);
        var response = _mapper.Map<SaleResponse>(result);

        return StatusCode(StatusCodes.Status201Created, new ApiResponseWithData<SaleResponse>
        {
            Success = true,
            Data = response
        });
    }

    /// <summary>Lists sales with pagination, ordering and filters.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List([FromQuery] ListSalesRequest request, CancellationToken ct)
    {
        var query = _mapper.Map<ListSalesQuery>(request);
        var result = await _mediator.Send(query, ct);
        var response = _mapper.Map<PaginatedResponse<SaleResponse>>(result);
        response.Success = true;
        return StatusCode(StatusCodes.Status200OK, response);
    }

    /// <summary>Retrieves a sale by its id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSaleByIdQuery { Id = id }, ct);
        var response = _mapper.Map<SaleResponse>(result);
        return Ok(response);
    }

    /// <summary>Updates the header fields (number, date, customer, branch) of a sale.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateSaleRequest request, CancellationToken ct)
    {
        var command = _mapper.Map<UpdateSaleCommand>(request);
        command.Id = id;
        var result = await _mediator.Send(command, ct);
        var response = _mapper.Map<SaleResponse>(result);
        return Ok(response);
    }

    /// <summary>Deletes a sale (hard delete).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSaleCommand { Id = id }, ct);
        return NoContent();
    }

    /// <summary>Cancels the sale (soft delete) and every non-cancelled item.</summary>
    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelSaleCommand { Id = id }, ct);
        var response = _mapper.Map<SaleResponse>(result);
        return Ok(response);
    }

    /// <summary>Cancels a single item of a sale.</summary>
    [HttpPatch("{id:guid}/items/{itemId:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelItem([FromRoute] Guid id, [FromRoute] Guid itemId, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelSaleItemCommand { SaleId = id, ItemId = itemId }, ct);
        var response = _mapper.Map<SaleResponse>(result);
        return Ok(response);
    }
}
