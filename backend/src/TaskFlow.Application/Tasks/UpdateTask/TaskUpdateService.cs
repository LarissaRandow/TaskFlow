using TaskFlow.Application.Tasks.CreateTask;
using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.UpdateTask;

public sealed class TaskUpdateService(ITaskRepository taskRepository) : ITaskUpdateService
{
    public async Task<UpdateTaskResult> UpdateAsync(
        Guid taskId,
        string? title,
        string? description,
        string? status,
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

        if (!TryParseStatus(status, out var taskState))
        {
            errors["status"] = ["Status must be Todo, InProgress, Completed, or Discarded."];
        }

        if (errors.Count > 0)
        {
            return UpdateTaskResult.Invalid(errors);
        }

        var task = await taskRepository.GetByIdAndUserIdAsync(
            taskId,
            userId,
            cancellationToken);
        if (task is null)
        {
            return UpdateTaskResult.NotFound();
        }

        task.Update(trimmedTitle, description, taskState, dueDate);
        if (!await taskRepository.UpdateByIdAndUserIdAsync(task, cancellationToken))
        {
            return UpdateTaskResult.NotFound();
        }

        return UpdateTaskResult.Updated(task);
    }

    private static bool TryParseStatus(string? status, out TaskState taskState)
    {
        if (Enum.TryParse(status, ignoreCase: false, out taskState) &&
            Enum.IsDefined(taskState) &&
            taskState.ToString() == status)
        {
            return true;
        }

        taskState = default;
        return false;
    }
}
