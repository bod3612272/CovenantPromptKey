namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經書籤模型
/// </summary>
public class BibleBookmark
{
    /// <summary>
    /// 書卷編號 (1-66)
    /// </summary>
    public int BookNumber { get; set; }

    /// <summary>
    /// 書卷名稱
    /// </summary>
    public string BookName { get; set; } = string.Empty;

    /// <summary>
    /// 章節編號
    /// </summary>
    public int ChapterNumber { get; set; }

    /// <summary>
    /// 建立時間 (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 顯示用參考字串 (e.g., "創世記 第 1 章")
    /// </summary>
    public string DisplayReference => $"{BookName} 第 {ChapterNumber} 章";
}
