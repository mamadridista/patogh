namespace Patogh.Application.Features.Media.DTOs;

public class UploadMediaResponseDto
{
    public bool Success { get; set; }

    public Guid MediaId { get; set; }

    public string Url { get; set; } = string.Empty;
}