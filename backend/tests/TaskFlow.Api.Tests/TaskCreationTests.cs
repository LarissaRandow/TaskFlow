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

public sealed class TaskCreationTests
{
    private static readonly UserAccount User = new(
        Guid.Parse("a9e39085-d437-4ea2-9008-f22db16da85e"),
        "person@example.com");

    [Fact]
    public async Task Create_returns_created_task_owned_by_authenticated_user_with_todo_status()
    {
        var repository = new TestTaskRepository();
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/tasks",
            new
            {
                title = "  Prepare demo  ",
                description = "Review the interview walkthrough",
                dueDate = "2026-01-01T15:00:00+03:00"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/api/tasks/" + repository.CreatedTask!.Id, response.Headers.Location?.OriginalString);

        var body = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(body);
        Assert.Equal(repository.CreatedTask.Id, body.Id);
        Assert.Equal("Prepare demo", body.Title);
        Assert.Equal("Review the interview walkthrough", body.Description);
        Assert.Equal("Todo", body.Status);
        Assert.Equal(User.Id, body.UserId);
        Assert.Equal(DateTimeOffset.Parse("2026-01-01T12:00:00Z"), body.DueDate);
    }

    [Fact]
    public async Task Create_requires_authentication()
    {
        using var factory = new TestApiFactory(new TestTaskRepository());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/tasks",
            new { title = "Prepare demo" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OpenApi_documents_create_request_and_bearer_authentication()
    {
        using var factory = new TestApiFactory(new TestTaskRepository());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths");
        var createOperation = paths.GetProperty("/api/tasks").GetProperty("post");
        Assert.True(createOperation.TryGetProperty("security", out var security));
        Assert.Equal(
            "Bearer",
            security[0].EnumerateObject().Single().Name);

        var requestSchema = createOperation
            .GetProperty("requestBody")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString()!;
        var schemaName = requestSchema.Split('/').Last();
        var properties = document.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty(schemaName)
            .GetProperty("properties");
        Assert.True(properties.TryGetProperty("title", out _));
        Assert.True(properties.TryGetProperty("description", out _));
        Assert.True(properties.TryGetProperty("dueDate", out _));
        Assert.False(properties.TryGetProperty("status", out _));
        Assert.False(properties.TryGetProperty("userId", out _));

        var registrationOperation = paths
            .GetProperty("/api/auth/register")
            .GetProperty("post");
        Assert.False(registrationOperation.TryGetProperty("security", out _));
    }

    [Theory]
    [InlineData("status")]
    [InlineData("userId")]
    public async Task Create_rejects_fields_that_are_not_writable(string property)
    {
        var repository = new TestTaskRepository();
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/tasks",
            new Dictionary<string, object>
            {
                ["title"] = "Prepare demo",
                [property] = property == "status" ? "Completed" : Guid.NewGuid()
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(repository.CreatedTask);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    public async Task Create_rejects_blank_title(string title)
    {
        var repository = new TestTaskRepository();
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.PostAsJsonAsync("/api/tasks", new { title });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(repository.CreatedTask);
    }

    [Fact]
    public async Task Create_rejects_title_over_120_characters()
    {
        var repository = new TestTaskRepository();
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/tasks",
            new { title = new string('a', 121) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(repository.CreatedTask);
    }

    [Fact]
    public async Task Create_trims_title_before_enforcing_the_length_limit()
    {
        var repository = new TestTaskRepository();
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/tasks",
            new { title = new string(' ', 20) + new string('a', 110) });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(new string('a', 110), repository.CreatedTask?.Title);
    }

    [Fact]
    public async Task Create_rejects_description_over_2000_characters()
    {
        var repository = new TestTaskRepository();
        using var factory = new TestApiFactory(repository);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/tasks",
            new { title = "Prepare demo", description = new string('a', 2001) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(repository.CreatedTask);
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

    private sealed class TestTaskRepository : ITaskRepository
    {
        public TaskItem? CreatedTask { get; private set; }

        public Task AddAsync(TaskItem task, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CreatedTask = task;
            return Task.CompletedTask;
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
