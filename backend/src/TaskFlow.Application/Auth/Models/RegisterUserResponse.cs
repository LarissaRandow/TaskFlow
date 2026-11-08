using TaskFlow.Domain.Users;

namespace TaskFlow.Application.Auth.Models;

public sealed record RegisterUserResponse(Guid Id, string Email)
{
    public static RegisterUserResponse From(UserAccount user) => new(user.Id, user.Email);
}
