using TaskFlow.Domain.Users;

namespace TaskFlow.Application.Auth.Models;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    LoginUserResponse User)
{
    public static LoginResponse From(UserAccount user, IssuedAccessToken accessToken) =>
        new(
            accessToken.Token,
            "Bearer",
            accessToken.ExpiresAt,
            LoginUserResponse.From(user));
}

public sealed record LoginUserResponse(Guid Id, string Email)
{
    public static LoginUserResponse From(UserAccount user) => new(user.Id, user.Email);
}
