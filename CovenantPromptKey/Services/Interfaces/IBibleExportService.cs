using CovenantPromptKey.Models.Bible;

namespace CovenantPromptKey.Services.Interfaces;

/// <summary>
/// 經文導出服務介面
/// </summary>
public interface IBibleExportService
{
    /// <summary>
    /// 導出經文為 Markdown 格式
    /// </summary>
    /// <param name="range">導出範圍</param>
    /// <param name="options">導出選項</param>
    /// <returns>Markdown 格式字串</returns>
    string ExportToMarkdown(ExportRange range, ExportOptions options);

    /// <summary>
    /// 導出多個範圍的經文為 Markdown 格式
    /// </summary>
    /// <param name="ranges">導出範圍列表</param>
    /// <param name="options">導出選項</param>
    /// <returns>Markdown 格式字串</returns>
    string ExportToMarkdown(IEnumerable<ExportRange> ranges, ExportOptions options);

    /// <summary>
    /// 導出整本書卷為 Markdown 格式
    /// </summary>
    /// <param name="bookNumber">書卷編號</param>
    /// <param name="options">導出選項</param>
    /// <returns>Markdown 格式字串</returns>
    string ExportBookToMarkdown(int bookNumber, ExportOptions options);

    /// <summary>
    /// 導出多本書卷，每本一個檔案
    /// </summary>
    /// <param name="bookNumbers">書卷編號列表</param>
    /// <param name="options">導出選項</param>
    /// <returns>Dictionary，Key 為檔名，Value 為 Markdown 內容</returns>
    Dictionary<string, string> ExportBooksToFiles(IEnumerable<int> bookNumbers, ExportOptions options);

    /// <summary>
    /// 取得導出風格的預覽範例
    /// </summary>
    /// <param name="style">導出風格</param>
    /// <returns>預覽範例字串</returns>
    string GetStylePreview(ExportStyle style);

    /// <summary>
    /// 導出選定的經節為 Markdown 格式
    /// </summary>
    /// <param name="bookNumber">書卷編號</param>
    /// <param name="chapterNumber">章編號</param>
    /// <param name="verseNumbers">節編號列表</param>
    /// <param name="options">導出選項</param>
    /// <returns>Markdown 格式字串</returns>
    string ExportSelectedVerses(int bookNumber, int chapterNumber, IEnumerable<int> verseNumbers, ExportOptions options);
}
