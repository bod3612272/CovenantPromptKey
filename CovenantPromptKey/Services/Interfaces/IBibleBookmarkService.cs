using CovenantPromptKey.Models.Bible;

namespace CovenantPromptKey.Services.Interfaces;

/// <summary>
/// 聖經書籤服務介面
/// </summary>
public interface IBibleBookmarkService
{
    /// <summary>
    /// 載入所有書籤
    /// </summary>
    /// <returns>書籤列表（依時間降序）</returns>
    Task<List<BibleBookmark>> LoadBookmarksAsync();

    /// <summary>
    /// 新增書籤
    /// </summary>
    /// <param name="bookNumber">書卷編號</param>
    /// <param name="bookName">書卷名稱</param>
    /// <param name="chapterNumber">章節編號</param>
    /// <returns>非同步任務</returns>
    Task AddBookmarkAsync(int bookNumber, string bookName, int chapterNumber);

    /// <summary>
    /// 移除書籤
    /// </summary>
    /// <param name="bookmark">書籤物件</param>
    /// <returns>非同步任務</returns>
    Task RemoveBookmarkAsync(BibleBookmark bookmark);

    /// <summary>
    /// 清除所有書籤
    /// </summary>
    /// <returns>非同步任務</returns>
    Task ClearAllBookmarksAsync();

    /// <summary>
    /// 取得書籤數量
    /// </summary>
    /// <returns>書籤數量</returns>
    Task<int> GetBookmarkCountAsync();
}
