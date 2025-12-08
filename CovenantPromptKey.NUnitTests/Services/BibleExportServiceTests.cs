using BibleData;
using BibleData.Models;
using CovenantPromptKey.Models.Bible;
using CovenantPromptKey.Services.Implementations;
using CovenantPromptKey.Services.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace CovenantPromptKey.NUnitTests.Services;

/// <summary>
/// BibleExportService 單元測試
/// </summary>
[TestFixture]
public class BibleExportServiceTests
{
    private BibleIndex _bibleIndex = null!;
    private IBibleReadingService _readingService = null!;
    private BibleExportService _sut = null!;

    [SetUp]
    public void Setup()
    {
        _bibleIndex = Substitute.For<BibleIndex>();
        _readingService = Substitute.For<IBibleReadingService>();
        _sut = new BibleExportService(_bibleIndex, _readingService);
    }

    [Test]
    public void ExportToMarkdown_Style1_ShouldFormatCorrectly()
    {
        // Arrange
        var verses = new List<VerseWithLocation>
        {
            new() { BookName = "創世記", ChapterNumber = 1, VerseNumber = 1, Content = "起初，神創造天地。" },
            new() { BookName = "創世記", ChapterNumber = 1, VerseNumber = 2, Content = "地是空虛混沌。" }
        };

        _readingService.GetBookName(1).Returns("創世記");
        _readingService.GetChapterVerses(1, 1).Returns(verses);

        var range = new ExportRange
        {
            BookNumber = 1,
            StartChapter = 1,
            StartVerse = 1,
            EndChapter = 1,
            EndVerse = 2
        };

        var options = new ExportOptions { Style = ExportStyle.Style1 };

        // Act
        var result = _sut.ExportToMarkdown(range, options);

        // Assert
        result.Should().Contain("(創世記 1:1) 起初，神創造天地。");
        result.Should().Contain("(創世記 1:2) 地是空虛混沌。");
    }

    [Test]
    public void ExportToMarkdown_Style2_ShouldFormatWithHeaderAndLineNumbers()
    {
        // Arrange
        var verses = new List<VerseWithLocation>
        {
            new() { BookName = "創世記", ChapterNumber = 1, VerseNumber = 1, Content = "起初，神創造天地。" },
            new() { BookName = "創世記", ChapterNumber = 1, VerseNumber = 2, Content = "地是空虛混沌。" }
        };

        _readingService.GetBookName(1).Returns("創世記");
        _readingService.GetChapterVerses(1, 1).Returns(verses);

        var range = new ExportRange
        {
            BookNumber = 1,
            StartChapter = 1,
            StartVerse = 1,
            EndChapter = 1,
            EndVerse = 2
        };

        var options = new ExportOptions { Style = ExportStyle.Style2 };

        // Act
        var result = _sut.ExportToMarkdown(range, options);

        // Assert
        result.Should().Contain("創世記 ( 第 1 章 1 ~ 2 節 )");
        result.Should().Contain("1 起初，神創造天地。");
        result.Should().Contain("2 地是空虛混沌。");
    }

    [Test]
    public void ExportToMarkdown_Style3_ShouldFormatWithGuillemetsAndCombinedText()
    {
        // Arrange
        var verses = new List<VerseWithLocation>
        {
            new() { BookName = "創世記", ChapterNumber = 1, VerseNumber = 1, Content = "起初，神創造天地。" },
            new() { BookName = "創世記", ChapterNumber = 1, VerseNumber = 2, Content = "地是空虛混沌。" }
        };

        _readingService.GetBookName(1).Returns("創世記");
        _readingService.GetChapterVerses(1, 1).Returns(verses);

        var range = new ExportRange
        {
            BookNumber = 1,
            StartChapter = 1,
            StartVerse = 1,
            EndChapter = 1,
            EndVerse = 2
        };

        var options = new ExportOptions { Style = ExportStyle.Style3 };

        // Act
        var result = _sut.ExportToMarkdown(range, options);

        // Assert
        result.Should().Contain("《創世記》 ( 第 1 章 1 ~ 2 節 )");
        result.Should().Contain("起初，神創造天地。地是空虛混沌。");
    }

