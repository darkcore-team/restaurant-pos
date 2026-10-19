using System.ComponentModel.DataAnnotations;

namespace OrdersService.Application.Orders;

public sealed class VoidItemRequest
{
    [Required] public string Reason { get; set; } = null!;
}