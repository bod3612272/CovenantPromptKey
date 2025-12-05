using CovenantPromptKey.Models.Bible;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// 聖經書籤服務實作
/// </summary>
public class BibleBookmarkService : IBibleBookmarkService
{
    private const string StorageKey = "bible_bookmarks";
    private const int MaxBookmarks = 10;

    private readonly ILocalStorageService _localStorageService;

    public BibleBookmarkService(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    /// <inheritdoc />
    public async Task<List<BibleBookmark>> LoadBookmarksAsync()
    {
        var bookmarks = await _localStorageService.GetItemAsync<List<BibleBookmark>>(StorageKey);
        return bookmarks?.OrderByDescending(b => b.CreatedAt).ToList() ?? new List<BibleBookmark>();
    }

    /// <inheritdoc />
    public async Task AddBookmarkAsync(int bookNumber, string bookName, int chapterNumber)
    {
        var bookmarks = await LoadBookmarksAsync();

        // 檢查是否已存在相同的書籤（相同書卷和章節）
        var existing = bookmarks.FirstOrDefault(b => 
            b.BookNumber == bookNumber && b.ChapterNumber == chapterNumber);

        if (existing != null)
        {
            // 更新時間並移至最前
            bookmarks.Remove(existing);
        }

        // 新增書籤
        var newBookmark = new BibleBookmark
        {
            BookNumber = bookNumber,
            BookName = bookName,
            ChapterNumber = chapterNumber,
            CreatedAt = DateTime.UtcNow
        };

        bookmarks.Insert(0, newBookmark);

        // 保留最多 MaxBookmarks 筆
        if (bookmarks.Count > MaxBookmarks)
        {
            bookmarks = bookmarks.Take(MaxBookmarks).ToList();
        }

        await _localStorageService.SetItemAsync(StorageKey, bookmarks);
    }

    /// <inheritdoc />
    public async Task RemoveBookmarkAsync(BibleBookmark bookmark)
    {
        var bookmarks = await LoadBookmarksAsync();
        bookmarks.RemoveAll(b => 
            b.BookNumber == bookmark.BookNumber && 
            b.ChapterNumber == bookmark.ChapterNumber);
        await _localStorageService.SetItemAsync(StorageKey, bookmarks);
    }

    /// <inheritdoc />
    public async Task ClearAllBookmarksAsync()
    {
        await _localStorageService.RemoveItemAsync(StorageKey);
    }

    /// <inheritdoc />
    public async Task<int> GetBookmarkCountAsync()
    {
        var bookmarks = await LoadBookmarksAsync();
        return bookmarks.Count;
    }
}
