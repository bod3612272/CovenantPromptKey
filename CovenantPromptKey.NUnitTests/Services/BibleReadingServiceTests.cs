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
        // Act - bookNumber=1 代表 BookNames[0] = "創世記"
        var count = _sut.GetChapterCount(1);

        // Assert - 創世記有 50 章
        count.Should().BeGreaterThan(0);
        count.Should().Be(_bibleIndex.GetChapterCount("創世記"));
    }

    [Test]
    public void GetChapterCount_LastBook_ShouldReturnCorrectCount()
    {
        // BibleIndex 的 BookNames 是實際書卷列表，使用其長度
        var totalBooks = _bibleIndex.BookNames.Count;
        var lastBookName = _bibleIndex.BookNames[totalBooks - 1]; // 啟示錄
        
        // Act
        var count = _sut.GetChapterCount(totalBooks);

        // Assert - 使用書卷名稱查詢來驗證
        count.Should().BeGreaterThan(0);
        count.Should().Be(_bibleIndex.GetChapterCount(lastBookName));
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

    #region BibleData DLL Design Validation Tests

    /// <summary>
    /// 驗證 DLL 的設計：舊約新約分開編號是預期行為
    /// Book.Number 在舊約是 1-39，新約是 1-27（獨立編號）
    /// </summary>
    [Test]
    public void BibleStaticData_ShouldHave66Books()
    {
        // Arrange
        var books = BibleData.BibleStaticData.Bible.Books;

        // Assert
        books.Should().HaveCount(66, "聖經應該有 66 卷書");
    }

    [Test]
    public void BibleStaticData_OldTestament_ShouldHave39BooksWithNumber1To39()
    {
        // Arrange - 舊約前 39 本
        var books = BibleData.BibleStaticData.Bible.Books.Take(39).ToList();

        // Assert
        books.Should().HaveCount(39);
        books.First().Number.Should().Be(1, "舊約第一卷（創世記）編號應為 1");
        books.First().Name.Should().Be("創世記");
        books.Last().Number.Should().Be(39, "舊約最後一卷（瑪拉基書）編號應為 39");
        books.Last().Name.Should().Be("瑪拉基書");
    }

    [Test]
    public void BibleStaticData_NewTestament_ShouldHave27BooksWithNumber1To27()
    {
        // Arrange - 新約後 27 本
        var books = BibleData.BibleStaticData.Bible.Books.Skip(39).ToList();

        // Assert
        books.Should().HaveCount(27);
        books.First().Number.Should().Be(1, "新約第一卷（馬太福音）編號應為 1（新約獨立編號）");
        books.First().Name.Should().Be("馬太福音");
        books.Last().Number.Should().Be(27, "新約最後一卷（啟示錄）編號應為 27");
        books.Last().Name.Should().Be("啟示錄");
    }

    /// <summary>
    /// 驗證書卷名稱是唯一的（這是正確的識別方式）
    /// </summary>
    [Test]
    public void BibleIndex_BookNamesShouldBeUnique()
    {
        // Arrange
        var books = BibleData.BibleStaticData.Bible.Books;

        // Act
        var bookNames = books.Select(b => b.Name).ToList();
        var uniqueBookNames = bookNames.Distinct().ToList();

        // Assert
        bookNames.Should().HaveCount(uniqueBookNames.Count,
            "所有 66 本書卷的名稱應該都是唯一的");
    }

    /// <summary>
    /// 驗證正確的查詢方式：使用書卷名稱查詢
    /// </summary>
    [Test]
    public void BibleIndex_GetBookByName_ShouldReturnCorrectBook()
    {
        // Act
        var genesis = _bibleIndex.GetBook("創世記");
        var matthew = _bibleIndex.GetBook("馬太福音");

        // Assert
        genesis.Should().NotBeNull();
        genesis!.Name.Should().Be("創世記");
        genesis.Number.Should().Be(1, "創世記在舊約中的編號是 1");

        matthew.Should().NotBeNull();
        matthew!.Name.Should().Be("馬太福音");
        matthew.Number.Should().Be(1, "馬太福音在新約中的編號也是 1（獨立編號系統）");
    }

    /// <summary>
    /// 驗證使用書卷名稱查詢經文是正確的方式
    /// </summary>
    [Test]
    public void BibleIndex_GetVerseByName_ShouldReturnCorrectContent()
    {
        // Arrange
        const string expectedGenesisStart = "起初";
        const string expectedMatthewStart = "亞伯拉罕";

        // Act
        var genesisVerse = _bibleIndex.GetVerse("創世記", 1, 1);
        var matthewVerse = _bibleIndex.GetVerse("馬太福音", 1, 1);

        // Assert
        genesisVerse.Should().NotBeNull();
        genesisVerse!.Content.Should().StartWith(expectedGenesisStart,
            "創世記 1:1 應該以「起初」開頭");

        matthewVerse.Should().NotBeNull();
        matthewVerse!.Content.Should().StartWith(expectedMatthewStart,
            "馬太福音 1:1 應該以「亞伯拉罕」開頭");
    }

    #endregion

    #region BibleReadingService Corrected Tests

    /// <summary>
    /// 驗證 GetBookName(1) 確實返回「創世記」
    /// </summary>
    [Test]
    public void GetBookName_Book1_ShouldReturnGenesis()
    {
        // Act
        var bookName = _sut.GetBookName(1);

        // Assert
        bookName.Should().Be("創世記");
    }

    /// <summary>
    /// 記錄 BibleIndex.GetChapter(string, int) 的已知限制：
    /// 該方法內部使用 Book.Number 查詢 _chapterIndex，
    /// 由於舊約新約 Book.Number 重複（都從 1 開始），會導致錯誤結果。
    /// 
    /// 正確做法：直接從 Book.Chapters 列表取得章節，不要使用此方法。
    /// </summary>
    [Test]
    public void BibleIndex_GetChapterByName_KnownLimitation()
    {
        // Arrange
        var genesisBook = _bibleIndex.GetBook("創世記");
        var matthewBook = _bibleIndex.GetBook("馬太福音");

        // Assert - GetBook(string) 可以正確取得書卷
        genesisBook.Should().NotBeNull();
        genesisBook!.Name.Should().Be("創世記");
        matthewBook.Should().NotBeNull();
        matthewBook!.Name.Should().Be("馬太福音");

        // 正確做法：直接從 Book.Chapters 取得章節
        var genesisChapter1 = genesisBook.Chapters[0];
        var matthewChapter1 = matthewBook.Chapters[0];

        genesisChapter1.Verses[0].Content.Should().StartWith("起初",
            "直接從 Book.Chapters 取得的創世記第 1 章應該正確");
        matthewChapter1.Verses[0].Content.Should().StartWith("亞伯拉罕",
            "直接從 Book.Chapters 取得的馬太福音第 1 章應該正確");

        // 注意：BibleIndex.GetChapter(string, int) 有已知限制
        // 因為它內部使用 Book.Number 查詢，而舊約新約編號重複
        // 所以不測試該方法的正確性，改用上述正確做法
    }

    /// <summary>
    /// 驗證 BibleReadingService 使用列表索引（1-66）查詢時返回正確的書卷內容
    /// 服務內部應該將索引轉換為書卷名稱進行查詢
    /// </summary>
    [Test]
    public void GetChapterVerses_Book1_ShouldReturnGenesisContent()
    {
        // Arrange
        const string expectedContentStart = "起初"; // 創世記 1:1
        
        // 先驗證 GetBookName 正確
        var bookName = _sut.GetBookName(1);
        bookName.Should().Be("創世記", "GetBookName(1) 應該返回創世記");

        // Act - bookNumber=1 代表 BookNames[0] = "創世記"
        var verses = _sut.GetChapterVerses(1, 1);

        // Assert
        verses.Should().NotBeEmpty("應該有經文返回");
        var firstVerse = verses.First();
        firstVerse.BookName.Should().Be("創世記", "BookName 應該是創世記");
        firstVerse.Content.Should().StartWith(expectedContentStart,
            $"GetChapterVerses(1, 1) 應該返回創世記第 1 章的內容，但實際得到：{firstVerse.Content.Substring(0, Math.Min(50, firstVerse.Content.Length))}");
    }

    [Test]
    public void GetChapterVerses_Book40_ShouldReturnMatthewContent()
    {
        // Arrange
        const string expectedContentStart = "亞伯拉罕"; // 馬太福音 1:1

        // Act - bookNumber=40 代表 BookNames[39] = "馬太福音"
        var verses = _sut.GetChapterVerses(40, 1);

        // Assert
        verses.Should().NotBeEmpty();
        var firstVerse = verses.First();
        firstVerse.BookName.Should().Be("馬太福音");
        firstVerse.Content.Should().StartWith(expectedContentStart,
            "GetChapterVerses(40, 1) 應該返回馬太福音第 1 章的內容");
    }

    [Test]
    public void GetVerse_Book1_ShouldReturnGenesisVerse()
    {
        // Arrange
        const string expectedContentStart = "起初";

        // Act
        var verse = _sut.GetVerse(1, 1, 1);

        // Assert
        verse.Should().NotBeNull();
        verse!.BookName.Should().Be("創世記");
        verse.Content.Should().StartWith(expectedContentStart);
    }

    [Test]
    public void GetVerse_Book40_ShouldReturnMatthewVerse()
    {
        // Arrange
        const string expectedContentStart = "亞伯拉罕";

        // Act
        var verse = _sut.GetVerse(40, 1, 1);

        // Assert
        verse.Should().NotBeNull();
        verse!.BookName.Should().Be("馬太福音");
        verse.Content.Should().StartWith(expectedContentStart);
    }

    [Test]
    public void GetChapterCount_Book1_ShouldReturnGenesisChapterCount()
    {
        // Act
        var count = _sut.GetChapterCount(1);

        // Assert - 創世記有 50 章
        count.Should().Be(50);
    }

    [Test]
    public void GetChapterCount_Book40_ShouldReturnMatthewChapterCount()
    {
        // Act
        var count = _sut.GetChapterCount(40);

        // Assert - 馬太福音有 28 章
        count.Should().Be(28);
    }

    #endregion
}
