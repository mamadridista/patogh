using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Common.Helpers;
using Patogh.Application.Features.Reservations.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Entities;
using Patogh.Domain.Enums;
using Patogh.Domain.Exceptions;
using System.Data;

namespace Patogh.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationCommandHandler
    : IRequestHandler<CreateReservationCommand, ReservationResponseDto>
{
    private static readonly TimeSpan ReservationDuration = TimeSpan.FromHours(2);

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISmsSender _smsSender;

    public CreateReservationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ISmsSender smsSender)
    {
        _context = context;
        _currentUser = currentUser;
        _smsSender = smsSender;
    }

    public async Task<ReservationResponseDto> Handle(
        CreateReservationCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Date validation using Tehran timezone
        if (request.ReservationDate < DateTimeHelper.TodayInTehran)
            throw new DomainValidationException("تاریخ رزرو نمی‌تواند در گذشته باشد.");

        // Step 2: Load and validate the restaurant
        var restaurant = await _context.Restaurants
            .Include(r => r.Owner)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.RestaurantId, cancellationToken);

        if (restaurant is null)
            throw new NotFoundException("Restaurant", request.RestaurantId);

        if (!restaurant.IsApproved)
            throw new DomainValidationException(
                "این رستوران هنوز توسط مدیر سیستم تأیید نشده است.");

        // Step 3: Verify operating hours
        var requestedEnd = request.StartTime.Add(ReservationDuration);

        if (request.StartTime < restaurant.StartTime || requestedEnd > restaurant.EndTime)
            throw new DomainValidationException(
                $"رستوران در ساعت درخواستی باز نیست. " +
                $"ساعت کار: {restaurant.StartTime:hh\\:mm} تا {restaurant.EndTime:hh\\:mm}.");

        // Step 4: Load and validate table
        var table = await _context.RestaurantTables
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TableId, cancellationToken);

        if (table is null)
            throw new NotFoundException("Table", request.TableId);

        if (table.RestaurantId != request.RestaurantId)
            throw new DomainValidationException("میز انتخاب شده به این رستوران تعلق ندارد.");

        if (table.Capacity < request.GuestCount)
            throw new DomainValidationException(
                $"ظرفیت میز ({table.Capacity} نفر) برای {request.GuestCount} نفر کافی نیست.");

        // Step 5: Conflict detection with serializable transaction
        // FIX: BeginTransactionAsync takes only IsolationLevel (no CancellationToken in this overload)
        // The correct extension is: Database.BeginTransactionAsync(IsolationLevel, ct)
        // which IS available in EF Core 8 via Microsoft.EntityFrameworkCore namespace.
        // We use System.Data.IsolationLevel explicitly to be unambiguous.
        using var tx = await _context.Database
            .BeginTransactionAsync( cancellationToken);

        try
        {
            var hasConflict = await _context.Reservations
                .AnyAsync(r =>
                    r.TableId == request.TableId &&
                    r.ReservationDate == request.ReservationDate &&
                    r.Status != ReservationStatus.Cancelled &&
                    r.Status != ReservationStatus.Rejected &&
                    r.StartTime < requestedEnd &&
                    r.EndTime > request.StartTime,
                    cancellationToken);

            if (hasConflict)
                throw new ConflictException(
                    "این میز در بازه زمانی انتخابی رزرو شده است. " +
                    "لطفاً زمان یا میز دیگری انتخاب کنید.");

            var reservation = new Reservation
            {
                CustomerId = _currentUser.UserId,
                RestaurantId = request.RestaurantId,
                TableId = request.TableId,
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone,
                Notes = request.Notes,
                GuestCount = request.GuestCount,
                ReservationDate = request.ReservationDate,
                StartTime = request.StartTime,
                EndTime = requestedEnd,
                Status = ReservationStatus.Pending
            };

            await _context.Reservations.AddAsync(reservation, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            // Step 6: Notify owner — fire-and-forget, never blocks the response
            _ = _smsSender.SendAsync(
                restaurant.Owner.PhoneNumber,
                $"رزرو جدید: {request.CustomerName} ({request.GuestCount} نفر) " +
                $"میز {table.TableNumber} را برای تاریخ {request.ReservationDate} " +
                $"ساعت {request.StartTime:hh\\:mm} رزرو کرده است.");

            return new ReservationResponseDto
            {
                Success = true,
                ReservationId = reservation.Id,
                Message = "رزرو با موفقیت ثبت شد و در انتظار تأیید صاحب رستوران است."
            };
        }
        catch (ConflictException)
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
        catch (DbUpdateException)
        {
            // PostgreSQL serialization failure (SQLSTATE 40001) surfaces as DbUpdateException
            await tx.RollbackAsync(cancellationToken);
            throw new ConflictException("تداخل زمانی رخ داد. لطفاً دوباره تلاش کنید.");
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}