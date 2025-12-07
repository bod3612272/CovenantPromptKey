using BibleData;
using CovenantPromptKey.Models;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// 聖經閱讀服務實作
/// </summary>
public class BibleReadingService : IBibleReadingService
{
    private readonly BibleIndex _bibleIndex;
    private readonly IDebugLogService _debugLogService;

    public BibleReadingService(BibleIndex bibleIndex, IDebugLogService debugLogService)
    {
        _bibleIndex = bibleIndex;
        _debugLogService = debugLogService;
    }

    /// <inheritdoc />
    public List<string> GetAllBookNames()
    {
        return _bibleIndex.BookNames.ToList();
    }

    /// <inheritdoc />
    public int GetChapterCount(int bookNumber)
    {
        if (!IsValidBookNumber(bookNumber))
        {
            _debugLogService.Log($"Invalid book number: {bookNumber}", Models.LogLevel.Warning);
            return 0;
        }

        // 使用書卷名稱查詢（避免舊約新約編號重複問題）
        // 注意：不能使用 _bibleIndex.GetChapterCount(bookName) 因為它內部可能也有問題
        // 正確做法：直接從 Book 物件取得章節數
        var bookName = GetBookName(bookNumber);
        var book = _bibleIndex.GetBook(bookName);
        return book?.Chapters.Count ?? 0;
    }

    /// <inheritdoc />
    public int GetVerseCount(int bookNumber, int chapterNumber)
    {
        if (!IsValidChapterNumber(bookNumber, chapterNumber))
        {
            return 0;
        }

        var verses = GetChapterVerses(bookNumber, chapterNumber);
        return verses.Count;
    }

    /// <inheritdoc />
    public List<VerseWithLocation> GetChapterVerses(int bookNumber, int chapterNumber)
    {
        if (!IsValidChapterNumber(bookNumber, chapterNumber))
        {
            _debugLogService.Log($"Invalid chapter: Book {bookNumber}, Chapter {chapterNumber}", Models.LogLevel.Warning);
            return new List<VerseWithLocation>();
        }

        try
        {
            // 使用書卷名稱查詢（避免舊約新約編號重複問題）
            var bookName = GetBookName(bookNumber);
            
            // 注意：不能使用 _bibleIndex.GetChapter(bookName, chapterNumber)
            // 因為該方法內部會使用 Book.Number 查詢 _chapterIndex，而舊約新約編號重複會導致錯誤
            // 正確做法：直接從 Book 物件的 Chapters 列表中取得
            var book = _bibleIndex.GetBook(bookName);
            if (book == null || chapterNumber < 1 || chapterNumber > book.Chapters.Count)
                return new List<VerseWithLocation>();

            var chapter = book.Chapters[chapterNumber - 1];

            return chapter.Verses.Select(v => new VerseWithLocation
            {
                BookNumber = bookNumber,
                BookName = bookName,
                ChapterNumber = chapterNumber,
                VerseNumber = v.Number,
                Content = v.Content
            }).ToList();
        }
        catch (Exception ex)
        {
            _debugLogService.Log($"Error getting chapter verses: {ex.Message}", Models.LogLevel.Error);
            return new List<VerseWithLocation>();
        }
    }

    /// <inheritdoc />
    public VerseWithLocation? GetVerse(int bookNumber, int chapterNumber, int verseNumber)
    {
        if (!IsValidChapterNumber(bookNumber, chapterNumber))
        {
            return null;
        }

        try
        {
            // 使用書卷名稱查詢（避免舊約新約編號重複問題）
            var bookName = GetBookName(bookNumber);
            var verse = _bibleIndex.GetVerse(bookName, chapterNumber, verseNumber);
            if (verse == null)
                return null;

            return new VerseWithLocation
            {
                BookNumber = bookNumber,
                BookName = bookName,
                ChapterNumber = chapterNumber,
                VerseNumber = verseNumber,
                Content = verse.Content
            };
        }
        catch (Exception ex)
        {
            _debugLogService.Log($"Error getting verse: {ex.Message}", Models.LogLevel.Error);
            return null;
        }
    }

    /// <inheritdoc />
    public string GetBookName(int bookNumber)
    {
        if (!IsValidBookNumber(bookNumber))
        {
            return string.Empty;
        }

        return _bibleIndex.BookNames[bookNumber - 1];
    }

    /// <inheritdoc />
    public int GetBookNumber(string bookName)
    {
        if (string.IsNullOrWhiteSpace(bookName))
        {
            return -1;
        }

        var index = _bibleIndex.BookNames.ToList().IndexOf(bookName);
        return index >= 0 ? index + 1 : -1;
    }

    /// <inheritdoc />
    public bool IsValidBookNumber(int bookNumber)
    {
        return bookNumber >= 1 && bookNumber <= _bibleIndex.BookNames.Count;
    }

    /// <inheritdoc />
    public bool IsValidChapterNumber(int bookNumber, int chapterNumber)
    {
        if (!IsValidBookNumber(bookNumber))
        {
            return false;
        }

        var maxChapter = GetChapterCount(bookNumber);
        return chapterNumber >= 1 && chapterNumber <= maxChapter;
    }

    /// <inheritdoc />
    public (int BookNumber, int ChapterNumber)? GetPreviousChapter(int bookNumber, int chapterNumber)
    {
        if (!IsValidChapterNumber(bookNumber, chapterNumber))
        {
            return null;
        }

        // 如果不是第一章，回傳上一章
        if (chapterNumber > 1)
        {
            return (bookNumber, chapterNumber - 1);
        }

        // 如果是第一章但不是創世記，回傳前一卷書的最後一章
        if (bookNumber > 1)
        {
            var prevBook = bookNumber - 1;
            var lastChapter = GetChapterCount(prevBook);
            return (prevBook, lastChapter);
        }

        // 已經是創世記第一章，沒有上一章
        return null;
    }

    /// <inheritdoc />
    public (int BookNumber, int ChapterNumber)? GetNextChapter(int bookNumber, int chapterNumber)
    {
        if (!IsValidChapterNumber(bookNumber, chapterNumber))
        {
            return null;
        }

        var maxChapter = GetChapterCount(bookNumber);

        // 如果不是最後一章，回傳下一章
        if (chapterNumber < maxChapter)
        {
            return (bookNumber, chapterNumber + 1);
        }

        // 如果是最後一章但不是啟示錄，回傳下一卷書的第一章
        if (bookNumber < 66)
        {
            return (bookNumber + 1, 1);
        }

        // 已經是啟示錄最後一章，沒有下一章
        return null;
    }

    /// <inheritdoc />
    public string GetChapterTitle(int bookNumber, int chapterNumber)
    {
        var bookName = GetBookName(bookNumber);
        if (string.IsNullOrEmpty(bookName))
        {
            return string.Empty;
        }

        return $"{bookName} 第{chapterNumber}章";
    }

    /// <inheritdoc />
    public string GetVerseReference(int bookNumber, int chapterNumber, int verseNumber)
    {
        var bookName = GetBookName(bookNumber);
        if (string.IsNullOrEmpty(bookName))
        {
            return string.Empty;
        }

        return $"{bookName} {chapterNumber}:{verseNumber}";
    }
}
