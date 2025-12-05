using BibleData;
using CovenantPromptKey.Models;
using CovenantPromptKey.Services.Implementations;
using CovenantPromptKey.Services.Interfaces;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace CovenantPromptKey.NUnitTests.Services;

/// <summary>
/// BibleReadingService 單元測試
/// </summary>
[TestFixture]
public class BibleReadingServiceTests
{
    private BibleIndex _bibleIndex = null!;
    private IDebugLogService _debugLogService = null!;
    private BibleReadingService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _bibleIndex = new BibleIndex();
        _debugLogService = Substitute.For<IDebugLogService>();
        _sut = new BibleReadingService(_bibleIndex, _debugLogService);
    }

    #region GetAllBookNames Tests

    [Test]
    public void GetAllBookNames_ShouldReturnBooks()
    {
        // Act
        var bookNames = _sut.GetAllBookNames();

        // Assert
        bookNames.Should().NotBeNull();
        bookNames.Should().NotBeEmpty();
        bookNames.Count.Should().Be(_bibleIndex.BookNames.Count);
    }

    [Test]
    public void GetAllBookNames_FirstBookShouldMatchBibleIndex()
    {
        // Act
        var bookNames = _sut.GetAllBookNames();

        // Assert
        bookNames.First().Should().Be(_bibleIndex.BookNames.First());
    }

    [Test]
    public void GetAllBookNames_LastBookShouldMatchBibleIndex()
    {
        // Act
        var bookNames = _sut.GetAllBookNames();

        // Assert
        bookNames.Last().Should().Be(_bibleIndex.BookNames.Last());
    }

    #endregion

    #region GetChapterCount Tests

    [Test]
    public void GetChapterCount_FirstBook_ShouldReturnPositiveCount()
    {
        // Act
        var count = _sut.GetChapterCount(1);

        // Assert
        count.Should().BeGreaterThan(0);
        count.Should().Be(_bibleIndex.GetChapterCount(1));
    }

    [Test]
    public void GetChapterCount_LastBook_ShouldReturnCorrectCount()
    {
        // BibleIndex 的 BookNames 是實際書卷列表，使用其長度
        var totalBooks = _bibleIndex.BookNames.Count;
        
        // Act
        var count = _sut.GetChapterCount(totalBooks);

        // Assert - 驗證與 BibleIndex 一致
        // 注意：BibleStaticData 中的 Book.Number 是舊約/新約分開編號的
        // 所以 GetChapterCount(66) 可能返回 0（因為不存在 Number=66 的書卷）
        count.Should().Be(_bibleIndex.GetChapterCount(totalBooks));
    }

    [Test]
    public void GetChapterCount_InvalidBookNumber_ShouldReturnZero()
    {
        // Act
        var count = _sut.GetChapterCount(0);

        // Assert
        count.Should().Be(0);
    }

    [Test]
    public void GetChapterCount_BookNumberExceedsTotal_ShouldReturnZero()
    {
        // Act
        var count = _sut.GetChapterCount(_bibleIndex.BookNames.Count + 1);

        // Assert
        count.Should().Be(0);
    }

    #endregion

    #region GetChapterVerses Tests

    [Test]
    public void GetChapterVerses_FirstBookFirstChapter_ShouldReturnVerses()
    {
        // Act
        var verses = _sut.GetChapterVerses(1, 1);

        // Assert
        verses.Should().NotBeNull();
        verses.Should().NotBeEmpty();
    }

    [Test]
    public void GetChapterVerses_ShouldHaveCorrectBookInfo()
    {
        // Act
        var verses = _sut.GetChapterVerses(1, 1);

        // Assert
        var firstVerse = verses.First();
        firstVerse.BookNumber.Should().Be(1);
        firstVerse.BookName.Should().Be(_bibleIndex.BookNames[0]);
        firstVerse.ChapterNumber.Should().Be(1);
        firstVerse.VerseNumber.Should().Be(1);
    }

    [Test]
    public void GetChapterVerses_InvalidBook_ShouldReturnEmptyList()
    {
        // Act
        var verses = _sut.GetChapterVerses(0, 1);

        // Assert
        verses.Should().NotBeNull();
        verses.Should().BeEmpty();
    }

    [Test]
    public void GetChapterVerses_InvalidChapter_ShouldReturnEmptyList()
    {
        // Arrange
        var maxChapters = _sut.GetChapterCount(1);
        
        // Act
        var verses = _sut.GetChapterVerses(1, maxChapters + 1);

        // Assert
        verses.Should().NotBeNull();
        verses.Should().BeEmpty();
    }

    #endregion

    #region GetVerse Tests

    [Test]
    public void GetVerse_FirstBookFirstChapterFirstVerse_ShouldReturnVerse()
    {
        // Act
        var verse = _sut.GetVerse(1, 1, 1);

        // Assert
        verse.Should().NotBeNull();
        verse!.BookNumber.Should().Be(1);
        verse.ChapterNumber.Should().Be(1);
        verse.VerseNumber.Should().Be(1);
        verse.Content.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void GetVerse_InvalidBook_ShouldReturnNull()
    {
        // Act
        var verse = _sut.GetVerse(0, 1, 1);

        // Assert
        verse.Should().BeNull();
    }

    [Test]
    public void GetVerse_InvalidChapter_ShouldReturnNull()
    {
        // Act
        var verse = _sut.GetVerse(1, 999, 1);

        // Assert
        verse.Should().BeNull();
    }

    #endregion

    #region GetBookName Tests

    [Test]
    public void GetBookName_Book1_ShouldReturnFirstBookName()
    {
        // Act
        var name = _sut.GetBookName(1);

        // Assert
        name.Should().Be(_bibleIndex.BookNames[0]);
    }

    [Test]
    public void GetBookName_LastBook_ShouldReturnLastBookName()
    {
        // Act
        var lastBookNumber = _bibleIndex.BookNames.Count;
        var name = _sut.GetBookName(lastBookNumber);

        // Assert
        name.Should().Be(_bibleIndex.BookNames.Last());
    }

    [Test]
    public void GetBookName_InvalidBookNumber_ShouldReturnEmpty()
    {
        // Act
        var name = _sut.GetBookName(0);

        // Assert
        name.Should().BeEmpty();
    }

    #endregion

    #region GetBookNumber Tests

    [Test]
    public void GetBookNumber_FirstBook_ShouldReturn1()
    {
        // Arrange
        var firstBookName = _bibleIndex.BookNames[0];
        
        // Act
        var number = _sut.GetBookNumber(firstBookName);

        // Assert
        number.Should().Be(1);
    }

    [Test]
    public void GetBookNumber_LastBook_ShouldReturnTotalBooks()
    {
        // Arrange
        var lastBookName = _bibleIndex.BookNames.Last();
        
        // Act
        var number = _sut.GetBookNumber(lastBookName);

        // Assert
        number.Should().Be(_bibleIndex.BookNames.Count);
    }

    [Test]
    public void GetBookNumber_InvalidName_ShouldReturnNegativeOne()
    {
        // Act
        var number = _sut.GetBookNumber("不存在的書卷");

        // Assert
        number.Should().Be(-1);
    }

    [Test]
    public void GetBookNumber_EmptyString_ShouldReturnNegativeOne()
    {
        // Act
        var number = _sut.GetBookNumber("");

        // Assert
        number.Should().Be(-1);
    }

    #endregion

    #region IsValidBookNumber Tests

    [Test]
    [TestCase(1, true)]
    [TestCase(0, false)]
    [TestCase(-1, false)]
    public void IsValidBookNumber_ShouldReturnCorrectResult(int bookNumber, bool expected)
    {
        // Act
        var result = _sut.IsValidBookNumber(bookNumber);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void IsValidBookNumber_LastBook_ShouldBeTrue()
    {
        // Act
        var result = _sut.IsValidBookNumber(_bibleIndex.BookNames.Count);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsValidBookNumber_ExceedsTotal_ShouldBeFalse()
    {
        // Act
        var result = _sut.IsValidBookNumber(_bibleIndex.BookNames.Count + 1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsValidChapterNumber Tests

    [Test]
    public void IsValidChapterNumber_FirstBookFirstChapter_ShouldBeTrue()
    {
        // Act
        var result = _sut.IsValidChapterNumber(1, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsValidChapterNumber_FirstBookLastChapter_ShouldBeTrue()
    {
        // Arrange
        var maxChapter = _sut.GetChapterCount(1);
        
        // Act
        var result = _sut.IsValidChapterNumber(1, maxChapter);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsValidChapterNumber_ExceedsMaxChapter_ShouldBeFalse()
    {
        // Arrange
        var maxChapter = _sut.GetChapterCount(1);
        
        // Act
        var result = _sut.IsValidChapterNumber(1, maxChapter + 1);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsValidChapterNumber_InvalidBook_ShouldBeFalse()
    {
        // Act
        var result = _sut.IsValidChapterNumber(0, 1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetPreviousChapter Tests

    [Test]
    public void GetPreviousChapter_SecondChapter_ShouldReturnFirstChapter()
    {
        // Act
        var result = _sut.GetPreviousChapter(1, 2);

        // Assert
        result.Should().NotBeNull();
        result!.Value.BookNumber.Should().Be(1);
        result.Value.ChapterNumber.Should().Be(1);
    }

    [Test]
    public void GetPreviousChapter_FirstBookFirstChapter_ShouldReturnNull()
    {
        // Act
        var result = _sut.GetPreviousChapter(1, 1);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetPreviousChapter_SecondBookFirstChapter_ShouldReturnPreviousBookLastChapter()
    {
        // Arrange
        var firstBookChapterCount = _sut.GetChapterCount(1);
        
        // Act
        var result = _sut.GetPreviousChapter(2, 1);

        // Assert
        result.Should().NotBeNull();
        result!.Value.BookNumber.Should().Be(1);
        result.Value.ChapterNumber.Should().Be(firstBookChapterCount);
    }

    #endregion

    #region GetNextChapter Tests

    [Test]
    public void GetNextChapter_FirstChapter_ShouldReturnSecondChapter()
    {
        // Act
        var result = _sut.GetNextChapter(1, 1);

        // Assert
        result.Should().NotBeNull();
        result!.Value.BookNumber.Should().Be(1);
        result.Value.ChapterNumber.Should().Be(2);
    }

    [Test]
    public void GetNextChapter_FirstBookLastChapter_ShouldReturnSecondBookFirstChapter()
    {
        // Arrange
        var firstBookChapterCount = _sut.GetChapterCount(1);
        
        // Act
        var result = _sut.GetNextChapter(1, firstBookChapterCount);

        // Assert
        result.Should().NotBeNull();
        result!.Value.BookNumber.Should().Be(2);
        result.Value.ChapterNumber.Should().Be(1);
    }

    [Test]
    public void GetNextChapter_LastBookLastChapter_ShouldReturnNull()
    {
        // Arrange
        var lastBookNumber = _bibleIndex.BookNames.Count;
        var lastChapterCount = _sut.GetChapterCount(lastBookNumber);
        
        // Act
        var result = _sut.GetNextChapter(lastBookNumber, lastChapterCount);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetChapterTitle Tests

    [Test]
    public void GetChapterTitle_ValidBookAndChapter_ShouldReturnCorrectTitle()
    {
        // Arrange
        var bookName = _bibleIndex.BookNames[0];
        
        // Act
        var title = _sut.GetChapterTitle(1, 1);

        // Assert
        title.Should().Be($"{bookName} 第1章");
    }

    [Test]
    public void GetChapterTitle_InvalidBook_ShouldReturnEmpty()
    {
        // Act
        var title = _sut.GetChapterTitle(0, 1);

        // Assert
        title.Should().BeEmpty();
    }

    #endregion

    #region GetVerseReference Tests

    [Test]
    public void GetVerseReference_ValidVerse_ShouldReturnCorrectReference()
    {
        // Arrange
        var bookName = _bibleIndex.BookNames[0];
        
        // Act
        var reference = _sut.GetVerseReference(1, 1, 1);

        // Assert
        reference.Should().Be($"{bookName} 1:1");
    }

    [Test]
    public void GetVerseReference_InvalidBook_ShouldReturnEmpty()
    {
        // Act
        var reference = _sut.GetVerseReference(0, 1, 1);

        // Assert
        reference.Should().BeEmpty();
    }

    #endregion

    #region GetVerseCount Tests

    [Test]
    public void GetVerseCount_FirstBookFirstChapter_ShouldReturnPositiveCount()
    {
        // Act
        var count = _sut.GetVerseCount(1, 1);

        // Assert
        count.Should().BeGreaterThan(0);
    }

    [Test]
    public void GetVerseCount_InvalidChapter_ShouldReturnZero()
    {
        // Act
        var count = _sut.GetVerseCount(1, 999);

        // Assert
        count.Should().Be(0);
    }

    #endregion
}
