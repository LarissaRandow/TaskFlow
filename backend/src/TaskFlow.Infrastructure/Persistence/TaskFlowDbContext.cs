using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Tasks;
using TaskFlow.Infrastructure.Auth;

namespace TaskFlow.Infrastructure.Persistence;

public sealed class TaskFlowDbContext(
    DbContextOptions<TaskFlowDbContext> options)
    : IdentityUserContext<ApplicationUser, Guid>(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TaskItem>(task =>
        {
            task.ToTable("Tasks", table =>
                table.HasCheckConstraint(
                    "CK_Tasks_Status",
                    "[Status] IN ('Todo', 'InProgress', 'Completed', 'Discarded')"));
            task.HasKey(item => item.Id);
            task.Property(item => item.Title)
                .HasMaxLength(120)
                .IsRequired();
            task.Property(item => item.Description)
                .HasMaxLength(2000);
            task.Property(item => item.Status)
                .HasConversion<string>()
                .HasMaxLength(24)
                .IsRequired();
            task.Property(item => item.DueDate)
                .HasColumnType("datetimeoffset");
            task.HasIndex(item => item.UserId);
            task.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        builder.Entity<ApplicationUser>()
            .HasIndex(user => user.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("EmailIndex");
    }
}
