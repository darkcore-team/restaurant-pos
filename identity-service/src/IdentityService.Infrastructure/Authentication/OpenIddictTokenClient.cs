using System.Text.Json;
using IdentityService.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace IdentityService.Infrastructure.Authentication;

public sealed class OpenIddictTokenClient(
    IHttpClientFactory httpClientFactory,
    IOptions<IdentityOptions> options)
{
    public const string HttpClientName = "openiddict-token";

    public sealed record TokenResult(
        bool Success,
        string? Error,
        string? ErrorDescription,
        string? AccessToken,
        string? RefreshToken,
        long ExpiresIn);

    public async Task<TokenResult> RequestTokenAsync(
        Dictionary<string, string> form,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(HttpClientName);
        var url = options.Value.TokenEndpointUrl;

        using var response = await client.PostAsync(url, new FormUrlEncodedContent(form), cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = "invalid_grant";
            string? description = null;

            if (TryReadPayload(payload, out var errorDocument))
            {
                if (errorDocument.TryGetProperty("error", out var errorElement))
                    error = errorElement.GetString() ?? error;

                if (errorDocument.TryGetProperty("error_description", out var descriptionElement))
                    description = descriptionElement.GetString();
            }

            return new TokenResult(false, error, description, null, null, 0);
        }

        if (!TryReadPayload(payload, out var document))
            return new TokenResult(false, "invalid_response", "Respuesta invalida del token endpoint.", null, null, 0);

        var accessToken = GetString(document, "access_token");
        var refreshToken = GetString(document, "refresh_token");
        var expiresIn = document.TryGetProperty("expires_in", out var expiresInElement)
                        && expiresInElement.TryGetInt64(out var seconds) ? seconds : 0;

        if (accessToken is null)
            return new TokenResult(false, "invalid_response", "El token endpoint no devolvio access_token.", null, null,
                0);
        
        if (refreshToken is null)
            return new TokenResult(false, 
                "invalid_response", "El token endpoint no devolvio refresh_token (falta el scope offline_access).", null, null, 0);

        return new TokenResult(true, null, null, accessToken, refreshToken, expiresIn);
    }

    private static bool TryReadPayload(string payload, out JsonElement element)
    {
        element = default;

        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return false;

            element = document.RootElement.Clone();
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string? GetString(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
