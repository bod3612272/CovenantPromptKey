using System.Text.Json;
using CovenantPromptKey.Services.Interfaces;
using Microsoft.JSInterop;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// 瀏覽器 SessionStorage 服務實作
/// 透過 JS Interop 與瀏覽器 sessionStorage API 互動
/// </summary>
public class SessionStorageService : ISessionStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public SessionStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <inheritdoc />
    public async Task<T?> GetItemAsync<T>(string key)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", key);
            
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json);
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
        var json = JsonSerializer.Serialize(value);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", key, json);
    }

    /// <inheritdoc />
    public async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", key);
    }

    /// <inheritdoc />
    public async Task ClearAsync()
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.clear");
    }
}
