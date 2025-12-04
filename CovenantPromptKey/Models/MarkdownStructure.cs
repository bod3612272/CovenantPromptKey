namespace CovenantPromptKey.Models;

/// <summary>
/// Markdown 結構分析結果
/// </summary>
public class MarkdownStructure
{
    /// <summary>
    /// URL 保護區域列表
    /// </summary>
    public List<TextRange> ProtectedUrls { get; set; } = [];

    /// <summary>
    /// 程式碼區塊區域列表（需全詞匹配）
    /// </summary>
    public List<TextRange> CodeBlocks { get; set; } = [];

    /// <summary>
    /// 行內程式碼區域列表
    /// </summary>
    public List<TextRange> InlineCode { get; set; } = [];
}

/// <summary>
/// 文本範圍
/// </summary>
/// <param name="Start">起始位置（包含）</param>
/// <param name="End">結束位置（不包含）</param>
public record TextRange(int Start, int End);
