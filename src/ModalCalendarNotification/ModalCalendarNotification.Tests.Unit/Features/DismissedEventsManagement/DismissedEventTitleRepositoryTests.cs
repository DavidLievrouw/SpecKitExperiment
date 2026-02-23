using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ModalCalendarNotification.Data;
using ModalCalendarNotification.Data.Features.DismissedEventsManagement;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.DismissedEventsManagement;

public sealed class DismissedEventTitleRepositoryTests
{
    [Fact]
    public async Task AddAndExists_WorkForCaseInsensitiveMatching()
    {
        await using var fixture = await SqliteFixture.CreateAsync();
        var sut = new DismissedEventTitleRepository(fixture.DbContext);

        await sut.AddAsync("Daily Standup", TestContext.Current.CancellationToken);

        (
            await sut.ExistsAsync("daily standup", TestContext.Current.CancellationToken)
        ).ShouldBeTrue();
    }

    [Fact]
    public async Task RemoveAsync_DeletesMatchingTitles()
    {
        await using var fixture = await SqliteFixture.CreateAsync();
        var sut = new DismissedEventTitleRepository(fixture.DbContext);

        await sut.AddAsync("Planning", TestContext.Current.CancellationToken);
        await sut.RemoveAsync("PLANNING", TestContext.Current.CancellationToken);

        (await sut.ExistsAsync("planning", TestContext.Current.CancellationToken)).ShouldBeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPersistedRows()
    {
        await using var fixture = await SqliteFixture.CreateAsync();
        var sut = new DismissedEventTitleRepository(fixture.DbContext);

        await sut.AddAsync("Event A", TestContext.Current.CancellationToken);
        await sut.AddAsync("Event B", TestContext.Current.CancellationToken);

        var all = await sut.GetAllAsync(TestContext.Current.CancellationToken);

        all.Count.ShouldBe(2);
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

            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;

            var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            return new SqliteFixture(connection, context);
        }
    }
}
