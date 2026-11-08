using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Auth;
using TaskFlow.Domain.Users;

namespace TaskFlow.Infrastructure.Identity;

public sealed class IdentityUserRegistrationService(
    UserManager<ApplicationUser> userManager) : IUserRegistrationService
{
    public async Task<RegistrationResult> RegisterAsync(string email, string password)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email
        };

        try
        {
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                return RegistrationResult.Created(new UserAccount(user.Id, user.Email!));
            }

            if (result.Errors.Any(IsDuplicateEmailError))
            {
                return RegistrationResult.DuplicateEmail();
            }

            var errors = result.Errors
                .GroupBy(error => error.Code.StartsWith("Password", StringComparison.Ordinal)
                    ? "password"
                    : "email")
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.Description).ToArray());

            return RegistrationResult.Invalid(errors);
        }
        catch (DbUpdateException exception) when (IsUniqueEmailViolation(exception))
        {
            return RegistrationResult.DuplicateEmail();
        }
    }

    private static bool IsDuplicateEmailError(IdentityError error) =>
        error.Code is "DuplicateEmail" or "DuplicateUserName";

    private static bool IsUniqueEmailViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException sqlException &&
        sqlException.Errors
            .Cast<SqlError>()
            .Any(error =>
                error.Number is 2601 or 2627 &&
                (error.Message.Contains("EmailIndex", StringComparison.OrdinalIgnoreCase) ||
                 error.Message.Contains("UserNameIndex", StringComparison.OrdinalIgnoreCase)));
}
