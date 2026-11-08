namespace TaskFlow.Application.Auth.Models;

public sealed record IssuedAccessToken(string Token, DateTimeOffset ExpiresAt);
