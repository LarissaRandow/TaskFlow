using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.CreateTask;

public sealed class TaskCreationService(ITaskRepository taskRepository) : ITaskCreationService
{
    public async Task<CreateTaskResult> CreateAsync(
        string? title,
        string? description,
        DateTimeOffset? dueDate,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        var trimmedTitle = title?.Trim() ?? string.Empty;
        if (trimmedTitle.Length == 0)
        {
            errors["title"] = ["Title is required."];
        }
        else if (trimmedTitle.Length > 120)
        {
            errors["title"] = ["Title must be 120 characters or fewer."];
        }

        if (description?.Length > 2000)
        {
            errors["description"] = ["Description must be 2000 characters or fewer."];
        }

        if (errors.Count > 0)
        {
            return CreateTaskResult.Invalid(errors);
        }

        var task = TaskItem.Create(trimmedTitle, description, dueDate, userId);
        await taskRepository.AddAsync(task, cancellationToken);
        return CreateTaskResult.Created(task);
    }
}
