namespace CovenantPromptKey.Models;

/// <summary>
/// 偵測到的關鍵字實例
/// </summary>
public class DetectedKeyword
{
    /// <summary>
    /// 對應的關鍵字映射
    /// </summary>
    public required KeywordMapping Mapping { get; init; }

    /// <summary>
    /// 所有出現位置
    /// </summary>
    public List<KeywordOccurrence> Occurrences { get; init; } = [];

    /// <summary>
    /// 是否選擇替換
    /// </summary>
    public bool IsSelected { get; set; } = true;

    /// <summary>
    /// 出現次數
    /// </summary>
    public int Count => Occurrences.Count;

    /// <summary>
    /// 是否有任何出現帶有語境警示
    /// </summary>
    public bool HasWarning => Occurrences.Any(o => o.HasContextWarning);
}

/// <summary>
/// 關鍵字單次出現的位置資訊
/// </summary>
public class KeywordOccurrence
{
    /// <summary>
    /// 在原文中的起始索引
    /// </summary>
    public int StartIndex { get; init; }

    /// <summary>
    /// 在原文中的結束索引
    /// </summary>
    public int EndIndex { get; init; }

    /// <summary>
    /// 所在行號 (1-based)
    /// </summary>
    public int LineNumber { get; init; }

    /// <summary>
    /// 原文中的實際文字（保留大小寫）
    /// </summary>
    public required string OriginalText { get; init; }

    /// <summary>
    /// 是否有語境警示（前後緊鄰中文字元）
    /// </summary>
    public bool HasContextWarning { get; init; }

    /// <summary>
    /// 是否位於程式碼區塊內
    /// </summary>
    public bool IsInCodeBlock { get; init; }

    /// <summary>
    /// 是否選擇替換此出現（個別控制）
    /// </summary>
    public bool IsSelected { get; set; } = true;
}
