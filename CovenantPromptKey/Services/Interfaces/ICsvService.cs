using CovenantPromptKey.Models;
using CovenantPromptKey.Models.Results;

namespace CovenantPromptKey.Services.Interfaces;

/// <summary>
/// CSV 匯入/匯出服務介面
/// </summary>
public interface ICsvService
{
    /// <summary>
    /// 匯出關鍵字字典為 CSV
    /// </summary>
    /// <param name="mappings">關鍵字映射列表</param>
    /// <returns>CSV 內容字串</returns>
    string ExportToCsv(IReadOnlyList<KeywordMapping> mappings);

    /// <summary>
    /// 從 CSV 匯入關鍵字
    /// </summary>
    /// <param name="csvContent">CSV 內容</param>
    /// <returns>匯入結果</returns>
    /// <remarks>
    /// 驗證規則：
    /// - 欄位數量必須正確
    /// - 不可包含未封閉的引號
    /// - SafeKey 不可重複
    /// - 保留字警告
    /// </remarks>
    CsvImportResult ImportFromCsv(string csvContent);

    /// <summary>
    /// 驗證 CSV 格式
    /// </summary>
    /// <param name="csvContent">CSV 內容</param>
    /// <returns>驗證結果，包含錯誤行號與訊息</returns>
    CsvValidationResult ValidateCsv(string csvContent);
}
