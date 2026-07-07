using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Interfaces;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Restaurants.Commands.UpdateRestaurant;

public class UpdateRestaurantCommandHandler
    : IRequestHandler<UpdateRestaurantCommand, UpdateRestaurantResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public UpdateRestaurantCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _context = context;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<UpdateRestaurantResponseDto> Handle(
        UpdateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(x => x.Id == request.RestaurantId, cancellationToken);

        if (restaurant is null)
            throw new NotFoundException("Restaurant", request.RestaurantId);

        if (restaurant.OwnerId != _currentUser.UserId)
            throw new ForbiddenException();

        restaurant.Name = request.Name;
        restaurant.Description = request.Description;
        restaurant.Location = request.Location;
        restaurant.FoodType = request.FoodType;
        restaurant.PriceRange = request.PriceRange;
        restaurant.StartTime = request.StartTime;
        restaurant.EndTime = request.EndTime;
        restaurant.CoverImageId = request.CoverImageId;
        restaurant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate cached venue list — name/location/foodType may have changed
        await _cache.RemoveAsync("venues:page1:default");

        return new UpdateRestaurantResponseDto
        {
            Success = true,
            Message = "اطلاعات رستوران با موفقیت بروزرسانی شد."
        };
    }
}