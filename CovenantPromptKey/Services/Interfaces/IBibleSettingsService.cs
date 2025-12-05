using CovenantPromptKey.Models.Bible;

namespace CovenantPromptKey.Services.Interfaces;

/// <summary>
/// 聖經設定服務介面
/// </summary>
public interface IBibleSettingsService
{
    /// <summary>
    /// 載入設定
    /// </summary>
    /// <returns>設定物件</returns>
    Task<BibleSettings> LoadSettingsAsync();

    /// <summary>
    /// 儲存設定
    /// </summary>
    /// <param name="settings">設定物件</param>
    /// <returns>非同步任務</returns>
    Task SaveSettingsAsync(BibleSettings settings);

    /// <summary>
    /// 重置為預設設定
    /// </summary>
    /// <returns>預設設定物件</returns>
    Task<BibleSettings> ResetToDefaultAsync();

    /// <summary>
    /// 取得預設設定
    /// </summary>
    /// <returns>預設設定物件</returns>
    BibleSettings GetDefaultSettings();
}
