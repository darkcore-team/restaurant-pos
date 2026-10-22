using System.Text;
using System.Text.Json;

namespace IdentityService.Infrastructure.Authentication;

public static class JwtPayloadReader
{
    public static JsonElement? TryReadPayload(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var parts = token.Split('.');
        if (parts.Length < 2)
            return null;

        var payload = parts[1].Replace('-', '+').Replace('_', '/');

        switch (payload.Length % 4)
        {
            case 0:
                break;
            case 2:
                payload += "==";
                break;
            case 3:
                payload += "=";
                break;
            default:
                return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(payload);
            using var document = JsonDocument.Parse(bytes);
            return document.RootElement.Clone();
        }
        catch (FormatException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static long ReadInt64(JsonElement root, string claim)
    {
        if (root.TryGetProperty(claim, out var element) && element.TryGetInt64(out var value))
            return value;

        return 0;
    }

    public static string ReadString(JsonElement root, string claim)
    {
        if (root.TryGetProperty(claim, out var element) && element.ValueKind == JsonValueKind.String)
            return element.GetString() ?? string.Empty;

        return string.Empty;
    }

    public static IReadOnlyList<string> ReadRoles(JsonElement root)
    {
        if (!root.TryGetProperty("role", out var element))
            return Array.Empty<string>();

        return element.ValueKind switch
        {
            JsonValueKind.Array => element.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString()!)
                .ToArray(),
            JsonValueKind.String => new[] { element.GetString()! },
            _ => Array.Empty<string>()
        };
    }
}
