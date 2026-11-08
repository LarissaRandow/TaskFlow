using TaskFlow.Domain.Users;

namespace TaskFlow.Application.Auth.Register;

public enum RegistrationStatus
{
    Created,
    DuplicateEmail,
    Invalid
}

public sealed record RegistrationResult(
    RegistrationStatus Status,
    UserAccount? User,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static RegistrationResult Created(UserAccount user) =>
        new(RegistrationStatus.Created, user, new Dictionary<string, string[]>());

    public static RegistrationResult DuplicateEmail() =>
        new(RegistrationStatus.DuplicateEmail, null, new Dictionary<string, string[]>());

    public static RegistrationResult Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(RegistrationStatus.Invalid, null, errors);
}
