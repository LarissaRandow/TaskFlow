using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.CreateTask;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task, CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskItem>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<TaskItem?> GetByIdAndUserIdAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<bool> UpdateByIdAndUserIdAsync(
        TaskItem task,
        CancellationToken cancellationToken);

    Task<bool> DeleteByIdAndUserIdAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken);
}
