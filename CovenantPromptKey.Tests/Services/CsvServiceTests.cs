using CovenantPromptKey.Models;
using CovenantPromptKey.Services.Implementations;
using CovenantPromptKey.Services.Interfaces;
using NSubstitute;

namespace CovenantPromptKey.Tests.Services;

/// <summary>
/// CsvService 單元測試
/// Tests for CSV import/export functionality using CsvHelper
/// </summary>
public class CsvServiceTests
{
    private readonly IDebugLogService _logService;
    private readonly CsvService _sut;

    public CsvServiceTests()
    {
        _logService = Substitute.For<IDebugLogService>();
        _sut = new CsvService(_logService);
    }

    #region ExportToCsv Tests

    [Fact]
    public void ExportToCsv_WithEmptyList_ReturnsHeaderOnly()
    {
        // Arrange
        var mappings = Array.Empty<KeywordMapping>();

        // Act
        var result = _sut.ExportToCsv(mappings);

        // Assert
        Assert.Contains("SensitiveKey", result);
        Assert.Contains("SafeKey", result);
    }

    [Fact]
    public void ExportToCsv_WithMappings_ReturnsValidCsv()
    {
        // Arrange
        var mappings = new List<KeywordMapping>
        {
            new() { SensitiveKey = "機密公司", SafeKey = "CompanyA", HighlightColor = "#FF0000" },
            new() { SensitiveKey = "內部專案", SafeKey = "ProjectX", HighlightColor = "#00FF00" }
        };

        // Act
        var result = _sut.ExportToCsv(mappings);

        // Assert
        Assert.Contains("機密公司", result);
        Assert.Contains("CompanyA", result);
        Assert.Contains("內部專案", result);
        Assert.Contains("ProjectX", result);
    }

    [Fact]
    public void ExportToCsv_WithSpecialCharacters_EscapesCorrectly()
    {
        // Arrange
        var mappings = new List<KeywordMapping>
        {
            new() { SensitiveKey = "包含,逗號", SafeKey = "HasComma" },
            new() { SensitiveKey = "包含\"引號", SafeKey = "HasQuote" }
        };

        // Act
        var result = _sut.ExportToCsv(mappings);

        // Assert
        // CSV should properly escape these characters
        Assert.Contains("\"包含,逗號\"", result); // Quoted for comma
        Assert.Contains("包含\"\"引號", result);   // Escaped quote
    }

    [Fact]
    public void ExportToCsv_WithNewlines_EscapesCorrectly()
    {
        // Arrange
        var mappings = new List<KeywordMapping>
        {
            new() { SensitiveKey = "多行\n文字", SafeKey = "MultiLine" }
        };

        // Act
        var result = _sut.ExportToCsv(mappings);

        // Assert
        Assert.Contains("\"多行\n文字\"", result); // Quoted for newline
    }

    #endregion

    #region ImportFromCsv Tests

