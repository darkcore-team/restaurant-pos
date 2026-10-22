using IdentityService.Domain.Constants;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using PosIdentityOptions = IdentityService.Infrastructure.Configuration.IdentityOptions;

namespace IdentityService.Infrastructure.Seed;

public sealed class IdentityDevSeeder(
    IdentityServiceDbContext db,
    IPasswordHasher<User> passwordHasher,
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictScopeManager scopeManager,
    IOptions<PosIdentityOptions> options)
{
    public const string WebClientId = "pos-web";
    public const string WebClientSecret = "dev-secret";
    public const string ApiScope = "api";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedAuthoritiesAsync(cancellationToken);
        await SeedRestaurantAndBranchAsync(cancellationToken);
        await SeedAdminUserAsync(cancellationToken);
        await SeedOpenIddictAsync(cancellationToken);
    }

    private async Task SeedAuthoritiesAsync(CancellationToken cancellationToken)
    {
        if (await db.Authorities.AnyAsync(cancellationToken))
        {
            return;
        }

        var authorities = new[]
        {
            ("ADMIN", "Administrador", 1),
            ("OWNER", "Propietario", 2),
            ("MANAGER", "Gerente", 3),
            ("CASHIER", "Cajero", 4),
            ("WAITER", "Mesero", 5),
            ("COOK", "Cocina", 6),
            ("HOST", "Anfitrión", 7),
        };
        
        db.Authorities.AddRange(authorities.Select(a => new Authority()
        {
            Code = a.Item1,
            Name = a.Item2,
            Position = (short)a.Item3,
            Status = RecordStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        }));

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedRestaurantAndBranchAsync(CancellationToken cancellationToken)
    {
        var restaurant = await db.Restaurants.FirstOrDefaultAsync(cancellationToken);
        if (restaurant is null)
        {
            restaurant = new Restaurant()
            {
                Name = "Rest A",
                Timezone = "America/Managua",
                DefaultLanguageId = 1,
                DefaultCurrencyId = 1,
                BaseCurrencyId = 1,
                Status = RecordStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };

            db.Restaurants.Add(restaurant);
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.Branches.AnyAsync(cancellationToken))
        {
            db.Branches.Add(new Branch()
            {
                RestaurantId = restaurant.Id,
                Code = "BR-A",
                Name = "Sucursal Principal",
                IsDefault = true,
                Status = RecordStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            });

            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(u => u.Username == "admin", cancellationToken))
        {
            return;
        }

        var restaurant = await db.Restaurants.SingleAsync(cancellationToken);
        var branch = await db.Branches.SingleAsync(b => b.RestaurantId == restaurant.Id, cancellationToken);

        var person = new Person
        {
            FirstName = "Administrador",
            LastName = "Sistema",
            Status = RecordStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.People.Add(person);
        await db.SaveChangesAsync(cancellationToken);

        var user = new User
        {
            PublicId = Guid.NewGuid(),
            PersonId = person.Id,
            Username = "admin",
            PasswordHash = passwordHasher.HashPassword(null!, "Admin123!"),
            MustChangePassword = false,
            Status = RecordStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        db.UserAuthorities.Add(new UserAuthority
        {
            UserId = user.Id,
            AuthorityId = await db.Authorities
                .Where(a => a.Code == "ADMIN")
                .Select(a => a.Id)
                .SingleAsync(cancellationToken),
            GrantedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        
        db.UserBranches.Add(new UserBranch
        {
            UserId = user.Id,
            BranchId = branch.Id,
            IsPrimary = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedOpenIddictAsync(CancellationToken cancellationToken)
    {
        if (await scopeManager.FindByNameAsync(ApiScope, cancellationToken) is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = ApiScope,
                DisplayName = "Restaurant POS API"
            }, cancellationToken);
        }

        var clientId = options.Value.WebClientId ?? WebClientId;
        var clientSecret = options.Value.WebClientSecret ?? WebClientSecret;

        if (await applicationManager.FindByClientIdAsync(clientId, cancellationToken) is null)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                DisplayName = "Restaurant POS Web",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Prefixes.Scope + ApiScope
                }
            };

            await applicationManager.CreateAsync(descriptor, cancellationToken);
        }
    }
}