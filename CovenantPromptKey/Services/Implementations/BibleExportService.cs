using System.Text;
using BibleData;
using CovenantPromptKey.Models.Bible;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// 經文導出服務實作
/// </summary>
public class BibleExportService : IBibleExportService
{
    private readonly BibleIndex _bibleIndex;
    private readonly IBibleReadingService _readingService;

    public BibleExportService(BibleIndex bibleIndex, IBibleReadingService readingService)
    {
        _bibleIndex = bibleIndex;
        _readingService = readingService;
    }

    /// <inheritdoc />
    public string ExportToMarkdown(ExportRange range, ExportOptions options)
    {
        return ExportToMarkdown(new[] { range }, options);
    }

    /// <inheritdoc />
    public string ExportToMarkdown(IEnumerable<ExportRange> ranges, ExportOptions options)
    {
        var sb = new StringBuilder();
        var rangeList = ranges.ToList();

        foreach (var range in rangeList)
        {
            var bookName = _readingService.GetBookName(range.BookNumber);
            if (string.IsNullOrEmpty(bookName)) continue;

            var verses = GetVersesInRange(range);
            if (!verses.Any()) continue;

            sb.Append(FormatVerses(verses, bookName, range, options));

            if (rangeList.IndexOf(range) < rangeList.Count - 1)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString().TrimEnd();
    }

    /// <inheritdoc />
    public string ExportBookToMarkdown(int bookNumber, ExportOptions options)
    {
        var bookName = _readingService.GetBookName(bookNumber);
        if (string.IsNullOrEmpty(bookName)) return string.Empty;

        var sb = new StringBuilder();

        if (options.IncludeBookTitle)
        {
            sb.AppendLine($"# {bookName}");
            sb.AppendLine();
        }

        var chapterCount = _readingService.GetChapterCount(bookNumber);
        for (int chapter = 1; chapter <= chapterCount; chapter++)
        {
            var verses = _readingService.GetChapterVerses(bookNumber, chapter);
            if (!verses.Any()) continue;

            sb.AppendLine($"## 第 {chapter} 章");
            sb.AppendLine();

            foreach (var verse in verses)
            {
                sb.AppendLine(FormatSingleVerse(verse, bookName, options.Style));
            }

            if (chapter < chapterCount)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString().TrimEnd();
    }

    /// <inheritdoc />
    public Dictionary<string, string> ExportBooksToFiles(IEnumerable<int> bookNumbers, ExportOptions options)
    {
        var result = new Dictionary<string, string>();

        foreach (var bookNumber in bookNumbers)
        {
            var bookName = _readingService.GetBookName(bookNumber);
            if (string.IsNullOrEmpty(bookName)) continue;

            var fileName = $"{bookNumber:D2}_{bookName}.md";
            var content = ExportBookToMarkdown(bookNumber, options);

            if (!string.IsNullOrEmpty(content))
            {
                result[fileName] = content;
            }
        }

        return result;
    }

    /// <inheritdoc />
    public string GetStylePreview(ExportStyle style)
    {
        return style switch
        {
            ExportStyle.Style1 => "(創世記 1:1) 起初，神創造天地。",
            ExportStyle.Style2 => """
                創世記 ( 第 1 章 1 ~ 2 節 )
                1 起初，神創造天地。
                2 地是空虛混沌，淵面黑暗；神的靈運行在水面上。
                """,
            ExportStyle.Style3 => """
                《創世記》( 第 1 章 1 ~ 2 節 )
                起初，神創造天地。地是空虛混沌，淵面黑暗；神的靈運行在水面上。
                """,
            _ => "(創世記 1:1) 起初，神創造天地。"
        };
    }

    /// <inheritdoc />
    public string ExportSelectedVerses(int bookNumber, int chapterNumber, IEnumerable<int> verseNumbers, ExportOptions options)
    {
        var bookName = _readingService.GetBookName(bookNumber);
        if (string.IsNullOrEmpty(bookName)) return string.Empty;

        var allVerses = _readingService.GetChapterVerses(bookNumber, chapterNumber);
        var verseNumberList = verseNumbers.ToList();
        var selectedVerses = allVerses.Where(v => verseNumberList.Contains(v.VerseNumber)).ToList();

        if (!selectedVerses.Any()) return string.Empty;

        var range = new ExportRange
        {
            BookNumber = bookNumber,
            StartChapter = chapterNumber,
            EndChapter = chapterNumber,
            StartVerse = selectedVerses.Min(v => v.VerseNumber),
            EndVerse = selectedVerses.Max(v => v.VerseNumber)
        };

        return FormatVerses(selectedVerses, bookName, range, options);
    }

    private List<VerseWithLocation> GetVersesInRange(ExportRange range)
    {
        var verses = new List<VerseWithLocation>();

        for (int chapter = range.StartChapter; chapter <= range.EndChapter; chapter++)
        {
            var chapterVerses = _readingService.GetChapterVerses(range.BookNumber, chapter);

            foreach (var verse in chapterVerses)
            {
                bool include = true;

                // 起始章的限制
                if (chapter == range.StartChapter && verse.VerseNumber < range.StartVerse)
                    include = false;

                // 結束章的限制
                if (chapter == range.EndChapter && verse.VerseNumber > range.EndVerse)
                    include = false;

                if (include)
                    verses.Add(verse);
            }
        }

        return verses;
    }

    private string FormatVerses(List<VerseWithLocation> verses, string bookName, ExportRange range, ExportOptions options)
    {
        var sb = new StringBuilder();

        switch (options.Style)
        {
            case ExportStyle.Style1:
                // 風格 1：(馬可福音 1:1) 經文內容
                foreach (var verse in verses)
                {
                    sb.AppendLine($"({bookName} {verse.ChapterNumber}:{verse.VerseNumber}) {verse.Content}");
                }
                break;

            case ExportStyle.Style2:
                // 風格 2：書卷 ( 第 X 章 Y ~ Z 節 ) + 逐行經文
                var headerStyle2 = FormatRangeHeader(bookName, range, useGuillemets: false);
                sb.AppendLine(headerStyle2);
                foreach (var verse in verses)
                {
                    sb.AppendLine($"{verse.VerseNumber} {verse.Content}");
                }
                break;

            case ExportStyle.Style3:
                // 風格 3：《書卷》( 第 X 章 Y ~ Z 節 ) + 連續段落
                var headerStyle3 = FormatRangeHeader(bookName, range, useGuillemets: true);
                sb.AppendLine(headerStyle3);
                var combinedText = string.Join("", verses.Select(v => v.Content));
                sb.AppendLine(combinedText);
                break;
        }

        return sb.ToString();
    }

    private string FormatSingleVerse(VerseWithLocation verse, string bookName, ExportStyle style)
    {
        return style switch
        {
            ExportStyle.Style1 => $"({bookName} {verse.ChapterNumber}:{verse.VerseNumber}) {verse.Content}",
            ExportStyle.Style2 => $"{verse.VerseNumber} {verse.Content}",
            ExportStyle.Style3 => verse.Content,
            _ => $"({bookName} {verse.ChapterNumber}:{verse.VerseNumber}) {verse.Content}"
        };
    }

    private string FormatRangeHeader(string bookName, ExportRange range, bool useGuillemets)
    {
        var bookPart = useGuillemets ? $"《{bookName}》" : bookName;

        if (range.StartChapter == range.EndChapter)
        {
            if (range.StartVerse == range.EndVerse)
            {
                return $"{bookPart} ( 第 {range.StartChapter} 章 {range.StartVerse} 節 )";
            }
            return $"{bookPart} ( 第 {range.StartChapter} 章 {range.StartVerse} ~ {range.EndVerse} 節 )";
        }

        return $"{bookPart} ( 第 {range.StartChapter} 章 {range.StartVerse} 節 ~ 第 {range.EndChapter} 章 {range.EndVerse} 節 )";
    }
}
