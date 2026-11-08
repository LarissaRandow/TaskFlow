using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TaskFlow.Application.Auth.Tokens;
using TaskFlow.Application.Tasks.CreateTask;
using TaskFlow.Domain.Tasks;
using TaskFlow.Domain.Users;
using TaskFlow.Infrastructure.Auth;
using Xunit;

namespace TaskFlow.Api.Tests;

public sealed class TaskModificationTests
{
    private static readonly UserAccount User = new(
        Guid.Parse("a9e39085-d437-4ea2-9008-f22db16da85e"),
        "person@example.com");

    [Fact]
    public async Task Put_updates_owned_task_and_returns_updated_representation()
    {
        var task = TaskItem.Create("Old title", null, null, User.Id);
        var repository = new TestTaskRepository([task]);
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.PutAsJsonAsync(
            $"/api/tasks/{task.Id}",
            new
            {
                title = "  Updated title  ",
                description = "Updated description",
                status = "InProgress",
                dueDate = "2026-01-01T15:00:00+03:00"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(body);
        Assert.Equal(task.Id, body.Id);
        Assert.Equal("Updated title", body.Title);
        Assert.Equal("Updated description", body.Description);
        Assert.Equal("InProgress", body.Status);
        Assert.Equal(DateTimeOffset.Parse("2026-01-01T12:00:00Z"), body.DueDate);
        Assert.Equal(User.Id, body.UserId);
        Assert.Equal(User.Id, repository.RequestedUserId);
        Assert.True(repository.WasUpdated);
    }

    [Theory]
    [InlineData("todo")]
    [InlineData("4")]
    [InlineData("Unknown")]
    public async Task Put_rejects_status_outside_the_exact_allowed_values(string status)
    {
        var task = TaskItem.Create("Existing", null, null, User.Id);
        var repository = new TestTaskRepository([task]);
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.PutAsJsonAsync(
            $"/api/tasks/{task.Id}",
            new { title = "Updated", status });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(repository.WasUpdated);
    }

    [Theory]
    [InlineData("   ", null)]
    [InlineData("Updated", 2001)]
    public async Task Put_rejects_invalid_title_or_description(string title, int? descriptionLength)
    {
        var task = TaskItem.Create("Existing", null, null, User.Id);
        var repository = new TestTaskRepository([task]);
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);
        var description = descriptionLength is null
            ? null
            : new string('a', descriptionLength.Value);

        var response = await client.PutAsJsonAsync(
            $"/api/tasks/{task.Id}",
            new { title, description, status = "Completed" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(repository.WasUpdated);
    }

    [Fact]
    public async Task Put_rejects_client_controlled_identity_fields()
    {
        var task = TaskItem.Create("Existing", null, null, User.Id);
        var repository = new TestTaskRepository([task]);
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.PutAsJsonAsync(
            $"/api/tasks/{task.Id}",
            new
            {
                title = "Updated",
                status = "Completed",
                id = Guid.NewGuid(),
                userId = Guid.NewGuid()
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(repository.WasUpdated);
    }

    [Fact]
    public async Task Put_returns_not_found_for_another_users_task()
    {
        var task = TaskItem.Create("Private", null, null, Guid.NewGuid());
        var repository = new TestTaskRepository([task]);
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.PutAsJsonAsync(
            $"/api/tasks/{task.Id}",
            new { title = "Updated", status = "Completed" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(User.Id, repository.RequestedUserId);
        Assert.False(repository.WasUpdated);
    }

    [Fact]
    public async Task Put_returns_not_found_for_missing_task()
    {
        var repository = new TestTaskRepository([]);
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.PutAsJsonAsync(
            $"/api/tasks/{Guid.NewGuid()}",
            new { title = "Updated", status = "Completed" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_removes_only_the_authenticated_users_task()
    {
        var ownedTask = TaskItem.Create("Owned", null, null, User.Id);
        var otherUserTask = TaskItem.Create("Private", null, null, Guid.NewGuid());
        var repository = new TestTaskRepository([ownedTask, otherUserTask]);
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.DeleteAsync($"/api/tasks/{ownedTask.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
        Assert.DoesNotContain(repository.Tasks, task => task.Id == ownedTask.Id);
        Assert.Contains(repository.Tasks, task => task.Id == otherUserTask.Id);
        Assert.Equal(User.Id, repository.RequestedUserId);
    }

    [Fact]
    public async Task Delete_returns_not_found_for_another_users_task()
    {
        var task = TaskItem.Create("Private", null, null, Guid.NewGuid());
        var repository = new TestTaskRepository([task]);
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.DeleteAsync($"/api/tasks/{task.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains(repository.Tasks, item => item.Id == task.Id);
        Assert.Equal(User.Id, repository.RequestedUserId);
    }

    [Fact]
    public async Task Delete_returns_not_found_for_missing_task()
    {
        var repository = new TestTaskRepository([]);
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_requires_authentication()
    {
        using var factory = new TestApiFactory(new TestTaskRepository([]));
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OpenApi_documents_update_and_delete_endpoints()
    {
        using var factory = new TestApiFactory(new TestTaskRepository([]));
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var operations = document.RootElement
            .GetProperty("paths")
            .GetProperty("/api/tasks/{id}");
        Assert.True(operations.TryGetProperty("put", out var put));
        Assert.True(put.TryGetProperty("security", out _));
        Assert.True(operations.TryGetProperty("delete", out var delete));
        Assert.True(delete.TryGetProperty("security", out _));
    }

    private static HttpClient CreateAuthenticatedClient(WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient();
        var options = Options.Create(new JwtOptions
        {
            Issuer = AuthRegistrationTests.TestConfiguration.Values["Jwt:Issuer"]!,
            Audience = AuthRegistrationTests.TestConfiguration.Values["Jwt:Audience"]!,
            SigningKey = AuthRegistrationTests.TestConfiguration.Values["Jwt:SigningKey"]!
        });
        var token = new JwtAccessTokenIssuer(options).Issue(User);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Token);
        return client;
    }

    private sealed class TestApiFactory(TestTaskRepository repository)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureTestServices(services =>
            {
                AuthRegistrationTests.ConfigureJwtOptions(services);
                services.RemoveAll<ITaskRepository>();
                services.AddSingleton<ITaskRepository>(repository);
            });
        }
    }

    private sealed class TestTaskRepository(IReadOnlyList<TaskItem> tasks) : ITaskRepository
    {
        public List<TaskItem> Tasks { get; } = [.. tasks];

        public Guid? RequestedUserId { get; private set; }

        public bool WasUpdated { get; private set; }

        public Task AddAsync(TaskItem task, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<TaskItem>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<TaskItem?> GetByIdAndUserIdAsync(
            Guid taskId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RequestedUserId = userId;
            return Task.FromResult(
                Tasks.SingleOrDefault(task => task.Id == taskId && task.UserId == userId));
        }

        public Task<bool> UpdateByIdAndUserIdAsync(
            TaskItem task,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var exists = Tasks.Any(item => item.Id == task.Id && item.UserId == task.UserId);
            WasUpdated = exists;
            return Task.FromResult(exists);
        }

        public Task<bool> DeleteByIdAndUserIdAsync(
            Guid taskId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RequestedUserId = userId;
            var task = Tasks.SingleOrDefault(item => item.Id == taskId && item.UserId == userId);
            if (task is null)
            {
                return Task.FromResult(false);
            }

            Tasks.Remove(task);
            return Task.FromResult(true);
        }
    }

    private sealed record TaskResponse(
        Guid Id,
        string Title,
        string? Description,
        string Status,
        DateTimeOffset? DueDate,
        Guid UserId);
}
