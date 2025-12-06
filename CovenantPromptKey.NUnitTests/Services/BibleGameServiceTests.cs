using BibleData;
using BibleData.Models;
using CovenantPromptKey.Models.Bible;
using CovenantPromptKey.Services.Implementations;
using CovenantPromptKey.Services.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace CovenantPromptKey.NUnitTests.Services;

/// <summary>
/// BibleGameService 單元測試
/// </summary>
[TestFixture]
public class BibleGameServiceTests
{
    private BibleIndex _bibleIndex = null!;
    private IBibleReadingService _readingService = null!;
    private ILocalStorageService _localStorageService = null!;
    private BibleGameService _sut = null!;

    [SetUp]
    public void Setup()
    {
        _bibleIndex = new BibleIndex();
        _readingService = new BibleReadingService(_bibleIndex, Substitute.For<IDebugLogService>());
        _localStorageService = Substitute.For<ILocalStorageService>();
        _sut = new BibleGameService(_bibleIndex, _readingService, _localStorageService);
    }

    [Test]
    public void StartNewGame_ShouldReturn10Questions()
    {
        // Act
        var questions = _sut.StartNewGame(10);

        // Assert
        questions.Should().HaveCount(10);
    }

    [Test]
    public void StartNewGame_EachQuestionShouldHave4Options()
    {
        // Act
        var questions = _sut.StartNewGame(10);

        // Assert
        foreach (var question in questions)
        {
            question.Options.Should().HaveCount(4);
        }
    }

    [Test]
    public void StartNewGame_CorrectAnswerShouldBeInOptions()
    {
        // Act
        var questions = _sut.StartNewGame(10);

        // Assert
        foreach (var question in questions)
        {
            question.Options.Should().Contain(question.CorrectAnswer);
        }
    }

    [Test]
    public void GenerateQuestion_ShouldReturnValidQuestion()
    {
        // Act
        var question = _sut.GenerateQuestion();

        // Assert
        question.Should().NotBeNull();
        question.Verse.Should().NotBeNull();
        question.Verse.Content.Should().NotBeEmpty();
        question.Verse.BookName.Should().NotBeEmpty();
        question.Options.Should().HaveCount(4);
    }

