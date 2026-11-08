using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Auth;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IUserRegistrationService registrationService) : ControllerBase
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
}
