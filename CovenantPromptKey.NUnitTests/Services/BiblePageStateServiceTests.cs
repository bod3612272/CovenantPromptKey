using CovenantPromptKey.Models.Bible;
using CovenantPromptKey.Services.Implementations;
using CovenantPromptKey.Services.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace CovenantPromptKey.NUnitTests.Services;

/// <summary>
/// BiblePageStateService 單元測試
/// </summary>
[TestFixture]
public class BiblePageStateServiceTests
{
    private ISessionStorageService _sessionStorageService = null!;
    private BiblePageStateService _sut = null!;

    [SetUp]
    public void Setup()
    {
        _sessionStorageService = Substitute.For<ISessionStorageService>();
        _sut = new BiblePageStateService(_sessionStorageService);
    }

    #region Search Page State Tests

    [Test]
    public async Task LoadSearchPageStateAsync_WhenNoState_ShouldReturnNewState()
    {
        // Arrange
        _sessionStorageService.GetItemAsync<BibleSearchPageState>("bible_search_state")
            .Returns((BibleSearchPageState?)null);

        // Act
        var result = await _sut.LoadSearchPageStateAsync();

        // Assert
        result.Should().NotBeNull();
        result.Keyword.Should().BeEmpty();
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Test]
    public async Task LoadSearchPageStateAsync_WhenStateExists_ShouldReturnStoredState()
    {
        // Arrange
        var storedState = new BibleSearchPageState
        {
            Keyword = "愛",
            CurrentPage = 3,
            PageSize = 30
        };
        _sessionStorageService.GetItemAsync<BibleSearchPageState>("bible_search_state")
            .Returns(storedState);

        // Act
        var result = await _sut.LoadSearchPageStateAsync();

        // Assert
        result.Should().BeEquivalentTo(storedState);
    }

    [Test]
    public async Task SaveSearchPageStateAsync_ShouldCallSessionStorageSetItem()
    {
        // Arrange
        var state = new BibleSearchPageState { Keyword = "信心" };

        // Act
        await _sut.SaveSearchPageStateAsync(state);

        // Assert
        await _sessionStorageService.Received(1)
            .SetItemAsync("bible_search_state", Arg.Is<BibleSearchPageState>(s => s.Keyword == "信心"));
    }

    #endregion

    #region Read Page State Tests

    [Test]
    public async Task LoadReadPageStateAsync_WhenNoState_ShouldReturnNewState()
    {
        // Arrange
        _sessionStorageService.GetItemAsync<BibleReadPageState>("bible_read_state")
            .Returns((BibleReadPageState?)null);

        // Act
        var result = await _sut.LoadReadPageStateAsync();

        // Assert
        result.Should().NotBeNull();
        result.BookNumber.Should().Be(1);
        result.ChapterNumber.Should().Be(1);
    }

    [Test]
    public async Task LoadReadPageStateAsync_WhenStateExists_ShouldReturnStoredState()
    {
        // Arrange
        var storedState = new BibleReadPageState
        {
            BookNumber = 43, // John
            ChapterNumber = 3
        };
        _sessionStorageService.GetItemAsync<BibleReadPageState>("bible_read_state")
            .Returns(storedState);

        // Act
        var result = await _sut.LoadReadPageStateAsync();

        // Assert
        result.Should().BeEquivalentTo(storedState);
    }

    [Test]
    public async Task SaveReadPageStateAsync_ShouldCallSessionStorageSetItem()
    {
        // Arrange
        var state = new BibleReadPageState { BookNumber = 1, ChapterNumber = 2 };

        // Act
        await _sut.SaveReadPageStateAsync(state);

        // Assert
        await _sessionStorageService.Received(1)
            .SetItemAsync("bible_read_state", Arg.Is<BibleReadPageState>(s => s.BookNumber == 1 && s.ChapterNumber == 2));
    }

    #endregion

    #region Game Page State Tests

    [Test]
    public async Task LoadGamePageStateAsync_WhenNoState_ShouldReturnNewState()
    {
        // Arrange
        _sessionStorageService.GetItemAsync<BibleGamePageState>("bible_game_state")
            .Returns((BibleGamePageState?)null);

        // Act
        var result = await _sut.LoadGamePageStateAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsGameInProgress.Should().BeFalse();
        result.CurrentQuestionNumber.Should().Be(0);
        result.CurrentScore.Should().Be(0);
    }

    [Test]
    public async Task LoadGamePageStateAsync_WhenStateExists_ShouldReturnStoredState()
    {
        // Arrange
        var storedState = new BibleGamePageState
        {
            IsGameInProgress = true,
            CurrentQuestionNumber = 5,
            CurrentScore = 3
        };
        _sessionStorageService.GetItemAsync<BibleGamePageState>("bible_game_state")
            .Returns(storedState);

        // Act
        var result = await _sut.LoadGamePageStateAsync();

        // Assert
        result.Should().BeEquivalentTo(storedState);
    }

    [Test]
    public async Task SaveGamePageStateAsync_ShouldCallSessionStorageSetItem()
    {
        // Arrange
        var state = new BibleGamePageState { IsGameInProgress = true, CurrentScore = 7 };

        // Act
        await _sut.SaveGamePageStateAsync(state);

        // Assert
        await _sessionStorageService.Received(1)
            .SetItemAsync("bible_game_state", Arg.Is<BibleGamePageState>(s => s.IsGameInProgress && s.CurrentScore == 7));
    }

    #endregion

    #region Clear All States Tests

    [Test]
    public async Task ClearAllStatesAsync_ShouldRemoveAllStates()
    {
        // Act
        await _sut.ClearAllStatesAsync();

        // Assert
        await _sessionStorageService.Received(1).RemoveItemAsync("bible_search_state");
        await _sessionStorageService.Received(1).RemoveItemAsync("bible_read_state");
        await _sessionStorageService.Received(1).RemoveItemAsync("bible_game_state");
    }

    #endregion
}
