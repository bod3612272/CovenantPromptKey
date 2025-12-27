using System.Text.Json;
using CovenantPromptKey.Services.Interfaces;
using Microsoft.JSInterop;

namespace CovenantPromptKey.Browser.Implementations;

/// <summary>
/// 瀏覽器 SessionStorage 服務實作
/// 透過 JS Interop 與瀏覽器 sessionStorage API 互動
///
/// Cross-host note:
/// - Server host 在 prerender / 未互動階段不能呼叫 JS interop。
/// - 因此本 service 以「interactive ready gating」模式：若 JS 尚不可用，讀取回傳 default，寫入/刪除則靜默忽略。
/// </summary>
public class SessionStorageService : ISessionStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public SessionStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("CovenantPromptKey.storage.session.getItem", key);

            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json);
        }
        catch (InvalidOperationException ex) when (IsJsInteropUnavailable(ex))
        {
            return default;
        }
        catch (JSDisconnectedException)
        {
            return default;
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _jsRuntime.InvokeVoidAsync("CovenantPromptKey.storage.session.setItem", key, json);
        }
        catch (InvalidOperationException ex) when (IsJsInteropUnavailable(ex))
        {
        }
        catch (JSDisconnectedException)
        {
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("CovenantPromptKey.storage.session.removeItem", key);
        }
        catch (InvalidOperationException ex) when (IsJsInteropUnavailable(ex))
        {
        }
        catch (JSDisconnectedException)
        {
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("CovenantPromptKey.storage.session.clear");
        }
        catch (InvalidOperationException ex) when (IsJsInteropUnavailable(ex))
        {
        }
        catch (JSDisconnectedException)
        {
        }
    }

    private static bool IsJsInteropUnavailable(InvalidOperationException ex)
    {
        var message = ex.Message ?? string.Empty;

        return message.Contains("prerender", StringComparison.OrdinalIgnoreCase)
               || message.Contains("statically", StringComparison.OrdinalIgnoreCase)
               || message.Contains("JavaScript interop", StringComparison.OrdinalIgnoreCase);
    }
}
