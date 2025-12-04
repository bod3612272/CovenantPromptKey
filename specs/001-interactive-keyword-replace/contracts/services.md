# Service Contracts: 互動式關鍵字替換介面

**Branch**: `001-interactive-keyword-replace` | **Date**: 2025-12-03

本文件定義各服務的介面契約，供實作階段參照。

---

## 1. IKeywordService

關鍵字偵測與替換核心服務。

```csharp
namespace CovenantPromptKey.Services;

/// <summary>
/// 關鍵字偵測與替換服務介面
/// </summary>
public interface IKeywordService
{
    /// <summary>
    /// 偵測文本中的所有關鍵字
    /// </summary>
    /// <param name="text">待偵測的原始文本</param>
    /// <param name="dictionary">關鍵字字典</param>
    /// <returns>偵測結果列表</returns>
    /// <remarks>
    /// - 採用 Aho-Corasick 演算法進行多模式匹配
    /// - 最長匹配優先
    /// - 自動標記語境警示（前後緊鄰中文字元）
    /// - 識別 Markdown 程式碼區塊並啟用全詞匹配
    /// - 保護 Markdown URL 區域
    /// </remarks>
    Task<List<DetectedKeyword>> DetectKeywordsAsync(
        string text,
        IReadOnlyList<KeywordMapping> dictionary);

    /// <summary>
    /// 執行遮罩替換
    /// </summary>
    /// <param name="text">原始文本</param>
    /// <param name="detectedKeywords">偵測結果（包含勾選狀態）</param>
    /// <returns>遮罩結果</returns>
    Task<MaskResult> ApplyMaskAsync(
        string text,
        List<DetectedKeyword> detectedKeywords);

    /// <summary>
    /// 執行還原操作
    /// </summary>
    /// <param name="maskedText">已遮罩的文本（如 AI 回應）</param>
    /// <param name="dictionary">關鍵字字典</param>
    /// <returns>還原結果</returns>
    Task<RestoreResult> RestoreTextAsync(
        string maskedText,
        IReadOnlyList<KeywordMapping> dictionary);

    /// <summary>
    /// 驗證文本長度
    /// </summary>
    /// <param name="text">待驗證文本</param>
    /// <returns>是否在允許範圍內（≤100,000 字元）</returns>
    bool ValidateTextLength(string text);
}
```

---

## 2. IDictionaryService

關鍵字字典管理服務。

```csharp
namespace CovenantPromptKey.Services;

/// <summary>
/// 關鍵字字典管理服務介面
/// </summary>
public interface IDictionaryService
{
    /// <summary>
    /// 當字典變更時觸發
    /// </summary>
    event Action? OnDictionaryChanged;

    /// <summary>
    /// 取得所有關鍵字映射
    /// </summary>
    Task<IReadOnlyList<KeywordMapping>> GetAllAsync();

    /// <summary>
    /// 取得單一關鍵字映射
    /// </summary>
    Task<KeywordMapping?> GetByIdAsync(Guid id);

    /// <summary>
    /// 新增關鍵字映射
    /// </summary>
    /// <returns>驗證結果，成功時包含新建的映射</returns>
    /// <remarks>
    /// 驗證規則：
    /// - SensitiveKey 不可為空白
    /// - SafeKey 必須唯一
    /// - 不可為程式語言保留字
    /// - 字典總數不超過 500
    /// </remarks>
    Task<ValidationResult<KeywordMapping>> AddAsync(KeywordMapping mapping);

    /// <summary>
    /// 更新關鍵字映射
    /// </summary>
    Task<ValidationResult<KeywordMapping>> UpdateAsync(KeywordMapping mapping);

    /// <summary>
    /// 刪除關鍵字映射
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// 清空所有關鍵字
    /// </summary>
    Task ClearAllAsync();

    /// <summary>
    /// 取得下一個可用的預設顏色
    /// </summary>
    string GetNextDefaultColor();

    /// <summary>
    /// 取得當前關鍵字數量
    /// </summary>
    Task<int> GetCountAsync();
}

/// <summary>
/// 驗證結果封裝
/// </summary>
public class ValidationResult<T>
{
    public bool IsValid { get; init; }
    public T? Value { get; init; }
    public List<string> Errors { get; init; } = new();
    public List<string> Warnings { get; init; } = new();

    public static ValidationResult<T> Success(T value) =>
        new() { IsValid = true, Value = value };

    public static ValidationResult<T> Failure(params string[] errors) =>
        new() { IsValid = false, Errors = errors.ToList() };

    public static ValidationResult<T> SuccessWithWarnings(T value, params string[] warnings) =>
        new() { IsValid = true, Value = value, Warnings = warnings.ToList() };
}
```

---

## 3. ICsvService

CSV 匯入/匯出服務。

