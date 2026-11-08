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

    public Task<TaskItem?> GetByIdAndUserIdAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken) =>
        dbContext.Tasks.SingleOrDefaultAsync(
            task => task.Id == taskId && task.UserId == userId,
            cancellationToken);

    public async Task<bool> UpdateByIdAndUserIdAsync(
        TaskItem task,
        CancellationToken cancellationToken) =>
        await dbContext.Tasks
            .Where(item => item.Id == task.Id && item.UserId == task.UserId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(item => item.Title, task.Title)
                    .SetProperty(item => item.Description, task.Description)
                    .SetProperty(item => item.Status, task.Status)
                    .SetProperty(item => item.DueDate, task.DueDate),
                cancellationToken) > 0;

    public async Task<bool> DeleteByIdAndUserIdAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken) =>
        await dbContext.Tasks
            .Where(task => task.Id == taskId && task.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken) > 0;
}
