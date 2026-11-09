using Xunit;

namespace TaskFlow.Infrastructure.Tests;

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(SqlServerTestDatabase.ConnectionStringVariable)))
        {
            Skip =
                $"Set {SqlServerTestDatabase.ConnectionStringVariable} to run SQL Server integration tests.";
        }
    }
}
