using System.Text;

namespace TaskFlow.Application.Auth.Models;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SigningKey { get; set; } = string.Empty;

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Issuer) &&
        !string.IsNullOrWhiteSpace(Audience) &&
        Encoding.UTF8.GetByteCount(SigningKey) >= 32;

    public void Validate()
    {
        if (!IsValid)
        {
            throw new InvalidOperationException(
                "Configure Jwt:Issuer, Jwt:Audience, and a signing key of at least 32 UTF-8 bytes.");
        }
    }
}
