using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Patogh.Application.Features.Tables.Commands.CreateTable;
using Patogh.Application.Features.Tables.Commands.DeleteTable;
using Patogh.Application.Features.Tables.Queries.GetTablesByRestaurant;

namespace Patogh.API.Controllers;

[ApiController]
[Route("api/tables")]
[Authorize(Roles = "RestaurantOwner")]
[EnableRateLimiting("general")]
public class TableController : ControllerBase
{
    private readonly IMediator _mediator;

    public TableController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// لیست میزهای رستوران — فقط صاحب رستوران.
    /// restaurantId از query string دریافت می‌شود.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetByRestaurant([FromQuery] Guid restaurantId)
    {
        var result = await _mediator.Send(
            new GetTablesByRestaurantQuery { RestaurantId = restaurantId });
        return Ok(result);
    }

    /// <summary>تعریف میز جدید</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateTableCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>حذف میز</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(
            new DeleteTableCommand { TableId = id });
        return Ok(result);
    }
}