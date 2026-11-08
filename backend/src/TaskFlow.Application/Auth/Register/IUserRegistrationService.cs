namespace TaskFlow.Application.Auth.Register;

public interface IUserRegistrationService
{
    Task<RegistrationResult> RegisterAsync(string email, string password);
}
