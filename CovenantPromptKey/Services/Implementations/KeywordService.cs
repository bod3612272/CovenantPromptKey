using System.Text;
using System.Text.RegularExpressions;
using CovenantPromptKey.Constants;
using CovenantPromptKey.Models;
using CovenantPromptKey.Models.Results;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// 關鍵字偵測與替換服務實作
/// 使用 Aho-Corasick 演算法進行高效能多模式匹配
/// </summary>
public partial class KeywordService : IKeywordService
{
    private readonly IMarkdownService _markdownService;
    private readonly IDebugLogService _debugLogService;

    [GeneratedRegex(@"\p{IsCJKUnifiedIdeographs}")]
    private static partial Regex ChineseCharRegex();

    public KeywordService(IMarkdownService markdownService, IDebugLogService debugLogService)
    {
        _markdownService = markdownService;
        _debugLogService = debugLogService;
    }

    /// <inheritdoc />
    public Task<List<DetectedKeyword>> DetectKeywordsAsync(
        string text,
        IReadOnlyList<KeywordMapping> dictionary)
    {
        var result = new List<DetectedKeyword>();

        if (string.IsNullOrEmpty(text) || dictionary.Count == 0)
        {
            return Task.FromResult(result);
        }

        _debugLogService.Log($"開始偵測關鍵字，文本長度: {text.Length}，字典大小: {dictionary.Count}", 
            Models.LogLevel.Debug, nameof(KeywordService));

        // Analyze markdown structure
        var mdStructure = _markdownService.Analyze(text);

        // Build Aho-Corasick automaton
        var matcher = new AhoCorasickMatcher();
        for (int i = 0; i < dictionary.Count; i++)
        {
            matcher.AddPattern(dictionary[i].SensitiveKey, i);
        }
        matcher.Build();

        // Search for all matches
        var allMatches = matcher.Search(text);
        var filteredMatches = AhoCorasickMatcher.FilterOverlaps(allMatches);

        // Calculate line numbers
        var lineStarts = CalculateLineStarts(text);

        // Group by keyword mapping
        var matchesByKeyword = filteredMatches
            .GroupBy(m => m.PatternIndex)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var kvp in matchesByKeyword)
        {
            var mapping = dictionary[kvp.Key];
            var occurrences = new List<KeywordOccurrence>();

            foreach (var match in kvp.Value)
            {
                // Check if in protected URL region
                if (IsInProtectedRegion(match.StartIndex, match.EndIndex, mdStructure.ProtectedUrls))
                {
                    continue;
                }

                // Check if in code block (for whole-word matching later)
                var isInCodeBlock = IsInProtectedRegion(match.StartIndex, match.EndIndex, mdStructure.CodeBlocks) ||
                                   IsInProtectedRegion(match.StartIndex, match.EndIndex, mdStructure.InlineCode);

                // Check for context warning (adjacent Chinese characters)
                var hasContextWarning = HasAdjacentChineseCharacter(text, match.StartIndex, match.EndIndex);

                // Calculate line number
                var lineNumber = GetLineNumber(lineStarts, match.StartIndex);

                occurrences.Add(new KeywordOccurrence
                {
                    StartIndex = match.StartIndex,
                    EndIndex = match.EndIndex,
                    LineNumber = lineNumber,
                    OriginalText = match.OriginalText,
                    HasContextWarning = hasContextWarning,
                    IsInCodeBlock = isInCodeBlock,
                    IsSelected = true
                });
            }

            if (occurrences.Count > 0)
            {
                result.Add(new DetectedKeyword
                {
                    Mapping = mapping,
                    Occurrences = occurrences,
                    IsSelected = true
                });
            }
        }

        _debugLogService.Log($"偵測完成，找到 {result.Count} 個不同關鍵字，共 {result.Sum(r => r.Count)} 處", 
            Models.LogLevel.Info, nameof(KeywordService));

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<MaskResult> ApplyMaskAsync(
        string text,
        List<DetectedKeyword> detectedKeywords)
    {
        _debugLogService.Log($"開始遮罩替換，共 {detectedKeywords.Count} 個關鍵字", 
            Models.LogLevel.Debug, nameof(KeywordService));

        // Collect all selected occurrences with their replacements
        var replacements = new List<(int Start, int End, string Replacement, string Original)>();

        foreach (var dk in detectedKeywords)
        {
            foreach (var occ in dk.Occurrences)
            {
                if (dk.IsSelected && occ.IsSelected)
                {
                    replacements.Add((occ.StartIndex, occ.EndIndex, dk.Mapping.SafeKey, occ.OriginalText));
                }
            }
        }

        // Sort by position descending (to replace from end to start, avoiding index shifts)
        replacements = replacements.OrderByDescending(r => r.Start).ToList();

        // Apply replacements
        var sb = new StringBuilder(text);
        foreach (var (start, end, replacement, _) in replacements)
        {
            sb.Remove(start, end - start);
            sb.Insert(start, replacement);
        }

        // Build result details
        var details = detectedKeywords
            .Where(dk => dk.IsSelected && dk.Occurrences.Any(o => o.IsSelected))
            .Select(dk => new ReplacementDetail
            {
                Original = dk.Mapping.SensitiveKey,
                Replacement = dk.Mapping.SafeKey,
                OccurrenceCount = dk.Occurrences.Count(o => o.IsSelected)
            })
            .ToList();

        var replacedCount = replacements.Count;
        var totalOccurrences = detectedKeywords.Sum(dk => dk.Count);
        var skippedCount = totalOccurrences - replacedCount;

        _debugLogService.Log($"遮罩完成，已替換 {replacedCount} 處，跳過 {skippedCount} 處", 
            Models.LogLevel.Info, nameof(KeywordService));

        return Task.FromResult(new MaskResult
        {
            MaskedText = sb.ToString(),
            ReplacedCount = replacedCount,
            SkippedCount = skippedCount,
            Details = details
        });
    }

