using TaskFlow.Application.Tasks.CreateTask;
using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tests;

internal sealed class RecordingTaskRepository : ITaskRepository
{
    public TaskItem? AddedTask { get; private set; }

    public TaskItem? TaskToReturn { get; set; }

    public IReadOnlyList<TaskItem> TasksToReturn { get; set; } = [];

    public List<TaskItem> TaskLookupCandidates { get; } = [];

    public bool UpdateResult { get; set; } = true;

    public bool DeleteResult { get; set; }

    public int AddCallCount { get; private set; }

    public int GetByUserIdCallCount { get; private set; }

    public int GetByIdAndUserIdCallCount { get; private set; }

    public int UpdateCallCount { get; private set; }

    public int DeleteCallCount { get; private set; }

    public Guid? RequestedTaskId { get; private set; }

    public Guid? RequestedUserId { get; private set; }

    public TaskItem? UpdatedTask { get; private set; }

    public CancellationToken AddCancellationToken { get; private set; }

    public CancellationToken GetByUserIdCancellationToken { get; private set; }

    public CancellationToken GetByIdAndUserIdCancellationToken { get; private set; }

    public CancellationToken UpdateCancellationToken { get; private set; }

    public CancellationToken DeleteCancellationToken { get; private set; }

    public Task AddAsync(TaskItem task, CancellationToken cancellationToken)
    {
        AddCallCount++;
        AddedTask = task;
        AddCancellationToken = cancellationToken;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TaskItem>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        GetByUserIdCallCount++;
        RequestedUserId = userId;
        GetByUserIdCancellationToken = cancellationToken;
        return Task.FromResult(TasksToReturn);
    }

    public Task<TaskItem?> GetByIdAndUserIdAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        GetByIdAndUserIdCallCount++;
        RequestedTaskId = taskId;
        RequestedUserId = userId;
        GetByIdAndUserIdCancellationToken = cancellationToken;
        return Task.FromResult(
            TaskToReturn ??
            TaskLookupCandidates.SingleOrDefault(
                task => task.Id == taskId && task.UserId == userId));
    }

    public Task<bool> UpdateByIdAndUserIdAsync(
        TaskItem task,
        CancellationToken cancellationToken)
    {
        UpdateCallCount++;
        UpdatedTask = task;
        UpdateCancellationToken = cancellationToken;
        return Task.FromResult(UpdateResult);
    }

    public Task<bool> DeleteByIdAndUserIdAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        DeleteCallCount++;
        RequestedTaskId = taskId;
        RequestedUserId = userId;
        DeleteCancellationToken = cancellationToken;
        return Task.FromResult(DeleteResult);
    }
}
