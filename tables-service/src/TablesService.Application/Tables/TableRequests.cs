using System.ComponentModel.DataAnnotations;

namespace TablesService.Application.Tables;

public sealed class CreateTableRequest
{
    [Required] public long ZoneId { get; set; }
    [Required] public string Code { get; set; } = null!;
    [Required] public string Name { get; set; } = null!;
    public short MinCapacity { get; set; } = 1;
    public short MaxCapacity { get; set; } = 4;
    public bool IsCombinable { get; set; }
    public short Position { get; set; }
    public string? Notes { get; set; }
}

public sealed class UpdateTableStatusRequest
{
    [Required] public string Status { get; set; } = null!;
}

public sealed class AssignTableRequest
{
    [Required] public long WaiterId { get; set; }
}