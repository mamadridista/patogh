using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Restaurants.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Restaurants.Commands.ApproveRestaurant;

public class ApproveRestaurantCommandHandler
    : IRequestHandler<ApproveRestaurantCommand, ApproveRestaurantResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cache;

    public ApproveRestaurantCommandHandler(
        IApplicationDbContext context,
        ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApproveRestaurantResponseDto> Handle(
        ApproveRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(x => x.Id == request.RestaurantId, cancellationToken);

        if (restaurant is null)
            throw new NotFoundException("Restaurant", request.RestaurantId);

        restaurant.IsApproved = request.IsApproved;
        restaurant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Approving/rejecting a restaurant directly affects the public venue list
        await _cache.RemoveAsync("venues:page1:default");

        return new ApproveRestaurantResponseDto
        {
            Success = true,
            Message = request.IsApproved
                ? "رستوران با موفقیت تأیید شد."
                : "رستوران رد شد."
        };
    }
}