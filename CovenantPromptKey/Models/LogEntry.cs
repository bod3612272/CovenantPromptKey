namespace CovenantPromptKey.Models;

/// <summary>
/// 除錯日誌項目
/// </summary>
public record LogEntry
{
    /// <summary>
    /// 時間戳記
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.Now;

    /// <summary>
    /// 日誌等級
    /// </summary>
    public LogLevel Level { get; init; }

    /// <summary>
    /// 訊息內容
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// 來源元件/服務
    /// </summary>
    public string? Source { get; init; }
}

/// <summary>
/// 日誌等級
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}
