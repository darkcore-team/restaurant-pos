using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

namespace IdentityService.Infrastructure.Authentication;

public sealed class UserSessionRevoker(
    IOpenIddictTokenManager tokenManager,
    IOpenIddictAuthorizationManager authorizationManager,
    ILogger<UserSessionRevoker> logger)
{
    public async Task RevokeAllAsync(string subject, CancellationToken cancellationToken = default)
    {
        await authorizationManager.RevokeBySubjectAsync(subject, cancellationToken);
        await tokenManager.RevokeBySubjectAsync(subject, cancellationToken);

        logger.LogInformation("Revoked every OpenIddict session of subject {Subject}.", subject);
    }
    
    public async Task<string?> FindSubjectAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await tokenManager.FindByReferenceIdAsync(refreshToken, cancellationToken);
        return token is null ? null : await tokenManager.GetSubjectAsync(token, cancellationToken);
    }
}
