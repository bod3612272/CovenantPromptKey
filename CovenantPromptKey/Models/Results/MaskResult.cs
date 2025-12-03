namespace CovenantPromptKey.Models.Results;

/// <summary>
/// 遮罩操作結果
/// </summary>
public class MaskResult
{
    /// <summary>
    /// 遮罩後的文本
    /// </summary>
    public required string MaskedText { get; init; }

    /// <summary>
    /// 已替換的關鍵字數量
    /// </summary>
    public int ReplacedCount { get; init; }

    /// <summary>
    /// 跳過的關鍵字數量
    /// </summary>
    public int SkippedCount { get; init; }

    /// <summary>
    /// 替換詳情
    /// </summary>
    public List<ReplacementDetail> Details { get; init; } = [];
}

/// <summary>
/// 還原操作結果
/// </summary>
public class RestoreResult
{
    /// <summary>
    /// 還原後的文本
    /// </summary>
    public required string RestoredText { get; init; }
    
    /// <summary>
    /// Alias for RestoredText for API consistency
    /// </summary>
    public string ResultText => RestoredText;

    /// <summary>
    /// 已還原的關鍵字數量
    /// </summary>
    public int RestoredCount { get; init; }

    /// <summary>
    /// 未匹配（無法還原）的安全詞數量
    /// </summary>
    public int UnmatchedCount { get; init; }
    
    /// <summary>
    /// 還原統計資訊
    /// </summary>
    public RestoreStatistics Statistics => new()
    {
        ReplacementCount = RestoredCount,
        UnmatchedCount = UnmatchedCount
    };
    
    /// <summary>
    /// 還原詳情列表
    /// </summary>
    public List<ReplacementDetail> Details { get; init; } = [];
}

/// <summary>
/// 還原統計資訊
/// </summary>
public class RestoreStatistics
{
    public int ReplacementCount { get; init; }
    public int UnmatchedCount { get; init; }
}

/// <summary>
/// 單次替換詳情
/// </summary>
public class ReplacementDetail
{
    public required string Original { get; init; }
    public required string Replacement { get; init; }
    public int OccurrenceCount { get; init; }
}
