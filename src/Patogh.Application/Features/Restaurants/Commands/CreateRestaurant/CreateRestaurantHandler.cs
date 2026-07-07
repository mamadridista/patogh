using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Restaurants.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Entities;

namespace Patogh.Application.Features.Restaurants.Commands.CreateRestaurant;

public class CreateRestaurantCommandHandler
    : IRequestHandler<CreateRestaurantCommand, CreateRestaurantResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CreateRestaurantCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _context = context;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<CreateRestaurantResponseDto> Handle(
        CreateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = new Restaurant
        {
            Name = request.Name,
            Description = request.Description,
            Location = request.Location,
            FoodType = request.FoodType,
            PriceRange = request.PriceRange,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            OwnerId = _currentUser.UserId,
            IsApproved = false
        };

        await _context.Restaurants.AddAsync(restaurant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var imageId in request.Images)
        {
            var exists = await _context.MediaAssets
                .AnyAsync(x => x.Id == imageId, cancellationToken);
            if (!exists) continue;

            await _context.RestaurantImages.AddAsync(new RestaurantImage
            {
                RestaurantId = restaurant.Id,
                MediaAssetId = imageId
            }, cancellationToken);
        }

        if (request.Images.Any())
            await _context.SaveChangesAsync(cancellationToken);

        // New restaurant is pending approval so it won't appear in the public list yet,
        // but we invalidate the cache anyway to ensure consistency
        await _cache.RemoveAsync("venues:page1:default");

        return new CreateRestaurantResponseDto
        {
            Success = true,
            RestaurantId = restaurant.Id,
            Message = "رستوران با موفقیت ثبت شد و در انتظار تأیید مدیر است."
        };
    }
}