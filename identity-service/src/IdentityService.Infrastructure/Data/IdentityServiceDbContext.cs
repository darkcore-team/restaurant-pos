using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using RestaurantPos.Shared.Persistence;

namespace IdentityService.Infrastructure.Data;

public sealed class IdentityServiceDbContext(DbContextOptions options) : TenantDbContext(options)
{
    protected override string Schema => "identity";
    
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<PersonContact> PeopleContacts => Set<PersonContact>();
    public DbSet<Authority> Authorities => Set<Authority>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserAuthority> UserAuthorities => Set<UserAuthority>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityServiceDbContext).Assembly);
    }
}
