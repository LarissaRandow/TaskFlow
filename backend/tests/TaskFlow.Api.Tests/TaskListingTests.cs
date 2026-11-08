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

public sealed class TaskListingTests
{
    private static readonly UserAccount User = new(
        Guid.Parse("a9e39085-d437-4ea2-9008-f22db16da85e"),
        "person@example.com");

    [Fact]
    public async Task Get_returns_flat_array_of_tasks_for_authenticated_user()
    {
        var ownedTask = TaskItem.Create(
            "Prepare demo",
            "Review the walkthrough",
            DateTimeOffset.Parse("2026-01-01T12:00:00Z"),
            User.Id);
        var otherUserTask = TaskItem.Create(
            "Private task",
            null,
            null,
            Guid.NewGuid());
        var repository = new TestTaskRepository([ownedTask, otherUserTask]);
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(User.Id, repository.RequestedUserId);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, body.RootElement.ValueKind);
        var tasks = await response.Content.ReadFromJsonAsync<TaskResponse[]>();
        Assert.NotNull(tasks);
        var task = Assert.Single(tasks);
        Assert.Equal(ownedTask.Id, task.Id);
        Assert.Equal("Prepare demo", task.Title);
        Assert.Equal("Review the walkthrough", task.Description);
        Assert.Equal("Todo", task.Status);
        Assert.Equal(User.Id, task.UserId);
        Assert.Equal(DateTimeOffset.Parse("2026-01-01T12:00:00Z"), task.DueDate);
    }

    [Fact]
    public async Task Get_returns_empty_array_when_user_has_no_tasks()
    {
        using var factory = new TestApiFactory(new TestTaskRepository([]));
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<TaskResponse[]>();
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task Get_requires_authentication()
    {
        using var factory = new TestApiFactory(new TestTaskRepository([]));
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OpenApi_documents_get_tasks_and_bearer_authentication()
    {
        using var factory = new TestApiFactory(new TestTaskRepository([]));
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var operation = document.RootElement
            .GetProperty("paths")
            .GetProperty("/api/tasks")
            .GetProperty("get");
        Assert.True(operation.TryGetProperty("security", out var security));
        Assert.Equal("Bearer", security[0].EnumerateObject().Single().Name);
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
        public Guid? RequestedUserId { get; private set; }

        public Task AddAsync(TaskItem task, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<TaskItem>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RequestedUserId = userId;
            return Task.FromResult<IReadOnlyList<TaskItem>>(
                tasks.Where(task => task.UserId == userId).ToArray());
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
