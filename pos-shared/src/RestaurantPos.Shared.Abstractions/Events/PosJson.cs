using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestaurantPos.Shared.Abstractions.Events;

public static class PosJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };
}