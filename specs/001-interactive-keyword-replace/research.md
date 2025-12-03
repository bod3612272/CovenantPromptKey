# Research: 互動式關鍵字替換介面

**Branch**: `001-interactive-keyword-replace` | **Date**: 2025-12-03

## 概要 (Executive Summary)

本研究旨在解決互動式關鍵字替換功能開發過程中所有技術不確定性，為後續設計與實作階段提供明確的技術決策依據。

---

## 技術決策記錄 (Technical Decisions)

### 1. 前端框架與渲染模式

**決策**: 採用 Blazor Server-Side Rendering with Interactive Server Components

**理由**:
- 專案已設定為 Blazor Server (`AddInteractiveServerComponents`)
- .NET 10.0 目標框架已於 `CovenantPromptKey.csproj` 中設定
- Server-Side 模式適合需要即時互動與狀態管理的場景
- 無需額外處理 WASM 下載延遲問題

**替代方案評估**:
- **Blazor WASM**: 需要較長的初始載入時間，對於開發者工具類應用不太理想
- **Blazor Hybrid**: 過於複雜，且目前無桌面應用需求

---

### 2. 狀態管理策略

**決策**: 採用 Cascading Parameter + Service Layer 搭配 Browser SessionStorage

**理由**:
- Blazor Server 已提供 Circuit 級別的狀態保持
- SessionStorage 用於瀏覽器刷新時的工作階段恢復 (FR-016)
- 採用 Service Layer 集中管理業務邏輯，符合 SOLID 原則
- Cascading Parameter 簡化跨元件狀態傳遞

**實作方式**:
```csharp
// 使用 IJSRuntime 與 sessionStorage 互動
public interface ISessionStorageService
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
    Task RemoveItemAsync(string key);
}
```

**替代方案評估**:
- **Redux-like State Container**: 過於複雜，YAGNI 原則
- **Fluxor**: 引入額外相依性，對此規模專案過重

---

### 3. 關鍵字偵測演算法

**決策**: 採用 Aho-Corasick 多模式字串匹配演算法

**理由**:
- 需支援最多 500 組關鍵字 (FR-032)
- 需支援最多 100,000 字元文本 (FR-033)
- Aho-Corasick 時間複雜度為 O(n + m + z)，n 為文本長度，m 為所有模式總長，z 為匹配數
- 單次掃描即可找出所有匹配，效能優異

**實作選項**:
1. 使用 NuGet 套件 `AhoCorasickNet` 或 `Aho-Corasick`
2. 自行實作以完全掌控邏輯（建議，避免外部相依）

**最長匹配優先處理** (Edge Case):
- 當 "Project" 與 "ProjectX" 同時存在時，先匹配最長者
- 實作時需對結果依位置排序並過濾重疊區間

**替代方案評估**:
- **簡單字串搜尋 (String.IndexOf)**: O(n×m) 複雜度，500 個關鍵字效能不佳
- **正則表達式**: 難以處理大量動態模式，且 Regex 編譯成本高

---

### 4. CSV 匯入/匯出處理

**決策**: 使用 CsvHelper 函式庫

**理由**:
- 成熟穩定的 .NET CSV 處理函式庫
- 內建完善的錯誤處理與驗證機制 (FR-022)
- 支援欄位映射與自訂格式
- NuGet 套件：`CsvHelper`

**CSV 格式規範**:
```csv
SensitiveKey,SafeKey,Color
武科電,T-Company,#FF6B6B
Eason,Dev_A,#4ECDC4
```

**替代方案評估**:
- **手動解析**: 容易忽略邊界情況（如引號內逗號、換行符）
- **NPOI/EPPlus (Excel)**: 過於複雜，使用者期望 CSV 格式

---

### 5. Markdown 解析與保護

**決策**: 使用 Markdig 函式庫解析 Markdown 結構

