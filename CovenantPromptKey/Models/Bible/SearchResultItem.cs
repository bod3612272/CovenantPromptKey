namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經搜尋結果項目（UI 顯示用）
/// </summary>
public class SearchResultItem
{
    /// <summary>
    /// 書卷名稱
    /// </summary>
    public string BookName { get; set; } = string.Empty;

    /// <summary>
    /// 章節編號
    /// </summary>
    public int ChapterNumber { get; set; }

    /// <summary>
    /// 節數編號
    /// </summary>
    public int VerseNumber { get; set; }

    /// <summary>
    /// 經文內容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 參考字串 (e.g., "約翰福音 3:16")
    /// </summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// 相關性分數
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// 高亮顯示的經文內容 (關鍵字標記)
    /// </summary>
    public string HighlightedContent { get; set; } = string.Empty;
}
