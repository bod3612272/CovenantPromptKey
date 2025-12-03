# Data Model: 互動式關鍵字替換介面

**Branch**: `001-interactive-keyword-replace` | **Date**: 2025-12-03

## 領域模型概覽 (Domain Model Overview)

```
┌─────────────────────────────────────────────────────────────────┐
│                        使用者介面層                              │
│  ┌──────────┐  ┌──────────────┐  ┌──────────────┐              │
│  │ MaskPage │  │ RestorePage  │  │ SettingsPage │              │
│  └────┬─────┘  └──────┬───────┘  └──────┬───────┘              │
│       │               │                 │                       │
└───────┼───────────────┼─────────────────┼───────────────────────┘
        │               │                 │
┌───────┼───────────────┼─────────────────┼───────────────────────┐
│       ▼               ▼                 ▼     服務層             │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                   KeywordService                          │   │
│  │  • DetectKeywords()  • ApplyMask()  • RestoreText()      │   │
│  └──────────────────────────────────────────────────────────┘   │
│                              │                                   │
│  ┌────────────────────┐     │     ┌──────────────────────────┐  │
│  │ DictionaryService  │◄────┼────►│ SessionStorageService    │  │
│  │ • CRUD Keywords    │           │ • Save/Load WorkSession  │  │
│  └────────────────────┘           └──────────────────────────┘  │
│                              │                                   │
│  ┌────────────────────┐     │                                   │
│  │ DebugLogService    │◄────┘                                   │
│  │ • Log()  • GetLogs │                                         │
│  └────────────────────┘                                         │
└─────────────────────────────────────────────────────────────────┘
```

---

## 核心實體 (Core Entities)

### 1. KeywordMapping

關鍵字映射實體，代表單一機敏詞與其替代詞的對應關係。

```csharp
namespace CovenantPromptKey.Models;

/// <summary>
/// 關鍵字映射實體，定義機敏詞與安全替代詞之間的對應關係
/// </summary>
public class KeywordMapping
{
    /// <summary>
    /// 唯一識別碼
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// 機敏關鍵字（原始詞彙）
    /// </summary>
    /// <remarks>
    /// - 不可為空或僅含空白
    /// - 不可為程式語言保留字
    /// - 最大長度 200 字元
    /// </remarks>
    public required string SensitiveKey { get; set; }

    /// <summary>
    /// 安全替代詞（遮罩後顯示）
    /// </summary>
    /// <remarks>
    /// - 不可為空或僅含空白
    /// - 必須在字典中保持唯一性
    /// - 最大長度 200 字元
    /// </remarks>
    public required string SafeKey { get; set; }

    /// <summary>
    /// 高亮顯示顏色 (HEX 格式)
    /// </summary>
    /// <example>#FF6B6B</example>
    public string HighlightColor { get; set; } = "#4ECDC4";

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 最後修改時間
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

**驗證規則**:
| 欄位 | 規則 |
|------|------|
| SensitiveKey | Required, MaxLength(200), NotReservedKeyword |
| SafeKey | Required, MaxLength(200), Unique |
| HighlightColor | Required, ValidHexColor |

---

### 2. DetectedKeyword

偵測到的關鍵字實例，包含在原文中的具體位置與狀態。

```csharp
namespace CovenantPromptKey.Models;

/// <summary>
/// 偵測到的關鍵字實例
/// </summary>
public class DetectedKeyword
{
    /// <summary>
    /// 對應的關鍵字映射
    /// </summary>
    public required KeywordMapping Mapping { get; init; }

    /// <summary>
    /// 所有出現位置
    /// </summary>
    public List<KeywordOccurrence> Occurrences { get; init; } = new();

    /// <summary>
    /// 是否選擇替換
    /// </summary>
    public bool IsSelected { get; set; } = true;

    /// <summary>
    /// 出現次數
    /// </summary>
    public int Count => Occurrences.Count;

    /// <summary>
    /// 是否有任何出現帶有語境警示
    /// </summary>
    public bool HasWarning => Occurrences.Any(o => o.HasContextWarning);
}