    /// <inheritdoc />
    public Task<RestoreResult> RestoreTextAsync(
        string maskedText,
        IReadOnlyList<KeywordMapping> dictionary)
    {
        var details = new List<ReplacementDetail>();
        
        if (string.IsNullOrEmpty(maskedText) || dictionary.Count == 0)
        {
            return Task.FromResult(new RestoreResult
            {
                RestoredText = maskedText ?? string.Empty,
                RestoredCount = 0,
                UnmatchedCount = 0,
                Details = details
            });
        }

        _debugLogService.Log($"開始還原文本，文本長度: {maskedText.Length}", 
            Models.LogLevel.Debug, nameof(KeywordService));

        // Use Aho-Corasick for SafeKey matching (to handle overlapping patterns correctly)
        var matcher = new AhoCorasickMatcher();
        for (int i = 0; i < dictionary.Count; i++)
        {
            matcher.AddPattern(dictionary[i].SafeKey, i);
        }
        matcher.Build();

        // Search for all SafeKey matches
        var allMatches = matcher.Search(maskedText);
        var filteredMatches = AhoCorasickMatcher.FilterOverlaps(allMatches);

        // Sort by position descending (to replace from end to start)
        var replacements = filteredMatches
            .OrderByDescending(m => m.StartIndex)
            .ToList();

        // Apply replacements
        var sb = new StringBuilder(maskedText);
        var restoredCount = 0;
        var detailCounts = new Dictionary<int, int>(); // Track count by pattern index

        foreach (var match in replacements)
        {
            var mapping = dictionary[match.PatternIndex];
            sb.Remove(match.StartIndex, match.EndIndex - match.StartIndex);
            sb.Insert(match.StartIndex, mapping.SensitiveKey);
            restoredCount++;

            // Track for details
            if (!detailCounts.ContainsKey(match.PatternIndex))
            {
                detailCounts[match.PatternIndex] = 0;
            }
            detailCounts[match.PatternIndex]++;
        }

        // Build details
        foreach (var kvp in detailCounts)
        {
            var mapping = dictionary[kvp.Key];
            details.Add(new ReplacementDetail
            {
                Original = mapping.SafeKey,  // In restore, SafeKey is what we find
                Replacement = mapping.SensitiveKey,  // SensitiveKey is what we replace with
                OccurrenceCount = kvp.Value
            });
        }

        _debugLogService.Log($"還原完成，已還原 {restoredCount} 處", 
            Models.LogLevel.Info, nameof(KeywordService));

        return Task.FromResult(new RestoreResult
        {
            RestoredText = sb.ToString(),
            RestoredCount = restoredCount,
            UnmatchedCount = 0,
            Details = details
        });
    }

    /// <inheritdoc />
    public bool ValidateTextLength(string text)
    {
        return string.IsNullOrEmpty(text) || text.Length <= AppConstants.MaxTextLength;
    }

    #region Private Helper Methods

    private static List<int> CalculateLineStarts(string text)
    {
        var lineStarts = new List<int> { 0 };
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                lineStarts.Add(i + 1);
            }
        }
        return lineStarts;
    }

    private static int GetLineNumber(List<int> lineStarts, int position)
    {
        // Binary search for the line
        int low = 0, high = lineStarts.Count - 1;
        while (low < high)
        {
            int mid = (low + high + 1) / 2;
            if (lineStarts[mid] <= position)
                low = mid;
            else
                high = mid - 1;
        }
        return low + 1; // 1-based line number
    }

    private static bool IsInProtectedRegion(int start, int end, List<TextRange> regions)
    {
        return regions.Any(r => start >= r.Start && end <= r.End);
    }

    private static bool HasAdjacentChineseCharacter(string text, int start, int end)
    {
        // Check character before
        if (start > 0)
        {
            var charBefore = text[start - 1].ToString();
            if (ChineseCharRegex().IsMatch(charBefore))
                return true;
        }

        // Check character after
        if (end < text.Length)
        {
            var charAfter = text[end].ToString();
            if (ChineseCharRegex().IsMatch(charAfter))
                return true;
        }

        return false;
    }

    private static int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }

    #endregion
}
