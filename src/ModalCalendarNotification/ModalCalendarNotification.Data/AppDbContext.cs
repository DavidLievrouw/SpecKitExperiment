using Microsoft.EntityFrameworkCore;
using ModalCalendarNotification.Data.Features.CalendarIntegration;

namespace ModalCalendarNotification.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<ConfigurationEntity> Configurations => Set<ConfigurationEntity>();

    public DbSet<DismissedEventTitleEntity> DismissedEventTitles =>
        Set<DismissedEventTitleEntity>();

    public DbSet<SelectedCalendarEntity> SelectedCalendars => Set<SelectedCalendarEntity>();

    public DbSet<ApplicationStateEntity> ApplicationStates => Set<ApplicationStateEntity>();

    public DbSet<CachedCalendarEventEntity> CachedCalendarEvents =>
        Set<CachedCalendarEventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConfigurationEntity>(entity =>
        {
            entity.ToTable("Configuration");
            entity.HasKey(x => x.Key);
            entity.Property(x => x.Key).HasMaxLength(128);
            entity.Property(x => x.Value).HasMaxLength(2048);
        });

        modelBuilder.Entity<DismissedEventTitleEntity>(entity =>
        {
            entity.ToTable("DismissedEventTitle");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(512);
            entity.HasIndex(x => x.Title);
        });

        modelBuilder.Entity<SelectedCalendarEntity>(entity =>
        {
            entity.ToTable("SelectedCalendar");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProviderName).HasMaxLength(128);
            entity.Property(x => x.CalendarId).HasMaxLength(256);
            entity.Property(x => x.DisplayName).HasMaxLength(256);
            entity.HasIndex(x => new { x.ProviderName, x.CalendarId }).IsUnique();
        });

        modelBuilder.Entity<ApplicationStateEntity>(entity =>
        {
            entity.ToTable("ApplicationState");
            entity.HasKey(x => x.Key);
            entity.Property(x => x.Key).HasMaxLength(128);
        });

        modelBuilder.Entity<CachedCalendarEventEntity>(entity =>
        {
            entity.ToTable("CachedCalendarEvent");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventId).HasMaxLength(256);
            entity.Property(x => x.Title).HasMaxLength(512);
            entity.Property(x => x.Provider).HasMaxLength(128);
            entity.Property(x => x.CalendarId).HasMaxLength(256);
            entity.HasIndex(x => new { x.Provider, x.EventId }).IsUnique();
            entity.HasIndex(x => x.StartUtc);
        });
    }
}

public sealed class ConfigurationEntity
{
    public required string Key { get; init; }

    public required string Value { get; set; }
}

public sealed class DismissedEventTitleEntity
{
    public int Id { get; init; }

    public required string Title { get; set; }

    public DateTimeOffset DismissedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class SelectedCalendarEntity
{
    public int Id { get; init; }

    public required string ProviderName { get; set; }

    public required string CalendarId { get; set; }

    public required string DisplayName { get; set; }

    public bool IsSelected { get; set; }
}

public sealed class ApplicationStateEntity
{
    public required string Key { get; init; }

    public string? Value { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
