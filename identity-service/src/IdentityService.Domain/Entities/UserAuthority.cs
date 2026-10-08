namespace IdentityService.Domain.Entities;

public class UserAuthority
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long AuthorityId { get; set; }
    public DateTimeOffset GrantedAt { get; set; }
    public long? GrantedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
}
