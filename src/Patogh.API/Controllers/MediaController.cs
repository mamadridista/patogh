using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Patogh.Application.Features.Media.Commands.UploadMedia;

namespace Patogh.API.Controllers;

[ApiController]
[Route("api/media")]
[Authorize(Roles = "RestaurantOwner")]
public class MediaController : ControllerBase
{
    private readonly IMediator _mediator;

    public MediaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>آپلود تصویر برای رستوران یا منو</summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var result = await _mediator.Send(new UploadMediaCommand { File = file });
        return Ok(result);
    }
}