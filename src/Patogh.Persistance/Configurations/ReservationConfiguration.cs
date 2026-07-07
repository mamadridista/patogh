using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Patogh.Domain.Entities;
using Patogh.Domain.Enums;

namespace Patogh.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.CustomerPhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.GuestCount).IsRequired();
        builder.Property(x => x.Status).IsRequired();

        builder.Property(x => x.CancelledBy).HasMaxLength(50);

        // ── Performance Indexes ───────────────────────────────────────────────
        // Primary conflict query: filter by table + date first
        builder.HasIndex(x => new { x.TableId, x.ReservationDate })
            .HasDatabaseName("IX_Reservations_TableId_Date");

        // Customer reservation list
        builder.HasIndex(x => x.CustomerId)
            .HasDatabaseName("IX_Reservations_CustomerId");

        // Owner dashboard: restaurant + date
        builder.HasIndex(x => new { x.RestaurantId, x.ReservationDate })
            .HasDatabaseName("IX_Reservations_RestaurantId_Date");

        // Reminder job: find reservations needing reminders today
        builder.HasIndex(x => new { x.ReservationDate, x.ReminderSent })
            .HasDatabaseName("IX_Reservations_Date_ReminderSent");

        // ── Database-Level Double-Booking Prevention ──────────────────────────
        // This unique partial index is the FINAL safety net against double-bookings.
        // It works together with the serializable transaction in the handler:
        // - The transaction prevents race conditions in most scenarios
        // - This index ensures that even if two transactions somehow both commit
        //   (e.g., due to application bug or manual DB insert), the database
        //   itself will reject the duplicate.
        //
        // HOW IT WORKS:
        // A PostgreSQL unique index can filter rows with a WHERE clause (partial index).
        // Here we say: "within Pending and Confirmed reservations for the same table
        // on the same date, no two rows can have identical StartTime."
        // Combined with the time-overlap check in the handler (which also blocks
        // reservations that START at different times but OVERLAP), this provides
        // defense in depth.
        //
        // NOTE: EF Core does not natively support partial indexes with WHERE clauses
        // in HasIndex().HasFilter(). We use raw SQL in OnModelCreating instead.
        // The index is created in the migration via a raw SQL annotation.
        //
        // The actual partial index SQL:
        // CREATE UNIQUE INDEX IF NOT EXISTS "IX_Reservations_NoDoubleBook"
        // ON "Reservations" ("TableId", "ReservationDate", "StartTime")
        // WHERE "Status" IN (1, 2);  -- 1=Pending, 2=Confirmed
        //
        // This is added via migration raw SQL (see the migration AddReservations).

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Restaurant)
            .WithMany()
            .HasForeignKey(x => x.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Table)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.TableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}