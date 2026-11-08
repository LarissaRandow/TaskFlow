using TaskFlow.Domain.Users;
using TaskFlow.Application.Auth.Models;

namespace TaskFlow.Application.Auth.Services;

public interface IAccessTokenIssuer
{
    IssuedAccessToken Issue(UserAccount user);
}
