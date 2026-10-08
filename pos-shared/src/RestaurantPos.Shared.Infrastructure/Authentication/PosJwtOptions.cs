namespace RestaurantPos.Shared.Infrastructure.Authentication;

public sealed class PosJwtOptions
{
    public const string SectionName = "Jwt";

    /// Base URL of identity-service, used to fetch OpenIddict discovery document.
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; } = true;
}
