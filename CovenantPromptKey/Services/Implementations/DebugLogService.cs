using System.Text;
using CovenantPromptKey.Constants;
using CovenantPromptKey.Models;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// 除錯日誌服務實作
/// 使用環形緩衝區限制最大日誌數量
/// </summary>
public class DebugLogService : IDebugLogService
{
    private readonly List<LogEntry> _logs = [];
    private readonly object _lock = new();

    /// <inheritdoc />
    public event Action? OnLogAdded;

    /// <inheritdoc />
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _logs.Count;
            }
        }
    }

    /// <inheritdoc />
    public void Log(string message, Models.LogLevel level = Models.LogLevel.Info, string? source = null)
    {
        var entry = new LogEntry
        {
            Message = message,
            Level = level,
            Source = source,
            Timestamp = DateTime.Now
        };

        lock (_lock)
        {
            _logs.Add(entry);

            // Ring buffer: remove oldest entries if exceeding max
            while (_logs.Count > AppConstants.MaxLogEntries)
            {
                _logs.RemoveAt(0);
            }
        }

        OnLogAdded?.Invoke();
    }

    /// <inheritdoc />
    public IReadOnlyList<LogEntry> GetLogs()
    {
        lock (_lock)
        {
            // Return newest first (reverse order)
            return _logs.AsEnumerable().Reverse().ToList().AsReadOnly();
        }
    }

    /// <inheritdoc />
    public string GetFormattedLogs()
    {
        lock (_lock)
        {
            if (_logs.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var log in _logs.AsEnumerable().Reverse())
            {
                var source = string.IsNullOrEmpty(log.Source) ? "" : $"[{log.Source}] ";
                var level = log.Level.ToString().ToUpperInvariant();
                sb.AppendLine($"[{log.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {source}{log.Message}");
            }
            return sb.ToString();
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        lock (_lock)
        {
            _logs.Clear();
        }
    }
}
