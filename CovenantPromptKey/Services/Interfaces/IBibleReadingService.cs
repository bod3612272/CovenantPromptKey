using BibleData;

namespace CovenantPromptKey.Services.Interfaces;

/// <summary>
/// 聖經閱讀服務介面
/// 提供書卷、章節、經文的讀取功能
/// </summary>
public interface IBibleReadingService
{
    /// <summary>
    /// 取得所有書卷名稱列表
    /// </summary>
    /// <returns>66 卷書名稱列表</returns>
    List<string> GetAllBookNames();

    /// <summary>
    /// 取得指定書卷的章節數
    /// </summary>
    /// <param name="bookNumber">書卷編號 (1-66)</param>
    /// <returns>該書卷的章節數</returns>
    int GetChapterCount(int bookNumber);

    /// <summary>
    /// 取得指定章節的經文數
    /// </summary>
    /// <param name="bookNumber">書卷編號 (1-66)</param>
    /// <param name="chapterNumber">章節編號</param>
    /// <returns>該章節的經文數</returns>
    int GetVerseCount(int bookNumber, int chapterNumber);

    /// <summary>
    /// 取得指定章節的所有經文
    /// </summary>
    /// <param name="bookNumber">書卷編號 (1-66)</param>
    /// <param name="chapterNumber">章節編號</param>
    /// <returns>經文列表</returns>
    List<VerseWithLocation> GetChapterVerses(int bookNumber, int chapterNumber);

    /// <summary>
    /// 取得指定經文
    /// </summary>
    /// <param name="bookNumber">書卷編號 (1-66)</param>
    /// <param name="chapterNumber">章節編號</param>
    /// <param name="verseNumber">經文編號</param>
    /// <returns>經文內容，若不存在則返回 null</returns>
    VerseWithLocation? GetVerse(int bookNumber, int chapterNumber, int verseNumber);

    /// <summary>
    /// 取得書卷名稱
    /// </summary>
    /// <param name="bookNumber">書卷編號 (1-66)</param>
    /// <returns>書卷名稱</returns>
    string GetBookName(int bookNumber);

    /// <summary>
    /// 根據書卷名稱取得書卷編號
    /// </summary>
    /// <param name="bookName">書卷名稱</param>
    /// <returns>書卷編號 (1-66)，若找不到則返回 -1</returns>
    int GetBookNumber(string bookName);

    /// <summary>
    /// 驗證書卷編號是否有效
    /// </summary>
    /// <param name="bookNumber">書卷編號</param>
    /// <returns>是否有效</returns>
    bool IsValidBookNumber(int bookNumber);

    /// <summary>
    /// 驗證章節編號是否有效
    /// </summary>
    /// <param name="bookNumber">書卷編號</param>
    /// <param name="chapterNumber">章節編號</param>
    /// <returns>是否有效</returns>
    bool IsValidChapterNumber(int bookNumber, int chapterNumber);

    /// <summary>
    /// 取得上一章資訊
    /// </summary>
    /// <param name="bookNumber">當前書卷編號</param>
    /// <param name="chapterNumber">當前章節編號</param>
    /// <returns>(書卷編號, 章節編號) 元組，若無上一章則返回 null</returns>
    (int BookNumber, int ChapterNumber)? GetPreviousChapter(int bookNumber, int chapterNumber);

    /// <summary>
    /// 取得下一章資訊
    /// </summary>
    /// <param name="bookNumber">當前書卷編號</param>
    /// <param name="chapterNumber">當前章節編號</param>
    /// <returns>(書卷編號, 章節編號) 元組，若無下一章則返回 null</returns>
    (int BookNumber, int ChapterNumber)? GetNextChapter(int bookNumber, int chapterNumber);

    /// <summary>
    /// 取得章節標題 (e.g., "創世記 第1章")
    /// </summary>
    /// <param name="bookNumber">書卷編號</param>
    /// <param name="chapterNumber">章節編號</param>
    /// <returns>章節標題</returns>
    string GetChapterTitle(int bookNumber, int chapterNumber);

    /// <summary>
    /// 取得經文參照 (e.g., "創世記 1:1")
    /// </summary>
    /// <param name="bookNumber">書卷編號</param>
    /// <param name="chapterNumber">章節編號</param>
    /// <param name="verseNumber">經文編號</param>
    /// <returns>經文參照</returns>
    string GetVerseReference(int bookNumber, int chapterNumber, int verseNumber);
}
