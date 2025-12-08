namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經遊戲錯題記錄集合
/// </summary>
public class BibleWrongAnswerCollection
{
    /// <summary>
    /// 最近 5 次遊戲的錯題記錄
    /// </summary>
    public List<WrongAnswerSession> Sessions { get; set; } = new();

    /// <summary>
    /// 總錯題數
    /// </summary>
    public int TotalWrongAnswers => Sessions.Sum(s => s.WrongAnswers.Count);
}

/// <summary>
/// 單次遊戲的錯題記錄
/// </summary>
public class WrongAnswerSession
{
    /// <summary>
    /// 對應的遊戲 ID (關聯 BibleGameSession.Id)
    /// </summary>
    public Guid GameSessionId { get; set; }

    /// <summary>
    /// 遊戲時間 (UTC)
    /// </summary>
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 該次遊戲的錯題列表
    /// </summary>
    public List<WrongAnswerRecord> WrongAnswers { get; set; } = new();
}

/// <summary>
/// 單題錯題記錄
/// </summary>
public class WrongAnswerRecord
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
    /// 經文出處 (e.g., "創世記 1:1")
    /// </summary>
    public string VerseReference { get; set; } = string.Empty;

    /// <summary>
    /// 使用者選擇的答案（書卷名稱）
    /// </summary>
    public string SelectedAnswer { get; set; } = string.Empty;

    /// <summary>
    /// 正確答案（書卷名稱）
    /// </summary>
    public string CorrectAnswer { get; set; } = string.Empty;
}
