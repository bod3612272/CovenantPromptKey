using CovenantPromptKey.Models;
using CovenantPromptKey.Models.Results;

namespace CovenantPromptKey.Services.Interfaces;

/// <summary>
/// 關鍵字字典管理服務介面
/// </summary>
public interface IDictionaryService
{
    /// <summary>
    /// 當字典變更時觸發
    /// </summary>
    event Action? OnDictionaryChanged;

    /// <summary>
    /// 取得所有關鍵字映射
    /// </summary>
    Task<IReadOnlyList<KeywordMapping>> GetAllAsync();

    /// <summary>
    /// 取得單一關鍵字映射
    /// </summary>
    Task<KeywordMapping?> GetByIdAsync(Guid id);

    /// <summary>
    /// 新增關鍵字映射
    /// </summary>
    /// <returns>驗證結果，成功時包含新建的映射</returns>
    /// <remarks>
    /// 驗證規則：
    /// - SensitiveKey 不可為空白
    /// - SafeKey 必須唯一
    /// - 不可為程式語言保留字
    /// - 字典總數不超過 500
    /// </remarks>
    Task<ValidationResult<KeywordMapping>> AddAsync(KeywordMapping mapping);

    /// <summary>
    /// 更新關鍵字映射
    /// </summary>
    Task<ValidationResult<KeywordMapping>> UpdateAsync(KeywordMapping mapping);

    /// <summary>
    /// 刪除關鍵字映射
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// 清空所有關鍵字
    /// </summary>
    Task ClearAllAsync();

    /// <summary>
    /// 取得下一個可用的預設顏色
    /// </summary>
    string GetNextDefaultColor();

    /// <summary>
    /// 取得當前關鍵字數量
    /// </summary>
    Task<int> GetCountAsync();
}
