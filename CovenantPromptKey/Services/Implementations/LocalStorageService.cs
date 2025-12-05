using System.Text.Json;
using CovenantPromptKey.Services.Interfaces;
using Microsoft.JSInterop;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// 瀏覽器 LocalStorage 服務實作
/// 透過 JS Interop 與瀏覽器 localStorage API 互動
/// </summary>
public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <inheritdoc />
    public async Task<T?> GetItemAsync<T>(string key)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("prerendering") || ex.Message.Contains("statically"))
        {
            // 在預渲染階段無法使用 JS interop
            return default;
        }
        catch (JsonException)
        {
            // Return default if JSON is invalid
            return default;
        }
    }

    /// <inheritdoc />
    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("prerendering") || ex.Message.Contains("statically"))
        {
            // 在預渲染階段無法使用 JS interop，靜默忽略
        }
    }

    /// <inheritdoc />
    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("prerendering") || ex.Message.Contains("statically"))
        {
            // 在預渲染階段無法使用 JS interop，靜默忽略
        }
    }

    /// <inheritdoc />
    public async Task ClearAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.clear");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("prerendering") || ex.Message.Contains("statically"))
        {
            // 在預渲染階段無法使用 JS interop，靜默忽略
        }
    }
}
