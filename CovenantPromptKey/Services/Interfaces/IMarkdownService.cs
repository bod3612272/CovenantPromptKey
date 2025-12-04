using CovenantPromptKey.Models;

namespace CovenantPromptKey.Services.Interfaces;

/// <summary>
/// Markdown 解析服務介面
/// </summary>
public interface IMarkdownService
{
    /// <summary>
    /// 分析 Markdown 結構
    /// </summary>
    /// <param name="text">Markdown 文本</param>
    /// <returns>結構分析結果</returns>
    MarkdownStructure Analyze(string text);
}
