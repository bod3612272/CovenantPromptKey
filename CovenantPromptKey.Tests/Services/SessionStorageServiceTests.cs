using System.Text.Json;
using CovenantPromptKey.Browser.Implementations;
using CovenantPromptKey.Services.Interfaces;
using Microsoft.JSInterop;
using NSubstitute;

namespace CovenantPromptKey.Tests.Services;

/// <summary>
/// Unit tests for SessionStorageService
/// TDD: These tests are written first and should FAIL until implementation is complete
/// </summary>
public class SessionStorageServiceTests
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ISessionStorageService _service;

    public SessionStorageServiceTests()
    {
        _jsRuntime = Substitute.For<IJSRuntime>();
        _service = new SessionStorageService(_jsRuntime);
    }

    #region GetItemAsync Tests

    [Fact]
    public async Task GetItemAsync_ReturnsStoredValue()
    {
        // Arrange
        const string key = "testKey";
        var expectedValue = new TestData { Name = "Test", Value = 42 };
        var jsonValue = JsonSerializer.Serialize(expectedValue);
        
        _jsRuntime.InvokeAsync<string?>("CovenantPromptKey.storage.session.getItem", Arg.Is<object[]>(args => args.Length == 1 && args[0].ToString() == key))
            .Returns(new ValueTask<string?>(jsonValue));

        // Act
        var result = await _service.GetItemAsync<TestData>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedValue.Name, result.Name);
        Assert.Equal(expectedValue.Value, result.Value);
    }

    [Fact]
    public async Task GetItemAsync_ReturnsNull_WhenKeyNotExists()
    {
        // Arrange
        const string key = "nonExistentKey";
        
        _jsRuntime.InvokeAsync<string?>("CovenantPromptKey.storage.session.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var result = await _service.GetItemAsync<TestData>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetItemAsync_ReturnsNull_WhenJsonInvalid()
    {
        // Arrange
        const string key = "invalidJsonKey";
        
        _jsRuntime.InvokeAsync<string?>("CovenantPromptKey.storage.session.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>("invalid json {"));

        // Act
        var result = await _service.GetItemAsync<TestData>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetItemAsync_WorksWithPrimitiveTypes()
    {
        // Arrange
        const string key = "stringKey";
        const string expectedValue = "Hello World";
        var jsonValue = JsonSerializer.Serialize(expectedValue);
        
        _jsRuntime.InvokeAsync<string?>("CovenantPromptKey.storage.session.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>(jsonValue));

        // Act
        var result = await _service.GetItemAsync<string>(key);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    #endregion

    #region SetItemAsync Tests

    [Fact]
    public async Task SetItemAsync_StoresValueAsJson()
    {
        // Arrange
        const string key = "testKey";
        var value = new TestData { Name = "Test", Value = 42 };

        // Act
        await _service.SetItemAsync(key, value);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "CovenantPromptKey.storage.session.setItem",
            Arg.Is<object[]>(args => 
                args.Length == 2 && 
                args[0].ToString() == key));
    }

    [Fact]
    public async Task SetItemAsync_WorksWithNullValue()
    {
        // Arrange
        const string key = "nullKey";

        // Act
        await _service.SetItemAsync<TestData?>(key, null);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "CovenantPromptKey.storage.session.setItem",
            Arg.Any<object[]>());
    }

    #endregion

    #region RemoveItemAsync Tests

    [Fact]
    public async Task RemoveItemAsync_RemovesItem()
    {
        // Arrange
        const string key = "testKey";

        // Act
        await _service.RemoveItemAsync(key);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "CovenantPromptKey.storage.session.removeItem", 
            Arg.Any<object[]>());
    }

    #endregion

    #region ClearAsync Tests

    [Fact]
    public async Task ClearAsync_ClearsAllItems()
    {
        // Act
        await _service.ClearAsync();

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "CovenantPromptKey.storage.session.clear",
            Arg.Any<object[]>());
    }

    #endregion

    #region Test Helpers

    private class TestData
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    #endregion
}
