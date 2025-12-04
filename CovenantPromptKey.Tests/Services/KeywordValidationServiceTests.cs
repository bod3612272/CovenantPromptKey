using CovenantPromptKey.Constants;
using CovenantPromptKey.Models;
using CovenantPromptKey.Services.Implementations;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey.Tests.Services;

/// <summary>
/// Unit tests for KeywordValidationService
/// TDD: These tests are written first (Red phase)
/// </summary>
public class KeywordValidationServiceTests
{
    private readonly IKeywordValidationService _service;

    public KeywordValidationServiceTests()
    {
        _service = new KeywordValidationService();
    }

    #region Validate Method Tests

    [Fact]
    public void Validate_ValidMapping_ReturnsSuccess()
    {
        // Arrange
        var mapping = new KeywordMapping
        {
            SensitiveKey = "CompanyX",
            SafeKey = "T-Company",
            HighlightColor = "#FF6B6B"
        };
        var existingMappings = new List<KeywordMapping>();

        // Act
        var result = _service.Validate(mapping, existingMappings);

        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_EmptySensitiveKey_ReturnsFailure(string? sensitiveKey)
    {
        // Arrange
        var mapping = new KeywordMapping
        {
            SensitiveKey = sensitiveKey!,
            SafeKey = "SafeKey"
        };
        var existingMappings = new List<KeywordMapping>();

        // Act
        var result = _service.Validate(mapping, existingMappings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("機敏詞") || e.Contains("Sensitive"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_EmptySafeKey_ReturnsFailure(string? safeKey)
    {
        // Arrange
        var mapping = new KeywordMapping
        {
            SensitiveKey = "SensitiveKey",
            SafeKey = safeKey!
        };
        var existingMappings = new List<KeywordMapping>();

        // Act
        var result = _service.Validate(mapping, existingMappings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("替代詞") || e.Contains("Safe"));
    }

    [Fact]
    public void Validate_SensitiveKeyTooLong_ReturnsFailure()
    {
        // Arrange
        var mapping = new KeywordMapping
        {
            SensitiveKey = new string('a', AppConstants.MaxKeywordLength + 1),
            SafeKey = "SafeKey"
        };
        var existingMappings = new List<KeywordMapping>();

        // Act
        var result = _service.Validate(mapping, existingMappings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("200") || e.Contains("長度"));
    }

    [Fact]
    public void Validate_SafeKeyTooLong_ReturnsFailure()
    {
        // Arrange
        var mapping = new KeywordMapping
        {
            SensitiveKey = "SensitiveKey",
            SafeKey = new string('a', AppConstants.MaxKeywordLength + 1)
        };
        var existingMappings = new List<KeywordMapping>();

        // Act
        var result = _service.Validate(mapping, existingMappings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("200") || e.Contains("長度"));
    }

    [Fact]
    public void Validate_DuplicateSafeKey_ReturnsFailure()
    {
        // Arrange
        var existingMapping = new KeywordMapping
        {
            SensitiveKey = "ExistingKey",
            SafeKey = "ExistingSafeKey"
        };
        var mapping = new KeywordMapping
        {
            SensitiveKey = "NewKey",
            SafeKey = "ExistingSafeKey" // Duplicate
        };
        var existingMappings = new List<KeywordMapping> { existingMapping };

        // Act
        var result = _service.Validate(mapping, existingMappings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("唯一") || e.Contains("duplicate") || e.Contains("已被使用"));
    }

    [Fact]
    public void Validate_DuplicateSafeKey_ExcludesOwnId_WhenUpdating()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var existingMapping = new KeywordMapping
        {
            SensitiveKey = "ExistingKey",
            SafeKey = "ExistingSafeKey"
        };
        // Simulate update: same ID should not flag as duplicate
        var updateMapping = new KeywordMapping
        {
            SensitiveKey = "UpdatedKey",
            SafeKey = "ExistingSafeKey"
        };
        
        // For proper update testing, we need to use the IsSafeKeyUnique method with excludeId
        var existingMappings = new List<KeywordMapping> { existingMapping };

        // Act
        var isUnique = _service.IsSafeKeyUnique(
            updateMapping.SafeKey, 
            existingMappings, 
            existingMapping.Id);

        // Assert - should be unique because we exclude the same ID
        Assert.True(isUnique);
    }

    [Fact]
    public void Validate_ReservedKeyword_ReturnsFailure()
    {
        // Arrange
        var mapping = new KeywordMapping
        {
            SensitiveKey = "public", // C# reserved keyword
            SafeKey = "SafeKey"
        };
        var existingMappings = new List<KeywordMapping>();

        // Act
        var result = _service.Validate(mapping, existingMappings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("保留") || e.Contains("reserved") || e.Contains("關鍵字"));
    }

    [Fact]
    public void Validate_InvalidHexColor_ReturnsFailure()
    {
        // Arrange
        var mapping = new KeywordMapping
        {
            SensitiveKey = "ValidKey",
            SafeKey = "SafeKey",
            HighlightColor = "not-a-color"
        };
        var existingMappings = new List<KeywordMapping>();

        // Act
        var result = _service.Validate(mapping, existingMappings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("顏色") || e.Contains("color") || e.Contains("HEX"));
    }

    [Theory]
    [InlineData("#FF6B6B")]
    [InlineData("#fff")]
    [InlineData("#AABBCC")]
    public void Validate_ValidHexColor_ReturnsSuccess(string color)
    {
        // Arrange
        var mapping = new KeywordMapping
        {
            SensitiveKey = "ValidKey",
            SafeKey = "SafeKey",
            HighlightColor = color
        };
        var existingMappings = new List<KeywordMapping>();

        // Act
        var result = _service.Validate(mapping, existingMappings);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_SubstringOfReserved_ReturnsWarning()
    {
        // Arrange - "pub" is a substring of "public"
        var mapping = new KeywordMapping
        {
            SensitiveKey = "pub",
            SafeKey = "SafeKey"
        };
        var existingMappings = new List<KeywordMapping>();

        // Act
        var result = _service.Validate(mapping, existingMappings);

        // Assert
        Assert.True(result.IsValid); // Should pass but with warning
        Assert.NotEmpty(result.Warnings);
    }

    #endregion

    #region IsReservedKeyword Tests

    [Theory]
    [InlineData("public")]
    [InlineData("class")]
    [InlineData("def")]
    [InlineData("function")]
    [InlineData("PUBLIC")] // Case insensitive
    public void IsReservedKeyword_ReservedWord_ReturnsTrue(string keyword)
    {
        // Act
        var result = _service.IsReservedKeyword(keyword);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("CompanyX")]
    [InlineData("MyKeyword")]
    [InlineData("notreserved")]
    public void IsReservedKeyword_NonReservedWord_ReturnsFalse(string keyword)
    {
        // Act
        var result = _service.IsReservedKeyword(keyword);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region IsSubstringOfReserved Tests

    [Fact]
    public void IsSubstringOfReserved_SubstringMatch_ReturnsTrue()
    {
        // Arrange - "pub" is part of "public"
        const string keyword = "pub";

        // Act
        var result = _service.IsSubstringOfReserved(keyword, out string matchedReserved);

        // Assert
        Assert.True(result);
        Assert.Equal("public", matchedReserved, ignoreCase: true);
    }

    [Fact]
    public void IsSubstringOfReserved_ExactMatch_ReturnsFalse()
    {
        // Arrange - Exact match should not trigger warning
        const string keyword = "public";

        // Act
        var result = _service.IsSubstringOfReserved(keyword, out _);

        // Assert - Exact match should NOT return true (it's handled by IsReservedKeyword)
        Assert.False(result);
    }

    [Fact]
    public void IsSubstringOfReserved_NoMatch_ReturnsFalse()
    {
        // Arrange
        const string keyword = "xyz";

        // Act
        var result = _service.IsSubstringOfReserved(keyword, out string matchedReserved);

        // Assert
        Assert.False(result);
        Assert.Empty(matchedReserved);
    }

    #endregion

    #region IsSafeKeyUnique Tests

    [Fact]
    public void IsSafeKeyUnique_UniqueKey_ReturnsTrue()
    {
        // Arrange
        var existingMappings = new List<KeywordMapping>
        {
            new() { SensitiveKey = "Key1", SafeKey = "Safe1" },
            new() { SensitiveKey = "Key2", SafeKey = "Safe2" }
        };

        // Act
        var result = _service.IsSafeKeyUnique("NewSafeKey", existingMappings);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSafeKeyUnique_DuplicateKey_ReturnsFalse()
    {
        // Arrange
        var existingMappings = new List<KeywordMapping>
        {
            new() { SensitiveKey = "Key1", SafeKey = "Safe1" }
        };

        // Act
        var result = _service.IsSafeKeyUnique("Safe1", existingMappings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSafeKeyUnique_WithExcludeId_ExcludesMatching()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var existingMappings = new List<KeywordMapping>
        {
            new KeywordMapping { SensitiveKey = "Key1", SafeKey = "Safe1" }
        };
        // Override ID for test
        var mapping = existingMappings[0];

        // Act - exclude the same ID (simulating update scenario)
        var result = _service.IsSafeKeyUnique("Safe1", existingMappings, mapping.Id);

        // Assert
        Assert.True(result);
    }

    #endregion
}
