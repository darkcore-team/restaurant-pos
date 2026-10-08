namespace IdentityService.Domain.Entities;

public class UserBranch
{
    public long Id  { get; set; }
    public long UserId { get; set; }
    public long BranchId { get; set; }
    public bool IsPrimary { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
}