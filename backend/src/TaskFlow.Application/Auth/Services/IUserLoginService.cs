using TaskFlow.Application.Auth.Models;

namespace TaskFlow.Application.Auth.Services;

public interface IUserLoginService
{
    Task<LoginResult> LoginAsync(string email, string password);
}