    [Test]
    public void ExportSelectedVerses_ShouldExportOnlySelectedVerses()
    {
        // Arrange
        var verses = new List<VerseWithLocation>
        {
            new() { BookName = "創世記", ChapterNumber = 1, VerseNumber = 1, Content = "起初，神創造天地。" },
            new() { BookName = "創世記", ChapterNumber = 1, VerseNumber = 2, Content = "地是空虛混沌。" },
            new() { BookName = "創世記", ChapterNumber = 1, VerseNumber = 3, Content = "神說：要有光。" }
        };

        _readingService.GetBookName(1).Returns("創世記");
        _readingService.GetChapterVerses(1, 1).Returns(verses);

        var options = new ExportOptions { Style = ExportStyle.Style1 };

        // Act
        var result = _sut.ExportSelectedVerses(1, 1, new[] { 1, 3 }, options);

        // Assert
        result.Should().Contain("(創世記 1:1) 起初，神創造天地。");
        result.Should().Contain("(創世記 1:3) 神說：要有光。");
        result.Should().NotContain("地是空虛混沌");
    }

    [Test]
    public void GetStylePreview_Style1_ShouldReturnCorrectPreview()
    {
        // Act
        var result = _sut.GetStylePreview(ExportStyle.Style1);

        // Assert
        result.Should().Contain("(創世記 1:1)");
    }

    [Test]
    public void GetStylePreview_Style2_ShouldReturnCorrectPreview()
    {
        // Act
        var result = _sut.GetStylePreview(ExportStyle.Style2);

        // Assert
        result.Should().Contain("創世記 ( 第 1 章");
        result.Should().Contain("1 起初");
    }

    [Test]
    public void GetStylePreview_Style3_ShouldReturnCorrectPreview()
    {
        // Act
        var result = _sut.GetStylePreview(ExportStyle.Style3);

        // Assert
        result.Should().Contain("《創世記》");
    }

    [Test]
    public void ExportBooksToFiles_ShouldReturnDictionaryWithFileNames()
    {
        // Arrange
        _readingService.GetBookName(1).Returns("創世記");
        _readingService.GetBookName(2).Returns("出埃及記");
        _readingService.GetChapterCount(1).Returns(1);
        _readingService.GetChapterCount(2).Returns(1);
        _readingService.GetChapterVerses(1, 1).Returns(new List<VerseWithLocation>
        {
            new() { BookName = "創世記", ChapterNumber = 1, VerseNumber = 1, Content = "起初。" }
        });
        _readingService.GetChapterVerses(2, 1).Returns(new List<VerseWithLocation>
        {
            new() { BookName = "出埃及記", ChapterNumber = 1, VerseNumber = 1, Content = "以色列。" }
        });

        var options = new ExportOptions { Style = ExportStyle.Style1 };

        // Act
        var result = _sut.ExportBooksToFiles(new[] { 1, 2 }, options);

        // Assert
        result.Should().ContainKey("01_創世記.md");
        result.Should().ContainKey("02_出埃及記.md");
    }

    [Test]
    public void ExportToMarkdown_WithInvalidBookNumber_ShouldReturnEmpty()
    {
        // Arrange
        _readingService.GetBookName(999).Returns((string?)null);

        var range = new ExportRange
        {
            BookNumber = 999,
            StartChapter = 1,
            StartVerse = 1,
            EndChapter = 1,
            EndVerse = 1
        };

        var options = new ExportOptions { Style = ExportStyle.Style1 };

        // Act
        var result = _sut.ExportToMarkdown(range, options);

        // Assert
        result.Should().BeEmpty();
    }
}