/// <summary>
/// 關鍵字單次出現的位置資訊
/// </summary>
public class KeywordOccurrence
{
    /// <summary>
    /// 在原文中的起始索引
    /// </summary>
    public int StartIndex { get; init; }

    /// <summary>
    /// 在原文中的結束索引
    /// </summary>
    public int EndIndex { get; init; }

    /// <summary>
    /// 所在行號 (1-based)
    /// </summary>
    public int LineNumber { get; init; }

    /// <summary>
    /// 原文中的實際文字（保留大小寫）
    /// </summary>
    public required string OriginalText { get; init; }

    /// <summary>
    /// 是否有語境警示（前後緊鄰中文字元）
    /// </summary>
    public bool HasContextWarning { get; init; }

    /// <summary>
    /// 是否位於程式碼區塊內
    /// </summary>
    public bool IsInCodeBlock { get; init; }

    /// <summary>
    /// 是否選擇替換此出現（個別控制）
    /// </summary>
    public bool IsSelected { get; set; } = true;
}
```

---

### 3. WorkSession

工作階段狀態，用於 SessionStorage 持久化。

```csharp
namespace CovenantPromptKey.Models;

/// <summary>
/// 工作階段狀態，支援頁面刷新後還原
/// </summary>
public class WorkSession
{
    /// <summary>
    /// 工作階段識別碼
    /// </summary>
    public Guid SessionId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// 當前模式
    /// </summary>
    public WorkMode Mode { get; set; } = WorkMode.Mask;

    /// <summary>
    /// 原始輸入文本
    /// </summary>
    public string SourceText { get; set; } = string.Empty;

    /// <summary>
    /// 偵測結果
    /// </summary>
    public List<DetectedKeyword> DetectedKeywords { get; set; } = new();

    /// <summary>
    /// 處理結果文本
    /// </summary>
    public string ResultText { get; set; } = string.Empty;

    /// <summary>
    /// 最後更新時間
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 工作模式
/// </summary>
public enum WorkMode
{
    /// <summary>遮罩模式</summary>
    Mask,
    /// <summary>還原模式</summary>
    Restore
}
```

---

### 4. LogEntry

除錯日誌項目。

```csharp
namespace CovenantPromptKey.Models;

/// <summary>
/// 除錯日誌項目
/// </summary>
public record LogEntry
{
    /// <summary>
    /// 時間戳記
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.Now;

    /// <summary>
    /// 日誌等級
    /// </summary>
    public LogLevel Level { get; init; }

    /// <summary>
    /// 訊息內容
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// 來源元件/服務
    /// </summary>
    public string? Source { get; init; }
}

/// <summary>
/// 日誌等級
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}
```

---

### 5. MaskResult / RestoreResult

操作結果封裝。

```csharp
namespace CovenantPromptKey.Models;

/// <summary>
/// 遮罩操作結果
/// </summary>
public class MaskResult
{
    /// <summary>
    /// 遮罩後的文本
    /// </summary>
    public required string MaskedText { get; init; }

    /// <summary>
    /// 已替換的關鍵字數量
    /// </summary>
    public int ReplacedCount { get; init; }

    /// <summary>
    /// 跳過的關鍵字數量
    /// </summary>
    public int SkippedCount { get; init; }

    /// <summary>
    /// 替換詳情
    /// </summary>
    public List<ReplacementDetail> Details { get; init; } = new();
}

/// <summary>
/// 還原操作結果
/// </summary>
public class RestoreResult
{
    /// <summary>
    /// 還原後的文本
    /// </summary>
    public required string RestoredText { get; init; }

    /// <summary>
    /// 已還原的關鍵字數量
    /// </summary>
    public int RestoredCount { get; init; }

    /// <summary>
    /// 未匹配（無法還原）的安全詞數量
    /// </summary>
    public int UnmatchedCount { get; init; }
}

