using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using TaskFlow.Application.Auth.Login;
using TaskFlow.Application.Auth.Register;
using TaskFlow.Application.Auth.Tokens;
using TaskFlow.Domain.Users;
using TaskFlow.Infrastructure.Auth;
using Xunit;

namespace TaskFlow.Api.Tests;

public sealed class AuthLoginTests
{
    private static readonly UserAccount User = new(
        Guid.Parse("a9e39085-d437-4ea2-9008-f22db16da85e"),
        "person@example.com");

    [Fact]
    public async Task Login_returns_bearer_token_expiry_and_user()
    {
        var issuedToken = new IssuedAccessToken(
            "signed-jwt",
            DateTimeOffset.Parse("2026-11-07T22:00:00Z"));
        using var factory = new TestApiFactory(
            LoginResult.Authenticated(User, issuedToken));
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = User.Email, password = "Example-password1!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var body = System.Text.Json.JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        Assert.Equal(
            new[] { "accessToken", "expiresAt", "tokenType", "user" },
            body.RootElement.EnumerateObject()
                .Select(property => property.Name)
                .Order()
                .ToArray());
        Assert.Equal("signed-jwt", body.RootElement.GetProperty("accessToken").GetString());
        Assert.Equal("Bearer", body.RootElement.GetProperty("tokenType").GetString());
        Assert.Equal(
            issuedToken.ExpiresAt,
            body.RootElement.GetProperty("expiresAt").GetDateTimeOffset());
        Assert.Equal(User.Id, body.RootElement.GetProperty("user").GetProperty("id").GetGuid());
        Assert.Equal(
            User.Email,
            body.RootElement.GetProperty("user").GetProperty("email").GetString());
    }

    [Fact]
    public async Task Login_returns_a_generic_problem_for_invalid_credentials()
    {
        using var factory = new TestApiFactory(LoginResult.InvalidCredentials());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = User.Email, password = "incorrect-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(401, problem.Status);
        Assert.Equal("Invalid email or password.", problem.Detail);
    }

    [Fact]
    public async Task Login_validates_request_fields()
    {
        using var factory = new TestApiFactory(LoginResult.InvalidCredentials());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "not-an-email", password = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Development_api_starts_without_a_configured_signing_key()
    {
        using var factory = new TestApiFactory(
            LoginResult.InvalidCredentials(),
            configureJwtOptions: false);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/_test/protected");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Issued_token_authenticates_protected_api_requests()
    {
        var issuer = new JwtAccessTokenIssuer(Options.Create(TestJwtOptions()));
        var issuedToken = issuer.Issue(User);
        using var factory = new TestApiFactory(LoginResult.InvalidCredentials());
        using var client = factory.CreateClient();

        var anonymousResponse = await client.GetAsync("/_test/protected");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "not-a-jwt");
        var invalidTokenResponse = await client.GetAsync("/_test/protected");
        Assert.Equal(HttpStatusCode.Unauthorized, invalidTokenResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateExpiredToken());
        var expiredTokenResponse = await client.GetAsync("/_test/protected");
        Assert.Equal(HttpStatusCode.Unauthorized, expiredTokenResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", issuedToken.Token);
        var authenticatedResponse = await client.GetAsync("/_test/protected");

        Assert.Equal(HttpStatusCode.OK, authenticatedResponse.StatusCode);
        Assert.Equal(
            User.Id.ToString(),
            await authenticatedResponse.Content.ReadAsStringAsync());
        Assert.InRange(
            issuedToken.ExpiresAt,
            DateTimeOffset.UtcNow.AddMinutes(59).AddSeconds(-1),
            DateTimeOffset.UtcNow.AddMinutes(61));
    }

    private static string CreateExpiredToken()
    {
        var settings = TestJwtOptions();
        var now = DateTime.UtcNow;
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: [new Claim("sub", User.Id.ToString())],
            notBefore: now.AddMinutes(-3),
            expires: now.AddMinutes(-2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static JwtOptions TestJwtOptions() => new()
    {
        Issuer = AuthRegistrationTests.TestConfiguration.Values["Jwt:Issuer"]!,
        Audience = AuthRegistrationTests.TestConfiguration.Values["Jwt:Audience"]!,
        SigningKey = AuthRegistrationTests.TestConfiguration.Values["Jwt:SigningKey"]!
    };

    private sealed class TestApiFactory(
        LoginResult result,
        bool configureJwtOptions = true) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureTestServices(services =>
            {
                if (configureJwtOptions)
                {
                    AuthRegistrationTests.ConfigureJwtOptions(services);
                }

                services.AddControllers()
                    .AddApplicationPart(typeof(ProtectedTestController).Assembly);
                services.RemoveAll<IUserRegistrationService>();
                services.AddSingleton<IUserRegistrationService>(new StubRegistrationService());
                services.RemoveAll<IUserLoginService>();
                services.AddSingleton<IUserLoginService>(new StubLoginService(result));
            });
        }
    }

    private sealed class StubRegistrationService : IUserRegistrationService
    {
        public Task<RegistrationResult> RegisterAsync(string email, string password) =>
            Task.FromResult(RegistrationResult.Invalid(
                new Dictionary<string, string[]>()));
    }

    private sealed class StubLoginService(LoginResult result) : IUserLoginService
    {
        public Task<LoginResult> LoginAsync(string email, string password) =>
            Task.FromResult(result);
    }
}

[ApiController]
[Authorize]
[Route("_test/protected")]
public sealed class ProtectedTestController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get() =>
        Ok(User.FindFirstValue("sub"));
}
