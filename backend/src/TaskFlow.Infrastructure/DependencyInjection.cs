using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Auth.Login;
using TaskFlow.Application.Auth.Register;
using TaskFlow.Application.Auth.Tokens;
using TaskFlow.Application.Tasks.CreateTask;
using TaskFlow.Application.Tasks.DeleteTask;
using TaskFlow.Application.Tasks.GetTasks;
using TaskFlow.Application.Tasks.UpdateTask;
using TaskFlow.Infrastructure.Auth;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Persistence.Tasks;

namespace TaskFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TaskFlowDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString("TaskFlow");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Configure ConnectionStrings:TaskFlow in .NET User Secrets or the ConnectionStrings__TaskFlow environment variable before using persistence.");
            }

            options.UseSqlServer(connectionString);
        });

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<TaskFlowDbContext>();

        services.AddScoped<IUserRegistrationService, IdentityUserRegistrationService>();
        services.AddScoped<IUserLoginService, IdentityUserLoginService>();
        services.AddScoped<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        services.AddScoped<ITaskCreationService, TaskCreationService>();
        services.AddScoped<ITaskListingService, TaskListingService>();
        services.AddScoped<ITaskUpdateService, TaskUpdateService>();
        services.AddScoped<ITaskDeletionService, TaskDeletionService>();
        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}