/// <summary>
/// 單次替換詳情
/// </summary>
public class ReplacementDetail
{
    public required string Original { get; init; }
    public required string Replacement { get; init; }
    public int OccurrenceCount { get; init; }
}
```

---

### 6. CsvImportResult

CSV 匯入結果。

```csharp
namespace CovenantPromptKey.Models;

/// <summary>
/// CSV 匯入操作結果
/// </summary>
public class CsvImportResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// 成功匯入的數量
    /// </summary>
    public int ImportedCount { get; init; }

    /// <summary>
    /// 跳過的數量（重複項）
    /// </summary>
    public int SkippedCount { get; init; }

    /// <summary>
    /// 錯誤訊息列表
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// 警告訊息列表
    /// </summary>
    public List<string> Warnings { get; init; } = new();
}
```

---

## 常數定義 (Constants)

```csharp
namespace CovenantPromptKey.Constants;

/// <summary>
/// 應用程式常數設定
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// 最大關鍵字數量限制 (FR-032)
    /// </summary>
    public const int MaxKeywordCount = 500;

    /// <summary>
    /// 最大文本長度限制 (FR-033)
    /// </summary>
    public const int MaxTextLength = 100_000;

    /// <summary>
    /// 最大日誌行數 (Story 6)
    /// </summary>
    public const int MaxLogEntries = 5_000;

    /// <summary>
    /// 預設高亮顏色調色盤 (FR-036)
    /// </summary>
    public static readonly string[] DefaultColors = new[]
    {
        "#FF6B6B", // 珊瑚紅
        "#4ECDC4", // 青綠
        "#45B7D1", // 天藍
        "#96CEB4", // 薄荷綠
        "#FFEAA7", // 檸檬黃
        "#DDA0DD", // 梅紫
        "#98D8C8", // 淺綠
        "#F7DC6F", // 金黃
        "#BB8FCE", // 淡紫
        "#85C1E9"  // 淺藍
    };

    /// <summary>
    /// SessionStorage 鍵名
    /// </summary>
    public static class StorageKeys
    {
        public const string WorkSession = "cpk_work_session";
        public const string KeywordDictionary = "cpk_keyword_dict";
    }
}
```

---

## 程式語言保留字 (Reserved Keywords)

```csharp
namespace CovenantPromptKey.Constants;

/// <summary>
/// 程式語言保留字清單 (FR-028, FR-029)
/// </summary>
public static class ReservedKeywords
{
    public static readonly HashSet<string> CSharp = new(StringComparer.OrdinalIgnoreCase)
    {
        "public", "private", "protected", "internal", "class", "struct",
        "interface", "enum", "namespace", "using", "static", "void", "int",
        "string", "bool", "var", "const", "readonly", "new", "null", "true",
        "false", "if", "else", "switch", "case", "default", "for", "foreach",
        "while", "do", "break", "continue", "return", "throw", "try", "catch",
        "finally", "async", "await", "override", "virtual", "abstract", "sealed",
        "this", "base", "get", "set", "value", "where", "select", "from"
    };

    public static readonly HashSet<string> Python = new(StringComparer.OrdinalIgnoreCase)
    {
        "def", "class", "import", "from", "as", "if", "elif", "else", "for",
        "while", "try", "except", "finally", "with", "return", "yield", "lambda",
        "pass", "break", "continue", "and", "or", "not", "in", "is", "True",
        "False", "None", "global", "nonlocal", "async", "await", "raise"
    };

    public static readonly HashSet<string> JavaScript = new(StringComparer.OrdinalIgnoreCase)
    {
        "function", "const", "let", "var", "if", "else", "for", "while",
        "switch", "case", "break", "continue", "return", "class", "extends",
        "import", "export", "default", "async", "await", "try", "catch",
        "finally", "throw", "new", "this", "null", "undefined", "true", "false",
        "typeof", "instanceof", "delete", "void", "yield"
    };

