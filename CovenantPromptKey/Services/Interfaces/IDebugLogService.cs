using CovenantPromptKey.Models;

namespace CovenantPromptKey.Services.Interfaces;

/// <summary>
/// 除錯日誌服務介面
/// </summary>
public interface IDebugLogService
{
    /// <summary>
    /// 當新日誌項目新增時觸發
    /// </summary>
    event Action? OnLogAdded;

    /// <summary>
    /// 記錄訊息
    /// </summary>
    /// <param name="message">訊息內容</param>
    /// <param name="level">日誌等級</param>
    /// <param name="source">來源元件/服務</param>
    void Log(string message, Models.LogLevel level = Models.LogLevel.Info, string? source = null);

    /// <summary>
    /// 取得所有日誌（最新在前）
    /// </summary>
    IReadOnlyList<LogEntry> GetLogs();

    /// <summary>
    /// 取得格式化的日誌文字（用於複製）
    /// </summary>
    string GetFormattedLogs();

    /// <summary>
    /// 清空所有日誌
    /// </summary>
    void Clear();

    /// <summary>
    /// 取得當前日誌數量
    /// </summary>
    int Count { get; }
}
