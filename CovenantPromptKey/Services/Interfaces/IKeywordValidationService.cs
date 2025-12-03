using CovenantPromptKey.Models;
using CovenantPromptKey.Models.Results;

namespace CovenantPromptKey.Services.Interfaces;

/// <summary>
/// 關鍵字驗證服務介面
/// </summary>
public interface IKeywordValidationService
{
    /// <summary>
    /// 驗證新關鍵字映射
    /// </summary>
    /// <param name="mapping">待驗證的映射</param>
    /// <param name="existingMappings">現有映射列表</param>
    /// <returns>驗證結果</returns>
    ValidationResult<KeywordMapping> Validate(
        KeywordMapping mapping,
        IReadOnlyList<KeywordMapping> existingMappings);

    /// <summary>
    /// 檢查是否為保留字
    /// </summary>
    bool IsReservedKeyword(string keyword);

    /// <summary>
    /// 檢查是否為保留字子字串
    /// </summary>
    /// <param name="keyword">關鍵字</param>
    /// <param name="matchedReserved">匹配到的保留字</param>
    bool IsSubstringOfReserved(string keyword, out string matchedReserved);

    /// <summary>
    /// 檢查 SafeKey 唯一性
    /// </summary>
    bool IsSafeKeyUnique(string safeKey, IReadOnlyList<KeywordMapping> existingMappings, Guid? excludeId = null);
}
