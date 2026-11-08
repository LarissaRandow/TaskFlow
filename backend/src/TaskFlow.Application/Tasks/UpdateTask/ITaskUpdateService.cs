using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.UpdateTask;

public interface ITaskUpdateService
{
    Task<UpdateTaskResult> UpdateAsync(
        Guid taskId,
        string? title,
        string? description,
        string? status,
        DateTimeOffset? dueDate,
        Guid userId,
        CancellationToken cancellationToken);
}
