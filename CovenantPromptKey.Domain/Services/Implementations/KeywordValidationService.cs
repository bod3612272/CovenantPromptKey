using System.Text.RegularExpressions;
using CovenantPromptKey.Constants;
using CovenantPromptKey.Models;
using CovenantPromptKey.Models.Results;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// 關鍵字驗證服務實作
/// 驗證關鍵字映射的有效性，包含保留字檢查、唯一性驗證等
/// </summary>
public partial class KeywordValidationService : IKeywordValidationService
{
    [GeneratedRegex(@"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")]
    private static partial Regex HexColorRegex();

    /// <inheritdoc />
    public ValidationResult<KeywordMapping> Validate(
        KeywordMapping mapping,
        IReadOnlyList<KeywordMapping> existingMappings)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate SensitiveKey
        if (string.IsNullOrWhiteSpace(mapping.SensitiveKey))
        {
            errors.Add("機敏詞不可為空白 (Sensitive key cannot be empty)");
        }
        else
        {
            if (mapping.SensitiveKey.Length > AppConstants.MaxKeywordLength)
            {
                errors.Add($"機敏詞長度超過 {AppConstants.MaxKeywordLength} 字元 (Sensitive key exceeds {AppConstants.MaxKeywordLength} characters)");
            }

            if (IsReservedKeyword(mapping.SensitiveKey))
            {
                errors.Add($"'{mapping.SensitiveKey}' 是受保護的程式語言關鍵字，無法作為替換目標 ('{mapping.SensitiveKey}' is a protected programming keyword)");
            }
            else if (IsSubstringOfReserved(mapping.SensitiveKey, out string matchedReserved))
            {
                warnings.Add($"警告：'{mapping.SensitiveKey}' 是程式語言關鍵字 '{matchedReserved}' 的一部分，可能導致非預期替換 (Warning: '{mapping.SensitiveKey}' is part of reserved keyword '{matchedReserved}')");
            }
        }

        // Validate SafeKey
        if (string.IsNullOrWhiteSpace(mapping.SafeKey))
        {
            errors.Add("替代詞不可為空白 (Safe key cannot be empty)");
        }
        else
        {
            if (mapping.SafeKey.Length > AppConstants.MaxKeywordLength)
            {
                errors.Add($"替代詞長度超過 {AppConstants.MaxKeywordLength} 字元 (Safe key exceeds {AppConstants.MaxKeywordLength} characters)");
            }

            if (!IsSafeKeyUnique(mapping.SafeKey, existingMappings, mapping.Id))
            {
                errors.Add($"替代詞 '{mapping.SafeKey}' 已被使用，請提供唯一的替代詞 (Safe key '{mapping.SafeKey}' is already in use)");
            }
        }

        // Validate HighlightColor
        if (!string.IsNullOrEmpty(mapping.HighlightColor) &&
            !HexColorRegex().IsMatch(mapping.HighlightColor))
        {
            errors.Add("顏色格式無效，請使用 HEX 格式 (如 #FF6B6B) (Invalid colour format, use HEX e.g. #FF6B6B)");
        }

        if (errors.Count > 0)
        {
            return new ValidationResult<KeywordMapping>
            {
                IsValid = false,
                Errors = errors,
                Warnings = warnings
            };
        }

        if (warnings.Count > 0)
        {
            return ValidationResult<KeywordMapping>.SuccessWithWarnings(mapping, [.. warnings]);
        }

        return ValidationResult<KeywordMapping>.Success(mapping);
    }

    /// <inheritdoc />
    public bool IsReservedKeyword(string keyword)
    {
        return ReservedKeywords.IsReserved(keyword);
    }

    /// <inheritdoc />
    public bool IsSubstringOfReserved(string keyword, out string matchedReserved)
    {
        return ReservedKeywords.IsSubstringOfReserved(keyword, out matchedReserved);
    }

    /// <inheritdoc />
    public bool IsSafeKeyUnique(
        string safeKey,
        IReadOnlyList<KeywordMapping> existingMappings,
        Guid? excludeId = null)
    {
        return !existingMappings.Any(m =>
            m.SafeKey.Equals(safeKey, StringComparison.OrdinalIgnoreCase) &&
            (excludeId == null || m.Id != excludeId));
    }
}
