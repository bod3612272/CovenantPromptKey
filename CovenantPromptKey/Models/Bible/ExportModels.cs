namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 經文導出選項
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// 導出風格
    /// </summary>
    public ExportStyle Style { get; set; } = ExportStyle.Style1;

    /// <summary>
    /// 是否包含書卷標題
    /// </summary>
    public bool IncludeBookTitle { get; set; } = true;

    /// <summary>
    /// 是否一本書一個檔案
    /// </summary>
    public bool OneFilePerBook { get; set; } = false;
}

/// <summary>
/// 經文導出範圍
/// </summary>
public class ExportRange
{
    /// <summary>
    /// 書卷編號
    /// </summary>
    public int BookNumber { get; set; }

    /// <summary>
    /// 起始章
    /// </summary>
    public int StartChapter { get; set; }

    /// <summary>
    /// 起始節
    /// </summary>
    public int StartVerse { get; set; }

    /// <summary>
    /// 結束章
    /// </summary>
    public int EndChapter { get; set; }

    /// <summary>
    /// 結束節
    /// </summary>
    public int EndVerse { get; set; }
}
