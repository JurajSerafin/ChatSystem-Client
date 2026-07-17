using System;
using ChatSystem.Client.Core.Interfaces.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Client.Tests.Infrastructure.Repositories;

/// <summary>
/// Base class for repository integration tests.
/// 
/// Provides a clean in-memory SQLite database for each test,
/// with automatic schema creation and cleanup.
/// 
/// Each derived test class gets its own database instance that is automatically
/// disposed after the test completes.
/// </summary>
public abstract class RepositoryTestBase : IDisposable {
    /// <summary>
    /// The SQLite connection to the in-memory database.
    /// Kept open for the lifetime of the test.
    /// </summary>
    private readonly SqliteConnection _connection;

    /// <summary>
    /// The EF Core database context for the in-memory SQLite database.
    /// Protected to allow initialization of tested repo for derived test classes.
    /// </summary>
    protected readonly ChatSystemLocalDbContext DbContext;

    /// <summary>
    /// Initializes a new instance of <see cref="RepositoryTestBase"/>.
    /// 
    /// Sets up an in-memory SQLite database with all migrations applied.
    /// The database is isolated per test.
    /// </summary>
    protected RepositoryTestBase() {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ChatSystemLocalDbContext>()
            .UseSqlite(_connection)
            .Options;

        DbContext = new ChatSystemLocalDbContext(options);
        DbContext.Database.EnsureCreated();
    }

    /// <summary>
    /// Disposes the database context and closes the SQLite connection.
    /// Called automatically by xUnit after each test completes.
    /// </summary>
    public void Dispose() {
        DbContext.Dispose();
        _connection.Dispose();
    }

    /// <summary>
    /// Clears the EF Core change tracker, forcing the next query to fetch fresh data from the database.
    /// 
    /// Used when testing code that uses <see cref="Microsoft.EntityFrameworkCore.Query.ExecuteUpdateAsync"/>
    /// or <see cref="Microsoft.EntityFrameworkCore.Query.ExecuteDeleteAsync"/>, which bypasses the change tracker
    /// and directly executes SQL. Without clearing the tracker, queries may return stale cached entities.
    /// </summary>
    protected void ClearTracker() {
        DbContext.ChangeTracker.Clear();
    }
}