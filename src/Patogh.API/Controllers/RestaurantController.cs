using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Patogh.Application.Features.Reservations.Queries.GetAvailableTables;
using Patogh.Application.Features.Restaurants.Commands.CreateRestaurant;
using Patogh.Application.Features.Restaurants.Commands.UpdateRestaurant;
using Patogh.Application.Features.Restaurants.Queries.GetMyRestaurants;
using Patogh.Application.Features.Restaurants.Queries.GetOwnerStats;
using Patogh.Application.Features.Restaurants.Queries.GetRestaurantById;
using Patogh.Application.Features.Restaurants.Queries.GetRestaurants;

namespace Patogh.API.Controllers;

[ApiController]
[Route("api/restaurants")]
[EnableRateLimiting("general")]
public class RestaurantController : ControllerBase
{
    private readonly IMediator _mediator;

    public RestaurantController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// لیست رستوران‌های تأیید شده با فیلتر و صفحه‌بندی — عمومی.
    /// Query: name, foodType, location, priceRange, page (default:1), pageSize (default:20)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetRestaurantsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// رستوران‌های من — فقط صاحب رستوران.
    /// لیست رستوران‌هایی که توسط کاربر جاری ثبت شده‌اند.
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var result = await _mediator.Send(new GetMyRestaurantsQuery());
        return Ok(result);
    }

    /// <summary>جزئیات کامل رستوران شامل منو و میزها — عمومی</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetRestaurantByIdQuery { Id = id });
        return Ok(result);
    }

    /// <summary>
    /// آمار و گزارش رستوران — فقط صاحب رستوران.
    /// Query params: from (yyyy-MM-dd), to (yyyy-MM-dd) — defaults to last 30 days.
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpGet("{id:guid}/stats")]
    public async Task<IActionResult> GetStats(
        Guid id,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var result = await _mediator.Send(new GetOwnerStatsQuery
        {
            RestaurantId = id,
            From = from,
            To = to
        });
        return Ok(result);
    }

    /// <summary>
    /// میزهای خالی برای تاریخ، زمان و تعداد نفر — عمومی.
    /// Query: date (yyyy-MM-dd), startTime (HH:mm:ss), guestCount
    /// </summary>
    [HttpGet("{id:guid}/available-tables")]
    public async Task<IActionResult> GetAvailableTables(
        Guid id,
        [FromQuery] DateOnly date,
        [FromQuery] TimeSpan startTime,
        [FromQuery] int guestCount)
    {
        var result = await _mediator.Send(new GetAvailableTablesQuery
        {
            RestaurantId = id,
            Date = date,
            StartTime = startTime,
            GuestCount = guestCount
        });
        return Ok(result);
    }

    /// <summary>ثبت رستوران جدید — فقط صاحب رستوران</summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateRestaurantCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// بروزرسانی رستوران — فقط صاحب آن رستوران.
    /// Id از URL، بقیه اطلاعات از body.
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateRestaurantCommand command)
    {
        command.RestaurantId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}