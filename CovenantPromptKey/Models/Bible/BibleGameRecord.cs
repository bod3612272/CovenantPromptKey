namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經遊戲記錄模型
/// </summary>
public class BibleGameRecordCollection
{
    /// <summary>
    /// 歷史最高分
    /// </summary>
    public int HighScore { get; set; } = 0;

    /// <summary>
    /// 歷史最高分達成時間 (UTC)
    /// </summary>
    public DateTime? HighScoreAt { get; set; }

    /// <summary>
    /// 最近 5 次遊戲記錄
    /// </summary>
    public List<BibleGameSession> RecentSessions { get; set; } = new();
}

/// <summary>
/// 單次遊戲紀錄
/// </summary>
public class BibleGameSession
{
    /// <summary>
    /// 遊戲 ID
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 本次分數 (0-10)
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// 總題數
    /// </summary>
    public int TotalQuestions { get; set; } = 10;

    /// <summary>
    /// 遊戲時間 (UTC)
    /// </summary>
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 答題詳情
    /// </summary>
    public List<BibleGameAnswer> Answers { get; set; } = new();
}

/// <summary>
/// 單題答題紀錄
/// </summary>
public class BibleGameAnswer
{
    /// <summary>
    /// 題目編號 (1-10)
    /// </summary>
    public int QuestionNumber { get; set; }

    /// <summary>
    /// 經文內容
    /// </summary>
    public string VerseContent { get; set; } = string.Empty;

    /// <summary>
    /// 經文出處參考
    /// </summary>
    public string VerseReference { get; set; } = string.Empty;

    /// <summary>
    /// 正確書卷名稱
    /// </summary>
    public string CorrectBookName { get; set; } = string.Empty;

    /// <summary>
    /// 使用者選擇的書卷名稱
    /// </summary>
    public string SelectedBookName { get; set; } = string.Empty;

    /// <summary>
    /// 是否答對
    /// </summary>
    public bool IsCorrect => CorrectBookName == SelectedBookName;
}
