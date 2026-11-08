using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Tasks.CreateTask;
using TaskFlow.Domain.Tasks;

namespace TaskFlow.Infrastructure.Persistence.Tasks;

public sealed class TaskRepository(TaskFlowDbContext dbContext) : ITaskRepository
{
    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken)
    {
        await dbContext.Tasks.AddAsync(task, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItem>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await dbContext.Tasks
            .Where(task => task.UserId == userId)
            .ToListAsync(cancellationToken);
}
