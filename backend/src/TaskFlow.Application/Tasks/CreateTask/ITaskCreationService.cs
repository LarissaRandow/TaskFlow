namespace TaskFlow.Application.Tasks.CreateTask;

public interface ITaskCreationService
{
    Task<CreateTaskResult> CreateAsync(
        string? title,
        string? description,
        DateTimeOffset? dueDate,
        Guid userId,
        CancellationToken cancellationToken);
}
