using TaskFlow.Application.Tasks.CreateTask;
using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.GetTasks;

public sealed class TaskListingService(ITaskRepository taskRepository) : ITaskListingService
{
    public Task<IReadOnlyList<TaskItem>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        taskRepository.GetByUserIdAsync(userId, cancellationToken);
}
