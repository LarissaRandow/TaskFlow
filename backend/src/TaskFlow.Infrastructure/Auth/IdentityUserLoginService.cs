using Microsoft.AspNetCore.Identity;
using TaskFlow.Application.Auth.Login;
using TaskFlow.Application.Auth.Tokens;
using TaskFlow.Domain.Users;

namespace TaskFlow.Infrastructure.Auth;

public sealed class IdentityUserLoginService(
    UserManager<ApplicationUser> userManager,
    IAccessTokenIssuer accessTokenIssuer) : IUserLoginService
{
    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
        {
            return LoginResult.InvalidCredentials();
        }

        var account = new UserAccount(user.Id, user.Email!);
        return LoginResult.Authenticated(account, accessTokenIssuer.Issue(account));
    }
}
