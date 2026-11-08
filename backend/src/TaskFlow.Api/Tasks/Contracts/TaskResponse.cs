using TaskFlow.Domain.Tasks;

namespace TaskFlow.Api.Tasks.Contracts;

public sealed record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    DateTimeOffset? DueDate,
    Guid UserId)
{
    public static TaskResponse From(TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.Status.ToString(), task.DueDate, task.UserId);
}
