using CovenantPromptKey.Models.Bible;
using CovenantPromptKey.Services.Implementations;
using CovenantPromptKey.Services.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace CovenantPromptKey.NUnitTests.Services;

/// <summary>
/// BibleBookmarkService 單元測試
/// </summary>
[TestFixture]
public class BibleBookmarkServiceTests
{
    private ILocalStorageService _localStorageService = null!;
    private BibleBookmarkService _sut = null!;

    [SetUp]
    public void Setup()
    {
        _localStorageService = Substitute.For<ILocalStorageService>();
        _sut = new BibleBookmarkService(_localStorageService);
    }

    [Test]
    public async Task LoadBookmarksAsync_WhenNoStoredBookmarks_ShouldReturnEmptyList()
    {
        // Arrange
        _localStorageService.GetItemAsync<List<BibleBookmark>>("bible_bookmarks")
            .Returns((List<BibleBookmark>?)null);

        // Act
        var result = await _sut.LoadBookmarksAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task LoadBookmarksAsync_WhenStoredBookmarksExist_ShouldReturnOrderedByCreatedAtDescending()
    {
        // Arrange
        var storedBookmarks = new List<BibleBookmark>
        {
            new() { BookNumber = 1, BookName = "創世記", ChapterNumber = 1, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new() { BookNumber = 2, BookName = "出埃及記", ChapterNumber = 1, CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new() { BookNumber = 3, BookName = "利未記", ChapterNumber = 1, CreatedAt = DateTime.UtcNow }
        };
        _localStorageService.GetItemAsync<List<BibleBookmark>>("bible_bookmarks")
            .Returns(storedBookmarks);

        // Act
        var result = await _sut.LoadBookmarksAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].BookName.Should().Be("利未記");
        result[1].BookName.Should().Be("出埃及記");
        result[2].BookName.Should().Be("創世記");
    }

    [Test]
    public async Task AddBookmarkAsync_WhenNoExistingBookmarks_ShouldAddNewBookmark()
    {
        // Arrange
        _localStorageService.GetItemAsync<List<BibleBookmark>>("bible_bookmarks")
            .Returns((List<BibleBookmark>?)null);

        // Act
        await _sut.AddBookmarkAsync(1, "創世記", 1);

        // Assert
        await _localStorageService.Received(1)
            .SetItemAsync("bible_bookmarks", Arg.Is<List<BibleBookmark>>(list =>
                list.Count == 1 &&
                list[0].BookNumber == 1 &&
                list[0].BookName == "創世記" &&
                list[0].ChapterNumber == 1));
    }

    [Test]
    public async Task AddBookmarkAsync_WhenSameBookmarkExists_ShouldUpdateTimestampAndMoveToFront()
    {
        // Arrange
        var existingBookmarks = new List<BibleBookmark>
        {
            new() { BookNumber = 2, BookName = "出埃及記", ChapterNumber = 1, CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new() { BookNumber = 1, BookName = "創世記", ChapterNumber = 1, CreatedAt = DateTime.UtcNow.AddHours(-2) }
        };
        _localStorageService.GetItemAsync<List<BibleBookmark>>("bible_bookmarks")
            .Returns(existingBookmarks);

        // Act
        await _sut.AddBookmarkAsync(1, "創世記", 1);

        // Assert
        await _localStorageService.Received(1)
            .SetItemAsync("bible_bookmarks", Arg.Is<List<BibleBookmark>>(list =>
                list.Count == 2 &&
                list[0].BookNumber == 1 &&
                list[0].BookName == "創世記"));
    }

    [Test]
    public async Task AddBookmarkAsync_WhenMaxBookmarksExceeded_ShouldRemoveOldest()
    {
        // Arrange
        var existingBookmarks = Enumerable.Range(1, 10)
            .Select(i => new BibleBookmark
            {
                BookNumber = i,
                BookName = $"書卷{i}",
                ChapterNumber = 1,
                CreatedAt = DateTime.UtcNow.AddHours(-i)
            })
            .ToList();
        _localStorageService.GetItemAsync<List<BibleBookmark>>("bible_bookmarks")
            .Returns(existingBookmarks);

        // Act
        await _sut.AddBookmarkAsync(11, "新書卷", 1);

        // Assert
        await _localStorageService.Received(1)
            .SetItemAsync("bible_bookmarks", Arg.Is<List<BibleBookmark>>(list =>
                list.Count == 10 &&
                list[0].BookNumber == 11 &&
                list.All(b => b.BookNumber != 10))); // 最舊的（書卷10）應該被移除
    }

    [Test]
    public async Task RemoveBookmarkAsync_ShouldRemoveMatchingBookmark()
    {
        // Arrange
        var existingBookmarks = new List<BibleBookmark>
        {
            new() { BookNumber = 1, BookName = "創世記", ChapterNumber = 1 },
            new() { BookNumber = 2, BookName = "出埃及記", ChapterNumber = 1 }
        };
        _localStorageService.GetItemAsync<List<BibleBookmark>>("bible_bookmarks")
            .Returns(existingBookmarks);

        var bookmarkToRemove = new BibleBookmark { BookNumber = 1, BookName = "創世記", ChapterNumber = 1 };

        // Act
        await _sut.RemoveBookmarkAsync(bookmarkToRemove);

        // Assert
        await _localStorageService.Received(1)
            .SetItemAsync("bible_bookmarks", Arg.Is<List<BibleBookmark>>(list =>
                list.Count == 1 &&
                list[0].BookNumber == 2));
    }

    [Test]
    public async Task ClearAllBookmarksAsync_ShouldCallRemoveItem()
    {
        // Act
        await _sut.ClearAllBookmarksAsync();

        // Assert
        await _localStorageService.Received(1).RemoveItemAsync("bible_bookmarks");
    }

    [Test]
    public async Task GetBookmarkCountAsync_WhenNoBookmarks_ShouldReturnZero()
    {
        // Arrange
        _localStorageService.GetItemAsync<List<BibleBookmark>>("bible_bookmarks")
            .Returns((List<BibleBookmark>?)null);

        // Act
        var result = await _sut.GetBookmarkCountAsync();

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public async Task GetBookmarkCountAsync_WhenBookmarksExist_ShouldReturnCorrectCount()
    {
        // Arrange
        var existingBookmarks = new List<BibleBookmark>
        {
            new() { BookNumber = 1, BookName = "創世記", ChapterNumber = 1 },
            new() { BookNumber = 2, BookName = "出埃及記", ChapterNumber = 1 },
            new() { BookNumber = 3, BookName = "利未記", ChapterNumber = 1 }
        };
        _localStorageService.GetItemAsync<List<BibleBookmark>>("bible_bookmarks")
            .Returns(existingBookmarks);

        // Act
        var result = await _sut.GetBookmarkCountAsync();

        // Assert
        result.Should().Be(3);
    }
}
