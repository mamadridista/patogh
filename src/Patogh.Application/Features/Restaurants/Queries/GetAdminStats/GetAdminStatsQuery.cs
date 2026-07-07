using MediatR;
using Patogh.Application.Features.Restaurants.DTOs;

namespace Patogh.Application.Features.Admin.Queries.GetAdminStats;

public class GetAdminStatsQuery : IRequest<AdminStatsDto> { }