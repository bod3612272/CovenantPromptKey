namespace CovenantPromptKey.Models.Results;

/// <summary>
/// CSV 匯入操作結果
/// </summary>
public class CsvImportResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Alias for Success for API consistency
    /// </summary>
    public bool IsSuccess => Success;

    /// <summary>
    /// 成功匯入的數量
    /// </summary>
    public int ImportedCount { get; init; }

    /// <summary>
    /// 跳過的數量（重複項）
    /// </summary>
    public int SkippedCount { get; init; }

    /// <summary>
    /// 匯入的關鍵字映射
    /// </summary>
    public List<KeywordMapping> ImportedMappings { get; init; } = [];

    /// <summary>
    /// 錯誤訊息列表
    /// </summary>
    public List<CsvError> Errors { get; init; } = [];

    /// <summary>
    /// 警告訊息列表
    /// </summary>
    public List<string> Warnings { get; init; } = [];
}
