namespace CovenantPromptKey.Services.Interfaces;

/// <summary>
/// 瀏覽器 SessionStorage 服務介面
/// </summary>
public interface ISessionStorageService
{
    /// <summary>
    /// 取得指定鍵的值
    /// </summary>
    Task<T?> GetItemAsync<T>(string key);

    /// <summary>
    /// 設定指定鍵的值
    /// </summary>
    Task SetItemAsync<T>(string key, T value);

    /// <summary>
    /// 移除指定鍵
    /// </summary>
    Task RemoveItemAsync(string key);

    /// <summary>
    /// 清空所有資料
    /// </summary>
    Task ClearAsync();
}
