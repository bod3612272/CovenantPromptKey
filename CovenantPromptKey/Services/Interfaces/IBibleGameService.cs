using CovenantPromptKey.Models.Bible;

namespace CovenantPromptKey.Services.Interfaces;

/// <summary>
/// 聖經遊戲服務介面
/// </summary>
public interface IBibleGameService
{
    /// <summary>
    /// 開始新遊戲
    /// </summary>
    /// <param name="questionCount">題目數量（預設 10）</param>
    /// <returns>遊戲問題列表</returns>
    List<BibleGameQuestion> StartNewGame(int questionCount = 10);

    /// <summary>
    /// 生成單一題目
    /// </summary>
    /// <returns>遊戲問題</returns>
    BibleGameQuestion GenerateQuestion();

    /// <summary>
    /// 檢查答案是否正確
    /// </summary>
    /// <param name="question">題目</param>
    /// <param name="selectedBook">使用者選擇的書卷</param>
    /// <returns>是否正確</returns>
    bool CheckAnswer(BibleGameQuestion question, string selectedBook);

    /// <summary>
    /// 儲存遊戲結果
    /// </summary>
    /// <param name="session">遊戲紀錄</param>
    /// <returns>非同步任務</returns>
    Task SaveGameResultAsync(BibleGameSession session);

    /// <summary>
    /// 取得遊戲記錄
    /// </summary>
    /// <returns>遊戲記錄集合</returns>
    Task<BibleGameRecordCollection> GetGameRecordsAsync();

    /// <summary>
    /// 清除所有遊戲記錄
    /// </summary>
    /// <returns>非同步任務</returns>
    Task ClearGameRecordsAsync();

    #region Wrong Answers Management

    /// <summary>
    /// 儲存錯題記錄（遊戲結束時呼叫）
    /// </summary>
    /// <param name="session">遊戲紀錄（包含答題詳情）</param>
    /// <returns>非同步任務</returns>
    Task SaveWrongAnswersAsync(BibleGameSession session);

    /// <summary>
    /// 取得錯題記錄
    /// </summary>
    /// <returns>錯題記錄集合</returns>
    Task<BibleWrongAnswerCollection> GetWrongAnswersAsync();

    /// <summary>
    /// 取得所有錯題的扁平化列表（用於複習顯示）
    /// </summary>
    /// <returns>錯題列表</returns>
    Task<List<WrongAnswerRecord>> GetAllWrongAnswerRecordsAsync();

    /// <summary>
    /// 清除所有錯題記錄
    /// </summary>
    /// <returns>非同步任務</returns>
    Task ClearWrongAnswersAsync();

    #endregion
}
