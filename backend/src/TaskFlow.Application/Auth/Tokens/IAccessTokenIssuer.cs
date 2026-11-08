using TaskFlow.Domain.Users;

namespace TaskFlow.Application.Auth.Tokens;

public interface IAccessTokenIssuer
{
    IssuedAccessToken Issue(UserAccount user);
}
