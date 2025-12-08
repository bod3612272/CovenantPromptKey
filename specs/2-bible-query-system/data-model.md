# Data Model: 聖經查詢系統 (Bible Query System)

**Date**: 2025-12-06  
**Status**: Draft  
**Related Plan**: [plan.md](./plan.md)

---

## Overview

本文件定義聖經查詢系統的資料模型，包含實體定義、欄位規格、驗證規則及狀態轉換。

---

## Entity Relationship Diagram

```
┌─────────────────────┐
│   BibleSettings     │ ←── localStorage (bible_settings)
│   (顯示設定)         │
└─────────────────────┘

┌─────────────────────┐
│   BibleBookmark     │ ←── localStorage (bible_bookmarks)
│   (書籤)            │      List<BibleBookmark> (max 10)
└─────────────────────┘

┌─────────────────────┐
│  BibleGameRecord    │ ←── localStorage (bible_game_records)
│  (遊戲記錄)          │
└─────────────────────┘

┌─────────────────────┐
│ BibleWrongAnswers   │ ←── localStorage (bible_wrong_answers)
│ (錯題記錄)           │      List<WrongAnswerSession> (max 5 sessions)
└─────────────────────┘

┌─────────────────────┐
│ BibleSearchPageState│ ←── sessionStorage (bible_search_state)
│ (查詢頁狀態)         │
└─────────────────────┘

┌─────────────────────┐
│ BibleReadPageState  │ ←── sessionStorage (bible_read_state)
│ (閱讀頁狀態)         │
└─────────────────────┘

┌─────────────────────┐
│ BibleGamePageState  │ ←── sessionStorage (bible_game_state)
│ (遊戲頁狀態)         │
└─────────────────────┘
```

---

## Entity Definitions

### 1. BibleSettings（聖經顯示設定）

**描述**：儲存使用者的聖經經文顯示偏好設定。

**儲存位置**：`localStorage["bible_settings"]`

```csharp
namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經顯示設定模型
/// </summary>
public class BibleSettings
{
    /// <summary>
    /// 字形選擇
    /// </summary>
    public FontFamily FontFamily { get; set; } = FontFamily.MicrosoftJhengHei;

    /// <summary>
    /// 字體大小 (px)
    /// </summary>
    public int FontSize { get; set; } = 16;

    /// <summary>
    /// 文字顏色
    /// </summary>
    public TextColor TextColor { get; set; } = TextColor.Black;

    /// <summary>
    /// 背景顏色
    /// </summary>
    public BackgroundColor BackgroundColor { get; set; } = BackgroundColor.White;

    /// <summary>
    /// 是否自動換行
    /// </summary>
    public bool AutoWrap { get; set; } = true;
}

/// <summary>
/// 字形列舉
/// </summary>
public enum FontFamily
{
    /// <summary>標楷體</summary>
    DFKai,
    /// <summary>微軟正黑體</summary>
    MicrosoftJhengHei
}

/// <summary>
/// 文字顏色列舉
/// </summary>
public enum TextColor
{
    /// <summary>全黑 (#000000)</summary>
    Black,
    /// <summary>深灰 (#333333)</summary>
    DarkGray,
    /// <summary>淺灰 (#666666)</summary>
    LightGray
}

/// <summary>
/// 背景顏色列舉
/// </summary>
public enum BackgroundColor
{
    /// <summary>白色 (#FFFFFF)</summary>
    White,
    /// <summary>米色 (#FFF8DC)</summary>
    Beige,
    /// <summary>淺灰 (#F5F5F5)</summary>
    LightGray,
    /// <summary>淺綠 (#F0FFF0)</summary>
    LightGreen,
    /// <summary>夜間模式 (黑底白字)</summary>
    NightMode
}
```

**欄位規格**：

| 欄位 | 型別 | 預設值 | 驗證規則 |
|------|------|--------|----------|
| FontFamily | enum | MicrosoftJhengHei | 必須為有效列舉值 |
| FontSize | int | 16 | 範圍 12-24 |
| TextColor | enum | Black | 必須為有效列舉值 |
| BackgroundColor | enum | White | 必須為有效列舉值 |
| AutoWrap | bool | true | N/A |

