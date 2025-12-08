using BibleData;
using CovenantPromptKey.Models.Bible;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// 聖經遊戲服務實作
/// </summary>
public class BibleGameService : IBibleGameService
{
    private const string GameRecordsKey = "bible_game_records";
    private const string WrongAnswersKey = "bible_wrong_answers";
    private const int MaxRecentSessions = 5;
    private const int MaxWrongAnswerSessions = 5;

    private readonly BibleIndex _bibleIndex;
    private readonly IBibleReadingService _readingService;
    private readonly ILocalStorageService _localStorageService;
    private readonly Random _random = new();

    public BibleGameService(BibleIndex bibleIndex, IBibleReadingService readingService, ILocalStorageService localStorageService)
    {
        _bibleIndex = bibleIndex;
        _readingService = readingService;
        _localStorageService = localStorageService;
    }

    /// <inheritdoc />
    public List<BibleGameQuestion> StartNewGame(int questionCount = 10)
    {
        var questions = new List<BibleGameQuestion>();
        var usedVerseIndices = new HashSet<int>();

        for (int i = 1; i <= questionCount; i++)
        {
            var question = GenerateUniqueQuestion(usedVerseIndices);
            question.QuestionNumber = i;
            questions.Add(question);
        }

        return questions;
    }

    /// <inheritdoc />
    public BibleGameQuestion GenerateQuestion()
    {
        return GenerateUniqueQuestion(new HashSet<int>());
    }

    private BibleGameQuestion GenerateUniqueQuestion(HashSet<int> usedIndices)
    {
        var totalVerses = _bibleIndex.TotalVerses;
        int verseIndex;

        // 確保不重複選取經文
        do
        {
            verseIndex = _random.Next(0, totalVerses);
        } while (usedIndices.Contains(verseIndex) && usedIndices.Count < totalVerses);

        usedIndices.Add(verseIndex);

        var verse = GetVerseByIndex(verseIndex);
        var options = GenerateOptions(verse.BookName);

        return new BibleGameQuestion
        {
            Verse = verse,
            Options = options
        };
    }

    private VerseWithLocation GetVerseByIndex(int index)
    {
        // 使用 IBibleReadingService 遍歷所有書卷和章節找到對應的經文
        int currentIndex = 0;
        var bookNames = _readingService.GetAllBookNames();

        for (int bookNumber = 1; bookNumber <= bookNames.Count; bookNumber++)
        {
            var chapterCount = _readingService.GetChapterCount(bookNumber);
            for (int chapterNumber = 1; chapterNumber <= chapterCount; chapterNumber++)
            {
                var verses = _readingService.GetChapterVerses(bookNumber, chapterNumber);
                foreach (var verse in verses)
                {
                    if (currentIndex == index)
                    {
                        return verse;
                    }
                    currentIndex++;
                }
            }
        }

        // 如果找不到，返回創世記 1:1
        var firstVerses = _readingService.GetChapterVerses(1, 1);
        if (firstVerses.Any())
        {
            return firstVerses.First();
        }

        // 最後備援
        return new VerseWithLocation
        {
            BookNumber = 1,
            BookName = bookNames.FirstOrDefault() ?? "創世記",
            ChapterNumber = 1,
            VerseNumber = 1,
            Content = "起初，神創造天地。"
        };
    }

    private List<string> GenerateOptions(string correctBookName)
    {
        var allBookNames = _readingService.GetAllBookNames();
        var options = new List<string> { correctBookName };

        // 選擇 3 個錯誤選項
        while (options.Count < 4)
        {
            var randomBook = allBookNames[_random.Next(allBookNames.Count)];
            if (!options.Contains(randomBook))
            {
                options.Add(randomBook);
            }
        }

        // 隨機排序選項
        return options.OrderBy(_ => _random.Next()).ToList();
    }

    /// <inheritdoc />
    public bool CheckAnswer(BibleGameQuestion question, string selectedBook)
    {
        return question.CorrectAnswer == selectedBook;
    }

    /// <inheritdoc />
    public async Task SaveGameResultAsync(BibleGameSession session)
    {
        var records = await GetGameRecordsAsync();

        // 更新最高分
        if (session.Score > records.HighScore)
        {
            records.HighScore = session.Score;
            records.HighScoreAt = DateTime.UtcNow;
        }

        // 添加到最近記錄
        records.RecentSessions.Insert(0, session);

        // 保留最多 MaxRecentSessions 筆
        if (records.RecentSessions.Count > MaxRecentSessions)
        {
            records.RecentSessions = records.RecentSessions.Take(MaxRecentSessions).ToList();
        }

        await _localStorageService.SetItemAsync(GameRecordsKey, records);

        // 同時儲存錯題記錄
        await SaveWrongAnswersAsync(session);
    }

    /// <inheritdoc />
    public async Task<BibleGameRecordCollection> GetGameRecordsAsync()
    {
        var records = await _localStorageService.GetItemAsync<BibleGameRecordCollection>(GameRecordsKey);
        return records ?? new BibleGameRecordCollection();
    }

    /// <inheritdoc />
    public async Task ClearGameRecordsAsync()
    {
        await _localStorageService.RemoveItemAsync(GameRecordsKey);
    }

    /// <inheritdoc />
    public async Task SaveWrongAnswersAsync(BibleGameSession session)
    {
        var wrongAnswers = session.Answers.Where(a => !a.IsCorrect).ToList();
        if (!wrongAnswers.Any()) return;

        var collection = await GetWrongAnswersAsync();

        var wrongSession = new WrongAnswerSession
        {
            GameSessionId = session.Id,
            PlayedAt = session.PlayedAt,
            WrongAnswers = wrongAnswers.Select(a => new WrongAnswerRecord
            {
                QuestionNumber = a.QuestionNumber,
                VerseContent = a.VerseContent,
                VerseReference = a.VerseReference,
                SelectedAnswer = a.SelectedBookName,
                CorrectAnswer = a.CorrectBookName
            }).ToList()
        };

        collection.Sessions.Insert(0, wrongSession);

        // 保留最多 MaxWrongAnswerSessions 次遊戲的錯題
        if (collection.Sessions.Count > MaxWrongAnswerSessions)
        {
            collection.Sessions = collection.Sessions.Take(MaxWrongAnswerSessions).ToList();
        }

        await _localStorageService.SetItemAsync(WrongAnswersKey, collection);
    }

    /// <inheritdoc />
    public async Task<BibleWrongAnswerCollection> GetWrongAnswersAsync()
    {
        var collection = await _localStorageService.GetItemAsync<BibleWrongAnswerCollection>(WrongAnswersKey);
        return collection ?? new BibleWrongAnswerCollection();
    }

    /// <inheritdoc />
    public async Task<List<WrongAnswerRecord>> GetAllWrongAnswerRecordsAsync()
    {
        var collection = await GetWrongAnswersAsync();
        return collection.Sessions.SelectMany(s => s.WrongAnswers).ToList();
    }

    /// <inheritdoc />
    public async Task ClearWrongAnswersAsync()
    {
        await _localStorageService.RemoveItemAsync(WrongAnswersKey);
    }
}