**理由**:
- .NET 生態系最成熟的 Markdown 解析器
- 可識別程式碼區塊、URL 等結構
- 支援擴展，便於後續功能添加
- NuGet 套件：`Markdig`

**處理邏輯**:
1. 解析 Markdown AST
2. 標記 URL 區域為「保護區」，不進行替換 (FR-007)
3. 標記程式碼區塊為「全詞匹配區」，僅匹配完整單詞 (FR-027)
4. 其餘區域正常偵測

**替代方案評估**:
- **正則表達式匹配**: 無法處理巢狀結構，容易出錯
- **CommonMark.NET**: 功能較少，擴展性不足

---

### 6. 前端 UI 元件策略

**決策**: 使用 Bootstrap 5 + 自訂 CSS，不引入額外元件庫

**理由**:
- 專案已包含 Bootstrap (`wwwroot/lib/bootstrap/`)
- 三欄式佈局可使用 Bootstrap Grid 或 CSS Flexbox/Grid
- 避免引入過多相依性
- 可透過 Blazor CSS Isolation 維護元件樣式

**UI 元件需求**:
- 頁籤元件 (Tabs): Bootstrap Nav Tabs
- 對話框 (Modal): Bootstrap Modal
- 按鈕群組: Bootstrap Button Group
- 顏色選擇器: 自訂 8-10 色 Swatch 選擇器

---

### 7. 程式語言保留字清單

**決策**: 建立內建靜態清單，涵蓋主流語言關鍵字

**理由**:
- 需防止使用者新增程式語言保留字 (FR-028)
- 需警告子字串匹配風險 (FR-029)

**支援語言清單**:
- C#: `public`, `private`, `class`, `namespace`, `void`, `int`, `string`, `var`, `using`, `return`, `if`, `else`, `for`, `foreach`, `while`, `switch`, `case`, `break`, `continue`, `new`, `null`, `true`, `false`, `async`, `await`, `static`, `override`, `virtual`, `abstract`, `interface`, `enum`, `struct`, `this`, `base`, `try`, `catch`, `finally`, `throw`
- Python: `def`, `class`, `import`, `from`, `as`, `if`, `elif`, `else`, `for`, `while`, `try`, `except`, `finally`, `with`, `return`, `yield`, `lambda`, `pass`, `break`, `continue`, `and`, `or`, `not`, `in`, `is`, `True`, `False`, `None`, `global`, `nonlocal`, `async`, `await`
- Java: `public`, `private`, `protected`, `class`, `interface`, `extends`, `implements`, `static`, `final`, `void`, `int`, `boolean`, `new`, `return`, `if`, `else`, `for`, `while`, `switch`, `case`, `break`, `continue`, `try`, `catch`, `finally`, `throw`, `throws`, `import`, `package`, `this`, `super`, `null`, `true`, `false`
- JavaScript/TypeScript: `function`, `const`, `let`, `var`, `if`, `else`, `for`, `while`, `switch`, `case`, `break`, `continue`, `return`, `class`, `extends`, `import`, `export`, `default`, `async`, `await`, `try`, `catch`, `finally`, `throw`, `new`, `this`, `null`, `undefined`, `true`, `false`, `typeof`, `instanceof`

---

### 8. Debug Log 實作方式

**決策**: 使用記憶體內環形緩衝區 (Ring Buffer) + Singleton Service

**理由**:
- 最多保留 5,000 行 (FR-033)
- 需全域統一記錄 (Story 6, Scenario 4)
- 僅 UI 顯示，不存檔

**實作架構**:
```csharp
public interface IDebugLogService
{
    void Log(string message, LogLevel level = LogLevel.Info);
    IReadOnlyList<LogEntry> GetLogs();
    void Clear();
    event Action? OnLogAdded;
}
```

---

### 9. 大小寫匹配策略

**決策**: 預設不區分大小寫，但保留原文大小寫樣式

**理由**:
- 符合 Clarification 中的決策
- 例如：關鍵字 "eason"，原文 "Eason"，應匹配但替換時顯示原始的 "Eason" 格式

