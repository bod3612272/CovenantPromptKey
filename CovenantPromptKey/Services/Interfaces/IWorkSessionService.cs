using CovenantPromptKey.Models;

namespace CovenantPromptKey.Services.Interfaces;

/// <summary>
/// 工作階段管理服務介面
/// </summary>
public interface IWorkSessionService
{
    /// <summary>
    /// 當工作階段變更時觸發
    /// </summary>
    event Action? OnSessionChanged;

    /// <summary>
    /// 取得當前工作階段
    /// </summary>
    Task<WorkSession> GetCurrentSessionAsync();

    /// <summary>
    /// 儲存工作階段
    /// </summary>
    Task SaveSessionAsync(WorkSession session);

    /// <summary>
    /// 重置工作階段
    /// </summary>
    Task ResetSessionAsync();

    /// <summary>
    /// 更新來源文本
    /// </summary>
    Task UpdateSourceTextAsync(string text);

    /// <summary>
    /// 更新偵測結果
    /// </summary>
    Task UpdateDetectedKeywordsAsync(List<DetectedKeyword> keywords);

    /// <summary>
    /// 更新結果文本
    /// </summary>
    Task UpdateResultTextAsync(string text);

    /// <summary>
    /// 切換工作模式
    /// </summary>
    Task SetModeAsync(WorkMode mode);
}
