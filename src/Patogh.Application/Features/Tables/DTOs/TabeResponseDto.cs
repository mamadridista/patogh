namespace Patogh.Application.Features.Tables.DTOs;

public class TableResponseDto
{
    public bool Success { get; set; }
    public Guid TableId { get; set; }
    public string Message { get; set; } = string.Empty;
}