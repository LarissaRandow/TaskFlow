using TaskFlow.Application.Tasks.CreateTask;

namespace TaskFlow.Application.Tasks.DeleteTask;

public sealed class TaskDeletionService(ITaskRepository taskRepository) : ITaskDeletionService
{
    public Task<bool> DeleteAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken) =>
        taskRepository.DeleteByIdAndUserIdAsync(taskId, userId, cancellationToken);
}
