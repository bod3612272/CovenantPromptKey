namespace CovenantPromptKey.Models;

/// <summary>
/// 關鍵字映射實體，定義機敏詞與安全替代詞之間的對應關係
/// </summary>
public class KeywordMapping
{
    /// <summary>
    /// 唯一識別碼
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// 機敏關鍵字（原始詞彙）
    /// </summary>
    /// <remarks>
    /// - 不可為空或僅含空白
    /// - 不可為程式語言保留字
    /// - 最大長度 200 字元
    /// </remarks>
    public required string SensitiveKey { get; set; }

    /// <summary>
    /// 安全替代詞（遮罩後顯示）
    /// </summary>
    /// <remarks>
    /// - 不可為空或僅含空白
    /// - 必須在字典中保持唯一性
    /// - 最大長度 200 字元
    /// </remarks>
    public required string SafeKey { get; set; }

    /// <summary>
    /// 高亮顯示顏色 (HEX 格式)
    /// </summary>
    /// <example>#FF6B6B</example>
    public string HighlightColor { get; set; } = "#4ECDC4";

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 最後修改時間
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
