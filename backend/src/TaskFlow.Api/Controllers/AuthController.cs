using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Auth.Models;
using TaskFlow.Application.Auth.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IUserRegistrationService registrationService,
    IUserLoginService loginService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterUserResponse>> Register(
        [FromBody] RegisterUserRequest request)
    {
        var result = await registrationService.RegisterAsync(request.Email, request.Password);

        return result.Status switch
        {
            RegistrationStatus.Created when result.User is not null =>
                Created("/api/auth/register", RegisterUserResponse.From(result.User)),
            RegistrationStatus.DuplicateEmail =>
                Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Email already registered",
                    detail: "An account with this email already exists."),
            RegistrationStatus.Invalid => BadRequest(new ValidationProblemDetails(
                result.Errors.ToDictionary(error => error.Key, error => error.Value))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            }),
            _ => throw new InvalidOperationException(
                "The registration service returned an inconsistent result.")
        };
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await loginService.LoginAsync(request.Email, request.Password);

        return result.Status switch
        {
            LoginStatus.Authenticated when result.User is not null && result.AccessToken is not null =>
                Ok(LoginResponse.From(result.User, result.AccessToken)),
            LoginStatus.InvalidCredentials =>
                Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Invalid credentials",
                    detail: "Invalid email or password."),
            _ => throw new InvalidOperationException(
                "The login service returned an inconsistent result.")
        };
    }
}
