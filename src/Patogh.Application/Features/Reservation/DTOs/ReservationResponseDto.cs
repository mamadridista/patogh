namespace Patogh.Application.Features.Reservations.DTOs;

public class ReservationResponseDto
{
    public bool Success { get; set; }
    public Guid ReservationId { get; set; }
    public string Message { get; set; } = string.Empty;
}