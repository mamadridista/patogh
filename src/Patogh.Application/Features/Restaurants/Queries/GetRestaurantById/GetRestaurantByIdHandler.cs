using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Restaurants.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Restaurants.Queries.GetRestaurantById;

public class GetRestaurantByIdQueryHandler
    : IRequestHandler<GetRestaurantByIdQuery, RestaurantDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetRestaurantByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RestaurantDetailsDto> Handle(
        GetRestaurantByIdQuery request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _context.Restaurants
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new RestaurantDetailsDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Location = x.Location,
                FoodType = x.FoodType,
                PriceRange = x.PriceRange,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                IsApproved = x.IsApproved,

                // Cover image: the CoverImage is a navigation to MediaAsset
                CoverImageUrl = x.CoverImage != null ? x.CoverImage.FilePath : null,

                // Gallery images via RestaurantImages join table
                GalleryImageUrls = x.Images
                    .Select(img => img.MediaAsset.FilePath)
                    .ToList(),

                MenuItems = x.MenuItems
                    .Select(m => new MenuItemDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        Price = m.Price
                    })
                    .ToList(),

                Tables = x.Tables
                    .Select(t => new RestaurantTableDto
                    {
                        Id = t.Id,
                        TableNumber = t.TableNumber,
                        Capacity = t.Capacity
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (restaurant is null)
            throw new NotFoundException("Restaurant", request.Id);

        return restaurant;
    }
}