```csharp
namespace CovenantPromptKey.Services;

/// <summary>
/// CSV 匯入/匯出服務介面
/// </summary>
public interface ICsvService
{
    /// <summary>
    /// 匯出關鍵字字典為 CSV
    /// </summary>
    /// <param name="mappings">關鍵字映射列表</param>
    /// <returns>CSV 內容字串</returns>
    string ExportToCsv(IReadOnlyList<KeywordMapping> mappings);

    /// <summary>
    /// 從 CSV 匯入關鍵字
    /// </summary>
    /// <param name="csvContent">CSV 內容</param>
    /// <returns>匯入結果</returns>
    /// <remarks>
    /// 驗證規則：
    /// - 欄位數量必須正確
    /// - 不可包含未封閉的引號
    /// - SafeKey 不可重複
    /// - 保留字警告
    /// </remarks>
    CsvImportResult ImportFromCsv(string csvContent);

    /// <summary>
    /// 驗證 CSV 格式
    /// </summary>
    /// <param name="csvContent">CSV 內容</param>
    /// <returns>驗證結果，包含錯誤行號與訊息</returns>
    CsvValidationResult ValidateCsv(string csvContent);
}

/// <summary>
/// CSV 驗證結果
/// </summary>
public class CsvValidationResult
{
    public bool IsValid { get; init; }
    public List<CsvError> Errors { get; init; } = new();
}

/// <summary>
/// CSV 錯誤詳情
/// </summary>
public class CsvError
{
    public int LineNumber { get; init; }
    public string Message { get; init; } = string.Empty;
}
```

---

## 4. ISessionStorageService

瀏覽器 SessionStorage 互動服務。

```csharp
namespace CovenantPromptKey.Services;

/// <summary>
/// 瀏覽器 SessionStorage 服務介面
/// </summary>
public interface ISessionStorageService
{
    /// <summary>
    /// 取得指定鍵的值
    /// </summary>
    Task<T?> GetItemAsync<T>(string key);

    /// <summary>
    /// 設定指定鍵的值
    /// </summary>
    Task SetItemAsync<T>(string key, T value);

    /// <summary>
    /// 移除指定鍵
    /// </summary>
    Task RemoveItemAsync(string key);

    /// <summary>
    /// 清空所有資料
    /// </summary>
    Task ClearAsync();
}
```

---

## 5. IWorkSessionService

工作階段管理服務。

```csharp
namespace CovenantPromptKey.Services;

/// <summary>
/// 工作階段管理服務介面
/// </summary>
public interface IWorkSessionService
{
    /// <summary>
    /// 當工作階段變更時觸發
    /// </summary>
    event Action? OnSessionChanged;

    /// <summary>
    /// 取得當前工作階段
    /// </summary>
    Task<WorkSession> GetCurrentSessionAsync();

    /// <summary>
    /// 儲存工作階段
    /// </summary>
    Task SaveSessionAsync(WorkSession session);

    /// <summary>
    /// 重置工作階段
    /// </summary>
    Task ResetSessionAsync();

    /// <summary>
    /// 更新來源文本
    /// </summary>
    Task UpdateSourceTextAsync(string text);

    /// <summary>
    /// 更新偵測結果
    /// </summary>
    Task UpdateDetectedKeywordsAsync(List<DetectedKeyword> keywords);

    /// <summary>
    /// 更新結果文本
    /// </summary>
    Task UpdateResultTextAsync(string text);

    /// <summary>
    /// 切換工作模式
    /// </summary>
    Task SetModeAsync(WorkMode mode);
}
```

---

## 6. IDebugLogService

除錯日誌服務。

```csharp
namespace CovenantPromptKey.Services;

/// <summary>
/// 除錯日誌服務介面
/// </summary>
public interface IDebugLogService
{
    /// <summary>
    /// 當新日誌項目新增時觸發
    /// </summary>
    event Action? OnLogAdded;

    /// <summary>
    /// 記錄訊息
    /// </summary>
    /// <param name="message">訊息內容</param>
    /// <param name="level">日誌等級</param>
    /// <param name="source">來源元件/服務</param>
    void Log(string message, LogLevel level = LogLevel.Info, string? source = null);

    /// <summary>
    /// 取得所有日誌（最新在前）
    /// </summary>
    IReadOnlyList<LogEntry> GetLogs();

    /// <summary>
    /// 取得格式化的日誌文字（用於複製）
    /// </summary>
    string GetFormattedLogs();

    /// <summary>
    /// 清空所有日誌
    /// </summary>
    void Clear();

    /// <summary>
    /// 取得當前日誌數量
    /// </summary>
    int Count { get; }
}
```

---

## 7. IMarkdownService

Markdown 解析服務。

