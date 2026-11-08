namespace TaskFlow.Application.Auth.Tokens;

public sealed record IssuedAccessToken(string Token, DateTimeOffset ExpiresAt);
