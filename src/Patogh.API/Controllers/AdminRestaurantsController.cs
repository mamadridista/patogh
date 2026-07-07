using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Patogh.Application.Features.Admin.Queries.GetAdminStats;
using Patogh.Application.Features.Restaurants.Commands.ApproveRestaurant;
using Patogh.Application.Features.Restaurants.Queries.GetAllRestaurantsForAdmin;

namespace Patogh.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("general")]
public class AdminRestaurantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminRestaurantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// لیست همه رستوران‌ها برای ادمین — شامل تأیید نشده‌ها.
    /// Query: isApproved (bool?), name, page, pageSize
    /// </summary>
    [HttpGet("restaurants")]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllRestaurantsForAdminQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>تأیید یا رد رستوران</summary>
    [HttpPut("restaurants/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] bool isApproved)
    {
        var result = await _mediator.Send(new ApproveRestaurantCommand
        {
            RestaurantId = id,
            IsApproved = isApproved
        });
        return Ok(result);
    }

    /// <summary>
    /// آمار کلی پلتفرم برای ادمین.
    /// شامل تعداد کاربران، رستوران‌ها، رزروها و برترین رستوران‌ها.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _mediator.Send(new GetAdminStatsQuery());
        return Ok(result);
    }
}