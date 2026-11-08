using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.UpdateTask;

public enum UpdateTaskStatus
{
    Updated,
    Invalid,
    NotFound
}

public sealed record UpdateTaskResult(
    UpdateTaskStatus Status,
    TaskItem? Task,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static UpdateTaskResult Updated(TaskItem task) =>
        new(UpdateTaskStatus.Updated, task, new Dictionary<string, string[]>());

    public static UpdateTaskResult Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(UpdateTaskStatus.Invalid, null, errors);

    public static UpdateTaskResult NotFound() =>
        new(UpdateTaskStatus.NotFound, null, new Dictionary<string, string[]>());
}
