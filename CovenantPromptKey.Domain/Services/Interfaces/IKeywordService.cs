using CovenantPromptKey.Models;
using CovenantPromptKey.Models.Results;

namespace CovenantPromptKey.Services.Interfaces;

/// <summary>
/// 關鍵字偵測與替換服務介面
/// </summary>
public interface IKeywordService
{
    /// <summary>
    /// 偵測文本中的所有關鍵字
    /// </summary>
    /// <param name="text">待偵測的原始文本</param>
    /// <param name="dictionary">關鍵字字典</param>
    /// <returns>偵測結果列表</returns>
    /// <remarks>
    /// - 採用 Aho-Corasick 演算法進行多模式匹配
    /// - 最長匹配優先
    /// - 自動標記語境警示（前後緊鄰中文字元）
    /// - 識別 Markdown 程式碼區塊並啟用全詞匹配
    /// - 保護 Markdown URL 區域
    /// </remarks>
    Task<List<DetectedKeyword>> DetectKeywordsAsync(
        string text,
        IReadOnlyList<KeywordMapping> dictionary);

    /// <summary>
    /// 執行遮罩替換
    /// </summary>
    /// <param name="text">原始文本</param>
    /// <param name="detectedKeywords">偵測結果（包含勾選狀態）</param>
    /// <returns>遮罩結果</returns>
    Task<MaskResult> ApplyMaskAsync(
        string text,
        List<DetectedKeyword> detectedKeywords);

    /// <summary>
    /// 執行還原操作
    /// </summary>
    /// <param name="maskedText">已遮罩的文本（如 AI 回應）</param>
    /// <param name="dictionary">關鍵字字典</param>
    /// <returns>還原結果</returns>
    Task<RestoreResult> RestoreTextAsync(
        string maskedText,
        IReadOnlyList<KeywordMapping> dictionary);

    /// <summary>
    /// 驗證文本長度
    /// </summary>
    /// <param name="text">待驗證文本</param>
    /// <returns>是否在允許範圍內（≤100,000 字元）</returns>
    bool ValidateTextLength(string text);
}
