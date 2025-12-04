namespace CovenantPromptKey.Models;

/// <summary>
/// 工作階段狀態，支援頁面刷新後還原
/// </summary>
public class WorkSession
{
    /// <summary>
    /// 工作階段識別碼
    /// </summary>
    public Guid SessionId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// 當前模式
    /// </summary>
    public WorkMode Mode { get; set; } = WorkMode.Mask;

    /// <summary>
    /// 原始輸入文本
    /// </summary>
    public string SourceText { get; set; } = string.Empty;

    /// <summary>
    /// 偵測結果
    /// </summary>
    public List<DetectedKeyword> DetectedKeywords { get; set; } = [];

    /// <summary>
    /// 處理結果文本
    /// </summary>
    public string ResultText { get; set; } = string.Empty;

    /// <summary>
    /// 最後更新時間
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 工作模式
/// </summary>
public enum WorkMode
{
    /// <summary>遮罩模式</summary>
    Mask,
    /// <summary>還原模式</summary>
    Restore
}
