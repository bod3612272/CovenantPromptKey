using CovenantPromptKey.Models.Bible;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// 聖經頁面狀態服務實作
/// </summary>
public class BiblePageStateService : IBiblePageStateService
{
    private const string SearchStateKey = "bible_search_state";
    private const string ReadStateKey = "bible_read_state";
    private const string GameStateKey = "bible_game_state";

    private readonly ISessionStorageService _sessionStorageService;

    public BiblePageStateService(ISessionStorageService sessionStorageService)
    {
        _sessionStorageService = sessionStorageService;
    }

    /// <inheritdoc />
    public async Task<BibleSearchPageState> LoadSearchPageStateAsync()
    {
        var state = await _sessionStorageService.GetItemAsync<BibleSearchPageState>(SearchStateKey);
        return state ?? new BibleSearchPageState();
    }

    /// <inheritdoc />
    public async Task SaveSearchPageStateAsync(BibleSearchPageState state)
    {
        await _sessionStorageService.SetItemAsync(SearchStateKey, state);
    }

    /// <inheritdoc />
    public async Task<BibleReadPageState> LoadReadPageStateAsync()
    {
        var state = await _sessionStorageService.GetItemAsync<BibleReadPageState>(ReadStateKey);
        return state ?? new BibleReadPageState();
    }

    /// <inheritdoc />
    public async Task SaveReadPageStateAsync(BibleReadPageState state)
    {
        await _sessionStorageService.SetItemAsync(ReadStateKey, state);
    }

    /// <inheritdoc />
    public async Task<BibleGamePageState> LoadGamePageStateAsync()
    {
        var state = await _sessionStorageService.GetItemAsync<BibleGamePageState>(GameStateKey);
        return state ?? new BibleGamePageState();
    }

    /// <inheritdoc />
    public async Task SaveGamePageStateAsync(BibleGamePageState state)
    {
        await _sessionStorageService.SetItemAsync(GameStateKey, state);
    }

    /// <inheritdoc />
    public async Task ClearAllStatesAsync()
    {
        await _sessionStorageService.RemoveItemAsync(SearchStateKey);
        await _sessionStorageService.RemoveItemAsync(ReadStateKey);
        await _sessionStorageService.RemoveItemAsync(GameStateKey);
    }
}