**特殊邏輯**：
- 當 `BackgroundColor` 為 `NightMode` 時，文字顏色自動覆寫為白色 (#FFFFFF)

---

### 2. BibleBookmark（書籤）

**描述**：記錄使用者最近閱讀的經文位置。

**儲存位置**：`localStorage["bible_bookmarks"]` (JSON Array)

**上限**：10 筆，超過時移除最舊記錄

```csharp
namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經書籤模型
/// </summary>
public class BibleBookmark
{
    /// <summary>
    /// 書卷編號 (1-66)
    /// </summary>
    public int BookNumber { get; set; }

    /// <summary>
    /// 書卷名稱
    /// </summary>
    public string BookName { get; set; } = string.Empty;

    /// <summary>
    /// 章節編號
    /// </summary>
    public int ChapterNumber { get; set; }

    /// <summary>
    /// 建立時間 (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 顯示用參考字串 (e.g., "創世記 第 1 章")
    /// </summary>
    public string DisplayReference => $"{BookName} 第 {ChapterNumber} 章";
}
```

**欄位規格**：

| 欄位 | 型別 | 驗證規則 |
|------|------|----------|
| BookNumber | int | 範圍 1-66 |
| BookName | string | 非空字串 |
| ChapterNumber | int | 範圍 1-150（依書卷不同） |
| CreatedAt | DateTime | 必須為 UTC |

---

### 3. BibleGameRecord（遊戲記錄）

**描述**：記錄經文猜猜遊戲的分數與歷史。

**儲存位置**：`localStorage["bible_game_records"]`

```csharp
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
```

**欄位規格**：

| 欄位 | 型別 | 驗證規則 |
|------|------|----------|
| HighScore | int | 範圍 0-10 |
| RecentSessions | List | 最多 5 筆 |
| Score | int | 範圍 0-10 |
| TotalQuestions | int | 固定 10 |

---

### 4. BibleSearchPageState（查詢頁狀態）

**描述**：記憶聖經查詢頁面的當前狀態。

**儲存位置**：`sessionStorage["bible_search_state"]`

```csharp
namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經查詢頁面狀態
/// </summary>
public class BibleSearchPageState
{
    /// <summary>
    /// 當前搜尋關鍵字
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// 當前頁碼 (1-based)
    /// </summary>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// 每頁筆數
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 捲動位置 (px)
    /// </summary>
    public double ScrollPosition { get; set; } = 0;
}
```

---

### 5. BibleReadPageState（閱讀頁狀態）

**描述**：記憶聖經閱讀頁面的當前狀態。

**儲存位置**：`sessionStorage["bible_read_state"]`

```csharp
namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經閱讀頁面狀態
/// </summary>
public class BibleReadPageState
{
    /// <summary>
    /// 當前書卷編號 (1-66)
    /// </summary>
    public int BookNumber { get; set; } = 1;

    /// <summary>
    /// 當前章節編號
    /// </summary>
    public int ChapterNumber { get; set; } = 1;

    /// <summary>
    /// 捲動位置 (px)
    /// </summary>
    public double ScrollPosition { get; set; } = 0;
}
```

---

### 6. BibleGamePageState（遊戲頁狀態）

**描述**：記憶聖經遊戲頁面的當前狀態（遊戲中斷不保留）。

**儲存位置**：`sessionStorage["bible_game_state"]`

```csharp
namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經遊戲頁面狀態
/// </summary>
public class BibleGamePageState
{
    /// <summary>
    /// 是否有進行中的遊戲
    /// </summary>
    public bool IsGameInProgress { get; set; } = false;

    /// <summary>
    /// 當前題目編號 (1-10)
    /// </summary>
    public int CurrentQuestionNumber { get; set; } = 0;

    /// <summary>
    /// 當前分數
    /// </summary>
    public int CurrentScore { get; set; } = 0;
}
```

**注意**：根據 spec Edge Cases，若使用者在遊戲進行中關閉頁面，該局遊戲不計入記錄，下次重新開始。因此此狀態僅用於頁面內導航時保持狀態，不用於恢復未完成的遊戲。

---

### 7. BibleWrongAnswerCollection（錯題記錄）

**描述**：記錄使用者最近 5 次遊戲中答錯的題目，用於學習與回憶錯誤。

**儲存位置**：`localStorage["bible_wrong_answers"]`

**上限**：最多保留 5 次遊戲的錯題（若每次全錯，最多 50 題）

```csharp
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
```

**欄位規格**：

| 欄位 | 型別 | 驗證規則 |
|------|------|----------|
| Sessions | List | 最多 5 筆遊戲 |
| GameSessionId | Guid | 必須為有效 GUID |
| PlayedAt | DateTime | 必須為 UTC |
| WrongAnswers | List | 最多 10 筆（單次遊戲全錯時） |
| QuestionNumber | int | 範圍 1-10 |
| VerseContent | string | 非空字串 |
| VerseReference | string | 非空字串 |
| SelectedAnswer | string | 非空字串 |
| CorrectAnswer | string | 非空字串 |

**特殊邏輯**：
- 當 `Sessions.Count > 5` 時，自動移除最舊的 Session（FIFO）
- 僅記錄答錯的題目（`IsCorrect == false`）
- 可獨立於遊戲記錄清除（提供單獨的清除功能）

---

## Supporting Models

### ExportOptions（導出選項）

```csharp
namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 經文導出選項
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// 導出風格
    /// </summary>
    public ExportStyle Style { get; set; } = ExportStyle.Style1;

    /// <summary>
    /// 是否包含書卷標題
    /// </summary>
    public bool IncludeBookTitle { get; set; } = true;

    /// <summary>
    /// 是否一本書一個檔案
    /// </summary>
    public bool OneFilePerBook { get; set; } = false;
}

/// <summary>
/// 導出風格列舉
/// </summary>
public enum ExportStyle
{
    /// <summary>
    /// 風格 1：(馬可福音 1:1) 經文內容
    /// </summary>
    Style1,

    /// <summary>
    /// 風格 2：書卷 ( 第 X 章 Y ~ Z 節 ) + 逐行經文
    /// </summary>
    Style2,

    /// <summary>
    /// 風格 3：《書卷》( 第 X 章 Y ~ Z 節 ) + 連續段落
    /// </summary>
    Style3
}
```

---

### ExportRange（導出範圍）

```csharp
namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 經文導出範圍
/// </summary>
public class ExportRange
{
    /// <summary>
    /// 書卷編號
    /// </summary>
    public int BookNumber { get; set; }

    /// <summary>
    /// 起始章
    /// </summary>
    public int StartChapter { get; set; }

    /// <summary>
    /// 起始節
    /// </summary>
    public int StartVerse { get; set; }

    /// <summary>
    /// 結束章
    /// </summary>
    public int EndChapter { get; set; }

    /// <summary>
    /// 結束節
    /// </summary>
    public int EndVerse { get; set; }
}
```

---

### SearchResultItem（搜尋結果項目）

```csharp
namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經搜尋結果項目（UI 顯示用）
/// </summary>
public class SearchResultItem
{
    /// <summary>
    /// 書卷名稱
    /// </summary>
    public string BookName { get; set; } = string.Empty;

    /// <summary>
    /// 章節編號
    /// </summary>
    public int ChapterNumber { get; set; }

    /// <summary>
    /// 節數編號
    /// </summary>
    public int VerseNumber { get; set; }

    /// <summary>
    /// 經文內容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 參考字串 (e.g., "約翰福音 3:16")
    /// </summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// 相關性分數
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// 高亮顯示的經文內容 (關鍵字標記)
    /// </summary>
    public string HighlightedContent { get; set; } = string.Empty;
}
```

---

## Validation Rules Summary

| Entity | Field | Rule | Error Message |
|--------|-------|------|---------------|
| BibleSettings | FontSize | 12 ≤ value ≤ 24 | 字體大小必須介於 12 至 24 px |
| BibleBookmark | BookNumber | 1 ≤ value ≤ 66 | 無效的書卷編號 |
| BibleBookmark | ChapterNumber | 1 ≤ value ≤ max(book) | 無效的章節編號 |
| BibleGameSession | Score | 0 ≤ value ≤ 10 | 分數必須介於 0 至 10 |
| ExportRange | StartChapter ≤ EndChapter | - | 起始章不可大於結束章 |
| ExportRange | StartVerse ≤ EndVerse (同章) | - | 起始節不可大於結束節 |

---

## Storage Keys Reference

| Key | Storage Type | Content Type | Max Size |
|-----|--------------|--------------|----------|
| `bible_settings` | localStorage | BibleSettings | ~200 bytes |
| `bible_bookmarks` | localStorage | List\<BibleBookmark\> | ~2 KB (10 items) |
| `bible_game_records` | localStorage | BibleGameRecordCollection | ~10 KB (5 sessions) |
| `bible_wrong_answers` | localStorage | BibleWrongAnswerCollection | ~15 KB (50 wrong answers) |
| `bible_search_state` | sessionStorage | BibleSearchPageState | ~500 bytes |
| `bible_read_state` | sessionStorage | BibleReadPageState | ~100 bytes |
| `bible_game_state` | sessionStorage | BibleGamePageState | ~100 bytes |

---

## CSS Helper Classes

為支援動態樣式設定，定義以下 CSS class 對應：

```csharp
public static class BibleStyleHelper
{
    public static string GetFontFamilyCss(FontFamily font) => font switch
    {
        FontFamily.DFKai => "'DFKai-SB', '標楷體', serif",
        FontFamily.MicrosoftJhengHei => "'Microsoft JhengHei', '微軟正黑體', sans-serif",
        _ => "'Microsoft JhengHei', sans-serif"
    };

    public static string GetTextColorHex(TextColor color) => color switch
    {
        TextColor.Black => "#000000",
        TextColor.DarkGray => "#333333",
        TextColor.LightGray => "#666666",
        _ => "#000000"
    };

    public static string GetBackgroundColorHex(BackgroundColor color) => color switch
    {
        BackgroundColor.White => "#FFFFFF",
        BackgroundColor.Beige => "#FFF8DC",
        BackgroundColor.LightGray => "#F5F5F5",
        BackgroundColor.LightGreen => "#F0FFF0",
        BackgroundColor.NightMode => "#000000",
        _ => "#FFFFFF"
    };

    public static string GetTextColorForBackground(BackgroundColor bg) =>
        bg == BackgroundColor.NightMode ? "#FFFFFF" : GetTextColorHex(TextColor.Black);
}
```
