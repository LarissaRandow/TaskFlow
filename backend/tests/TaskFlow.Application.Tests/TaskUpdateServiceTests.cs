using TaskFlow.Application.Tasks.UpdateTask;
using TaskFlow.Domain.Tasks;
using Xunit;

namespace TaskFlow.Application.Tests;

public sealed class TaskUpdateServiceTests
{
    private static readonly Guid UserId =
        Guid.Parse("a9e39085-d437-4ea2-9008-f22db16da85e");

    private static readonly Guid TaskId =
        Guid.Parse("c6f6608e-171c-4c28-9c4a-0e91b0a41139");

    [Theory]
    [InlineData("Todo")]
    [InlineData("InProgress")]
    [InlineData("Completed")]
    [InlineData("Discarded")]
    public async Task Update_accepts_each_exact_allowed_status(string status)
    {
        var task = TaskItem.Create("Original", null, null, UserId);
        var repository = new RecordingTaskRepository { TaskToReturn = task };
        var service = new TaskUpdateService(repository);
        var dueDate = DateTimeOffset.Parse("2026-01-01T15:00:00+03:00");

        var result = await service.UpdateAsync(
            TaskId,
            "  Updated  ",
            "Updated description",
            status,
            dueDate,
            UserId,
            CancellationToken.None);

        Assert.Equal(UpdateTaskStatus.Updated, result.Status);
        Assert.Same(task, result.Task);
        Assert.Equal(TaskId, repository.RequestedTaskId);
        Assert.Equal(UserId, repository.RequestedUserId);
        Assert.Equal("Updated", task.Title);
        Assert.Equal("Updated description", task.Description);
        Assert.Equal(status, task.Status.ToString());
        Assert.Equal(dueDate.ToUniversalTime(), task.DueDate);
        Assert.Same(task, repository.UpdatedTask);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    [Theory]
    [InlineData("todo")]
    [InlineData("4")]
    [InlineData("Unknown")]
    [InlineData(null)]
    public async Task Update_rejects_status_outside_the_exact_allowed_values(string? status)
    {
        var task = TaskItem.Create("Original", null, null, UserId);
        var repository = new RecordingTaskRepository { TaskToReturn = task };
        var service = new TaskUpdateService(repository);

        var result = await service.UpdateAsync(
            TaskId,
            "Updated",
            null,
            status,
            null,
            UserId,
            CancellationToken.None);

        Assert.Equal(UpdateTaskStatus.Invalid, result.Status);
        Assert.Contains("status", result.Errors.Keys);
        Assert.Equal(0, repository.GetByIdAndUserIdCallCount);
        Assert.Equal(0, repository.UpdateCallCount);
        Assert.Equal("Original", task.Title);
    }

    [Fact]
    public async Task Update_rejects_invalid_title_and_description_before_repository_access()
    {
        var repository = new RecordingTaskRepository
        {
            TaskToReturn = TaskItem.Create("Original", null, null, UserId)
        };
        var service = new TaskUpdateService(repository);

        var result = await service.UpdateAsync(
            TaskId,
            " ",
            new string('d', 2001),
            "Completed",
            null,
            UserId,
            CancellationToken.None);

        Assert.Equal(UpdateTaskStatus.Invalid, result.Status);
        Assert.Contains("title", result.Errors.Keys);
        Assert.Contains("description", result.Errors.Keys);
        Assert.Equal(0, repository.GetByIdAndUserIdCallCount);
        Assert.Equal(0, repository.UpdateCallCount);
    }

    [Fact]
    public async Task Update_accepts_the_maximum_title_and_description_lengths()
    {
        var task = TaskItem.Create("Original", null, null, UserId);
        var repository = new RecordingTaskRepository { TaskToReturn = task };
        var service = new TaskUpdateService(repository);

        var result = await service.UpdateAsync(
            task.Id,
            new string('t', 120),
            new string('d', 2000),
            "Completed",
            null,
            UserId,
            CancellationToken.None);

        Assert.Equal(UpdateTaskStatus.Updated, result.Status);
        Assert.Equal(120, task.Title.Length);
        Assert.Equal(2000, task.Description?.Length);
    }

    [Theory]
    [InlineData(121, 2000, "title")]
    [InlineData(120, 2001, "description")]
    public async Task Update_rejects_values_over_field_limits(
        int titleLength,
        int descriptionLength,
        string expectedError)
    {
        var task = TaskItem.Create("Original", null, null, UserId);
        var repository = new RecordingTaskRepository { TaskToReturn = task };
        var service = new TaskUpdateService(repository);

        var result = await service.UpdateAsync(
            task.Id,
            new string('t', titleLength),
            new string('d', descriptionLength),
            "Completed",
            null,
            UserId,
            CancellationToken.None);

        Assert.Equal(UpdateTaskStatus.Invalid, result.Status);
        Assert.Contains(expectedError, result.Errors.Keys);
        Assert.Equal(0, repository.GetByIdAndUserIdCallCount);
        Assert.Equal(0, repository.UpdateCallCount);
        Assert.Equal("Original", task.Title);
        Assert.Null(task.Description);
    }

    [Fact]
    public async Task Update_returns_not_found_when_task_is_missing()
    {
        var repository = new RecordingTaskRepository();
        var service = new TaskUpdateService(repository);

        var result = await service.UpdateAsync(
            TaskId,
            "Updated",
            null,
            "Completed",
            null,
            UserId,
            CancellationToken.None);

        Assert.Equal(UpdateTaskStatus.NotFound, result.Status);
        Assert.Equal(TaskId, repository.RequestedTaskId);
        Assert.Equal(UserId, repository.RequestedUserId);
        Assert.Equal(0, repository.UpdateCallCount);
    }

    [Fact]
    public async Task Update_returns_not_found_when_task_belongs_to_another_user()
    {
        var privateTask = TaskItem.Create("Private", null, null, Guid.NewGuid());
        var repository = new RecordingTaskRepository();
        repository.TaskLookupCandidates.Add(privateTask);
        var service = new TaskUpdateService(repository);

        var result = await service.UpdateAsync(
            privateTask.Id,
            "Updated",
            null,
            "Completed",
            null,
            UserId,
            CancellationToken.None);

        Assert.Equal(UpdateTaskStatus.NotFound, result.Status);
        Assert.Equal(privateTask.Id, repository.RequestedTaskId);
        Assert.Equal(UserId, repository.RequestedUserId);
        Assert.Equal(0, repository.UpdateCallCount);
        Assert.Equal("Private", privateTask.Title);
    }

    [Fact]
    public async Task Update_returns_not_found_when_repository_does_not_update_the_task()
    {
        var repository = new RecordingTaskRepository
        {
            TaskToReturn = TaskItem.Create("Original", null, null, UserId),
            UpdateResult = false
        };
        var service = new TaskUpdateService(repository);

        var result = await service.UpdateAsync(
            TaskId,
            "Updated",
            null,
            "Completed",
            null,
            UserId,
            CancellationToken.None);

        Assert.Equal(UpdateTaskStatus.NotFound, result.Status);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    [Fact]
    public async Task Update_forwards_the_cancellation_token_to_repository_calls()
    {
        var repository = new RecordingTaskRepository
        {
            TaskToReturn = TaskItem.Create("Original", null, null, UserId)
        };
        var service = new TaskUpdateService(repository);
        using var cancellationSource = new CancellationTokenSource();

        await service.UpdateAsync(
            TaskId,
            "Updated",
            null,
            "Completed",
            null,
            UserId,
            cancellationSource.Token);

        Assert.Equal(cancellationSource.Token, repository.GetByIdAndUserIdCancellationToken);
        Assert.Equal(cancellationSource.Token, repository.UpdateCancellationToken);
    }
}
