using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Auth.Models;

public sealed class RegisterUserRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
