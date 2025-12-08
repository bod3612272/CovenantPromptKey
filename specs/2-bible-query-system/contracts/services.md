# Service Contracts: 聖經查詢系統

**Date**: 2025-12-06  
**Related Plan**: [../plan.md](../plan.md)

---

## Overview

本文件定義聖經查詢系統的服務介面契約，遵循現有 CovenantPromptKey 專案的 Service 設計模式（Interface + Implementation 分離）。

---

## Service Interfaces

### 1. IBibleSearchService（聖經搜尋服務）

**職責**：提供聖經經文搜尋功能，包含關鍵字搜尋、即時搜尋、多關鍵字搜尋。

**生命週期**：Scoped

```csharp
namespace CovenantPromptKey.Services.Interfaces;

using BibleData;
using CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經搜尋服務介面
/// </summary>
public interface IBibleSearchService
{
    /// <summary>
    /// 使用單一關鍵字搜尋經文
    /// </summary>
    /// <param name="keyword">搜尋關鍵字</param>
    /// <param name="top">回傳筆數上限</param>
    /// <returns>搜尋結果列表</returns>
    List<SearchResultItem> Search(string keyword, int top = 50);

    /// <summary>
    /// 使用單一關鍵字搜尋經文（帶排名分數）
    /// </summary>
    /// <param name="keyword">搜尋關鍵字</param>
    /// <param name="top">回傳筆數上限</param>
    /// <returns>排名搜尋結果列表</returns>
    List<SearchResultItem> SearchRanked(string keyword, int top = 50);

    /// <summary>
    /// 即時搜尋（支援取消）
    /// </summary>
    /// <param name="keyword">搜尋關鍵字</param>
    /// <param name="top">回傳筆數上限</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>搜尋結果列表</returns>
    List<SearchResultItem> SearchWithCancellation(string keyword, int top, CancellationToken cancellationToken);

    /// <summary>
    /// 多關鍵字搜尋（AND 邏輯）
    /// </summary>
    /// <param name="keywords">關鍵字陣列</param>
    /// <param name="top">回傳筆數上限</param>
    /// <returns>搜尋結果列表</returns>
    List<SearchResultItem> SearchMultiple(string[] keywords, int top = 50);

    /// <summary>
    /// 解析輸入字串並執行搜尋（空格分隔多關鍵字）
    /// </summary>
    /// <param name="input">使用者輸入</param>
    /// <param name="top">回傳筆數上限</param>
    /// <returns>搜尋結果列表</returns>
    List<SearchResultItem> SearchFromInput(string input, int top = 50);

    /// <summary>
    /// 在搜尋結果中高亮關鍵字
    /// </summary>
    /// <param name="content">原始經文內容</param>
    /// <param name="keywords">要高亮的關鍵字</param>
    /// <returns>高亮後的 HTML 字串</returns>
    string HighlightKeywords(string content, string[] keywords);
}
```

---

### 2. IBibleReadingService（聖經閱讀服務）

**職責**：提供聖經章節閱讀功能，包含書卷/章節查詢、導航。

**生命週期**：Scoped

