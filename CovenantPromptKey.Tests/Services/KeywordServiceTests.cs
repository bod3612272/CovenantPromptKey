using CovenantPromptKey.Constants;
using CovenantPromptKey.Models;
using CovenantPromptKey.Services.Implementations;
using CovenantPromptKey.Services.Interfaces;
using NSubstitute;

namespace CovenantPromptKey.Tests.Services;

/// <summary>
/// Unit tests for KeywordService
/// TDD: These tests are written first (Red phase)
/// Covers T029 (DetectKeywordsAsync) and T030 (ApplyMaskAsync)
/// </summary>
public class KeywordServiceTests
{
    private readonly IKeywordService _service;
    private readonly IMarkdownService _markdownService;
    private readonly IDebugLogService _debugLogService;

    public KeywordServiceTests()
    {
        _markdownService = Substitute.For<IMarkdownService>();
        _debugLogService = Substitute.For<IDebugLogService>();
        
        // Default: return empty structure (no protected regions)
        _markdownService.Analyze(Arg.Any<string>())
            .Returns(new MarkdownStructure());
        
        _service = new KeywordService(_markdownService, _debugLogService);
    }

    #region DetectKeywordsAsync Tests

    [Fact]
    public async Task DetectKeywordsAsync_EmptyText_ReturnsEmptyList()
    {
        // Arrange
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "test", SafeKey = "T001" }
        };

        // Act
        var result = await _service.DetectKeywordsAsync("", dictionary);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task DetectKeywordsAsync_EmptyDictionary_ReturnsEmptyList()
    {
        // Arrange
        const string text = "Some text with keywords";
        var dictionary = new List<KeywordMapping>();

        // Act
        var result = await _service.DetectKeywordsAsync(text, dictionary);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task DetectKeywordsAsync_SingleKeywordSingleOccurrence_DetectsCorrectly()
    {
        // Arrange
        const string text = "The company CompanyX is great.";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "CompanyX", SafeKey = "T-Company" }
        };

        // Act
        var result = await _service.DetectKeywordsAsync(text, dictionary);

        // Assert
        Assert.Single(result);
        Assert.Equal("CompanyX", result[0].Mapping.SensitiveKey);
        Assert.Single(result[0].Occurrences);
        Assert.Equal(12, result[0].Occurrences[0].StartIndex); // "The company " = 12 chars
    }

    [Fact]
    public async Task DetectKeywordsAsync_SingleKeywordMultipleOccurrences_DetectsAll()
    {
        // Arrange
        const string text = "CompanyX is great. CompanyX is the best.";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "CompanyX", SafeKey = "T-Company" }
        };

        // Act
        var result = await _service.DetectKeywordsAsync(text, dictionary);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].Count);
    }

    [Fact]
    public async Task DetectKeywordsAsync_MultipleKeywords_DetectsAll()
    {
        // Arrange
        const string text = "CompanyX and PersonY are working together.";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "CompanyX", SafeKey = "T-Company" },
            new() { SensitiveKey = "PersonY", SafeKey = "Dev_A" }
        };

        // Act
        var result = await _service.DetectKeywordsAsync(text, dictionary);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task DetectKeywordsAsync_CaseInsensitive_MatchesDifferentCases()
    {
        // Arrange
        const string text = "COMPANYX companyx CompanyX";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "CompanyX", SafeKey = "T-Company" }
        };

        // Act
        var result = await _service.DetectKeywordsAsync(text, dictionary);

        // Assert
        Assert.Single(result);
        Assert.Equal(3, result[0].Count);
    }

    [Fact]
    public async Task DetectKeywordsAsync_PreservesOriginalCase()
    {
        // Arrange
        const string text = "COMPANYX is here";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "CompanyX", SafeKey = "T-Company" }
        };

        // Act
        var result = await _service.DetectKeywordsAsync(text, dictionary);

        // Assert
        Assert.Single(result);
        Assert.Equal("COMPANYX", result[0].Occurrences[0].OriginalText);
    }

    [Fact]
    public async Task DetectKeywordsAsync_LongestMatchFirst()
    {
        // Arrange - "ProjectX" should match, not just "Project"
        const string text = "Working on ProjectX today";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "Project", SafeKey = "T-Project" },
            new() { SensitiveKey = "ProjectX", SafeKey = "T-ProjectX" }
        };

        // Act
        var result = await _service.DetectKeywordsAsync(text, dictionary);

        // Assert
        Assert.Single(result);
        Assert.Equal("ProjectX", result[0].Mapping.SensitiveKey);
    }

    [Fact]
    public async Task DetectKeywordsAsync_SetsCorrectLineNumber()
    {
        // Arrange
        const string text = "Line1\nLine2\nCompanyX here\nLine4";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "CompanyX", SafeKey = "T-Company" }
        };

        // Act
        var result = await _service.DetectKeywordsAsync(text, dictionary);

        // Assert
        Assert.Single(result);
        Assert.Equal(3, result[0].Occurrences[0].LineNumber); // Line 3 (1-based)
    }

    [Fact]
    public async Task DetectKeywordsAsync_ContextWarning_AdjacentChineseCharacter()
    {
        // Arrange - Chinese character adjacent to keyword should trigger warning
        const string text = "武科電的規範很重要"; // 武科電 with adjacent 的
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "武科電", SafeKey = "T-Company" }
        };

        // Act
        var result = await _service.DetectKeywordsAsync(text, dictionary);

        // Assert
        Assert.Single(result);
        Assert.True(result[0].HasWarning);
        Assert.True(result[0].Occurrences[0].HasContextWarning);
    }

    [Fact]
    public async Task DetectKeywordsAsync_NoContextWarning_WithSpaces()
    {
        // Arrange - Space before/after should not trigger warning
        const string text = "公司 武科電 的規範";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "武科電", SafeKey = "T-Company" }
        };

        // Act
        var result = await _service.DetectKeywordsAsync(text, dictionary);

        // Assert
        Assert.Single(result);
        Assert.False(result[0].HasWarning);
    }

    [Fact]
    public async Task DetectKeywordsAsync_ProtectsUrlRegions()
    {
        // Arrange
        const string text = "Visit https://CompanyX.com for info about CompanyX";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "CompanyX", SafeKey = "T-Company" }
        };
        
        // Setup markdown service to return URL protection
        _markdownService.Analyze(text)
            .Returns(new MarkdownStructure
            {
                ProtectedUrls = [new TextRange(6, 27)] // "https://CompanyX.com"
            });

        // Act
        var result = await _service.DetectKeywordsAsync(text, dictionary);

        // Assert
        Assert.Single(result);
        Assert.Single(result[0].Occurrences); // Only the second occurrence should be detected
        Assert.True(result[0].Occurrences[0].StartIndex > 27); // After the URL
    }

    [Fact]
    public async Task DetectKeywordsAsync_CodeBlock_SetsIsInCodeBlock()
    {
        // Arrange
        const string text = "```\nCompanyX code\n```\nCompanyX text";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "CompanyX", SafeKey = "T-Company" }
        };
        
        // Setup markdown service to return code block
        _markdownService.Analyze(text)
            .Returns(new MarkdownStructure
            {
                CodeBlocks = [new TextRange(0, 21)] // "```\nCompanyX code\n```"
            });

        // Act
        var result = await _service.DetectKeywordsAsync(text, dictionary);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].Count);
        // First occurrence in code block
        Assert.True(result[0].Occurrences[0].IsInCodeBlock);
        // Second occurrence not in code block
        Assert.False(result[0].Occurrences[1].IsInCodeBlock);
    }

    #endregion

    #region ApplyMaskAsync Tests

    [Fact]
    public async Task ApplyMaskAsync_NoKeywordsSelected_ReturnsOriginalText()
    {
        // Arrange
        const string text = "CompanyX is great";
        var detectedKeywords = new List<DetectedKeyword>
        {
            new()
            {
                Mapping = new KeywordMapping { SensitiveKey = "CompanyX", SafeKey = "T-Company" },
                IsSelected = false,
                Occurrences = [new KeywordOccurrence
                {
                    StartIndex = 0,
                    EndIndex = 8,
                    LineNumber = 1,
                    OriginalText = "CompanyX",
                    IsSelected = false
                }]
            }
        };

        // Act
        var result = await _service.ApplyMaskAsync(text, detectedKeywords);

        // Assert
        Assert.Equal(text, result.MaskedText);
        Assert.Equal(0, result.ReplacedCount);
        Assert.Equal(1, result.SkippedCount);
    }

    [Fact]
    public async Task ApplyMaskAsync_AllSelected_ReplacesAll()
    {
        // Arrange
        const string text = "CompanyX is great";
        var detectedKeywords = new List<DetectedKeyword>
        {
            new()
            {
                Mapping = new KeywordMapping { SensitiveKey = "CompanyX", SafeKey = "T-Company" },
                IsSelected = true,
                Occurrences = [new KeywordOccurrence
                {
                    StartIndex = 0,
                    EndIndex = 8,
                    LineNumber = 1,
                    OriginalText = "CompanyX",
                    IsSelected = true
                }]
            }
        };

        // Act
        var result = await _service.ApplyMaskAsync(text, detectedKeywords);

        // Assert
        Assert.Equal("T-Company is great", result.MaskedText);
        Assert.Equal(1, result.ReplacedCount);
        Assert.Equal(0, result.SkippedCount);
    }

    [Fact]
    public async Task ApplyMaskAsync_MultipleOccurrences_ReplacesSelectedOnly()
    {
        // Arrange
        const string text = "CompanyX and CompanyX";
        var occurrence1 = new KeywordOccurrence
        {
            StartIndex = 0,
            EndIndex = 8,
            LineNumber = 1,
            OriginalText = "CompanyX",
            IsSelected = true
        };
        var occurrence2 = new KeywordOccurrence
        {
            StartIndex = 13,
            EndIndex = 21,
            LineNumber = 1,
            OriginalText = "CompanyX",
            IsSelected = false
        };
        var detectedKeywords = new List<DetectedKeyword>
        {
            new()
            {
                Mapping = new KeywordMapping { SensitiveKey = "CompanyX", SafeKey = "T-Company" },
                IsSelected = true,
                Occurrences = [occurrence1, occurrence2]
            }
        };

        // Act
        var result = await _service.ApplyMaskAsync(text, detectedKeywords);

        // Assert
        Assert.Equal("T-Company and CompanyX", result.MaskedText);
        Assert.Equal(1, result.ReplacedCount);
        Assert.Equal(1, result.SkippedCount);
    }

    [Fact]
    public async Task ApplyMaskAsync_ReturnsReplacementDetails()
    {
        // Arrange
        const string text = "CompanyX and PersonY";
        var detectedKeywords = new List<DetectedKeyword>
        {
            new()
            {
                Mapping = new KeywordMapping { SensitiveKey = "CompanyX", SafeKey = "T-Company" },
                IsSelected = true,
                Occurrences = [new KeywordOccurrence
                {
                    StartIndex = 0, EndIndex = 8, LineNumber = 1, OriginalText = "CompanyX", IsSelected = true
                }]
            },
            new()
            {
                Mapping = new KeywordMapping { SensitiveKey = "PersonY", SafeKey = "Dev_A" },
                IsSelected = true,
                Occurrences = [new KeywordOccurrence
                {
                    StartIndex = 13, EndIndex = 20, LineNumber = 1, OriginalText = "PersonY", IsSelected = true
                }]
            }
        };

        // Act
        var result = await _service.ApplyMaskAsync(text, detectedKeywords);

        // Assert
        Assert.Equal(2, result.Details.Count);
        Assert.Contains(result.Details, d => d.Original == "CompanyX" && d.Replacement == "T-Company");
        Assert.Contains(result.Details, d => d.Original == "PersonY" && d.Replacement == "Dev_A");
    }

    #endregion

    #region ValidateTextLength Tests

    [Fact]
    public void ValidateTextLength_WithinLimit_ReturnsTrue()
    {
        // Arrange
        var text = new string('a', AppConstants.MaxTextLength);

        // Act
        var result = _service.ValidateTextLength(text);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateTextLength_ExceedsLimit_ReturnsFalse()
    {
        // Arrange
        var text = new string('a', AppConstants.MaxTextLength + 1);

        // Act
        var result = _service.ValidateTextLength(text);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateTextLength_EmptyText_ReturnsTrue()
    {
        // Act
        var result = _service.ValidateTextLength("");

        // Assert
        Assert.True(result);
    }

    #endregion

    #region RestoreTextAsync Tests (T060)

    [Fact]
    public async Task RestoreTextAsync_EmptyText_ReturnsEmptyResult()
    {
        // Arrange
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "CompanyX", SafeKey = "T001" }
        };

        // Act
        var result = await _service.RestoreTextAsync("", dictionary);

        // Assert
        Assert.Equal("", result.ResultText);
        Assert.Equal(0, result.Statistics.ReplacementCount);
    }

    [Fact]
    public async Task RestoreTextAsync_EmptyDictionary_ReturnsOriginalText()
    {
        // Arrange
        const string text = "Some text with T001 code.";
        var dictionary = new List<KeywordMapping>();

        // Act
        var result = await _service.RestoreTextAsync(text, dictionary);

        // Assert
        Assert.Equal(text, result.ResultText);
        Assert.Equal(0, result.Statistics.ReplacementCount);
    }

    [Fact]
    public async Task RestoreTextAsync_SingleSafeKey_RestoresCorrectly()
    {
        // Arrange
        const string text = "The company T001 is great.";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "CompanyX", SafeKey = "T001" }
        };

        // Act
        var result = await _service.RestoreTextAsync(text, dictionary);

        // Assert
        Assert.Equal("The company CompanyX is great.", result.ResultText);
        Assert.Equal(1, result.Statistics.ReplacementCount);
    }

    [Fact]
    public async Task RestoreTextAsync_MultipleSafeKeys_RestoresAllCorrectly()
    {
        // Arrange
        const string text = "T001 hired Dev_A for the project.";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "CompanyX", SafeKey = "T001" },
            new() { SensitiveKey = "PersonY", SafeKey = "Dev_A" }
        };

        // Act
        var result = await _service.RestoreTextAsync(text, dictionary);

        // Assert
        Assert.Equal("CompanyX hired PersonY for the project.", result.ResultText);
        Assert.Equal(2, result.Statistics.ReplacementCount);
    }

    [Fact]
    public async Task RestoreTextAsync_SameSafeKeyMultipleOccurrences_RestoresAll()
    {
        // Arrange
        const string text = "T001 has a subsidiary also called T001.";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "CompanyX", SafeKey = "T001" }
        };

        // Act
        var result = await _service.RestoreTextAsync(text, dictionary);

        // Assert
        Assert.Equal("CompanyX has a subsidiary also called CompanyX.", result.ResultText);
        Assert.Equal(2, result.Statistics.ReplacementCount);
    }

    [Fact]
    public async Task RestoreTextAsync_CaseInsensitive_MatchesBothCases()
    {
        // Arrange
        const string text = "Use T001 not t001.";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "CompanyX", SafeKey = "T001" }
        };

        // Act
        var result = await _service.RestoreTextAsync(text, dictionary);

        // Assert
        // Both T001 and t001 should be replaced (case-insensitive matching)
        Assert.Equal("Use CompanyX not CompanyX.", result.ResultText);
        Assert.Equal(2, result.Statistics.ReplacementCount);
    }

    [Fact]
    public async Task RestoreTextAsync_NoMatchingSafeKeys_ReturnsOriginalText()
    {
        // Arrange
        const string text = "No SafeKeys in this text.";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "CompanyX", SafeKey = "T001" }
        };

        // Act
        var result = await _service.RestoreTextAsync(text, dictionary);

        // Assert
        Assert.Equal(text, result.ResultText);
        Assert.Equal(0, result.Statistics.ReplacementCount);
    }

    [Fact]
    public async Task RestoreTextAsync_ReturnsCorrectDetails()
    {
        // Arrange
        const string text = "T001 and Dev_A worked together.";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "CompanyX", SafeKey = "T001" },
            new() { SensitiveKey = "PersonY", SafeKey = "Dev_A" }
        };

        // Act
        var result = await _service.RestoreTextAsync(text, dictionary);

        // Assert
        Assert.Equal(2, result.Details.Count);
        Assert.Contains(result.Details, d => d.Original == "T001" && d.Replacement == "CompanyX");
        Assert.Contains(result.Details, d => d.Original == "Dev_A" && d.Replacement == "PersonY");
    }

    [Fact]
    public async Task RestoreTextAsync_ChineseCharacters_RestoresCorrectly()
    {
        // Arrange - Chinese company name restoration
        const string text = "WKTECH_01 的產品很好。";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "武科電", SafeKey = "WKTECH_01" }
        };

        // Act
        var result = await _service.RestoreTextAsync(text, dictionary);

        // Assert
        Assert.Equal("武科電 的產品很好。", result.ResultText);
        Assert.Equal(1, result.Statistics.ReplacementCount);
    }

    [Fact]
    public async Task RestoreTextAsync_OverlappingPatterns_HandlesCorrectly()
    {
        // Arrange - SafeKey that could be a substring of another
        const string text = "The T001 and T00123 codes.";
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "Alpha", SafeKey = "T001" },
            new() { SensitiveKey = "AlphaExtended", SafeKey = "T00123" }
        };

        // Act
        var result = await _service.RestoreTextAsync(text, dictionary);

        // Assert - Longer pattern should be replaced, not the shorter one twice
        Assert.Equal("The Alpha and AlphaExtended codes.", result.ResultText);
        Assert.Equal(2, result.Statistics.ReplacementCount);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task DetectKeywordsAsync_1000Characters_CompletesUnder200ms()
    {
        // Arrange - Generate 1000 character text with keywords scattered
        var baseText = "這是一段測試文字，包含武科電公司的產品資訊。";
        var text = string.Concat(Enumerable.Repeat(baseText, 40)); // ~1000 characters
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "武科電", SafeKey = "T001" },
            new() { SensitiveKey = "公司", SafeKey = "T002" },
            new() { SensitiveKey = "產品", SafeKey = "T003" }
        };

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _service.DetectKeywordsAsync(text, dictionary);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 200, 
            $"Detection took {stopwatch.ElapsedMilliseconds}ms, expected < 200ms");
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task DetectKeywordsAsync_100KCharacters_CompletesUnder1000ms()
    {
        // Arrange - Generate 100K character text
        var baseText = "這是一段測試文字，包含武科電公司的產品資訊。Eason負責開發。";
        var repeatCount = 100000 / baseText.Length + 1;
        var text = string.Concat(Enumerable.Repeat(baseText, repeatCount));
        text = text[..100000]; // Ensure exactly 100K characters
        
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "武科電", SafeKey = "T001" },
            new() { SensitiveKey = "公司", SafeKey = "T002" },
            new() { SensitiveKey = "產品", SafeKey = "T003" },
            new() { SensitiveKey = "Eason", SafeKey = "Dev_A" },
            new() { SensitiveKey = "開發", SafeKey = "T005" }
        };

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _service.DetectKeywordsAsync(text, dictionary);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"Detection took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ApplyMaskAsync_100KCharacters_CompletesUnder1000ms()
    {
        // Arrange - Generate 100K character text
        var baseText = "這是一段測試文字，包含武科電公司的產品資訊。";
        var repeatCount = 100000 / baseText.Length + 1;
        var text = string.Concat(Enumerable.Repeat(baseText, repeatCount));
        text = text[..100000];
        
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "武科電", SafeKey = "T001" },
            new() { SensitiveKey = "公司", SafeKey = "T002" },
            new() { SensitiveKey = "產品", SafeKey = "T003" }
        };

        // Detect first
        var detected = await _service.DetectKeywordsAsync(text, dictionary);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _service.ApplyMaskAsync(text, detected);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"Masking took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
        Assert.NotEmpty(result.MaskedText);
    }

    [Fact]
    public async Task RestoreTextAsync_100KCharacters_CompletesUnder1000ms()
    {
        // Arrange - Generate 100K character masked text
        var baseText = "這是一段測試文字，包含T001的T003資訊。";
        var repeatCount = 100000 / baseText.Length + 1;
        var text = string.Concat(Enumerable.Repeat(baseText, repeatCount));
        text = text[..100000];
        
        var dictionary = new List<KeywordMapping>
        {
            new() { SensitiveKey = "武科電", SafeKey = "T001" },
            new() { SensitiveKey = "產品", SafeKey = "T003" }
        };

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _service.RestoreTextAsync(text, dictionary);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"Restoration took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
        Assert.NotEmpty(result.ResultText);
    }

    [Fact]
    public async Task DetectKeywordsAsync_LargeDictionary500Keywords_CompletesReasonably()
    {
        // Arrange - Text with 10K characters and 500 keyword dictionary
        var text = new string('A', 5000) + "武科電" + new string('B', 4997);
        var dictionary = new List<KeywordMapping>();
        
        for (int i = 0; i < 500; i++)
        {
            dictionary.Add(new KeywordMapping 
            { 
                SensitiveKey = $"Keyword{i:D3}", 
                SafeKey = $"T{i:D3}" 
            });
        }
        // Add one that will match
        dictionary.Add(new KeywordMapping { SensitiveKey = "武科電", SafeKey = "WKTECH" });

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _service.DetectKeywordsAsync(text, dictionary);
        stopwatch.Stop();

        // Assert - Should complete in reasonable time even with large dictionary
        Assert.True(stopwatch.ElapsedMilliseconds < 500, 
            $"Detection with 500 keywords took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
        Assert.Single(result);
        Assert.Equal("武科電", result[0].Mapping.SensitiveKey);
    }

    #endregion
}
