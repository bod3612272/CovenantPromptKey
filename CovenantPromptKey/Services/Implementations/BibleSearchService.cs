using System.Web;
using BibleData;
using CovenantPromptKey.Models;
using CovenantPromptKey.Models.Bible;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// 聖經搜尋服務實作
/// </summary>
public class BibleSearchService : IBibleSearchService
{
    private readonly BibleIndex _bibleIndex;
    private readonly IDebugLogService _debugLogService;

    public BibleSearchService(BibleIndex bibleIndex, IDebugLogService debugLogService)
    {
        _bibleIndex = bibleIndex;
        _debugLogService = debugLogService;
    }

    /// <inheritdoc />
    public List<SearchResultItem> Search(string keyword, int top = 50)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<SearchResultItem>();

        try
        {
            var results = _bibleIndex.SearchTop(keyword, top);
            return results.Select(v => new SearchResultItem
            {
                BookName = v.BookName,
                ChapterNumber = v.ChapterNumber,
                VerseNumber = v.VerseNumber,
                Content = v.Content,
                Reference = v.Reference,
                Score = 0,
                HighlightedContent = HighlightKeywords(v.Content, new[] { keyword })
            }).ToList();
        }
        catch (Exception ex)
        {
            _debugLogService.Log($"Search error: {ex.Message}", Models.LogLevel.Error);
            return new List<SearchResultItem>();
        }
    }

    /// <inheritdoc />
    public List<SearchResultItem> SearchRanked(string keyword, int top = 50)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<SearchResultItem>();

        try
        {
            var results = _bibleIndex.SearchTopRanked(keyword, top);
            return results.Select(r => new SearchResultItem
            {
                BookName = r.Verse.BookName,
                ChapterNumber = r.Verse.ChapterNumber,
                VerseNumber = r.Verse.VerseNumber,
                Content = r.Verse.Content,
                Reference = r.Verse.Reference,
                Score = r.Score,
                HighlightedContent = HighlightKeywords(r.Verse.Content, new[] { keyword })
            }).ToList();
        }
        catch (Exception ex)
        {
            _debugLogService.Log($"SearchRanked error: {ex.Message}", Models.LogLevel.Error);
            return new List<SearchResultItem>();
        }
    }

    /// <inheritdoc />
    public List<SearchResultItem> SearchWithCancellation(string keyword, int top, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<SearchResultItem>();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var results = _bibleIndex.SearchTopWithCancellation(keyword, top, cancellationToken);
            
            cancellationToken.ThrowIfCancellationRequested();
            
            return results.Select(v => new SearchResultItem
            {
                BookName = v.BookName,
                ChapterNumber = v.ChapterNumber,
                VerseNumber = v.VerseNumber,
                Content = v.Content,
                Reference = v.Reference,
                Score = 0,
                HighlightedContent = HighlightKeywords(v.Content, new[] { keyword })
            }).ToList();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _debugLogService.Log($"SearchWithCancellation error: {ex.Message}", Models.LogLevel.Error);
            return new List<SearchResultItem>();
        }
    }

    /// <inheritdoc />
    public List<SearchResultItem> SearchMultiple(string[] keywords, int top = 50)
    {
        if (keywords == null || keywords.Length == 0)
            return new List<SearchResultItem>();

        var validKeywords = keywords.Where(k => !string.IsNullOrWhiteSpace(k)).ToArray();
        if (validKeywords.Length == 0)
            return new List<SearchResultItem>();

        try
        {
            var results = _bibleIndex.SearchMultipleKeywords(validKeywords, top);
            return results.Select(v => new SearchResultItem
            {
                BookName = v.BookName,
                ChapterNumber = v.ChapterNumber,
                VerseNumber = v.VerseNumber,
                Content = v.Content,
                Reference = v.Reference,
                Score = 0,
                HighlightedContent = HighlightKeywords(v.Content, validKeywords)
            }).ToList();
        }
        catch (Exception ex)
        {
            _debugLogService.Log($"SearchMultiple error: {ex.Message}", Models.LogLevel.Error);
            return new List<SearchResultItem>();
        }
    }

    /// <inheritdoc />
    public List<SearchResultItem> SearchFromInput(string input, int top = 50)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<SearchResultItem>();

        var keywords = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (keywords.Length == 0)
            return new List<SearchResultItem>();
        
        if (keywords.Length == 1)
            return SearchRanked(keywords[0], top);
        
        return SearchMultiple(keywords, top);
    }

    /// <inheritdoc />
    public string HighlightKeywords(string content, string[] keywords)
    {
        if (string.IsNullOrEmpty(content) || keywords == null || keywords.Length == 0)
            return HttpUtility.HtmlEncode(content);

        var result = HttpUtility.HtmlEncode(content);

        foreach (var keyword in keywords)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                continue;

            var encodedKeyword = HttpUtility.HtmlEncode(keyword);
            result = result.Replace(
                encodedKeyword,
                $"<mark class=\"bible-verse-highlight\">{encodedKeyword}</mark>",
                StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }
}