```csharp
namespace CovenantPromptKey.Services.Interfaces;

using BibleData;
using BibleData.Models;

/// <summary>
/// 聖經閱讀服務介面
/// </summary>
public interface IBibleReadingService
{
    /// <summary>
    /// 取得所有書卷名稱列表
    /// </summary>
    /// <returns>書卷名稱列表（按編號排序）</returns>
    IReadOnlyList<string> GetBookNames();

    /// <summary>
    /// 取得書卷資訊
    /// </summary>
    /// <param name="bookNumber">書卷編號 (1-66)</param>
    /// <returns>書卷資訊，若不存在回傳 null</returns>
    Book? GetBook(int bookNumber);

    /// <summary>
    /// 取得書卷資訊
    /// </summary>
    /// <param name="bookName">書卷名稱</param>
    /// <returns>書卷資訊，若不存在回傳 null</returns>
    Book? GetBook(string bookName);

    /// <summary>
    /// 取得指定章節的所有經文
    /// </summary>
    /// <param name="bookNumber">書卷編號</param>
    /// <param name="chapterNumber">章節編號</param>
    /// <returns>經文列表</returns>
    List<VerseWithLocation> GetChapterVerses(int bookNumber, int chapterNumber);

    /// <summary>
    /// 取得指定章節的所有經文
    /// </summary>
    /// <param name="bookName">書卷名稱</param>
    /// <param name="chapterNumber">章節編號</param>
    /// <returns>經文列表</returns>
    List<VerseWithLocation> GetChapterVerses(string bookName, int chapterNumber);

    /// <summary>
    /// 取得書卷的章數
    /// </summary>
    /// <param name="bookNumber">書卷編號</param>
    /// <returns>章數</returns>
    int GetChapterCount(int bookNumber);

    /// <summary>
    /// 取得書卷的章數
    /// </summary>
    /// <param name="bookName">書卷名稱</param>
    /// <returns>章數</returns>
    int GetChapterCount(string bookName);

    /// <summary>
    /// 檢查章節是否有效
    /// </summary>
    /// <param name="bookNumber">書卷編號</param>
    /// <param name="chapterNumber">章節編號</param>
    /// <returns>是否有效</returns>
    bool IsValidChapter(int bookNumber, int chapterNumber);

    /// <summary>
    /// 取得下一章資訊
    /// </summary>
    /// <param name="bookNumber">當前書卷編號</param>
    /// <param name="chapterNumber">當前章節編號</param>
    /// <returns>下一章的 (書卷編號, 章節編號)，若已是最後一章回傳 null</returns>
    (int BookNumber, int ChapterNumber)? GetNextChapter(int bookNumber, int chapterNumber);

    /// <summary>
    /// 取得上一章資訊
    /// </summary>
    /// <param name="bookNumber">當前書卷編號</param>
    /// <param name="chapterNumber">當前章節編號</param>
    /// <returns>上一章的 (書卷編號, 章節編號)，若已是第一章回傳 null</returns>
    (int BookNumber, int ChapterNumber)? GetPreviousChapter(int bookNumber, int chapterNumber);

    /// <summary>
    /// 取得聖經統計資訊
    /// </summary>
    /// <returns>(總書卷數, 總章數, 總節數)</returns>
    (int TotalBooks, int TotalChapters, int TotalVerses) GetStatistics();
}
```

---

### 3. IBibleExportService（經文導出服務）

**職責**：提供經文導出為 Markdown 格式的功能。

**生命週期**：Scoped

```csharp
namespace CovenantPromptKey.Services.Interfaces;

using BibleData;
using CovenantPromptKey.Models.Bible;

/// <summary>
/// 經文導出服務介面
/// </summary>
public interface IBibleExportService
{
    /// <summary>
    /// 導出經文為 Markdown 格式
    /// </summary>
    /// <param name="range">導出範圍</param>
    /// <param name="options">導出選項</param>
    /// <returns>Markdown 格式字串</returns>
    string ExportToMarkdown(ExportRange range, ExportOptions options);

    /// <summary>
    /// 導出多個範圍的經文為 Markdown 格式
    /// </summary>
    /// <param name="ranges">導出範圍列表</param>
    /// <param name="options">導出選項</param>
    /// <returns>Markdown 格式字串</returns>
    string ExportToMarkdown(IEnumerable<ExportRange> ranges, ExportOptions options);

    /// <summary>
    /// 導出整本書卷為 Markdown 格式
    /// </summary>
    /// <param name="bookNumber">書卷編號</param>
    /// <param name="options">導出選項</param>
    /// <returns>Markdown 格式字串</returns>
    string ExportBookToMarkdown(int bookNumber, ExportOptions options);

    /// <summary>
    /// 導出多本書卷，每本一個檔案
    /// </summary>
    /// <param name="bookNumbers">書卷編號列表</param>
    /// <param name="options">導出選項</param>
    /// <returns>Dictionary，Key 為檔名，Value 為 Markdown 內容</returns>
    Dictionary<string, string> ExportBooksToFiles(IEnumerable<int> bookNumbers, ExportOptions options);

    /// <summary>
    /// 取得導出風格的預覽範例
    /// </summary>
    /// <param name="style">導出風格</param>
    /// <returns>預覽範例字串</returns>
    string GetStylePreview(ExportStyle style);
}
```

