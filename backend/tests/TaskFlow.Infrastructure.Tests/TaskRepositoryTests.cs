using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Tasks;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Persistence.Tasks;
using Xunit;

namespace TaskFlow.Infrastructure.Tests;

public sealed class TaskRepositoryTests
{
    [SqlServerFact]
    public async Task Add_and_list_persist_tasks_scoped_to_the_requested_user()
    {
        await using var database = await SqlServerTestDatabase.CreateAsync();
        var owner = await database.AddUserAsync();
        var anotherOwner = await database.AddUserAsync();
        var dueDate = DateTimeOffset.Parse("2026-01-01T15:00:00+03:00");
        var task = TaskItem.Create("Prepare demo", "Review the walkthrough", dueDate, owner.Id);
        var otherUsersTask = TaskItem.Create("Private", null, null, anotherOwner.Id);
        var repository = new TaskRepository(database.Context);

        await repository.AddAsync(task, CancellationToken.None);
        await repository.AddAsync(otherUsersTask, CancellationToken.None);

        var result = await repository.GetByUserIdAsync(owner.Id, CancellationToken.None);

        var savedTask = Assert.Single(result);
        Assert.Equal(task.Id, savedTask.Id);
        Assert.Equal("Prepare demo", savedTask.Title);
        Assert.Equal("Review the walkthrough", savedTask.Description);
        Assert.Equal(TaskState.Todo, savedTask.Status);
        Assert.Equal(dueDate.ToUniversalTime(), savedTask.DueDate);
        Assert.Equal(owner.Id, savedTask.UserId);
    }

    [SqlServerFact]
    public async Task Get_by_id_and_user_id_does_not_return_another_users_task()
    {
        await using var database = await SqlServerTestDatabase.CreateAsync();
        var owner = await database.AddUserAsync();
        var anotherOwner = await database.AddUserAsync();
        var task = TaskItem.Create("Private", null, null, owner.Id);
        var repository = new TaskRepository(database.Context);
        await repository.AddAsync(task, CancellationToken.None);

        var ownedTask = await repository.GetByIdAndUserIdAsync(
            task.Id,
            owner.Id,
            CancellationToken.None);
        var otherUsersResult = await repository.GetByIdAndUserIdAsync(
            task.Id,
            anotherOwner.Id,
            CancellationToken.None);

        Assert.NotNull(ownedTask);
        Assert.Equal(task.Id, ownedTask.Id);
        Assert.Null(otherUsersResult);
    }

    [SqlServerFact]
    public async Task Update_persists_changes_to_the_owned_task()
    {
        await using var database = await SqlServerTestDatabase.CreateAsync();
        var owner = await database.AddUserAsync();
        var task = TaskItem.Create("Original", null, null, owner.Id);
        var repository = new TaskRepository(database.Context);
        await repository.AddAsync(task, CancellationToken.None);

        var taskToUpdate = await repository.GetByIdAndUserIdAsync(
            task.Id,
            owner.Id,
            CancellationToken.None);
        Assert.NotNull(taskToUpdate);
        taskToUpdate.Update(
            "Updated",
            "Updated description",
            TaskState.Completed,
            DateTimeOffset.Parse("2026-01-01T15:00:00+03:00"));

        var updated = await repository.UpdateByIdAndUserIdAsync(
            taskToUpdate,
            CancellationToken.None);

        Assert.True(updated);
        await using var verificationContext = new TaskFlowDbContext(
            new DbContextOptionsBuilder<TaskFlowDbContext>()
                .UseSqlServer(database.Context.Database.GetConnectionString())
                .Options);
        var persistedTask = await verificationContext.Tasks
            .AsNoTracking()
            .SingleAsync(item => item.Id == task.Id);
        Assert.Equal("Updated", persistedTask.Title);
        Assert.Equal("Updated description", persistedTask.Description);
        Assert.Equal(TaskState.Completed, persistedTask.Status);
        Assert.Equal(
            DateTimeOffset.Parse("2026-01-01T12:00:00Z"),
            persistedTask.DueDate);
        Assert.Equal(owner.Id, persistedTask.UserId);
    }

    [SqlServerFact]
    public async Task Delete_only_removes_a_task_when_both_ids_match()
    {
        await using var database = await SqlServerTestDatabase.CreateAsync();
        var owner = await database.AddUserAsync();
        var anotherOwner = await database.AddUserAsync();
        var task = TaskItem.Create("Owned", null, null, owner.Id);
        var repository = new TaskRepository(database.Context);
        await repository.AddAsync(task, CancellationToken.None);

        var deletedByOtherUser = await repository.DeleteByIdAndUserIdAsync(
            task.Id,
            anotherOwner.Id,
            CancellationToken.None);
        var stillExists = await repository.GetByIdAndUserIdAsync(
            task.Id,
            owner.Id,
            CancellationToken.None);
        var deletedByOwner = await repository.DeleteByIdAndUserIdAsync(
            task.Id,
            owner.Id,
            CancellationToken.None);
        var deletedAgain = await repository.DeleteByIdAndUserIdAsync(
            task.Id,
            owner.Id,
            CancellationToken.None);

        Assert.False(deletedByOtherUser);
        Assert.NotNull(stillExists);
        Assert.True(deletedByOwner);
        Assert.False(deletedAgain);
    }
}
