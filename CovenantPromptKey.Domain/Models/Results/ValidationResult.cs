namespace CovenantPromptKey.Models.Results;

/// <summary>
/// 驗證結果封裝
/// </summary>
/// <typeparam name="T">驗證對象類型</typeparam>
public class ValidationResult<T>
{
    public bool IsValid { get; init; }

    /// <summary>
    /// Alias for IsValid for API consistency
    /// </summary>
    public bool IsSuccess => IsValid;

    public T? Value { get; init; }
    public List<string> Errors { get; init; } = [];
    public List<string> Warnings { get; init; } = [];

    /// <summary>
    /// Gets the first error message or empty string
    /// </summary>
    public string ErrorMessage => Errors.FirstOrDefault() ?? string.Empty;

    public static ValidationResult<T> Success(T value) =>
        new() { IsValid = true, Value = value };

    public static ValidationResult<T> Failure(params string[] errors) =>
        new() { IsValid = false, Errors = [.. errors] };

    public static ValidationResult<T> SuccessWithWarnings(T value, params string[] warnings) =>
        new() { IsValid = true, Value = value, Warnings = [.. warnings] };
}

/// <summary>
/// CSV 驗證結果
/// </summary>
public class CsvValidationResult
{
    public bool IsValid { get; init; }
    public List<CsvError> Errors { get; init; } = [];
}

/// <summary>
/// CSV 錯誤詳情
/// </summary>
public class CsvError
{
    public int LineNumber { get; init; }
    public string Message { get; init; } = string.Empty;
}
