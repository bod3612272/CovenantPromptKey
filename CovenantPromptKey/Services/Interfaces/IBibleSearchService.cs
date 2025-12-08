using CovenantPromptKey.Models.Bible;

namespace CovenantPromptKey.Services.Interfaces;

/// <summary>
/// 聖經搜尋服務介面
/// </summary>
public interface IBibleSearchService
{
    /// <summary>
    /// 使用單一關鍵字搜尋經文
    /// </summary>
    /// <param name="keyword">搜尋關鍵字</param>
    /// <param name="top">回傳筆數上限</param>
    /// <returns>搜尋結果列表</returns>
    List<SearchResultItem> Search(string keyword, int top = 50);

    /// <summary>
    /// 使用單一關鍵字搜尋經文（帶排名分數）
    /// </summary>
    /// <param name="keyword">搜尋關鍵字</param>
    /// <param name="top">回傳筆數上限</param>
    /// <returns>排名搜尋結果列表</returns>
    List<SearchResultItem> SearchRanked(string keyword, int top = 50);

    /// <summary>
    /// 即時搜尋（支援取消）
    /// </summary>
    /// <param name="keyword">搜尋關鍵字</param>
    /// <param name="top">回傳筆數上限</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>搜尋結果列表</returns>
    List<SearchResultItem> SearchWithCancellation(string keyword, int top, CancellationToken cancellationToken);

    /// <summary>
    /// 多關鍵字搜尋（AND 邏輯）
    /// </summary>
    /// <param name="keywords">關鍵字陣列</param>
    /// <param name="top">回傳筆數上限</param>
    /// <returns>搜尋結果列表</returns>
    List<SearchResultItem> SearchMultiple(string[] keywords, int top = 50);

    /// <summary>
    /// 解析輸入字串並執行搜尋（空格分隔多關鍵字）
    /// </summary>
    /// <param name="input">使用者輸入</param>
    /// <param name="top">回傳筆數上限</param>
    /// <returns>搜尋結果列表</returns>
    List<SearchResultItem> SearchFromInput(string input, int top = 50);

    /// <summary>
    /// 在搜尋結果中高亮關鍵字
    /// </summary>
    /// <param name="content">原始經文內容</param>
    /// <param name="keywords">要高亮的關鍵字</param>
    /// <returns>高亮後的 HTML 字串</returns>
    string HighlightKeywords(string content, string[] keywords);
}
