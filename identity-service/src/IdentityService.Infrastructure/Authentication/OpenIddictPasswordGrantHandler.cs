using System.Collections.Immutable;
using System.Security.Claims;
using IdentityService.Domain.Constants;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using RestaurantPos.Shared.Infrastructure.Authentication;
using IdentityOptions = IdentityService.Infrastructure.Configuration.IdentityOptions;

namespace IdentityService.Infrastructure.Authentication;

public sealed class OpenIddictPasswordGrantHandler(
    IdentityServiceDbContext db,
    IPasswordHasher<User> passwordHasher,
    IOptions<IdentityOptions> options,
    IHttpContextAccessor httpContextAccessor) : IOpenIddictServerHandler<OpenIddictServerEvents.HandleTokenRequestContext>
{
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public static OpenIddictServerHandlerDescriptor Descriptor { get; } = OpenIddictServerHandlerDescriptor
        .CreateBuilder<OpenIddictServerEvents.HandleTokenRequestContext>()
        .UseSingletonHandler<OpenIddictPasswordGrantHandler>()
        .Build();
    
    public async ValueTask HandleAsync(OpenIddictServerEvents.HandleTokenRequestContext context)
    {
        if (!context.Request.IsPasswordGrantType())
            return;
        
        if (string.IsNullOrWhiteSpace(context.Request.Username) || string.IsNullOrWhiteSpace(context.Request.Password))
        {
            context.Reject(OpenIddictConstants.Errors.InvalidGrant, "The username or password is required");
            return;
        }

        var username = context.Request.Username.Trim();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Username == username)
                   ?? await UnknownAttemptAsync(username);

        if (user == null)
        {
            context.Reject(OpenIddictConstants.Errors.InvalidGrant, "The username or password is incorrect");
            return;
        };

        if (user.Status != RecordStatus.Active)
        {
            await RecordAttemptAsync(username, user.Id, wasSuccessful: false, failureReason: "INACTIVE_ACCOUNT");
            context.Reject(OpenIddictConstants.Errors.InvalidGrant, "The account is not active");
            return;
        }

        if (user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            await RecordAttemptAsync(username, user.Id, wasSuccessful: false, failureReason: "LOCKED_OUT");
            context.Reject(OpenIddictConstants.Errors.InvalidGrant, "The account is locked");
            return;
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, context.Request.Password);

        if (verification == PasswordVerificationResult.Failed)
        {
            user.FailedCount = (short)(user.FailedCount + 1);
            if (user.FailedCount >= options.Value.LoginMaxFailedAttempts)
            {
                user.FailedCount = 0;
                user.LockoutEnd = DateTimeOffset.UtcNow.Add(LockoutDuration);
            }
            
            await db.SaveChangesAsync();
            await RecordAttemptAsync(username, user.Id, wasSuccessful: false, failureReason: "BAD_PASSWORD");
            context.Reject(OpenIddictConstants.Errors.InvalidGrant, "The username or password is incorrect");
            return;
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, context.Request.Password);
        }

        user.FailedCount = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync();
        await RecordAttemptAsync(username, user.Id, wasSuccessful: true, failureReason: null);
        
        var person = await db.People.SingleOrDefaultAsync(p => p.Id == user.PersonId);

        var roleCodes = await db.UserAuthorities
            .Where(ua => ua.UserId == user.Id)
            .Join(db.Authorities,
                ua => ua.AuthorityId,
                a => a.Id,
                (ua, a) => a.Code)
            .ToListAsync();

        var branch = await db.UserBranches
            .Where(ub => ub.UserId == user.Id && ub.IsPrimary)
            .Join(db.Branches,
                ub => ub.BranchId,
                b => b.Id,
                (ub, b) => b)
            .SingleOrDefaultAsync();
        
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: OpenIddictConstants.Claims.Name, 
            roleType: OpenIddictConstants.Claims.Role);
        
        identity.SetClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());
        identity.SetClaim(PosClaims.DisplayName, $"{person?.FirstName} {person?.LastName}".Trim());
        identity.SetClaim(PosClaims.RestaurantId, branch?.RestaurantId.ToString() ?? string.Empty);
        identity.SetClaim(PosClaims.BranchId, branch?.Id.ToString() ?? string.Empty);
        identity.SetClaims(OpenIddictConstants.Claims.Role, roleCodes.ToImmutableArray());
        
        identity.SetDestinations(_ => new[] { OpenIddictConstants.Destinations.AccessToken });

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(context.Request.GetScopes());
        
        context.SignIn(principal);
    }

    private async Task<User?> UnknownAttemptAsync(string username)
    {
        await RecordAttemptAsync(username, userId: null, wasSuccessful: false, failureReason: "USER_NOT_FOUND");
        return null;
    }

    private async Task RecordAttemptAsync(string username, long? userId, bool wasSuccessful, string? failureReason)
    {
        var http = httpContextAccessor.HttpContext;
        var entity = new LoginAttempt()
        {
            UserId = userId,
            UsernameTry = username,
            Ip = http?.Connection.RemoteIpAddress,
            UserAgent = http?.Request?.Headers.UserAgent.ToString(),
            WasSuccessful = wasSuccessful,
            FailureReason = failureReason
        };
        
        db.LoginAttempts.Add(entity);
        await db.SaveChangesAsync();
    }
}