**實作方式**:
- 匹配時使用 `StringComparison.OrdinalIgnoreCase`
- 儲存匹配位置與原始文字，不改變視覺呈現

---

### 10. 全單詞匹配邏輯 (Whole-Word Match)

**決策**: 使用單詞邊界檢測

**理由**:
- 程式碼區塊需全詞匹配 (FR-027)
- 避免 "task" 匹配到 "TaskRunner" 中的 "Task"

**邊界定義**:
- 單詞邊界字元：空白、標點符號、括號、運算符號
- 正則表達式模式：`\b{keyword}\b` (英文)
- 中文需額外處理：中文字元本身即為邊界

---

## 效能考量

### 關鍵字偵測效能目標

| 場景 | 文本長度 | 關鍵字數量 | 目標回應時間 |
|------|---------|-----------|-------------|
| 小型 | 1,000 字元 | 50 | < 50ms |
| 中型 | 10,000 字元 | 200 | < 200ms |
| 大型 | 100,000 字元 | 500 | < 1,000ms |

### UI 回應效能

- 捲動定位動畫：< 300ms
- 勾選狀態切換：< 16ms (60 fps)
- 高亮重繪：< 100ms

---

## 相依套件清單

| 套件名稱 | 版本 | 用途 |
|---------|------|------|
| CsvHelper | 最新穩定版 | CSV 匯入/匯出 |
| Markdig | 最新穩定版 | Markdown 解析 |
| coverlet.collector | 最新穩定版 | 程式碼覆蓋率收集 |
| xunit | 最新穩定版 | 單元測試框架 |
| xunit.runner.visualstudio | 最新穩定版 | VS Test Runner 整合 |

---

## TDD 開發流程 (Test-Driven Development)

### 開發方法論

後端核心服務採用 **TDD (Red-Green-Refactor)** 流程：

1. **Red (紅燈)**: 先撰寫失敗的測試案例，定義預期行為
2. **Green (綠燈)**: 實作最小可行程式碼使測試通過
3. **Refactor (重構)**: 優化程式碼結構，確保測試仍通過

### 測試覆蓋率目標

| 服務層級 | 覆蓋率目標 | 備註 |
|---------|-----------|------|
| 核心服務 (Services/) | ≥ 50% | 必須達成 |
| 前端 UI (Components/) | N/A | 手動驗證，不計入覆蓋率 |

### 需 TDD 開發的核心服務

| 服務 | 測試重點 |
|------|---------|
| `KeywordService` | 偵測演算法、最長匹配、語境警示 |
| `KeywordValidationService` | 保留字檢查、SafeKey 唯一性、長度限制 |
| `CsvService` | 匯入解析、匯出格式、錯誤處理 |
| `MarkdownService` | URL 保護區、程式碼區塊識別 |
| `DictionaryService` | CRUD 操作、數量限制、事件觸發 |
| `DebugLogService` | 環形緩衝區、最大行數限制 |

### 測試專案結構

```text
CovenantPromptKey.Tests/
├── CovenantPromptKey.Tests.csproj
├── Services/
│   ├── KeywordServiceTests.cs
│   ├── KeywordValidationServiceTests.cs
│   ├── CsvServiceTests.cs
│   ├── MarkdownServiceTests.cs
│   ├── DictionaryServiceTests.cs
│   └── DebugLogServiceTests.cs
├── TestData/
│   ├── valid-keywords.csv
│   ├── invalid-keywords.csv
│   └── sample-markdown.md
└── Helpers/
    └── TestFixtures.cs
```

### 覆蓋率檢查指令

```powershell
# 執行測試並產生覆蓋率報告
dotnet test --collect:"XPlat Code Coverage"

# 產生 HTML 報告 (需安裝 reportgenerator)
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

---

## 待確認事項 (Resolved)

所有 NEEDS CLARIFICATION 項目已於本研究中解決，無待確認事項。
