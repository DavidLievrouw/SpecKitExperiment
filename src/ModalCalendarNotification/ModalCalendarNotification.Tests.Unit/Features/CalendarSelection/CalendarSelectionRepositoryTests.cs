using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ModalCalendarNotification.Core.Features.CalendarSelection;
using ModalCalendarNotification.Data;
using ModalCalendarNotification.Data.Features.CalendarSelection;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.CalendarSelection;

public sealed class CalendarSelectionRepositoryTests
{
    [Fact]
    public async Task SaveThenGetSelected_ReturnsPersistedSelections()
    {
        await using var fixture = await SqliteFixture.CreateAsync();
        var sut = new CalendarSelectionRepository(fixture.DbContext);

        await sut.SaveSelectedAsync([
            new SelectedCalendar
            {
                ProviderName = "Outlook365",
                CalendarId = "primary",
                DisplayName = "Primary",
                IsSelected = true,
            },
            new SelectedCalendar
            {
                ProviderName = "GoogleCalendar",
                CalendarId = "team",
                DisplayName = "Team",
                IsSelected = false,
            },
        ]);

        IReadOnlyList<SelectedCalendar> selected = await sut.GetSelectedAsync();

        selected.Count.ShouldBe(2);
        selected.Single(x => x.CalendarId == "primary").IsSelected.ShouldBeTrue();
    }

    private sealed class SqliteFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private SqliteFixture(SqliteConnection connection, AppDbContext dbContext)
        {
            _connection = connection;
            DbContext = dbContext;
        }

        public AppDbContext DbContext { get; }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }

        public static async Task<SqliteFixture> CreateAsync()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            return new SqliteFixture(connection, context);
        }
    }
}