    [Test]
    public void CheckAnswer_WhenCorrect_ShouldReturnTrue()
    {
        // Arrange
        var question = _sut.GenerateQuestion();

        // Act
        var result = _sut.CheckAnswer(question, question.CorrectAnswer);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void CheckAnswer_WhenIncorrect_ShouldReturnFalse()
    {
        // Arrange
        var question = _sut.GenerateQuestion();
        var wrongAnswer = question.Options.First(o => o != question.CorrectAnswer);

        // Act
        var result = _sut.CheckAnswer(question, wrongAnswer);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task SaveGameResultAsync_ShouldStoreSession()
    {
        // Arrange
        var existingRecords = new BibleGameRecordCollection();
        _localStorageService.GetItemAsync<BibleGameRecordCollection>("bible_game_records")
            .Returns(existingRecords);

        var session = new BibleGameSession
        {
            Score = 8,
            TotalQuestions = 10,
            Answers = new List<BibleGameAnswer>()
        };

        // Act
        await _sut.SaveGameResultAsync(session);

        // Assert
        await _localStorageService.Received(1)
            .SetItemAsync("bible_game_records", Arg.Is<BibleGameRecordCollection>(r =>
                r.RecentSessions.Count == 1 &&
                r.RecentSessions[0].Score == 8));
    }

    [Test]
    public async Task SaveGameResultAsync_ShouldUpdateHighScore_WhenNewScoreIsHigher()
    {
        // Arrange
        var existingRecords = new BibleGameRecordCollection { HighScore = 5 };
        _localStorageService.GetItemAsync<BibleGameRecordCollection>("bible_game_records")
            .Returns(existingRecords);

        var session = new BibleGameSession
        {
            Score = 8,
            TotalQuestions = 10,
            Answers = new List<BibleGameAnswer>()
        };

        // Act
        await _sut.SaveGameResultAsync(session);

        // Assert
        await _localStorageService.Received(1)
            .SetItemAsync("bible_game_records", Arg.Is<BibleGameRecordCollection>(r =>
                r.HighScore == 8));
    }

    [Test]
    public async Task SaveGameResultAsync_ShouldNotUpdateHighScore_WhenNewScoreIsLower()
    {
        // Arrange
        var existingRecords = new BibleGameRecordCollection { HighScore = 10 };
        _localStorageService.GetItemAsync<BibleGameRecordCollection>("bible_game_records")
            .Returns(existingRecords);

        var session = new BibleGameSession
        {
            Score = 5,
            TotalQuestions = 10,
            Answers = new List<BibleGameAnswer>()
        };

        // Act
        await _sut.SaveGameResultAsync(session);

        // Assert
        await _localStorageService.Received(1)
            .SetItemAsync("bible_game_records", Arg.Is<BibleGameRecordCollection>(r =>
                r.HighScore == 10));
    }

    [Test]
    public async Task GetGameRecordsAsync_WhenNoRecords_ShouldReturnEmpty()
    {
        // Arrange
        _localStorageService.GetItemAsync<BibleGameRecordCollection>("bible_game_records")
            .Returns((BibleGameRecordCollection?)null);

        // Act
        var result = await _sut.GetGameRecordsAsync();

        // Assert
        result.Should().NotBeNull();
        result.HighScore.Should().Be(0);
        result.RecentSessions.Should().BeEmpty();
    }

    [Test]
    public async Task ClearGameRecordsAsync_ShouldRemoveFromStorage()
    {
        // Act
        await _sut.ClearGameRecordsAsync();

        // Assert
        await _localStorageService.Received(1).RemoveItemAsync("bible_game_records");
    }

    [Test]
    public async Task SaveWrongAnswersAsync_ShouldStoreWrongAnswers()
    {
        // Arrange
        _localStorageService.GetItemAsync<BibleWrongAnswerCollection>("bible_wrong_answers")
            .Returns((BibleWrongAnswerCollection?)null);

        var session = new BibleGameSession
        {
            Score = 7,
            Answers = new List<BibleGameAnswer>
            {
                new() { QuestionNumber = 1, CorrectBookName = "創世記", SelectedBookName = "創世記", VerseContent = "test", VerseReference = "創 1:1" },
                new() { QuestionNumber = 2, CorrectBookName = "出埃及記", SelectedBookName = "利未記", VerseContent = "test2", VerseReference = "出 1:1" }
            }
        };

        // Act
        await _sut.SaveWrongAnswersAsync(session);

        // Assert
        await _localStorageService.Received(1)
            .SetItemAsync("bible_wrong_answers", Arg.Is<BibleWrongAnswerCollection>(c =>
                c.Sessions.Count == 1 &&
                c.Sessions[0].WrongAnswers.Count == 1));
    }

    [Test]
    public async Task GetAllWrongAnswerRecordsAsync_ShouldReturnFlatList()
    {
        // Arrange
        var collection = new BibleWrongAnswerCollection
        {
            Sessions = new List<WrongAnswerSession>
            {
                new()
                {
                    WrongAnswers = new List<WrongAnswerRecord>
                    {
                        new() { QuestionNumber = 1, CorrectAnswer = "創世記" },
                        new() { QuestionNumber = 2, CorrectAnswer = "出埃及記" }
                    }
                },
                new()
                {
                    WrongAnswers = new List<WrongAnswerRecord>
                    {
                        new() { QuestionNumber = 3, CorrectAnswer = "利未記" }
                    }
                }
            }
        };

        _localStorageService.GetItemAsync<BibleWrongAnswerCollection>("bible_wrong_answers")
            .Returns(collection);

        // Act
        var result = await _sut.GetAllWrongAnswerRecordsAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Test]
    public async Task ClearWrongAnswersAsync_ShouldRemoveFromStorage()
    {
        // Act
        await _sut.ClearWrongAnswersAsync();

        // Assert
        await _localStorageService.Received(1).RemoveItemAsync("bible_wrong_answers");
    }

    [Test]
    public void StartNewGame_QuestionsShoulddHaveUniqueNumbers()
    {
        // Act
        var questions = _sut.StartNewGame(10);

        // Assert
        var numbers = questions.Select(q => q.QuestionNumber).ToList();
        numbers.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
    }
}
