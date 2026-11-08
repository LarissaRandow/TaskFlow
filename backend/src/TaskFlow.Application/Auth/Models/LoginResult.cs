using TaskFlow.Domain.Users;

namespace TaskFlow.Application.Auth.Models;

public enum LoginStatus
{
    Authenticated,
    InvalidCredentials
}

public sealed record LoginResult(
    LoginStatus Status,
    UserAccount? User,
    IssuedAccessToken? AccessToken)
{
    public static LoginResult Authenticated(UserAccount user, IssuedAccessToken accessToken) =>
        new(LoginStatus.Authenticated, user, accessToken);

    public static LoginResult InvalidCredentials() =>
        new(LoginStatus.InvalidCredentials, null, null);
}
