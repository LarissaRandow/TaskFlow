using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TaskFlow.Application.Auth;
using TaskFlow.Domain.Users;
using TaskFlow.Infrastructure;
using Xunit;

namespace TaskFlow.Api.Tests;

public sealed class AuthRegistrationTests
{
    [Fact]
    public async Task Register_returns_created_user_without_password_material()
    {
        var user = new UserAccount(Guid.NewGuid(), "person@example.com");
        using var factory = new TestApiFactory(RegistrationResult.Created(user));
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { email = user.Email, password = "Example-password1!" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            new[] { "email", "id" },
            body.RootElement.EnumerateObject()
                .Select(property => property.Name)
                .Order()
                .ToArray());
        Assert.Equal(user.Id, body.RootElement.GetProperty("id").GetGuid());
        Assert.Equal(user.Email, body.RootElement.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Register_returns_conflict_for_duplicate_email()
    {
        using var factory = new TestApiFactory(RegistrationResult.DuplicateEmail());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { email = "person@example.com", password = "Example-password1!" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Register_returns_problem_details_for_invalid_email()
    {
        using var factory = new TestApiFactory(
            RegistrationResult.Invalid(new Dictionary<string, string[]>
            {
                ["password"] = ["Password must meet the configured requirements."]
            }));
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { email = "not-an-email", password = "weak" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("Email", problem.Errors.Keys);
    }

    [Fact]
    public async Task Register_returns_problem_details_for_identity_validation_errors()
    {
        using var factory = new TestApiFactory(
            RegistrationResult.Invalid(new Dictionary<string, string[]>
            {
                ["password"] = ["Passwords must be at least 8 characters."]
            }));
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { email = "person@example.com", password = "weak" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("password", problem.Errors.Keys);
    }

    [Fact]
    public void Identity_configuration_enforces_the_approved_password_policy()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(new ConfigurationBuilder().Build());

        using var provider = services.BuildServiceProvider();
        var policy = provider.GetRequiredService<IOptions<IdentityOptions>>().Value.Password;

        Assert.Equal(8, policy.RequiredLength);
        Assert.True(policy.RequireUppercase);
        Assert.True(policy.RequireLowercase);
        Assert.True(policy.RequireDigit);
        Assert.True(policy.RequireNonAlphanumeric);
        Assert.True(provider.GetRequiredService<IOptions<IdentityOptions>>().Value.User.RequireUniqueEmail);
    }

    private sealed class TestApiFactory(RegistrationResult result)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IUserRegistrationService>();
                services.AddSingleton<IUserRegistrationService>(
                    new StubRegistrationService(result));
            });
        }
    }

    private sealed class StubRegistrationService(RegistrationResult result)
        : IUserRegistrationService
    {
        public Task<RegistrationResult> RegisterAsync(string email, string password) =>
            Task.FromResult(result);
    }
}
