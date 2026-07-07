using MediatR;
using Patogh.Application.Features.Media.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Entities;

namespace Patogh.Application.Features.Media.Commands.UploadMedia;

public class UploadMediaHandler
    : IRequestHandler<UploadMediaCommand,
        UploadMediaResponseDto>
{
    private readonly IApplicationDbContext _context;

    private readonly IFileStorageService _storage;

    public UploadMediaHandler(
        IApplicationDbContext context,
        IFileStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<UploadMediaResponseDto> Handle(
        UploadMediaCommand request,
        CancellationToken cancellationToken)
    {
        var path = await _storage.SaveAsync(
            request.File.OpenReadStream(),
            request.File.FileName,
            cancellationToken);

        var media = new MediaAsset
        {
            FileName = request.File.FileName,
            FilePath = path,
            ContentType = request.File.ContentType
        };

        await _context.MediaAssets.AddAsync(
            media,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);

        return new UploadMediaResponseDto
        {
            Success = true,
            MediaId = media.Id,
            Url = media.FilePath
        };
    }
}