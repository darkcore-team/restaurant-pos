namespace RestaurantPos.Shared.Abstractions.Common;

public sealed class Answer
{
    public string? Code { get; init; }

    public string Message { get; init; } = string.Empty;

    public bool Success { get; init; }

    public object? Data { get; init; }

    public static Answer Ok(object? data = null, string message = "OK", string? code = null)
        => new() { Code = code, Message = message, Success = true, Data = data };

    public static Answer Fail(string code, string message, object? data = null)
        => new() { Code = code, Message = message, Success = false, Data = data };
}

public sealed class Answer<T>
{
    public string? Code { get; init; }

    public string Message { get; init; } = string.Empty;

    public bool Success { get; init; }

    public T? Data { get; init; }

    public static Answer<T> Ok(T? data = default, string message = "OK", string? code = null)
        => new() { Code = code, Message = message, Success = true, Data = data };

    public static Answer<T> Fail(string code, string message, T? data = default)
        => new() { Code = code, Message = message, Success = false, Data = data };
}