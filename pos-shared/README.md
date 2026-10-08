# RestaurantPos.Shared

Biblioteca compartida por los servicios del Restaurant POS. Empaquetada y publicada en GitHub Packages bajo `darkcore-team`.

## Paquetes

| Paquete | Contenido |
| --- | --- |
| `RestaurantPos.Shared.Abstractions` | `ITenantContext`, `DomainEvent`, `IEventBus`, `PagedResult` |
| `RestaurantPos.Shared.Persistence` | `TenantDbContext`, RLS, outbox transaccional |
| `RestaurantPos.Shared.Infrastructure` | JWT, CSRF, health checks, RabbitMQ |

## Instalación

```bash
dotnet nuget add source https://nuget.pkg.github.com/darkcore-team/index.json \
  --name GitHubPackages \
  --username darkcore-team \
  --password <PAT>
<PackageReference Include="RestaurantPos.Shared.Infrastructure" Version="1.0.0" />

El PAT requiere read:packages.

Versiones

Abstractions define el contrato que consumen los siete servicios. Un cambio incompatible obliga a
publicar una versión mayor y a actualizar todos los servicios en el mismo despliegue.

Licencia

MIT.

## `NuGet.config` — nuevo

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>

  <packageSources>
    <!-- Drops any source configured at machine level. Scoped to this repo on purpose:
         a service repo must not silently resolve packages from an unrelated feed. -->
    <clear />

    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="GitHubPackages" value="https://nuget.pkg.github.com/darkcore-team/index.json" />
  </packageSources>

  <!-- Without this, every package is looked up in both feeds. -->
  <packageSourceMapping>
    <packageSource key="GitHubPackages">
      <package pattern="RestaurantPos.Shared.*" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>

</configuration>