using TaskFlow.Domain.Users;

using TaskFlow.Application.Auth.Tokens;

namespace TaskFlow.Application.Auth.Login;

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
