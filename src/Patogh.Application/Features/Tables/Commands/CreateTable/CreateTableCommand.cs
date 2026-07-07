using MediatR;
using Patogh.Application.Features.Tables.DTOs;

namespace Patogh.Application.Features.Tables.Commands.CreateTable;

public class CreateTableCommand : IRequest<TableResponseDto>
{
    public Guid RestaurantId { get; set; }
    public int TableNumber { get; set; }
    public int Capacity { get; set; }
}