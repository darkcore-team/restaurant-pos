using System.ComponentModel.DataAnnotations;

namespace IdentityService.Infrastructure.Configuration;

public sealed class IdentityOptions
{
    public const string SectionName = "Identity";

    public string Issuer { get; set; } = string.Empty;
    public string JwtAudience { get; set; } = string.Empty;

    public int AccessTokenExpiration { get; set; } = 15;
    public int LoginMaxFailedAttempts { get; set; } = 5;
    
    public string WebClientId { get; set; } = "pos-web";
    public string WebClientSecret { get; set; } = "dev-secret";
    public string TokenEndpointUrl { get; set; } = "http://localhost:8081/connect/token";
    public int RefreshTokenExpiration { get; set; } = 20160;
    public bool CookieSecure { get; set; }
    public string CookieSameSite { get; set; } = "Strict";
}
