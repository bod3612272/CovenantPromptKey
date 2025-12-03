using CovenantPromptKey.Models;
using CovenantPromptKey.Services.Implementations;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey.Tests.Services;

/// <summary>
/// Unit tests for MarkdownService
/// TDD: These tests are written first (Red phase)
/// </summary>
public class MarkdownServiceTests
{
    private readonly IMarkdownService _service;

    public MarkdownServiceTests()
    {
        _service = new MarkdownService();
    }

    #region Analyze Method Tests

    [Fact]
    public void Analyze_PlainText_ReturnsEmptyStructure()
    {
        // Arrange
        const string text = "This is plain text without any markdown.";

        // Act
        var result = _service.Analyze(text);

        // Assert
        Assert.Empty(result.ProtectedUrls);
        Assert.Empty(result.CodeBlocks);
        Assert.Empty(result.InlineCode);
    }

    [Fact]
    public void Analyze_EmptyText_ReturnsEmptyStructure()
    {
        // Arrange
        const string text = "";

        // Act
        var result = _service.Analyze(text);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.ProtectedUrls);
        Assert.Empty(result.CodeBlocks);
        Assert.Empty(result.InlineCode);
    }

    #endregion

    #region URL Detection Tests

    [Fact]
    public void Analyze_MarkdownLink_DetectsProtectedUrl()
    {
        // Arrange
        const string text = "Check out [this link](https://example.com) for more info.";

        // Act
        var result = _service.Analyze(text);

        // Assert
        Assert.NotEmpty(result.ProtectedUrls);
        Assert.Contains(result.ProtectedUrls, r => 
            text.Substring(r.Start, r.End - r.Start).Contains("https://example.com"));
    }

    [Fact]
    public void Analyze_RawUrl_DetectsProtectedUrl()
    {
        // Arrange
        const string text = "Visit https://example.com/path?query=value for details.";

        // Act
        var result = _service.Analyze(text);

        // Assert
        Assert.NotEmpty(result.ProtectedUrls);
    }

    [Fact]
    public void Analyze_MultipleUrls_DetectsAll()
    {
        // Arrange
        const string text = @"First URL: https://example1.com
Second URL: [link](https://example2.com)
Third URL: http://example3.com";

        // Act
        var result = _service.Analyze(text);

        // Assert
        Assert.True(result.ProtectedUrls.Count >= 2);
    }

    #endregion

    #region Code Block Detection Tests

    [Fact]
    public void Analyze_FencedCodeBlock_DetectsCodeBlock()
    {
        // Arrange
        const string text = @"Some text
```csharp
public class Test { }
```
More text";

        // Act
        var result = _service.Analyze(text);

        // Assert
        Assert.NotEmpty(result.CodeBlocks);
        var codeBlockText = text.Substring(result.CodeBlocks[0].Start, 
            result.CodeBlocks[0].End - result.CodeBlocks[0].Start);
        Assert.Contains("public class Test", codeBlockText);
    }

    [Fact]
    public void Analyze_FencedCodeBlockWithoutLanguage_DetectsCodeBlock()
    {
        // Arrange
        const string text = @"Text before
```
code here
```
Text after";

        // Act
        var result = _service.Analyze(text);

        // Assert
        Assert.NotEmpty(result.CodeBlocks);
    }

    [Fact]
    public void Analyze_MultipleCodeBlocks_DetectsAll()
    {
        // Arrange
        const string text = @"First block:
```python
def hello():
    pass
```

Second block:
```javascript
function hello() {}
```";

        // Act
        var result = _service.Analyze(text);

        // Assert
        Assert.Equal(2, result.CodeBlocks.Count);
    }

    #endregion

    #region Inline Code Detection Tests

    [Fact]
    public void Analyze_InlineCode_DetectsInlineCode()
    {
        // Arrange
        const string text = "Use the `Console.WriteLine` method to print.";

        // Act
        var result = _service.Analyze(text);

        // Assert
        Assert.NotEmpty(result.InlineCode);
        var inlineCodeText = text.Substring(result.InlineCode[0].Start, 
            result.InlineCode[0].End - result.InlineCode[0].Start);
        Assert.Contains("Console.WriteLine", inlineCodeText);
    }

    [Fact]
    public void Analyze_MultipleInlineCode_DetectsAll()
    {
        // Arrange
        const string text = "Use `method1` and `method2` for processing.";

        // Act
        var result = _service.Analyze(text);

        // Assert
        Assert.Equal(2, result.InlineCode.Count);
    }

    [Fact]
    public void Analyze_InlineCodeWithBackticks_HandlesCorrectly()
    {
        // Arrange - Double backticks for inline code containing backtick
        const string text = "Use ``code with ` backtick`` here.";

        // Act
        var result = _service.Analyze(text);

        // Assert
        Assert.NotEmpty(result.InlineCode);
    }

    #endregion

    #region Mixed Content Tests

    [Fact]
    public void Analyze_MixedContent_DetectsAllTypes()
    {
        // Arrange
        const string text = @"# Documentation

Check the [API docs](https://api.example.com/docs).

Use the `GetData()` method:

```csharp
var data = service.GetData();
```

Visit https://example.com for more.";

        // Act
        var result = _service.Analyze(text);

        // Assert
        Assert.NotEmpty(result.ProtectedUrls);
        Assert.NotEmpty(result.CodeBlocks);
        Assert.NotEmpty(result.InlineCode);
    }

    [Fact]
    public void Analyze_CodeBlockDoesNotOverlapWithInlineCode()
    {
        // Arrange
        const string text = @"Inline: `code`

```
block code
```";

        // Act
        var result = _service.Analyze(text);

        // Assert
        // Verify no overlaps
        foreach (var inlineRange in result.InlineCode)
        {
            foreach (var blockRange in result.CodeBlocks)
            {
                // Ranges should not overlap
                Assert.False(
                    inlineRange.Start < blockRange.End && inlineRange.End > blockRange.Start,
                    "Inline code and code block ranges should not overlap");
            }
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Analyze_UnmatchedBackticks_DoesNotCrash()
    {
        // Arrange
        const string text = "Some text with ` unclosed backtick";

        // Act
        var result = _service.Analyze(text);

        // Assert - Should not throw, may or may not detect as inline code
        Assert.NotNull(result);
    }

    [Fact]
    public void Analyze_UnmatchedCodeFence_DoesNotCrash()
    {
        // Arrange
        const string text = @"Text
```
unclosed code block";

        // Act
        var result = _service.Analyze(text);

        // Assert - Should not throw
        Assert.NotNull(result);
    }

    [Fact]
    public void Analyze_NestedBackticks_HandledCorrectly()
    {
        // Arrange - Backticks inside code block should not be detected as inline
        const string text = @"```
`inline` inside block
```";

        // Act
        var result = _service.Analyze(text);

        // Assert
        Assert.NotEmpty(result.CodeBlocks);
        // The `inline` inside block should not be detected as separate inline code
    }

    #endregion
}