```csharp
namespace CovenantPromptKey.Services;

/// <summary>
/// Markdown 解析服務介面
/// </summary>
public interface IMarkdownService
{
    /// <summary>
    /// 分析 Markdown 結構
    /// </summary>
    /// <param name="text">Markdown 文本</param>
    /// <returns>結構分析結果</returns>
    MarkdownStructure Analyze(string text);
}

/// <summary>
/// Markdown 結構分析結果
/// </summary>
public class MarkdownStructure
{
    /// <summary>
    /// URL 保護區域列表
    /// </summary>
    public List<TextRange> ProtectedUrls { get; init; } = new();

    /// <summary>
    /// 程式碼區塊區域列表（需全詞匹配）
    /// </summary>
    public List<TextRange> CodeBlocks { get; init; } = new();

    /// <summary>
    /// 行內程式碼區域列表
    /// </summary>
    public List<TextRange> InlineCode { get; init; } = new();
}

/// <summary>
/// 文本範圍
/// </summary>
public record TextRange(int Start, int End);
```

---

## 8. IKeywordValidationService

關鍵字驗證服務。

```csharp
namespace CovenantPromptKey.Services;

/// <summary>
/// 關鍵字驗證服務介面
/// </summary>
public interface IKeywordValidationService
{
    /// <summary>
    /// 驗證新關鍵字映射
    /// </summary>
    /// <param name="mapping">待驗證的映射</param>
    /// <param name="existingMappings">現有映射列表</param>
    /// <returns>驗證結果</returns>
    ValidationResult<KeywordMapping> Validate(
        KeywordMapping mapping,
        IReadOnlyList<KeywordMapping> existingMappings);

    /// <summary>
    /// 檢查是否為保留字
    /// </summary>
    bool IsReservedKeyword(string keyword);

    /// <summary>
    /// 檢查是否為保留字子字串
    /// </summary>
    /// <param name="keyword">關鍵字</param>
    /// <param name="matchedReserved">匹配到的保留字</param>
    bool IsSubstringOfReserved(string keyword, out string matchedReserved);

    /// <summary>
    /// 檢查 SafeKey 唯一性
    /// </summary>
    bool IsSafeKeyUnique(string safeKey, IReadOnlyList<KeywordMapping> existingMappings, Guid? excludeId = null);
}
```

---

## 服務註冊 (Dependency Injection)

```csharp
// Program.cs 中的服務註冊

// Singleton 服務（全域狀態）
builder.Services.AddSingleton<IDebugLogService, DebugLogService>();

// Scoped 服務（每個 Circuit 一個實例）
builder.Services.AddScoped<ISessionStorageService, SessionStorageService>();
builder.Services.AddScoped<IWorkSessionService, WorkSessionService>();
builder.Services.AddScoped<IDictionaryService, DictionaryService>();

// Transient 服務（無狀態）
builder.Services.AddTransient<IKeywordService, KeywordService>();
builder.Services.AddTransient<ICsvService, CsvService>();
builder.Services.AddTransient<IMarkdownService, MarkdownService>();
builder.Services.AddTransient<IKeywordValidationService, KeywordValidationService>();
```

---

## 錯誤碼對照表 (Error Codes)

| 代碼 | 訊息 (zh-TW) | 訊息 (en-GB) |
|------|-------------|--------------|
| ERR_EMPTY_SENSITIVE | 機敏詞不可為空白 | Sensitive key cannot be empty |
| ERR_SENSITIVE_TOO_LONG | 機敏詞長度超過 200 字元 | Sensitive key exceeds 200 characters |
| ERR_RESERVED_KEYWORD | '{0}' 是受保護的程式語言關鍵字，無法作為替換目標 | '{0}' is a protected programming keyword |
| ERR_EMPTY_SAFE | 替代詞不可為空白 | Safe key cannot be empty |
| ERR_SAFE_TOO_LONG | 替代詞長度超過 200 字元 | Safe key exceeds 200 characters |
| ERR_DUPLICATE_SAFE | 替代詞 '{0}' 已被使用，請提供唯一的替代詞 | Safe key '{0}' is already in use |
| ERR_INVALID_COLOR | 顏色格式無效，請使用 HEX 格式 (如 #FF6B6B) | Invalid colour format, use HEX (e.g. #FF6B6B) |
| ERR_MAX_KEYWORDS | 已達關鍵字數量上限 (500)，請刪除部分項目後再試 | Maximum keywords limit (500) reached |
| ERR_TEXT_TOO_LONG | 文本長度超過 100,000 字元，請分批處理 | Text exceeds 100,000 characters limit |
| ERR_CSV_INVALID_FORMAT | 匯入失敗：檔案在第 {0} 行格式有誤 | Import failed: invalid format at line {0} |
| WARN_SUBSTRING_RESERVED | 警告：'{0}' 是程式語言關鍵字 '{1}' 的一部分，可能導致非預期替換 | Warning: '{0}' is part of reserved keyword '{1}' |