    public static readonly HashSet<string> Java = new(StringComparer.OrdinalIgnoreCase)
    {
        "public", "private", "protected", "class", "interface", "extends",
        "implements", "static", "final", "void", "int", "boolean", "new",
        "return", "if", "else", "for", "while", "switch", "case", "break",
        "continue", "try", "catch", "finally", "throw", "throws", "import",
        "package", "this", "super", "null", "true", "false", "abstract"
    };

    /// <summary>
    /// 所有語言的保留字聯集
    /// </summary>
    public static HashSet<string> All => CSharp
        .Union(Python)
        .Union(JavaScript)
        .Union(Java)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 檢查是否為保留字
    /// </summary>
    public static bool IsReserved(string keyword) =>
        All.Contains(keyword);

    /// <summary>
    /// 檢查是否為保留字的子字串（長度 < 4 時警告）
    /// </summary>
    public static bool IsSubstringOfReserved(string keyword, out string matchedKeyword)
    {
        matchedKeyword = All.FirstOrDefault(r =>
            r.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
            !r.Equals(keyword, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
        return !string.IsNullOrEmpty(matchedKeyword);
    }
}
```

---

## 實體關係圖 (Entity Relationship)

```
┌──────────────────┐
│  KeywordMapping  │
├──────────────────┤
│ Id (PK)          │
│ SensitiveKey     │◄──┐
│ SafeKey (Unique) │   │
│ HighlightColor   │   │
│ CreatedAt        │   │
│ UpdatedAt        │   │
└──────────────────┘   │
         │             │
         │ 1:N         │
         ▼             │
┌──────────────────┐   │
│ DetectedKeyword  │   │
├──────────────────┤   │
│ Mapping (FK)     │───┘
│ IsSelected       │
│ Occurrences[]    │
└──────────────────┘
         │
         │ 1:N
         ▼
┌──────────────────┐
│ KeywordOccurrence│
├──────────────────┤
│ StartIndex       │
│ EndIndex         │
│ LineNumber       │
│ OriginalText     │
│ HasContextWarning│
│ IsInCodeBlock    │
│ IsSelected       │
└──────────────────┘
```

---

## 狀態轉換圖 (State Transitions)

### WorkSession 狀態

```
                    ┌─────────────┐
                    │   Initial   │
                    └──────┬──────┘
                           │ 使用者貼上文本
                           ▼
                    ┌─────────────┐
              ┌────►│  Detecting  │
              │     └──────┬──────┘
              │            │ 偵測完成
              │            ▼
              │     ┌─────────────┐
   重新偵測   │     │  Detected   │◄───┐
              │     └──────┬──────┘    │
              │            │           │ 切換勾選
              │            ▼           │
              │     ┌─────────────┐    │
              └─────┤  Selecting  │────┘
                    └──────┬──────┘
                           │ 執行替換
                           ▼
                    ┌─────────────┐
                    │ Confirming  │
                    └──────┬──────┘
                           │ 確認
                           ▼
                    ┌─────────────┐
                    │  Completed  │
                    └─────────────┘
```

---

## 驗證規則摘要 (Validation Summary)

| 實體 | 欄位 | 規則 | 錯誤代碼 |
|------|------|------|---------|
| KeywordMapping | SensitiveKey | 不可為空白 | ERR_EMPTY_SENSITIVE |
| KeywordMapping | SensitiveKey | 最大 200 字元 | ERR_SENSITIVE_TOO_LONG |
| KeywordMapping | SensitiveKey | 不可為保留字 | ERR_RESERVED_KEYWORD |
| KeywordMapping | SafeKey | 不可為空白 | ERR_EMPTY_SAFE |
| KeywordMapping | SafeKey | 最大 200 字元 | ERR_SAFE_TOO_LONG |
| KeywordMapping | SafeKey | 必須唯一 | ERR_DUPLICATE_SAFE |
| KeywordMapping | HighlightColor | 有效 HEX 格式 | ERR_INVALID_COLOR |
| Dictionary | Count | 最大 500 筆 | ERR_MAX_KEYWORDS |
| SourceText | Length | 最大 100,000 字元 | ERR_TEXT_TOO_LONG |
