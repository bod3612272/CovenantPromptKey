using System.Text.Json;
using CovenantPromptKey.Constants;
using CovenantPromptKey.Models;
using CovenantPromptKey.Models.Results;
using CovenantPromptKey.Services.Interfaces;
using Microsoft.JSInterop;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// 關鍵字字典管理服務實作
/// 使用 localStorage 進行持久化儲存
/// </summary>
public class DictionaryService : IDictionaryService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IKeywordValidationService _validationService;
    private readonly IDebugLogService _logService;
    private List<KeywordMapping>? _cache;
    private int _colorIndex;
    private bool _isInitialized;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public event Action? OnDictionaryChanged;

    public DictionaryService(
        IJSRuntime jsRuntime,
        IKeywordValidationService validationService,
        IDebugLogService logService)
    {
        _jsRuntime = jsRuntime;
        _validationService = validationService;
        _logService = logService;
    }

    public async Task<IReadOnlyList<KeywordMapping>> GetAllAsync()
    {
        await EnsureInitializedAsync();
        return _cache!.AsReadOnly();
    }

    public async Task<KeywordMapping?> GetByIdAsync(Guid id)
    {
        await EnsureInitializedAsync();
        return _cache!.FirstOrDefault(m => m.Id == id);
    }

    public async Task<ValidationResult<KeywordMapping>> AddAsync(KeywordMapping mapping)
    {
        await EnsureInitializedAsync();

        // Check limit
        if (_cache!.Count >= AppConstants.MaxKeywordCount)
        {
            var errorMsg = $"已達到關鍵字上限 ({AppConstants.MaxKeywordCount})";
            _logService.Log(errorMsg, Models.LogLevel.Warning);
            return ValidationResult<KeywordMapping>.Failure(errorMsg);
        }

        // Validate mapping
        var validation = _validationService.Validate(mapping, _cache);
        if (!validation.IsSuccess)
        {
            _logService.Log($"新增關鍵字驗證失敗: {validation.ErrorMessage}", Models.LogLevel.Warning);
            return validation;
        }

        // Assign default color if not set
        if (string.IsNullOrEmpty(mapping.HighlightColor))
        {
            mapping.HighlightColor = GetNextDefaultColor();
        }

        _cache.Add(mapping);
        await SaveToCacheAsync();
        
        _logService.Log($"新增關鍵字: {mapping.SensitiveKey} -> {mapping.SafeKey}", Models.LogLevel.Info);
        OnDictionaryChanged?.Invoke();
        
        return ValidationResult<KeywordMapping>.Success(mapping);
    }

    public async Task<ValidationResult<KeywordMapping>> UpdateAsync(KeywordMapping mapping)
    {
        await EnsureInitializedAsync();

        var existingIndex = _cache!.FindIndex(m => m.Id == mapping.Id);
        if (existingIndex < 0)
        {
            var errorMsg = $"找不到 ID 為 {mapping.Id} 的關鍵字映射";
            _logService.Log(errorMsg, Models.LogLevel.Warning);
            return ValidationResult<KeywordMapping>.Failure(errorMsg);
        }

        // Validate mapping (exclude self from duplicate check)
        var othersForValidation = _cache.Where(m => m.Id != mapping.Id).ToList();
        var validation = _validationService.Validate(mapping, othersForValidation);
        if (!validation.IsSuccess)
        {
            _logService.Log($"更新關鍵字驗證失敗: {validation.ErrorMessage}", Models.LogLevel.Warning);
            return validation;
        }

        mapping.UpdatedAt = DateTime.UtcNow;
        _cache[existingIndex] = mapping;
        await SaveToCacheAsync();
        
        _logService.Log($"更新關鍵字: {mapping.SensitiveKey} -> {mapping.SafeKey}", Models.LogLevel.Info);
        OnDictionaryChanged?.Invoke();
        
        return ValidationResult<KeywordMapping>.Success(mapping);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await EnsureInitializedAsync();

        if (_cache == null) return false;
        
        var mapping = _cache.FirstOrDefault(m => m.Id == id);
        if (mapping == null)
        {
            _logService.Log($"刪除失敗: 找不到 ID 為 {id} 的關鍵字映射", Models.LogLevel.Warning);
            return false;
        }

        _cache.Remove(mapping);
        await SaveToCacheAsync();
        
        _logService.Log($"刪除關鍵字: {mapping.SensitiveKey}", Models.LogLevel.Info);
        OnDictionaryChanged?.Invoke();
        
        return true;
    }

    public async Task ClearAllAsync()
    {
        await EnsureInitializedAsync();

        if (_cache == null) return;
        
        var count = _cache.Count;
        _cache.Clear();
        
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AppConstants.StorageKeys.KeywordDictionary);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("prerendering") || ex.Message.Contains("statically"))
        {
            // 在預渲染階段無法使用 JS interop，靜默忽略
        }
        catch (Exception ex)
        {
            _logService.Log($"清空字典時發生錯誤: {ex.Message}", Models.LogLevel.Error);
        }
        
        _logService.Log($"已清空所有關鍵字 (共 {count} 筆)", Models.LogLevel.Info);
        OnDictionaryChanged?.Invoke();
    }

    public string GetNextDefaultColor()
    {
        var color = AppConstants.DefaultColors[_colorIndex % AppConstants.DefaultColors.Length];
        _colorIndex++;
        return color;
    }

    public async Task<int> GetCountAsync()
    {
        await EnsureInitializedAsync();
        return _cache!.Count;
    }

    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized && _cache != null)
            return;

        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>(
                "localStorage.getItem", 
                AppConstants.StorageKeys.KeywordDictionary);

            if (string.IsNullOrEmpty(json))
            {
                _cache = new List<KeywordMapping>();
            }
            else
            {
                _cache = JsonSerializer.Deserialize<List<KeywordMapping>>(json, JsonOptions) 
                         ?? new List<KeywordMapping>();
            }

            _isInitialized = true;
            _logService.Log($"字典服務初始化完成，載入 {_cache.Count} 筆關鍵字", Models.LogLevel.Debug);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("prerendering") || ex.Message.Contains("statically"))
        {
            // 在預渲染階段無法使用 JS interop，使用空快取
            _cache ??= new List<KeywordMapping>();
            // 不設定 _isInitialized = true，讓下次呼叫時重試
            _logService.Log("在預渲染階段無法載入字典，將在客戶端渲染後重試", Models.LogLevel.Debug);
        }
        catch (Exception ex)
        {
            _logService.Log($"載入字典時發生錯誤: {ex.Message}", Models.LogLevel.Error);
            _cache = new List<KeywordMapping>();
            _isInitialized = true;
        }
    }

    private async Task SaveToCacheAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_cache, JsonOptions);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", AppConstants.StorageKeys.KeywordDictionary, json);
            _logService.Log($"字典已儲存 ({_cache!.Count} 筆)", Models.LogLevel.Debug);
        }
        catch (Exception ex)
        {
            _logService.Log($"儲存字典時發生錯誤: {ex.Message}", Models.LogLevel.Error);
            throw;
        }
    }
}
