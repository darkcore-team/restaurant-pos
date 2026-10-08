using System.ComponentModel.DataAnnotations;

namespace IdentityService.Infrastructure.Configuration;

public sealed class IdentityOptions
{
    public const string SectionName = "Identity";

    public string Issuer { get; set; } = string.Empty;
    public string JwtAudience { get; set; } = string.Empty;

    public int AccessTokenExpiration { get; set; } = 15;
    public int LoginMaxFailedAttempts { get; set; } = 5;
}
