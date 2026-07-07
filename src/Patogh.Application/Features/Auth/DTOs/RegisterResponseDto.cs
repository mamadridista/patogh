namespace Patogh.Application.Features.Auth.DTOs;

public class RegisterResponseDto
{
    public bool Success { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
}