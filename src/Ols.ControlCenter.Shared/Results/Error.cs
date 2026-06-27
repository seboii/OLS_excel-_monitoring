namespace Ols.ControlCenter.Shared.Results;

/// <summary>
/// Standart hata tipi. <see cref="Code"/> makine-okunur, <see cref="Message"/> son kullanıcıya
/// gösterilir (Türkçe). <see cref="Type"/> API katmanında HTTP durum koduna eşlenir.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error Validation(string message, string code = "validation_error")
        => new(code, message, ErrorType.Validation);

    public static Error NotFound(string message, string code = "not_found")
        => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string message, string code = "conflict")
        => new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(string message, string code = "unauthorized")
        => new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(string message, string code = "forbidden")
        => new(code, message, ErrorType.Forbidden);

    public static Error Failure(string message, string code = "failure")
        => new(code, message, ErrorType.Failure);
}

/// <summary>Hata sınıfı — API katmanında HTTP durum koduna eşlenmek için kullanılır.</summary>
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5
}
