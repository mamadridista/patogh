namespace Patogh.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // ── Soft Delete ───────────────────────────────────────────────────────────
    // All delete operations set IsDeleted = true instead of removing the row.
    // EF Core global query filters (in ApplicationDbContext.OnModelCreating)
    // automatically exclude soft-deleted rows from EVERY query in the system.
    // This means handlers never need to add .Where(x => !x.IsDeleted) manually.
    //
    // WHY: Reservation history must never disappear. A cancelled booking must
    // remain queryable for reporting, audit, and customer support purposes.
    // The same applies to restaurants and menu items.
    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    // Stores the UserId (as string) of who deleted this record
    public string? DeletedBy { get; set; }
}