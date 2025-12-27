using CovenantPromptKey.Constants;
using CovenantPromptKey.Models;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// 工作階段管理服務實作
/// 管理當前工作階段狀態，透過 SessionStorage 持久化
/// </summary>
public class WorkSessionService : IWorkSessionService
{
    private readonly ISessionStorageService _sessionStorage;
    private readonly IDebugLogService _debugLogService;
    private WorkSession? _currentSession;

    public WorkSessionService(
        ISessionStorageService sessionStorage,
        IDebugLogService debugLogService)
    {
        _sessionStorage = sessionStorage;
        _debugLogService = debugLogService;
    }

    /// <inheritdoc />
    public event Action? OnSessionChanged;

    /// <inheritdoc />
    public async Task<WorkSession> GetCurrentSessionAsync()
    {
        if (_currentSession != null)
        {
            return _currentSession;
        }

        // Try to restore from SessionStorage
        _currentSession = await _sessionStorage.GetItemAsync<WorkSession>(
            AppConstants.StorageKeys.WorkSession);

        if (_currentSession == null)
        {
            _currentSession = new WorkSession();
            _debugLogService.Log("建立新工作階段", Models.LogLevel.Debug, nameof(WorkSessionService));
        }
        else
        {
            _debugLogService.Log("從 SessionStorage 還原工作階段", Models.LogLevel.Debug, nameof(WorkSessionService));
        }

        return _currentSession;
    }

    /// <inheritdoc />
    public async Task SaveSessionAsync(WorkSession session)
    {
        _currentSession = session;
        _currentSession.LastUpdated = DateTime.UtcNow;

        await _sessionStorage.SetItemAsync(
            AppConstants.StorageKeys.WorkSession,
            _currentSession);

        _debugLogService.Log("工作階段已儲存", Models.LogLevel.Debug, nameof(WorkSessionService));
        OnSessionChanged?.Invoke();
    }

    /// <inheritdoc />
    public async Task ResetSessionAsync()
    {
        _currentSession = new WorkSession();

        await _sessionStorage.SetItemAsync(
            AppConstants.StorageKeys.WorkSession,
            _currentSession);

        _debugLogService.Log("工作階段已重置", Models.LogLevel.Info, nameof(WorkSessionService));
        OnSessionChanged?.Invoke();
    }

    /// <inheritdoc />
    public async Task UpdateSourceTextAsync(string text)
    {
        var session = await GetCurrentSessionAsync();
        session.SourceText = text;
        session.DetectedKeywords = []; // Clear detection results when source changes
        session.ResultText = string.Empty;
        await SaveSessionAsync(session);
    }

    /// <inheritdoc />
    public async Task UpdateDetectedKeywordsAsync(List<DetectedKeyword> keywords)
    {
        var session = await GetCurrentSessionAsync();
        session.DetectedKeywords = keywords;
        await SaveSessionAsync(session);
    }

    /// <inheritdoc />
    public async Task UpdateResultTextAsync(string text)
    {
        var session = await GetCurrentSessionAsync();
        session.ResultText = text;
        await SaveSessionAsync(session);
    }

    /// <inheritdoc />
    public async Task SetModeAsync(WorkMode mode)
    {
        var session = await GetCurrentSessionAsync();
        session.Mode = mode;
        await SaveSessionAsync(session);

        _debugLogService.Log($"工作模式切換為: {mode}", Models.LogLevel.Info, nameof(WorkSessionService));
    }
}
