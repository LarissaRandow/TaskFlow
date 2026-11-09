using TaskFlow.Application.Tasks.CreateTask;
using TaskFlow.Domain.Tasks;
using Xunit;

namespace TaskFlow.Application.Tests;

public sealed class TaskCreationServiceTests
{
    private static readonly Guid UserId =
        Guid.Parse("a9e39085-d437-4ea2-9008-f22db16da85e");

    [Fact]
    public async Task Create_adds_a_trimmed_task_owned_by_the_authenticated_user()
    {
        var repository = new RecordingTaskRepository();
        var service = new TaskCreationService(repository);
        var dueDate = DateTimeOffset.Parse("2026-01-01T15:00:00+03:00");

        var result = await service.CreateAsync(
            "  Prepare demo  ",
            "Review the walkthrough",
            dueDate,
            UserId,
            CancellationToken.None);

        Assert.Equal(CreateTaskStatus.Created, result.Status);
        Assert.NotNull(result.Task);
        Assert.Same(result.Task, repository.AddedTask);
        Assert.Equal("Prepare demo", result.Task.Title);
        Assert.Equal("Review the walkthrough", result.Task.Description);
        Assert.Equal(TaskState.Todo, result.Task.Status);
        Assert.Equal(dueDate.ToUniversalTime(), result.Task.DueDate);
        Assert.Equal(UserId, result.Task.UserId);
        Assert.Equal(1, repository.AddCallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_rejects_a_missing_or_blank_title(string? title)
    {
        var repository = new RecordingTaskRepository();
        var service = new TaskCreationService(repository);

        var result = await service.CreateAsync(
            title,
            null,
            null,
            UserId,
            CancellationToken.None);

        Assert.Equal(CreateTaskStatus.Invalid, result.Status);
        Assert.Contains("title", result.Errors.Keys);
        Assert.Null(result.Task);
        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public async Task Create_rejects_title_and_description_over_their_limits()
    {
        var repository = new RecordingTaskRepository();
        var service = new TaskCreationService(repository);

        var result = await service.CreateAsync(
            new string('t', 121),
            new string('d', 2001),
            null,
            UserId,
            CancellationToken.None);

        Assert.Equal(CreateTaskStatus.Invalid, result.Status);
        Assert.Contains("title", result.Errors.Keys);
        Assert.Contains("description", result.Errors.Keys);
        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public async Task Create_accepts_the_maximum_title_and_description_lengths()
    {
        var repository = new RecordingTaskRepository();
        var service = new TaskCreationService(repository);

        var result = await service.CreateAsync(
            new string('t', 120),
            new string('d', 2000),
            null,
            UserId,
            CancellationToken.None);

        Assert.Equal(CreateTaskStatus.Created, result.Status);
        Assert.NotNull(repository.AddedTask);
        Assert.Equal(120, repository.AddedTask.Title.Length);
        Assert.Equal(2000, repository.AddedTask.Description?.Length);
    }

    [Fact]
    public async Task Create_forwards_the_cancellation_token_to_the_repository()
    {
        var repository = new RecordingTaskRepository();
        var service = new TaskCreationService(repository);
        using var cancellationSource = new CancellationTokenSource();

        await service.CreateAsync(
            "Prepare demo",
            null,
            null,
            UserId,
            cancellationSource.Token);

        Assert.Equal(cancellationSource.Token, repository.AddCancellationToken);
    }
}
