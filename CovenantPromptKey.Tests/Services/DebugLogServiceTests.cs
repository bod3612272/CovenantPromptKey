using CovenantPromptKey.Constants;
using CovenantPromptKey.Models;
using CovenantPromptKey.Services.Implementations;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey.Tests.Services;

/// <summary>
/// Unit tests for DebugLogService
/// TDD: These tests are written first and should FAIL until implementation is complete
/// </summary>
public class DebugLogServiceTests
{
    private readonly IDebugLogService _service;

    public DebugLogServiceTests()
    {
        _service = new DebugLogService();
    }

    #region Log Method Tests

    [Fact]
    public void Log_WithMessage_AddsLogEntry()
    {
        // Arrange
        const string message = "Test log message";

        // Act
        _service.Log(message);

        // Assert
        var logs = _service.GetLogs();
        Assert.Single(logs);
        Assert.Equal(message, logs[0].Message);
        Assert.Equal(LogLevel.Info, logs[0].Level);
    }

    [Fact]
    public void Log_WithLevelAndSource_SetsPropertiesCorrectly()
    {
        // Arrange
        const string message = "Error occurred";
        const LogLevel level = LogLevel.Error;
        const string source = "KeywordService";

        // Act
        _service.Log(message, level, source);

        // Assert
        var logs = _service.GetLogs();
        Assert.Single(logs);
        Assert.Equal(message, logs[0].Message);
        Assert.Equal(level, logs[0].Level);
        Assert.Equal(source, logs[0].Source);
    }

    [Fact]
    public void Log_SetsTimestamp()
    {
        // Arrange
        var beforeLog = DateTime.Now.AddMilliseconds(-1);

        // Act
        _service.Log("Test");

        // Assert
        var logs = _service.GetLogs();
        Assert.True(logs[0].Timestamp >= beforeLog);
        Assert.True(logs[0].Timestamp <= DateTime.Now.AddMilliseconds(1));
    }

    [Fact]
    public void Log_TriggersOnLogAddedEvent()
    {
        // Arrange
        var eventTriggered = false;
        _service.OnLogAdded += () => eventTriggered = true;

        // Act
        _service.Log("Test");

        // Assert
        Assert.True(eventTriggered);
    }

    #endregion

    #region GetLogs Method Tests

    [Fact]
    public void GetLogs_ReturnsNewestFirst()
    {
        // Arrange
        _service.Log("First");
        _service.Log("Second");
        _service.Log("Third");

        // Act
        var logs = _service.GetLogs();

        // Assert
        Assert.Equal(3, logs.Count);
        Assert.Equal("Third", logs[0].Message);
        Assert.Equal("Second", logs[1].Message);
        Assert.Equal("First", logs[2].Message);
    }

    [Fact]
    public void GetLogs_ReturnsEmptyList_WhenNoLogs()
    {
        // Act
        var logs = _service.GetLogs();

        // Assert
        Assert.Empty(logs);
    }

    #endregion

    #region Ring Buffer Tests (MaxLogEntries)

    [Fact]
    public void Log_RespectsMaxLogEntries_RemovesOldestWhenFull()
    {
        // Arrange
        var maxEntries = AppConstants.MaxLogEntries;
        
        // Add max + 10 entries
        for (int i = 0; i < maxEntries + 10; i++)
        {
            _service.Log($"Message {i}");
        }

        // Act
        var logs = _service.GetLogs();

        // Assert
        Assert.Equal(maxEntries, logs.Count);
        // Newest entry (maxEntries + 9) should be first
        Assert.Equal($"Message {maxEntries + 9}", logs[0].Message);
        // Oldest remaining entry should be entry 10
        Assert.Equal($"Message 10", logs[^1].Message);
    }

    #endregion

    #region Clear Method Tests

    [Fact]
    public void Clear_RemovesAllLogs()
    {
        // Arrange
        _service.Log("Test 1");
        _service.Log("Test 2");

        // Act
        _service.Clear();

        // Assert
        Assert.Empty(_service.GetLogs());
        Assert.Equal(0, _service.Count);
    }

    #endregion

    #region Count Property Tests

    [Fact]
    public void Count_ReturnsCorrectCount()
    {
        // Arrange & Act
        _service.Log("Test 1");
        _service.Log("Test 2");
        _service.Log("Test 3");

        // Assert
        Assert.Equal(3, _service.Count);
    }

    [Fact]
    public void Count_ReturnsZero_WhenEmpty()
    {
        // Assert
        Assert.Equal(0, _service.Count);
    }

    #endregion

    #region GetFormattedLogs Tests

    [Fact]
    public void GetFormattedLogs_ReturnsFormattedString()
    {
        // Arrange
        _service.Log("Test message", LogLevel.Info, "TestSource");

        // Act
        var formatted = _service.GetFormattedLogs();

        // Assert
        Assert.Contains("Test message", formatted);
        Assert.Contains("INFO", formatted);
        Assert.Contains("TestSource", formatted);
    }

    [Fact]
    public void GetFormattedLogs_ReturnsEmptyString_WhenNoLogs()
    {
        // Act
        var formatted = _service.GetFormattedLogs();

        // Assert
        Assert.Equal(string.Empty, formatted);
    }

    [Fact]
    public void GetFormattedLogs_IncludesAllLogLevels()
    {
        // Arrange
        _service.Log("Debug message", LogLevel.Debug);
        _service.Log("Info message", LogLevel.Info);
        _service.Log("Warning message", LogLevel.Warning);
        _service.Log("Error message", LogLevel.Error);

        // Act
        var formatted = _service.GetFormattedLogs();

        // Assert
        Assert.Contains("DEBUG", formatted);
        Assert.Contains("INFO", formatted);
        Assert.Contains("WARNING", formatted);
        Assert.Contains("ERROR", formatted);
    }

    #endregion
}
