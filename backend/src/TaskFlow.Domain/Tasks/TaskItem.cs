namespace TaskFlow.Domain.Tasks;

public sealed class TaskItem
{
    private TaskItem()
    {
    }

    private TaskItem(
        Guid id,
        string title,
        string? description,
        DateTimeOffset? dueDate,
        Guid userId)
    {
        Id = id;
        Title = title;
        Description = description;
        Status = TaskState.Todo;
        DueDate = dueDate;
        UserId = userId;
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public TaskState Status { get; private set; }

    public DateTimeOffset? DueDate { get; private set; }

    public Guid UserId { get; private set; }

    public static TaskItem Create(
        string title,
        string? description,
        DateTimeOffset? dueDate,
        Guid userId)
    {
        var trimmedTitle = title.Trim();
        if (trimmedTitle.Length == 0 || trimmedTitle.Length > 120)
        {
            throw new ArgumentException(
                "A task title must be between 1 and 120 characters after trimming.",
                nameof(title));
        }

        if (description?.Length > 2000)
        {
            throw new ArgumentException(
                "A task description cannot exceed 2000 characters.",
                nameof(description));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A task must have an owner.", nameof(userId));
        }

        return new TaskItem(
            Guid.NewGuid(),
            trimmedTitle,
            description,
            dueDate?.ToUniversalTime(),
            userId);
    }
}
