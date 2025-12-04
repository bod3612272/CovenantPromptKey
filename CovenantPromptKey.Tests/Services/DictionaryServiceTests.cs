using CovenantPromptKey.Models;
using CovenantPromptKey.Models.Results;
using CovenantPromptKey.Services.Implementations;
using CovenantPromptKey.Services.Interfaces;
using Microsoft.JSInterop;
using NSubstitute;

namespace CovenantPromptKey.Tests.Services;

/// <summary>
/// DictionaryService 單元測試
/// Tests for CRUD operations and localStorage persistence
/// </summary>
public class DictionaryServiceTests
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IKeywordValidationService _validationService;
    private readonly IDebugLogService _logService;
    private readonly DictionaryService _sut;

    public DictionaryServiceTests()
    {
        _jsRuntime = Substitute.For<IJSRuntime>();
        _validationService = Substitute.For<IKeywordValidationService>();
        _logService = Substitute.For<IDebugLogService>();
        
        // Default validation returns success
        _validationService.Validate(Arg.Any<KeywordMapping>(), Arg.Any<IReadOnlyList<KeywordMapping>>())
            .Returns(ValidationResult<KeywordMapping>.Success(null!));
        
        _sut = new DictionaryService(_jsRuntime, _validationService, _logService);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ReturnsEmptyList()
    {
        // Arrange
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WithData_ReturnsMappings()
    {
        // Arrange
        var mappings = new List<KeywordMapping>
        {
            new() { SensitiveKey = "機密A", SafeKey = "SafeA" },
            new() { SensitiveKey = "機密B", SafeKey = "SafeB" }
        };
        var json = System.Text.Json.JsonSerializer.Serialize(mappings);
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>(json));

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.SensitiveKey == "機密A");
        Assert.Contains(result, m => m.SensitiveKey == "機密B");
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsMapping()
    {
        // Arrange
        var mapping = new KeywordMapping { SensitiveKey = "Test", SafeKey = "SafeTest" };
        var mappings = new List<KeywordMapping> { mapping };
        var json = System.Text.Json.JsonSerializer.Serialize(mappings);
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>(json));

        // Act
        var result = await _sut.GetByIdAsync(mapping.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.SensitiveKey);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidMapping_ReturnsSuccess()
    {
        // Arrange
        var mapping = new KeywordMapping { SensitiveKey = "NewKey", SafeKey = "NewSafe" };
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));
        _validationService.Validate(mapping, Arg.Any<IReadOnlyList<KeywordMapping>>())
            .Returns(ValidationResult<KeywordMapping>.Success(mapping));

        // Act
        var result = await _sut.AddAsync(mapping);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("NewKey", result.Value.SensitiveKey);
    }

    [Fact]
    public async Task AddAsync_WithDuplicateSafeKey_ReturnsFailure()
    {
        // Arrange
        var existingMapping = new KeywordMapping { SensitiveKey = "Existing", SafeKey = "DupSafe" };
        var newMapping = new KeywordMapping { SensitiveKey = "New", SafeKey = "DupSafe" };
        
        var json = System.Text.Json.JsonSerializer.Serialize(new[] { existingMapping });
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>(json));
                _validationService.Validate(newMapping, Arg.Any<IReadOnlyList<KeywordMapping>>())
            .Returns(ValidationResult<KeywordMapping>.Failure("SafeKey 已存在"));

        // Act
        var result = await _sut.AddAsync(newMapping);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("SafeKey", result.ErrorMessage);
    }

    [Fact]
    public async Task AddAsync_WhenAtMaxLimit_ReturnsFailure()
    {
        // Arrange
        var mappings = Enumerable.Range(1, 500)
            .Select(i => new KeywordMapping { SensitiveKey = $"Key{i}", SafeKey = $"Safe{i}" })
            .ToList();
        var json = System.Text.Json.JsonSerializer.Serialize(mappings);
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>(json));

        var newMapping = new KeywordMapping { SensitiveKey = "New", SafeKey = "NewSafe" };
                _validationService.Validate(newMapping, Arg.Any<IReadOnlyList<KeywordMapping>>())
            .Returns(ValidationResult<KeywordMapping>.Failure("已達到關鍵字上限 (500)"));

        // Act
        var result = await _sut.AddAsync(newMapping);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("上限", result.ErrorMessage);
    }

    [Fact]
    public async Task AddAsync_FiresOnDictionaryChangedEvent()
    {
        // Arrange
        var mapping = new KeywordMapping { SensitiveKey = "Key", SafeKey = "Safe" };
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));
        _validationService.Validate(mapping, Arg.Any<IReadOnlyList<KeywordMapping>>())
            .Returns(ValidationResult<KeywordMapping>.Success(mapping));

        var eventFired = false;
        _sut.OnDictionaryChanged += () => eventFired = true;

        // Act
        await _sut.AddAsync(mapping);

        // Assert
        Assert.True(eventFired);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidMapping_ReturnsSuccess()
    {
        // Arrange
        var mapping = new KeywordMapping { SensitiveKey = "Original", SafeKey = "SafeOriginal" };
        var json = System.Text.Json.JsonSerializer.Serialize(new[] { mapping });
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>(json));

        mapping.SensitiveKey = "Updated";
        _validationService.Validate(mapping, Arg.Any<IReadOnlyList<KeywordMapping>>())
            .Returns(ValidationResult<KeywordMapping>.Success(mapping));

        // Act
        var result = await _sut.UpdateAsync(mapping);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Updated", result.Value?.SensitiveKey);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ReturnsFailure()
    {
        // Arrange
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        var mapping = new KeywordMapping { SensitiveKey = "Test", SafeKey = "Safe" };

        // Act
        var result = await _sut.UpdateAsync(mapping);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("找不到", result.ErrorMessage);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var mapping = new KeywordMapping { SensitiveKey = "ToDelete", SafeKey = "SafeDelete" };
        var json = System.Text.Json.JsonSerializer.Serialize(new[] { mapping });
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>(json));

        // Act
        var result = await _sut.DeleteAsync(mapping.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var result = await _sut.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_FiresOnDictionaryChangedEvent()
    {
        // Arrange
        var mapping = new KeywordMapping { SensitiveKey = "Key", SafeKey = "Safe" };
        var json = System.Text.Json.JsonSerializer.Serialize(new[] { mapping });
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>(json));

        var eventFired = false;
        _sut.OnDictionaryChanged += () => eventFired = true;

        // Act
        await _sut.DeleteAsync(mapping.Id);

        // Assert
        Assert.True(eventFired);
    }

    #endregion

    #region ClearAllAsync Tests

    [Fact]
    public async Task ClearAllAsync_RemovesAllMappings()
    {
        // Arrange
        var mappings = new List<KeywordMapping>
        {
            new() { SensitiveKey = "A", SafeKey = "SafeA" },
            new() { SensitiveKey = "B", SafeKey = "SafeB" }
        };
        var json = System.Text.Json.JsonSerializer.Serialize(mappings);
        
        var callCount = 0;
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(_ =>
            {
                callCount++;
                return new ValueTask<string?>(callCount == 1 ? json : "[]");
            });

        // Act
        await _sut.ClearAllAsync();
        var result = await _sut.GetAllAsync();

        // Assert
        await _jsRuntime.Received().InvokeVoidAsync("localStorage.removeItem", Arg.Any<object[]>());
    }

    #endregion

    #region GetNextDefaultColor Tests

    [Fact]
    public void GetNextDefaultColor_ReturnsValidHexColor()
    {
        // Act
        var color = _sut.GetNextDefaultColor();

        // Assert
        Assert.NotNull(color);
        Assert.StartsWith("#", color);
        Assert.Equal(7, color.Length); // #RRGGBB
    }

    [Fact]
    public void GetNextDefaultColor_CyclesThroughColors()
    {
        // Act
        var colors = new HashSet<string>();
        for (int i = 0; i < 20; i++)
        {
            colors.Add(_sut.GetNextDefaultColor());
        }

        // Assert - should have multiple unique colors (cycled through palette)
        Assert.True(colors.Count >= 2);
    }

    #endregion

    #region GetCountAsync Tests

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var mappings = new List<KeywordMapping>
        {
            new() { SensitiveKey = "A", SafeKey = "SafeA" },
            new() { SensitiveKey = "B", SafeKey = "SafeB" },
            new() { SensitiveKey = "C", SafeKey = "SafeC" }
        };
        var json = System.Text.Json.JsonSerializer.Serialize(mappings);
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>(json));

        // Act
        var count = await _sut.GetCountAsync();

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task GetCountAsync_WhenEmpty_ReturnsZero()
    {
        // Arrange
        _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var count = await _sut.GetCountAsync();

        // Assert
        Assert.Equal(0, count);
    }

    #endregion
}
