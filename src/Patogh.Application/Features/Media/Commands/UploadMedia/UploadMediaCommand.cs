using MediatR;
using Microsoft.AspNetCore.Http;
using Patogh.Application.Features.Media.DTOs;

namespace Patogh.Application.Features.Media.Commands.UploadMedia;

public class UploadMediaCommand
    : IRequest<UploadMediaResponseDto>
{
    public IFormFile File { get; set; } = null!;
}