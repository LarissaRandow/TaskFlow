namespace TaskFlow.Api.Auth.Contracts;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    LoginUserResponse User);
