namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經查詢頁面狀態
/// </summary>
public class BibleSearchPageState
{
    /// <summary>
    /// 當前搜尋關鍵字
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// 當前頁碼 (1-based)
    /// </summary>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// 每頁筆數
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 捲動位置 (px)
    /// </summary>
    public double ScrollPosition { get; set; } = 0;
}

/// <summary>
/// 聖經閱讀頁面狀態
/// </summary>
public class BibleReadPageState
{
    /// <summary>
    /// 當前書卷編號 (1-66)
    /// </summary>
    public int BookNumber { get; set; } = 1;

    /// <summary>
    /// 當前章節編號
    /// </summary>
    public int ChapterNumber { get; set; } = 1;

    /// <summary>
    /// 捲動位置 (px)
    /// </summary>
    public double ScrollPosition { get; set; } = 0;
}

/// <summary>
/// 聖經遊戲頁面狀態
/// </summary>
public class BibleGamePageState
{
    /// <summary>
    /// 是否有進行中的遊戲
    /// </summary>
    public bool IsGameInProgress { get; set; } = false;

    /// <summary>
    /// 當前題目編號 (1-10)
    /// </summary>
    public int CurrentQuestionNumber { get; set; } = 0;

    /// <summary>
    /// 當前分數
    /// </summary>
    public int CurrentScore { get; set; } = 0;
}
