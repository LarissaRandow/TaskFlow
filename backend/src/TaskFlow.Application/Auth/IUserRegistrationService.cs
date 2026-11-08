using TaskFlow.Domain.Users;

namespace TaskFlow.Application.Auth;

public interface IUserRegistrationService
{
    Task<RegistrationResult> RegisterAsync(string email, string password);
}
