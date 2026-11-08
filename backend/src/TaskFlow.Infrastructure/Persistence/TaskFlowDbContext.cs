using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.Infrastructure.Persistence;

public sealed class TaskFlowDbContext(
    DbContextOptions<TaskFlowDbContext> options)
    : IdentityUserContext<ApplicationUser, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasIndex(user => user.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("EmailIndex");
    }
}
