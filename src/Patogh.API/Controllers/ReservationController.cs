using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Patogh.Application.Features.Reservations.Commands.CancelReservation;
using Patogh.Application.Features.Reservations.Commands.ConfirmReservation;
using Patogh.Application.Features.Reservations.Commands.CreateReservation;
using Patogh.Application.Features.Reservations.Commands.RejectReservation;
using Patogh.Application.Features.Reservations.Queries.GetMyReservations;
using Patogh.Application.Features.Reservations.Queries.GetReservationById;
using Patogh.Application.Features.Reservations.Queries.GetRestaurantReservations;
using Patogh.Domain.Enums;

namespace Patogh.API.Controllers;

[ApiController]
[Route("api/reservations")]
[EnableRateLimiting("general")]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>ایجاد رزرو جدید — فقط مشتری</summary>
    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateReservationCommand command)
    {
        var result = await _mediator.Send(command);
        return StatusCode(201, result);
    }

    /// <summary>
    /// جزئیات یک رزرو — برای مشتری صاحب رزرو یا صاحب رستوران.
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(
            new GetReservationByIdQuery { ReservationId = id });
        return Ok(result);
    }

    /// <summary>
    /// لیست رزروهای مشتری جاری.
    /// Query: status, page, pageSize
    /// </summary>
    [Authorize(Roles = "Customer")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReservations(
        [FromQuery] GetMyReservationsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>لغو رزرو توسط مشتری</summary>
    [Authorize(Roles = "Customer")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _mediator.Send(
            new CancelReservationCommand { ReservationId = id });
        return Ok(result);
    }

    /// <summary>
    /// لیست رزروهای رستوران — فقط صاحب رستوران.
    /// Query: date, status, page, pageSize
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpGet("restaurant/{restaurantId:guid}")]
    public async Task<IActionResult> GetRestaurantReservations(
        Guid restaurantId,
        [FromQuery] GetRestaurantReservationsQuery query)
    {
        query.RestaurantId = restaurantId;
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>تأیید رزرو — فقط صاحب رستوران</summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPut("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _mediator.Send(
            new ConfirmReservationCommand { ReservationId = id });
        return Ok(result);
    }

    /// <summary>رد رزرو — فقط صاحب رستوران</summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPut("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var result = await _mediator.Send(
            new RejectReservationCommand { ReservationId = id });
        return Ok(result);
    }

    /// <summary>لغو رزرو توسط صاحب رستوران</summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> CancelByOwner(Guid id)
    {
        var result = await _mediator.Send(
            new CancelReservationCommand { ReservationId = id });
        return Ok(result);
    }
}