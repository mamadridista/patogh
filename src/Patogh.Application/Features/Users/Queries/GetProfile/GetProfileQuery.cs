using MediatR;
using Patogh.Application.Features.Users.DTOs;

namespace Patogh.Application.Features.Users.Queries.GetProfile;

public class GetProfileQuery : IRequest<UserProfileDto> { }