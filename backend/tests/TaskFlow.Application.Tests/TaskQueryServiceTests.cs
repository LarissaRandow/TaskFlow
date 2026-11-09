using TaskFlow.Application.Tasks.DeleteTask;
using TaskFlow.Application.Tasks.GetTasks;
using TaskFlow.Domain.Tasks;
using Xunit;

namespace TaskFlow.Application.Tests;

public sealed class TaskQueryServiceTests
{
    [Fact]
    public async Task Listing_service_forwards_user_and_cancellation_token()
    {
        var userId = Guid.NewGuid();
        var tasks = new[]
        {
            TaskItem.Create("Task", null, null, userId)
        };
        var repository = new RecordingTaskRepository { TasksToReturn = tasks };
        var service = new TaskListingService(repository);
        using var cancellationSource = new CancellationTokenSource();

        var result = await service.GetByUserIdAsync(userId, cancellationSource.Token);

        Assert.Same(tasks, result);
        Assert.Equal(1, repository.GetByUserIdCallCount);
        Assert.Equal(userId, repository.RequestedUserId);
        Assert.Equal(cancellationSource.Token, repository.GetByUserIdCancellationToken);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Deletion_service_forwards_identifiers_and_repository_result(bool deleted)
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var repository = new RecordingTaskRepository { DeleteResult = deleted };
        var service = new TaskDeletionService(repository);
        using var cancellationSource = new CancellationTokenSource();

        var result = await service.DeleteAsync(
            taskId,
            userId,
            cancellationSource.Token);

        Assert.Equal(deleted, result);
        Assert.Equal(1, repository.DeleteCallCount);
        Assert.Equal(taskId, repository.RequestedTaskId);
        Assert.Equal(userId, repository.RequestedUserId);
        Assert.Equal(cancellationSource.Token, repository.DeleteCancellationToken);
    }
}
