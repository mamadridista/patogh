namespace Patogh.Application.Features.Reservations.DTOs;

public class AvailableTableDto
{
    public Guid TableId { get; set; }
    public int TableNumber { get; set; }
    public int Capacity { get; set; }
}