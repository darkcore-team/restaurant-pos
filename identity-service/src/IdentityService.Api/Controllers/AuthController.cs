using IdentityService.Application.Auth;
using IdentityService.Infrastructure.Authentication;
using IdentityService.Infrastructure.Configuration;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    OpenIddictTokenClient tokenClient,
    UserSessionRevoker sessionRevoker,
    IAntiforgery antiforgery,
    IOptions<IdentityOptions> options) : ControllerBase
{
    [HttpGet("csrf")]
    [AllowAnonymous]
    public IActionResult GetCsrfToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(Answer<CsrfTokenDto>.Ok(new CsrfTokenDto(tokens.RequestToken ?? string.Empty)));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(Answer.Fail("INVALID_CREDENTIALS", "Usuario y contrasena son requeridos."));

        var result = await tokenClient.RequestTokenAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = request.Username.Trim(),
            ["password"] = request.Password,
            ["client_id"] = options.Value.WebClientId,
            ["client_secret"] = options.Value.WebClientSecret,
            ["scope"] = "api"
        }, cancellationToken);

        if (!result.Success || string.IsNullOrEmpty(result.AccessToken))
            return Unauthorized(Answer.Fail(result.Error ?? "invalid_grant", result.ErrorDescription ?? "Credenciales invalidas."));

        SetAuthCookies(result);
        return Ok(Answer<SessionDto>.Ok(BuildSession(result.AccessToken)));
    }
    
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[PosAuthCookies.RefreshToken];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(Answer.Fail("NO_REFRESH_TOKEN", "No hay sesion activa."));

        var result = await tokenClient.RequestTokenAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = options.Value.WebClientId,
            ["client_secret"] = options.Value.WebClientSecret,
            ["scope"] = "api"
        }, cancellationToken);

        if (!result.Success || string.IsNullOrEmpty(result.AccessToken))
        {
            ClearAuthCookies();
            return Unauthorized(Answer.Fail(result.Error ?? "invalid_grant", result.ErrorDescription ?? "Sesion expirada."));
        }

        SetAuthCookies(result);
        return Ok(Answer<SessionDto>.Ok(BuildSession(result.AccessToken)));
    }
    
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[PosAuthCookies.RefreshToken];

        if (!string.IsNullOrEmpty(refreshToken))
        {
            var subject = await sessionRevoker.FindSubjectAsync(refreshToken, cancellationToken);
            if (subject is not null)
                await sessionRevoker.RevokeAllAsync(subject, cancellationToken);
        }

        ClearAuthCookies();
        return Ok(Answer<object?>.Ok(null));
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var subject = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (subject is null || !long.TryParse(subject, out var userId))
            return Unauthorized(Answer.Fail("INVALID_SESSION", "Sesion invalida."));

        var session = new SessionDto(
            userId,
            User.FindFirst(PosClaims.PreferredUsername)?.Value ?? string.Empty,
            User.FindFirst(PosClaims.DisplayName)?.Value ?? string.Empty,
            User.RestaurantId() ?? 0,
            User.BranchId() ?? 0,
            User.FindAll(OpenIddictConstants.Claims.Role).Select(claim => claim.Value).ToArray());

        return Ok(Answer<SessionDto>.Ok(session));
    }

    private void SetAuthCookies(OpenIddictTokenClient.TokenResult result)
    {
        if (!string.IsNullOrEmpty(result.AccessToken))
            Response.Cookies.Append(PosAuthCookies.AccessToken, result.AccessToken, BuildCookieOptions(options.Value.AccessTokenExpiration));

        if (!string.IsNullOrEmpty(result.RefreshToken))
            Response.Cookies.Append(PosAuthCookies.RefreshToken, result.RefreshToken, BuildCookieOptions(options.Value.RefreshTokenExpiration));
    }

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete(PosAuthCookies.AccessToken, new CookieOptions { Path = "/" });
        Response.Cookies.Delete(PosAuthCookies.RefreshToken, new CookieOptions { Path = "/" });
    }

    private CookieOptions BuildCookieOptions(int lifetimeMinutes)
    {
        var sameSite = options.Value.CookieSameSite?.ToLowerInvariant() switch
        {
            "none" => SameSiteMode.None,
            "lax" => SameSiteMode.Lax,
            _ => SameSiteMode.Strict
        };

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = options.Value.CookieSecure,
            SameSite = sameSite,
            Path = "/",
            MaxAge = TimeSpan.FromMinutes(lifetimeMinutes),
            IsEssential = true
        };
    }

    private static SessionDto? BuildSession(string accessToken)
    {
        var payload = JwtPayloadReader.TryReadPayload(accessToken);
        if (payload is null)
            return null;

        var root = payload.Value;
        return new SessionDto(
            JwtPayloadReader.ReadInt64(root, OpenIddictConstants.Claims.Subject),
            JwtPayloadReader.ReadString(root, PosClaims.PreferredUsername),
            JwtPayloadReader.ReadString(root, PosClaims.DisplayName),
            JwtPayloadReader.ReadInt64(root, PosClaims.RestaurantId),
            JwtPayloadReader.ReadInt64(root, PosClaims.BranchId),
            JwtPayloadReader.ReadRoles(root));
    }
}
