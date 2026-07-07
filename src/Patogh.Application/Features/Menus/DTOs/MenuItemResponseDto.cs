namespace Patogh.Application.Features.Menus.DTOs;

public class MenuItemResponseDto
{
    public bool Success { get; set; }
    public Guid MenuItemId { get; set; }
    public string Message { get; set; } = string.Empty;
}