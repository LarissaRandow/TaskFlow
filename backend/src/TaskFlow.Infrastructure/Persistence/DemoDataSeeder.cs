using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskFlow.Domain.Tasks;
using TaskFlow.Infrastructure.Auth;

namespace TaskFlow.Infrastructure.Persistence;

public sealed class DemoDataSeeder(
    UserManager<ApplicationUser> userManager,
    TaskFlowDbContext dbContext,
    IConfiguration configuration)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var email = configuration["DemoData:Email"];
        var password = configuration["DemoData:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Configure DemoData:Email and DemoData:Password to seed Development demo data.");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errorCodes = string.Join(", ", result.Errors.Select(error => error.Code));
                throw new InvalidOperationException(
                    $"Could not create the demo account. Identity errors: {errorCodes}");
            }
        }

        if (await dbContext.Tasks.AnyAsync(task => task.UserId == user.Id, cancellationToken))
        {
            return;
        }

        await dbContext.Tasks.AddRangeAsync(
            [
                CreateTask(
                    user.Id,
                    "Plan the product launch",
                    "Finalize the checklist for the next release.",
                    TaskState.Todo),
                CreateTask(
                    user.Id,
                    "Review the onboarding flow",
                    "Check the registration and first-login experience.",
                    TaskState.InProgress),
                CreateTask(
                    user.Id,
                    "Prepare the demo",
                    "Walk through the completed task board.",
                    TaskState.Completed),
                CreateTask(
                    user.Id,
                    "Archive the old prototype",
                    "The prototype is no longer part of the current plan.",
                    TaskState.Discarded)
            ],
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static TaskItem CreateTask(
        Guid userId,
        string title,
        string description,
        TaskState status)
    {
        var task = TaskItem.Create(title, description, null, userId);
        if (status != TaskState.Todo)
        {
            task.Update(title, description, status, null);
        }

        return task;
    }
}
