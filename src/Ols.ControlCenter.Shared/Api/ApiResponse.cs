namespace Ols.ControlCenter.Shared.Api;

/// <summary>Tek bir hata kalemi. <see cref="Field"/> doluysa form alanına bağlı validasyon hatasıdır.</summary>
public sealed record ApiError(string Code, string Message, string? Field = null);

/// <summary>Tüm API yanıtları için standart zarf (veri taşımayan yanıtlar).</summary>
public class ApiResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<ApiError> Errors { get; init; } = Array.Empty<ApiError>();
    public string? TraceId { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public static ApiResponse Ok(string? message = null)
        => new() { Success = true, Message = message };

    public static ApiResponse Fail(string message, IReadOnlyList<ApiError>? errors = null)
        => new() { Success = false, Message = message, Errors = errors ?? Array.Empty<ApiError>() };
}

/// <summary>Veri taşıyan standart API yanıt zarfı.</summary>
public sealed class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static new ApiResponse<T> Fail(string message, IReadOnlyList<ApiError>? errors = null)
        => new() { Success = false, Message = message, Errors = errors ?? Array.Empty<ApiError>() };
}
