using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Patogh.Application.Features.Menus.Commands.CreateMenuItem;
using Patogh.Application.Features.Menus.Commands.DeleteMenuItem;
using Patogh.Application.Features.Menus.Commands.UpdateMenuItem;
using Patogh.Application.Features.Menus.Queries.GetMenuByRestaurant;

namespace Patogh.API.Controllers;

[ApiController]
[Route("api/menus")]
[EnableRateLimiting("general")]
public class MenuController : ControllerBase
{
    private readonly IMediator _mediator;

    public MenuController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// منوی رستوران — عمومی، بدون نیاز به احراز هویت.
    /// restaurantId از query string دریافت می‌شود.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetByRestaurant([FromQuery] Guid restaurantId)
    {
        var result = await _mediator.Send(
            new GetMenuByRestaurantQuery { RestaurantId = restaurantId });
        return Ok(result);
    }

    /// <summary>افزودن آیتم به منو — فقط صاحب رستوران</summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateMenuItemCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>ویرایش آیتم منو — فقط صاحب رستوران</summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMenuItemCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>حذف آیتم منو — فقط صاحب رستوران</summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(
            new DeleteMenuItemCommand { Id = id });
        return Ok(result);
    }
}