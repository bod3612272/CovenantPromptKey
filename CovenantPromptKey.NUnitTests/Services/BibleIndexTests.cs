using BibleData;
using BibleData.Models;
using NUnit.Framework;

namespace CovenantPromptKey.NUnitTests.Services;

/// <summary>
/// BibleIndex 與 BibleData 資料結構測試
/// </summary>
[TestFixture]
public class BibleIndexTests
{
    private BibleIndex _bibleIndex = null!;

    [SetUp]
    public void Setup()
    {
        _bibleIndex = new BibleIndex();
    }

    [Test]
    public void BibleIndex_應該載入66本書卷()
    {
        // Act
        var bookNames = _bibleIndex.BookNames;

        // Assert
        Assert.That(bookNames.Count, Is.EqualTo(66), "聖經應該有 66 本書卷");
    }

    [Test]
    public void BibleStaticData_每本書卷都應該有Testament屬性()
    {
        // Arrange
        var books = BibleStaticData.Bible.Books;

        // Act & Assert
        var booksWithoutTestament = books.Where(b => string.IsNullOrEmpty(b.Testament)).ToList();

        Assert.That(booksWithoutTestament, Is.Empty, 
            $"以下書卷缺少 Testament 屬性: {string.Join(", ", booksWithoutTestament.Select(b => b.Name))}");
    }

    [Test]
    public void BibleIndex_GetTestament_應該能取得每本書卷的約別()
    {
        // Act
        var booksWithoutTestament = new List<string>();
        
        foreach (var bookName in _bibleIndex.BookNames)
        {
            var testament = _bibleIndex.GetTestament(bookName);
            if (string.IsNullOrEmpty(testament))
            {
                booksWithoutTestament.Add(bookName);
            }
        }

        // Assert
        Assert.That(booksWithoutTestament, Is.Empty,
            $"以下書卷無法取得約別: {string.Join(", ", booksWithoutTestament)}");
    }

    [Test]
    public void BibleIndex_舊約應該有39本書()
    {
        // Act
        var oldTestamentBooks = new List<string>();
        
        foreach (var bookName in _bibleIndex.BookNames)
        {
            var testament = _bibleIndex.GetTestament(bookName);
            if (IsOldTestament(testament))
            {
                oldTestamentBooks.Add(bookName);
            }
        }

        // Assert
        Assert.That(oldTestamentBooks.Count, Is.EqualTo(39), 
            $"舊約應該有 39 本書卷，實際: {oldTestamentBooks.Count}");
    }

    [Test]
    public void BibleIndex_新約應該有27本書()
    {
        // Act
        var newTestamentBooks = new List<string>();
        
        foreach (var bookName in _bibleIndex.BookNames)
        {
            var testament = _bibleIndex.GetTestament(bookName);
            if (IsNewTestament(testament))
            {
                newTestamentBooks.Add(bookName);
            }
        }

        // Assert
        Assert.That(newTestamentBooks.Count, Is.EqualTo(27), 
            $"新約應該有 27 本書卷，實際: {newTestamentBooks.Count}");
    }

    [Test]
    public void BibleIndex_驗證約別的值()
    {
        // Act
        var testamentValues = new HashSet<string>();
        
        foreach (var bookName in _bibleIndex.BookNames)
        {
            var testament = _bibleIndex.GetTestament(bookName);
            if (!string.IsNullOrEmpty(testament))
            {
                testamentValues.Add(testament);
            }
        }

        // Assert
        Console.WriteLine($"發現的約別值: {string.Join(", ", testamentValues)}");
        Assert.That(testamentValues.Count, Is.GreaterThan(0), "應該至少有一種約別值");
    }

    [Test]
    public void BibleIndex_檢查前5本和後5本書卷的約別()
    {
        // Act
        var firstFive = _bibleIndex.BookNames.Take(5).ToList();
        var lastFive = _bibleIndex.BookNames.Skip(_bibleIndex.BookNames.Count - 5).ToList();

        Console.WriteLine("前 5 本書卷:");
        foreach (var bookName in firstFive)
        {
            var book = _bibleIndex.GetBook(bookName);
            var testament = _bibleIndex.GetTestament(bookName);
            Console.WriteLine($"  {bookName}: Testament={testament ?? "null"}, Book.Testament={book?.Testament ?? "null"}");
        }

        Console.WriteLine("\n後 5 本書卷:");
        foreach (var bookName in lastFive)
        {
            var book = _bibleIndex.GetBook(bookName);
            var testament = _bibleIndex.GetTestament(bookName);
            Console.WriteLine($"  {bookName}: Testament={testament ?? "null"}, Book.Testament={book?.Testament ?? "null"}");
        }

        // Assert - 至少要能執行不出錯
        Assert.Pass("輸出約別資訊完成");
    }

    private static bool IsOldTestament(string? testament)
    {
        if (string.IsNullOrEmpty(testament)) return false;
        
        return string.Equals(testament, Testament.Old, StringComparison.OrdinalIgnoreCase)
            || string.Equals(testament, "Old Testament", StringComparison.OrdinalIgnoreCase)
            || string.Equals(testament, "舊約", StringComparison.OrdinalIgnoreCase)
            || string.Equals(testament, "OT", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNewTestament(string? testament)
    {
        if (string.IsNullOrEmpty(testament)) return false;
        
        return string.Equals(testament, Testament.New, StringComparison.OrdinalIgnoreCase)
            || string.Equals(testament, "New Testament", StringComparison.OrdinalIgnoreCase)
            || string.Equals(testament, "新約", StringComparison.OrdinalIgnoreCase)
            || string.Equals(testament, "NT", StringComparison.OrdinalIgnoreCase);
    }
}
