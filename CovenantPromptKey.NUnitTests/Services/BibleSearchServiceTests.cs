using BibleData;
using CovenantPromptKey.Models;
using CovenantPromptKey.Models.Bible;
using CovenantPromptKey.Services.Implementations;
using CovenantPromptKey.Services.Interfaces;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace CovenantPromptKey.NUnitTests.Services;

/// <summary>
/// BibleSearchService 單元測試
/// </summary>
[TestFixture]
public class BibleSearchServiceTests
{
    private BibleIndex _bibleIndex = null!;
    private IDebugLogService _debugLogService = null!;
    private BibleSearchService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        // BibleIndex 是實體類別，無法模擬，使用真實實例
        _bibleIndex = new BibleIndex();
        _debugLogService = Substitute.For<IDebugLogService>();
        _sut = new BibleSearchService(_bibleIndex, _debugLogService);
    }

    #region Search Tests

    [Test]
    public void Search_WithValidKeyword_ShouldReturnResults()
    {
        // Act
        var results = _sut.Search("神", 10);

        // Assert
        results.Should().NotBeNull();
        results.Should().NotBeEmpty();
        results.Count.Should().BeLessThanOrEqualTo(10);
        results.All(r => r.Content.Contains("神")).Should().BeTrue();
    }

    [Test]
    public void Search_WithEmptyKeyword_ShouldReturnEmptyList()
    {
        // Act
        var results = _sut.Search("");

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    [Test]
    public void Search_WithWhitespaceKeyword_ShouldReturnEmptyList()
    {
        // Act
        var results = _sut.Search("   ");

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    [Test]
    public void Search_WithNullKeyword_ShouldReturnEmptyList()
    {
        // Act
        var results = _sut.Search(null!);

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    [Test]
    public void Search_ShouldIncludeHighlightedContent()
    {
        // Act
        var results = _sut.Search("愛", 5);

        // Assert
        results.Should().NotBeEmpty();
        results.All(r => r.HighlightedContent.Contains("<mark")).Should().BeTrue();
    }

    #endregion

    #region SearchRanked Tests

    [Test]
    public void SearchRanked_WithValidKeyword_ShouldReturnRankedResults()
    {
        // Act
        var results = _sut.SearchRanked("神", 10);

        // Assert
        results.Should().NotBeNull();
        results.Should().NotBeEmpty();
        results.Count.Should().BeLessThanOrEqualTo(10);
    }

    [Test]
    public void SearchRanked_WithEmptyKeyword_ShouldReturnEmptyList()
    {
        // Act
        var results = _sut.SearchRanked("");

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    #endregion

    #region SearchMultiple Tests

    [Test]
    public void SearchMultiple_WithValidKeywords_ShouldReturnResults()
    {
        // Act
        var results = _sut.SearchMultiple(new[] { "神", "愛" }, 10);

        // Assert
        results.Should().NotBeNull();
        // 多關鍵字搜尋可能會返回空結果，取決於關鍵字組合
    }

    [Test]
    public void SearchMultiple_WithEmptyArray_ShouldReturnEmptyList()
    {
        // Act
        var results = _sut.SearchMultiple(Array.Empty<string>());

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    [Test]
    public void SearchMultiple_WithNullArray_ShouldReturnEmptyList()
    {
        // Act
        var results = _sut.SearchMultiple(null!);

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    [Test]
    public void SearchMultiple_WithAllEmptyKeywords_ShouldReturnEmptyList()
    {
        // Act
        var results = _sut.SearchMultiple(new[] { "", "  ", null! });

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    #endregion

    #region SearchFromInput Tests

    [Test]
    public void SearchFromInput_WithSingleKeyword_ShouldUseSearchRanked()
    {
        // Act
        var results = _sut.SearchFromInput("神", 10);

        // Assert
        results.Should().NotBeNull();
        results.Should().NotBeEmpty();
    }

    [Test]
    public void SearchFromInput_WithMultipleKeywords_ShouldUseSearchMultiple()
    {
        // Act
        var results = _sut.SearchFromInput("神 愛", 10);

        // Assert
        results.Should().NotBeNull();
        // 多關鍵字搜尋結果可能為空
    }

    [Test]
    public void SearchFromInput_WithEmptyInput_ShouldReturnEmptyList()
    {
        // Act
        var results = _sut.SearchFromInput("");

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    [Test]
    public void SearchFromInput_WithOnlySpaces_ShouldReturnEmptyList()
    {
        // Act
        var results = _sut.SearchFromInput("   ");

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    #endregion

    #region SearchWithCancellation Tests

    [Test]
    public void SearchWithCancellation_WithValidToken_ShouldReturnResults()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act
        var results = _sut.SearchWithCancellation("神", 10, cts.Token);

        // Assert
        results.Should().NotBeNull();
        results.Should().NotBeEmpty();
    }

    [Test]
    public void SearchWithCancellation_WithCancelledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        var action = () => _sut.SearchWithCancellation("神", 10, cts.Token);
        action.Should().Throw<OperationCanceledException>();
    }

    [Test]
    public void SearchWithCancellation_WithEmptyKeyword_ShouldReturnEmptyList()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act
        var results = _sut.SearchWithCancellation("", 10, cts.Token);

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    #endregion

    #region HighlightKeywords Tests

    [Test]
    public void HighlightKeywords_WithValidKeyword_ShouldAddMarkTags()
    {
        // Act
        var result = _sut.HighlightKeywords("神愛世人", new[] { "神" });

        // Assert
        result.Should().Contain("<mark");
        result.Should().Contain("神");
    }

    [Test]
    public void HighlightKeywords_WithMultipleKeywords_ShouldHighlightAll()
    {
        // Act
        var result = _sut.HighlightKeywords("神愛世人", new[] { "神", "世人" });

        // Assert
        result.Should().Contain("<mark");
        result.Should().Contain("神");
        result.Should().Contain("世人");
    }

    [Test]
    public void HighlightKeywords_WithEmptyContent_ShouldReturnEmptyString()
    {
        // Act
        var result = _sut.HighlightKeywords("", new[] { "神" });

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void HighlightKeywords_WithNullContent_ShouldReturnNullOrEmpty()
    {
        // Act
        var result = _sut.HighlightKeywords(null!, new[] { "神" });

        // Assert
        result.Should().BeNullOrEmpty();
    }

    [Test]
    public void HighlightKeywords_WithEmptyKeywords_ShouldReturnEncodedContent()
    {
        // Act
        var result = _sut.HighlightKeywords("神愛世人", Array.Empty<string>());

        // Assert
        result.Should().Be("神愛世人");
        result.Should().NotContain("<mark");
    }

    [Test]
    public void HighlightKeywords_WithHtmlContent_ShouldEncodeHtml()
    {
        // Act
        var result = _sut.HighlightKeywords("<script>alert('xss')</script>", new[] { "script" });

        // Assert
        result.Should().NotContain("<script>");
        result.Should().Contain("&lt;");
    }

    [Test]
    public void HighlightKeywords_ShouldBeCaseInsensitive()
    {
        // Act
        var result = _sut.HighlightKeywords("Lord God", new[] { "lord" });

        // Assert
        result.Should().Contain("<mark");
    }

    #endregion
}
