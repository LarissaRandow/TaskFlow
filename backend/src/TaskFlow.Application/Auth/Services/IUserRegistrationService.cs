using TaskFlow.Domain.Users;
using TaskFlow.Application.Auth.Models;

namespace TaskFlow.Application.Auth.Services;

public interface IUserRegistrationService
{
    Task<RegistrationResult> RegisterAsync(string email, string password);
}
