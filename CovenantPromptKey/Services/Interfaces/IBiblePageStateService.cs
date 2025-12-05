using CovenantPromptKey.Models.Bible;

namespace CovenantPromptKey.Services.Interfaces;

/// <summary>
/// 聖經頁面狀態服務介面
/// </summary>
public interface IBiblePageStateService
{
    /// <summary>
    /// 載入查詢頁狀態
    /// </summary>
    /// <returns>查詢頁狀態</returns>
    Task<BibleSearchPageState> LoadSearchPageStateAsync();

    /// <summary>
    /// 儲存查詢頁狀態
    /// </summary>
    /// <param name="state">狀態物件</param>
    /// <returns>非同步任務</returns>
    Task SaveSearchPageStateAsync(BibleSearchPageState state);

    /// <summary>
    /// 載入閱讀頁狀態
    /// </summary>
    /// <returns>閱讀頁狀態</returns>
    Task<BibleReadPageState> LoadReadPageStateAsync();

    /// <summary>
    /// 儲存閱讀頁狀態
    /// </summary>
    /// <param name="state">狀態物件</param>
    /// <returns>非同步任務</returns>
    Task SaveReadPageStateAsync(BibleReadPageState state);

    /// <summary>
    /// 載入遊戲頁狀態
    /// </summary>
    /// <returns>遊戲頁狀態</returns>
    Task<BibleGamePageState> LoadGamePageStateAsync();

    /// <summary>
    /// 儲存遊戲頁狀態
    /// </summary>
    /// <param name="state">狀態物件</param>
    /// <returns>非同步任務</returns>
    Task SaveGamePageStateAsync(BibleGamePageState state);

    /// <summary>
    /// 清除所有頁面狀態
    /// </summary>
    /// <returns>非同步任務</returns>
    Task ClearAllStatesAsync();
}
