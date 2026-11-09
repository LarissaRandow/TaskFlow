using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Auth;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Tests;

internal sealed class SqlServerTestDatabase : IAsyncDisposable
{
    public const string ConnectionStringVariable = "TASKFLOW_TEST_SQLSERVER";

    private readonly DbContextOptions<TaskFlowDbContext> options;

    private SqlServerTestDatabase(DbContextOptions<TaskFlowDbContext> options)
    {
        this.options = options;
        Context = new TaskFlowDbContext(options);
    }

    public TaskFlowDbContext Context { get; }

    public static async Task<SqlServerTestDatabase> CreateAsync()
    {
        var baseConnectionString =
            Environment.GetEnvironmentVariable(ConnectionStringVariable);
        if (string.IsNullOrWhiteSpace(baseConnectionString))
        {
            throw new InvalidOperationException(
                $"{ConnectionStringVariable} must contain a SQL Server connection string.");
        }

        var connectionString = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = $"TaskFlowTests_{Guid.NewGuid():N}"
        }.ConnectionString;
        var options = new DbContextOptionsBuilder<TaskFlowDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        var database = new SqlServerTestDatabase(options);

        try
        {
            await database.Context.Database.MigrateAsync();
            return database;
        }
        catch
        {
            await database.DisposeAsync();
            throw;
        }
    }

    public async Task<ApplicationUser> AddUserAsync(Guid? id = null)
    {
        var userId = id ?? Guid.NewGuid();
        var email = $"{userId:N}@example.com";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant()
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        return user;
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await using var cleanupContext = new TaskFlowDbContext(options);
        await cleanupContext.Database.EnsureDeletedAsync();
    }
}