---

### 4. IBibleGameService（聖經遊戲服務）

**職責**：提供經文猜猜遊戲功能，包含分數記錄與錯題記錄管理。

**生命週期**：Scoped

```csharp
namespace CovenantPromptKey.Services.Interfaces;

using BibleData;
using CovenantPromptKey.Models.Bible;

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
```

---

### 5. IBibleSettingsService（聖經設定服務）

**職責**：管理聖經顯示設定的載入與儲存。

**生命週期**：Scoped

```csharp
namespace CovenantPromptKey.Services.Interfaces;

using CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經設定服務介面
/// </summary>
public interface IBibleSettingsService
{
    /// <summary>
    /// 載入設定
    /// </summary>
    /// <returns>設定物件</returns>
    Task<BibleSettings> LoadSettingsAsync();

    /// <summary>
    /// 儲存設定
    /// </summary>
    /// <param name="settings">設定物件</param>
    /// <returns>非同步任務</returns>
    Task SaveSettingsAsync(BibleSettings settings);

    /// <summary>
    /// 重置為預設設定
    /// </summary>
    /// <returns>預設設定物件</returns>
    Task<BibleSettings> ResetToDefaultAsync();

    /// <summary>
    /// 取得預設設定
    /// </summary>
    /// <returns>預設設定物件</returns>
    BibleSettings GetDefaultSettings();
}
```

---

### 6. IBibleBookmarkService（書籤服務）

**職責**：管理聖經書籤的載入與儲存。

**生命週期**：Scoped

```csharp
namespace CovenantPromptKey.Services.Interfaces;

using CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經書籤服務介面
/// </summary>
public interface IBibleBookmarkService
{
    /// <summary>
    /// 載入所有書籤
    /// </summary>
    /// <returns>書籤列表（依時間降序）</returns>
    Task<List<BibleBookmark>> LoadBookmarksAsync();

    /// <summary>
    /// 新增書籤
    /// </summary>
    /// <param name="bookNumber">書卷編號</param>
    /// <param name="bookName">書卷名稱</param>
    /// <param name="chapterNumber">章節編號</param>
    /// <returns>非同步任務</returns>
    Task AddBookmarkAsync(int bookNumber, string bookName, int chapterNumber);

    /// <summary>
    /// 移除書籤
    /// </summary>
    /// <param name="bookmark">書籤物件</param>
    /// <returns>非同步任務</returns>
    Task RemoveBookmarkAsync(BibleBookmark bookmark);

    /// <summary>
    /// 清除所有書籤
    /// </summary>
    /// <returns>非同步任務</returns>
    Task ClearAllBookmarksAsync();

    /// <summary>
    /// 取得書籤數量
    /// </summary>
    /// <returns>書籤數量</returns>
    Task<int> GetBookmarkCountAsync();
}
```

---

### 7. IBiblePageStateService（頁面狀態服務）

**職責**：管理聖經各頁面的狀態。

**生命週期**：Scoped

