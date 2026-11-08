namespace TaskFlow.Application.Tasks.DeleteTask;

public interface ITaskDeletionService
{
    Task<bool> DeleteAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken);
}
