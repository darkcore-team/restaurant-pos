using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantPos.Shared.Infrastructure.Authentication;

public static class PosAntiForgeryExtensions
{
    public const string HeaderName = PosAuthCookies.CsrfHeader;

    public static IServiceCollection AddPosAntiForgery(this IServiceCollection services)
    {
        var dp = services.AddDataProtection()
            .SetApplicationName("RestaurantPos");

        var sharedSecret = Environment.GetEnvironmentVariable("DATA_PROTECTION_KEY");
        var keysDir = Environment.GetEnvironmentVariable("DATA_PROTECTION_KEYS_DIR");

        if (!string.IsNullOrWhiteSpace(sharedSecret))
        {
            dp.DisableAutomaticKeyGeneration();
            services.Configure<KeyManagementOptions>(o =>
            {
                o.XmlRepository = new EnvSharedKeyXmlRepository(sharedSecret);
            });
        }
        else if (!string.IsNullOrWhiteSpace(keysDir))
        {
            dp.PersistKeysToFileSystem(new DirectoryInfo(keysDir));
        }

        services.AddAntiforgery(options =>
        {
            options.HeaderName = HeaderName;
            options.Cookie.Name = PosAuthCookies.Csrf;
            options.Cookie.Path = "/";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        return services;
    }

    /// <summary>
    /// Deterministic in-memory DataProtection key ring derived from DATA_PROTECTION_KEY so
    /// multiple stateless containers (e.g. Render Free) can validate each other's CSRF cookies
    /// without requiring a shared disk volume.
    /// </summary>
    private sealed class EnvSharedKeyXmlRepository : IXmlRepository
    {
        private static readonly Guid FixedKeyId = new("7f9c4e12-3b6a-4d8e-9a21-5c0b8e7d6a43");
        private readonly XElement _keyElement;

        public EnvSharedKeyXmlRepository(string secret)
        {
            var masterKey = Convert.ToBase64String(SHA512.HashData(Encoding.UTF8.GetBytes(secret)));
            XNamespace p4 = "http://schemas.aspNet.org/2015/03/dataProtection";

            _keyElement = new XElement("key",
                new XAttribute("id", FixedKeyId),
                new XAttribute("version", "1"),
                new XElement("creationDate", "2026-01-01T00:00:00.0000000Z"),
                new XElement("activationDate", "2026-01-01T00:00:00.0000000Z"),
                new XElement("expirationDate", "2099-01-01T00:00:00.0000000Z"),
                new XElement("descriptor",
                    new XAttribute("deserializerType",
                        "Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel.AuthenticatedEncryptorDescriptorDeserializer, Microsoft.AspNetCore.DataProtection"),
                    new XElement("descriptor",
                        new XElement("encryption", new XAttribute("algorithm", "AES_256_CBC")),
                        new XElement("validation", new XAttribute("algorithm", "HMACSHA256")),
                        new XElement("masterKey",
                            new XAttribute(XNamespace.Xmlns + "p4", p4.NamespaceName),
                            new XAttribute(p4 + "requiresEncryption", "false"),
                            new XElement("value", masterKey)))));
        }

        public IReadOnlyCollection<XElement> GetAllElements() => [_keyElement];

        public void StoreElement(XElement element, string friendlyName)
        {
            // Intentionally read-only: key rotation is controlled via DATA_PROTECTION_KEY.
        }
    }

    public class Filter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
            await antiforgery.ValidateRequestAsync(context.HttpContext);
            return await next(context);
        }
    }
}