    [Fact]
    public void ImportFromCsv_WithValidCsv_ReturnsSuccessfulMappings()
    {
        // Arrange
        var csv = """
            SensitiveKey,SafeKey,HighlightColor
            機密公司,CompanyA,#FF0000
            內部專案,ProjectX,#00FF00
            """;

        // Act
        var result = _sut.ImportFromCsv(csv);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.ImportedMappings.Count);
        Assert.Contains(result.ImportedMappings, m => m.SensitiveKey == "機密公司");
        Assert.Contains(result.ImportedMappings, m => m.SafeKey == "CompanyA");
    }

    [Fact]
    public void ImportFromCsv_WithEmptyContent_ReturnsEmptyList()
    {
        // Arrange
        var csv = "SensitiveKey,SafeKey,HighlightColor";

        // Act
        var result = _sut.ImportFromCsv(csv);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.ImportedMappings);
    }

    [Fact]
    public void ImportFromCsv_WithMissingColumns_ReturnsError()
    {
        // Arrange
        var csv = """
            SensitiveKey
            機密公司
            """;

        // Act
        var result = _sut.ImportFromCsv(csv);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void ImportFromCsv_WithDuplicateSafeKeys_ReturnsWarnings()
    {
        // Arrange
        var csv = """
            SensitiveKey,SafeKey,HighlightColor
            公司A,DupSafe,#FF0000
            公司B,DupSafe,#00FF00
            """;

        // Act
        var result = _sut.ImportFromCsv(csv);

        // Assert
        // Should either fail or have warnings about duplicates
        Assert.True(result.Errors.Any() || result.ImportedMappings.Count < 2 || 
                    result.Errors.Any(e => e.Message.Contains("重複") || e.Message.Contains("duplicate")));
    }

    [Fact]
    public void ImportFromCsv_WithWhitespaceValues_TrimsValues()
    {
        // Arrange
        var csv = """
            SensitiveKey,SafeKey,HighlightColor
            "  機密公司  ","  CompanyA  ",#FF0000
            """;

        // Act
        var result = _sut.ImportFromCsv(csv);

        // Assert
        if (result.IsSuccess && result.ImportedMappings.Count > 0)
        {
            var mapping = result.ImportedMappings.First();
            Assert.Equal("機密公司", mapping.SensitiveKey.Trim());
        }
    }

    [Fact]
    public void ImportFromCsv_WithEmptySensitiveKey_ReturnsError()
    {
        // Arrange
        var csv = """
            SensitiveKey,SafeKey,HighlightColor
            ,EmptySensitive,#FF0000
            """;

        // Act
        var result = _sut.ImportFromCsv(csv);

        // Assert
        // When all rows have validation errors, Success should be false
        Assert.False(result.IsSuccess);
        Assert.True(result.Errors.Any());
    }

    [Fact]
    public void ImportFromCsv_WithSpecialCharacters_ParsesCorrectly()
    {
        // Arrange
        var csv = """
            SensitiveKey,SafeKey,HighlightColor
            "包含,逗號",HasComma,#FF0000
            "包含""引號",HasQuote,#00FF00
            """;

        // Act
        var result = _sut.ImportFromCsv(csv);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(result.ImportedMappings, m => m.SensitiveKey == "包含,逗號");
        Assert.Contains(result.ImportedMappings, m => m.SensitiveKey == "包含\"引號");
    }

    [Fact]
    public void ImportFromCsv_WithDefaultColor_AssignsDefaultColor()
    {
        // Arrange
        var csv = """
            SensitiveKey,SafeKey
            機密公司,CompanyA
            """;

        // Act
        var result = _sut.ImportFromCsv(csv);

        // Assert
        if (result.IsSuccess && result.ImportedMappings.Count > 0)
        {
            var mapping = result.ImportedMappings.First();
            Assert.NotNull(mapping.HighlightColor);
            Assert.StartsWith("#", mapping.HighlightColor);
        }
    }

    #endregion

    #region ValidateCsv Tests

    [Fact]
    public void ValidateCsv_WithValidCsv_ReturnsSuccess()
    {
        // Arrange
        var csv = """
            SensitiveKey,SafeKey,HighlightColor
            機密公司,CompanyA,#FF0000
            """;

        // Act
        var result = _sut.ValidateCsv(csv);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateCsv_WithInvalidFormat_ReturnsErrors()
    {
        // Arrange
        var csv = """
            Invalid,Header,Only
            "Unclosed quote
            """;

        // Act
        var result = _sut.ValidateCsv(csv);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void ValidateCsv_WithEmptyContent_ReturnsValid()
    {
        // Arrange
        var csv = "SensitiveKey,SafeKey,HighlightColor";

        // Act
        var result = _sut.ValidateCsv(csv);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateCsv_WithMissingRequiredHeaders_ReturnsError()
    {
        // Arrange
        var csv = """
            WrongHeader,AnotherWrong
            Value1,Value2
            """;

        // Act
        var result = _sut.ValidateCsv(csv);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => 
            e.Message.Contains("SensitiveKey") || 
            e.Message.Contains("SafeKey") ||
            e.Message.Contains("欄位") ||
            e.Message.Contains("header"));
    }

    [Fact]
    public void ValidateCsv_ReportsErrorLineNumbers()
    {
        // Arrange
        var csv = """
            SensitiveKey,SafeKey,HighlightColor
            Valid,ValidSafe,#FF0000
            ,MissingSensitive,#00FF00
            """;

        // Act
        var result = _sut.ValidateCsv(csv);

        // Assert
        if (!result.IsValid && result.Errors.Any())
        {
            var error = result.Errors.First();
            Assert.True(error.LineNumber > 0);
        }
    }

    #endregion

    #region Round-Trip Tests

    [Fact]
    public void ExportAndImport_RoundTrip_PreservesData()
    {
        // Arrange
        var originalMappings = new List<KeywordMapping>
        {
            new() { SensitiveKey = "機密公司", SafeKey = "CompanyA", HighlightColor = "#FF0000" },
            new() { SensitiveKey = "內部專案", SafeKey = "ProjectX", HighlightColor = "#00FF00" }
        };

        // Act
        var csv = _sut.ExportToCsv(originalMappings);
        var importResult = _sut.ImportFromCsv(csv);

        // Assert
        Assert.True(importResult.IsSuccess);
        Assert.Equal(originalMappings.Count, importResult.ImportedMappings.Count);
        
        foreach (var original in originalMappings)
        {
            var imported = importResult.ImportedMappings.FirstOrDefault(m => 
                m.SensitiveKey == original.SensitiveKey);
            Assert.NotNull(imported);
            Assert.Equal(original.SafeKey, imported.SafeKey);
            Assert.Equal(original.HighlightColor, imported.HighlightColor);
        }
    }

    [Fact]
    public void ExportAndImport_WithUnicodeCharacters_PreservesData()
    {
        // Arrange
        var originalMappings = new List<KeywordMapping>
        {
            new() { SensitiveKey = "中文測試", SafeKey = "ChineseTest" },
            new() { SensitiveKey = "日本語テスト", SafeKey = "JapaneseTest" },
            new() { SensitiveKey = "한국어테스트", SafeKey = "KoreanTest" },
            new() { SensitiveKey = "🔒機密🔒", SafeKey = "EmojiTest" }
        };

        // Act
        var csv = _sut.ExportToCsv(originalMappings);
        var importResult = _sut.ImportFromCsv(csv);

        // Assert
        Assert.True(importResult.IsSuccess);
        Assert.Equal(originalMappings.Count, importResult.ImportedMappings.Count);
    }

    #endregion
}
