using CovenantPromptKey.Models.Bible;
using CovenantPromptKey.Services.Implementations;
using CovenantPromptKey.Services.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace CovenantPromptKey.NUnitTests.Services;

/// <summary>
/// BibleSettingsService 單元測試
/// </summary>
[TestFixture]
public class BibleSettingsServiceTests
{
    private ILocalStorageService _localStorageService = null!;
    private BibleSettingsService _sut = null!;

    [SetUp]
    public void Setup()
    {
        _localStorageService = Substitute.For<ILocalStorageService>();
        _sut = new BibleSettingsService(_localStorageService);
    }

    [Test]
    public async Task LoadSettingsAsync_WhenNoStoredSettings_ShouldReturnDefaultSettings()
    {
        // Arrange
        _localStorageService.GetItemAsync<BibleSettings>("bible_settings")
            .Returns((BibleSettings?)null);

        // Act
        var result = await _sut.LoadSettingsAsync();

        // Assert
        result.Should().NotBeNull();
        result.FontFamily.Should().Be(FontFamily.MicrosoftJhengHei);
        result.FontSize.Should().Be(16);
        result.TextColor.Should().Be(TextColor.Black);
        result.BackgroundColor.Should().Be(BackgroundColor.White);
        result.AutoWrap.Should().BeTrue();
    }

    [Test]
    public async Task LoadSettingsAsync_WhenStoredSettingsExist_ShouldReturnStoredSettings()
    {
        // Arrange
        var storedSettings = new BibleSettings
        {
            FontFamily = FontFamily.DFKai,
            FontSize = 20,
            TextColor = TextColor.DarkGray,
            BackgroundColor = BackgroundColor.Beige,
            AutoWrap = false
        };
        _localStorageService.GetItemAsync<BibleSettings>("bible_settings")
            .Returns(storedSettings);

        // Act
        var result = await _sut.LoadSettingsAsync();

        // Assert
        result.Should().BeEquivalentTo(storedSettings);
    }

    [Test]
    public async Task SaveSettingsAsync_ShouldCallLocalStorageSetItem()
    {
        // Arrange
        var settings = new BibleSettings { FontSize = 18 };

        // Act
        await _sut.SaveSettingsAsync(settings);

        // Assert
        await _localStorageService.Received(1)
            .SetItemAsync("bible_settings", Arg.Is<BibleSettings>(s => s.FontSize == 18));
    }

    [Test]
    public async Task SaveSettingsAsync_WhenFontSizeTooSmall_ShouldClampToMinimum()
    {
        // Arrange
        var settings = new BibleSettings { FontSize = 8 };

        // Act
        await _sut.SaveSettingsAsync(settings);

        // Assert
        await _localStorageService.Received(1)
            .SetItemAsync("bible_settings", Arg.Is<BibleSettings>(s => s.FontSize == 12));
    }

    [Test]
    public async Task SaveSettingsAsync_WhenFontSizeTooLarge_ShouldClampToMaximum()
    {
        // Arrange
        var settings = new BibleSettings { FontSize = 30 };

        // Act
        await _sut.SaveSettingsAsync(settings);

        // Assert
        await _localStorageService.Received(1)
            .SetItemAsync("bible_settings", Arg.Is<BibleSettings>(s => s.FontSize == 24));
    }

    [Test]
    public async Task ResetToDefaultAsync_ShouldSaveAndReturnDefaultSettings()
    {
        // Act
        var result = await _sut.ResetToDefaultAsync();

        // Assert
        result.FontFamily.Should().Be(FontFamily.MicrosoftJhengHei);
        result.FontSize.Should().Be(16);
        await _localStorageService.Received(1)
            .SetItemAsync("bible_settings", Arg.Any<BibleSettings>());
    }

    [Test]
    public void GetDefaultSettings_ShouldReturnCorrectDefaults()
    {
        // Act
        var result = _sut.GetDefaultSettings();

        // Assert
        result.FontFamily.Should().Be(FontFamily.MicrosoftJhengHei);
        result.FontSize.Should().Be(16);
        result.TextColor.Should().Be(TextColor.Black);
        result.BackgroundColor.Should().Be(BackgroundColor.White);
        result.AutoWrap.Should().BeTrue();
    }
}
