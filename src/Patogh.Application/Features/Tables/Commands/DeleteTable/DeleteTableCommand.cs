using MediatR;
using Patogh.Application.Features.Tables.DTOs;

namespace Patogh.Application.Features.Tables.Commands.DeleteTable;

public class DeleteTableCommand : IRequest<TableResponseDto>
{
    public Guid TableId { get; set; }
}