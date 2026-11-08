using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.CreateTask;

public enum CreateTaskStatus
{
    Created,
    Invalid
}

public sealed record CreateTaskResult(
    CreateTaskStatus Status,
    TaskItem? Task,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static CreateTaskResult Created(TaskItem task) =>
        new(CreateTaskStatus.Created, task, new Dictionary<string, string[]>());

    public static CreateTaskResult Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(CreateTaskStatus.Invalid, null, errors);
}
