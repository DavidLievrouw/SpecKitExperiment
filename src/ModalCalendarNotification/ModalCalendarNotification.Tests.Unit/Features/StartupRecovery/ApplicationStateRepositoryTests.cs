using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ModalCalendarNotification.Data;
using ModalCalendarNotification.Data.Features.StartupRecovery;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.StartupRecovery;

public sealed class ApplicationStateRepositoryTests
{
    [Fact]
    public async Task SetThenGetLastRunUtc_PersistsTimestamp()
    {
        await using var fixture = await SqliteFixture.CreateAsync();
        var sut = new ApplicationStateRepository(fixture.DbContext);
        var now = DateTimeOffset.UtcNow;

        await sut.SetLastRunUtcAsync(now, TestContext.Current.CancellationToken);
        var loaded = await sut.GetLastRunUtcAsync(TestContext.Current.CancellationToken);

        loaded.ShouldNotBeNull();
        loaded.Value.ShouldBe(now);
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
