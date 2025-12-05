using CovenantPromptKey.Models.Bible;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// 聖經設定服務實作
/// </summary>
public class BibleSettingsService : IBibleSettingsService
{
    private const string StorageKey = "bible_settings";
    private readonly ILocalStorageService _localStorageService;

    public BibleSettingsService(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    /// <inheritdoc />
    public async Task<BibleSettings> LoadSettingsAsync()
    {
        var settings = await _localStorageService.GetItemAsync<BibleSettings>(StorageKey);
        return settings ?? GetDefaultSettings();
    }

    /// <inheritdoc />
    public async Task SaveSettingsAsync(BibleSettings settings)
    {
        // 驗證字體大小範圍
        if (settings.FontSize < 12)
            settings.FontSize = 12;
        else if (settings.FontSize > 24)
            settings.FontSize = 24;

        await _localStorageService.SetItemAsync(StorageKey, settings);
    }

    /// <inheritdoc />
    public async Task<BibleSettings> ResetToDefaultAsync()
    {
        var defaultSettings = GetDefaultSettings();
        await SaveSettingsAsync(defaultSettings);
        return defaultSettings;
    }

    /// <inheritdoc />
    public BibleSettings GetDefaultSettings()
    {
        return new BibleSettings
        {
            FontFamily = FontFamily.MicrosoftJhengHei,
            FontSize = 16,
            TextColor = TextColor.Black,
            BackgroundColor = BackgroundColor.White,
            AutoWrap = true
        };
    }
}
