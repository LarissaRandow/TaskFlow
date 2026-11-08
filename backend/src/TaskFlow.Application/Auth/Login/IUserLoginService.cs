namespace TaskFlow.Application.Auth.Login;

public interface IUserLoginService
{
    Task<LoginResult> LoginAsync(string email, string password);
}