```csharp
namespace CovenantPromptKey.Services.Interfaces;

using CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經頁面狀態服務介面
/// </summary>
public interface IBiblePageStateService
{
    /// <summary>
    /// 載入查詢頁狀態
    /// </summary>
    /// <returns>查詢頁狀態</returns>
    Task<BibleSearchPageState> LoadSearchPageStateAsync();

    /// <summary>
    /// 儲存查詢頁狀態
    /// </summary>
    /// <param name="state">狀態物件</param>
    /// <returns>非同步任務</returns>
    Task SaveSearchPageStateAsync(BibleSearchPageState state);

    /// <summary>
    /// 載入閱讀頁狀態
    /// </summary>
    /// <returns>閱讀頁狀態</returns>
    Task<BibleReadPageState> LoadReadPageStateAsync();

    /// <summary>
    /// 儲存閱讀頁狀態
    /// </summary>
    /// <param name="state">狀態物件</param>
    /// <returns>非同步任務</returns>
    Task SaveReadPageStateAsync(BibleReadPageState state);

    /// <summary>
    /// 載入遊戲頁狀態
    /// </summary>
    /// <returns>遊戲頁狀態</returns>
    Task<BibleGamePageState> LoadGamePageStateAsync();

    /// <summary>
    /// 儲存遊戲頁狀態
    /// </summary>
    /// <param name="state">狀態物件</param>
    /// <returns>非同步任務</returns>
    Task SaveGamePageStateAsync(BibleGamePageState state);

    /// <summary>
    /// 清除所有頁面狀態
    /// </summary>
    /// <returns>非同步任務</returns>
    Task ClearAllStatesAsync();
}
```

---

## Dependency Injection Configuration

```csharp
// Program.cs - DI 註冊範例

// BibleData 索引 (Singleton - 啟動時建立一次)
builder.Services.AddSingleton<BibleIndex>();

// Bible Services (Scoped)
builder.Services.AddScoped<IBibleSearchService, BibleSearchService>();
builder.Services.AddScoped<IBibleReadingService, BibleReadingService>();
builder.Services.AddScoped<IBibleExportService, BibleExportService>();
builder.Services.AddScoped<IBibleGameService, BibleGameService>();
builder.Services.AddScoped<IBibleSettingsService, BibleSettingsService>();
builder.Services.AddScoped<IBibleBookmarkService, BibleBookmarkService>();
builder.Services.AddScoped<IBiblePageStateService, BiblePageStateService>();
```

---

## Service Dependencies

```
┌─────────────────────────┐
│      BibleIndex         │  ← Singleton (BibleData DLL)
└────────────┬────────────┘
             │
    ┌────────┴────────┬──────────────┬─────────────────┐
    │                 │              │                 │
    ▼                 ▼              ▼                 ▼
┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────────┐
│ Search   │  │ Reading  │  │  Game    │  │   Export     │
│ Service  │  │ Service  │  │ Service  │  │   Service    │
└──────────┘  └────┬─────┘  └────┬─────┘  └──────────────┘
                   │             │
                   ▼             ▼
            ┌──────────┐  ┌──────────────┐
            │ Bookmark │  │ GameRecord   │
            │ Service  │  │ (via Storage)│
            └────┬─────┘  └──────┬───────┘
                 │               │
                 ▼               ▼
          ┌─────────────────────────────┐
          │  ISessionStorageService     │  ← 現有服務
          │  (localStorage/sessionStorage)
          └─────────────────────────────┘
```

---

## Error Handling Convention

所有服務方法遵循以下錯誤處理慣例：

1. **參數驗證**：無效參數拋出 `ArgumentException` 或 `ArgumentNullException`
2. **查無結果**：回傳空集合或 `null`（依回傳型別）
3. **Storage 錯誤**：記錄 Debug Log，回傳預設值或空集合
4. **非預期錯誤**：記錄 Debug Log，向上拋出或回傳預設值

```csharp
// 範例：Search 方法錯誤處理
public List<SearchResultItem> Search(string keyword, int top = 50)
{
    if (string.IsNullOrWhiteSpace(keyword))
        return new List<SearchResultItem>(); // 空輸入回傳空集合
    
    if (top <= 0)
        throw new ArgumentException("Top must be greater than 0", nameof(top));
    
    try
    {
        // 執行搜尋...
    }
    catch (Exception ex)
    {
        _debugLogService.Log($"Search error: {ex.Message}", LogLevel.Error);
        return new List<SearchResultItem>();
    }
}
```
