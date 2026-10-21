using BatusSales.Api.Data;
using BatusSales.Api.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Tests;

public sealed class DatabaseReadinessTests
{
    [Fact]
    public async Task Database_connection_without_application_tables_is_not_ready()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);

        Assert.True(await db.Database.CanConnectAsync());
        Assert.False(await DatabaseReadiness.IsReadyAsync(db, CancellationToken.None));
    }

    [Fact]
    public async Task Database_with_schema_is_ready_but_missing_contact_table_is_not()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);

        await db.Database.EnsureCreatedAsync();
        Assert.True(await DatabaseReadiness.IsReadyAsync(db, CancellationToken.None));

        // Simulate a partially deployed schema while the DB remains reachable.
        await db.Database.ExecuteSqlRawAsync("DROP TABLE \"Contacts\"");
        Assert.True(await db.Database.CanConnectAsync());
        Assert.False(await DatabaseReadiness.IsReadyAsync(db, CancellationToken.None));
    }
}
