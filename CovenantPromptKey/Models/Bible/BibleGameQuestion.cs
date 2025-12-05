using BibleData;

namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 遊戲題目模型
/// </summary>
public class BibleGameQuestion
{
    /// <summary>
    /// 題目編號
    /// </summary>
    public int QuestionNumber { get; set; }

    /// <summary>
    /// 隨機選取的經文
    /// </summary>
    public VerseWithLocation Verse { get; set; } = null!;

    /// <summary>
    /// 四個書卷選項
    /// </summary>
    public List<string> Options { get; set; } = new();

    /// <summary>
    /// 正確答案（書卷名稱）
    /// </summary>
    public string CorrectAnswer => Verse.BookName;
}
