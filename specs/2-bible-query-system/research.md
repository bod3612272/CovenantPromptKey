# Research: 聖經查詢系統 (Bible Query System)

**Date**: 2025-12-06  
**Status**: Completed  
**Related Plan**: [plan.md](./plan.md)

---

## Research Summary

本文件記錄聖經查詢系統功能開發前的技術研究與決策，確保所有技術選型與實作方式皆經過充分評估。

---

## 1. BibleData DLL 整合方式

### Decision
採用 **DLL 參考方式** 整合 BibleData 類別庫，並於 `Program.cs` 註冊 `BibleIndex` 為 Singleton。

### Rationale
- BibleData DLL 為 .NET Standard 2.1，與 .NET 10.0 完全相容
- `BibleIndex` 採用字典索引實現 O(1) 查詢，建議應用程式啟動時建立一次
- Singleton 生命週期可避免重複建立索引，節省記憶體與 CPU 資源

### Alternatives Considered
| 方案 | 優點 | 缺點 | 結論 |
|------|------|------|------|
| 專案參考 (ProjectReference) | 方便除錯 | 需要 BibleData 原始碼在 Solution 中 | 不適用（DLL 已編譯） |
| NuGet 套件 | 版本管理方便 | BibleData 未發布至 NuGet | 不適用 |
| **DLL 參考** | 簡單直接，無需原始碼 | 需手動管理 DLL 更新 | ✅ **採用** |

### Implementation Notes
```xml
<!-- CovenantPromptKey.csproj -->
<ItemGroup>
  <Reference Include="BibleData">
    <HintPath>..\libs\BibleData.dll</HintPath>
  </Reference>
</ItemGroup>
```

```csharp
// Program.cs
builder.Services.AddSingleton<BibleIndex>();
```

---

## 2. 即時搜尋實作策略

### Decision
採用 **防抖動 (Debounce) + 可取消搜尋 (CancellationToken)** 策略，延遲 250ms 後執行搜尋。

### Rationale
- BibleData 提供 `SearchTopWithCancellation` 方法，原生支援取消功能
- 防抖動可減少不必要的搜尋請求，提升使用者體驗
- 250ms 延遲在 spec 要求的 200-300ms 範圍內，平衡即時性與效能

### Alternatives Considered
| 方案 | 優點 | 缺點 | 結論 |
|------|------|------|------|
| 無防抖動 | 最即時 | 大量無用搜尋，效能差 | ❌ 不採用 |
| 節流 (Throttle) | 固定頻率執行 | 可能遺漏最後輸入 | ❌ 不採用 |
| **防抖動 + 取消** | 平衡即時與效能，可取消過時搜尋 | 實作稍複雜 | ✅ **採用** |

### Implementation Pattern
```csharp
private CancellationTokenSource? _cts;

private async Task OnSearchInput(ChangeEventArgs e)
{
    _cts?.Cancel();
    _cts = new CancellationTokenSource();
    
    var keyword = e.Value?.ToString() ?? "";
    if (string.IsNullOrWhiteSpace(keyword)) return;
    
    try
    {
        await Task.Delay(250, _cts.Token);
        var results = _bibleIndex.SearchTopWithCancellation(keyword, 20, _cts.Token);
        // Update UI
    }
    catch (TaskCanceledException) { /* Ignored */ }
}
```

---

## 3. 狀態儲存策略

### Decision
採用 **localStorage + sessionStorage 分層儲存** 策略。

### Rationale
- 顯示設定、書籤、遊戲記錄需跨工作階段保留 → localStorage
- 頁面狀態（當前章節、搜尋關鍵字）僅需工作階段期間保留 → sessionStorage
- 沿用現有 `ISessionStorageService` 介面模式，保持一致性

### Storage Allocation

| 資料類型 | 儲存位置 | Key 格式 | 說明 |
|----------|----------|----------|------|
| 顯示設定 | localStorage | `bible_settings` | 字形、大小、顏色等 |
| 書籤列表 | localStorage | `bible_bookmarks` | 最近 10 筆閱讀記錄 |
| 遊戲記錄 | localStorage | `bible_game_records` | 最高分 + 最近 5 次 |
| 錯題記錄 | localStorage | `bible_wrong_answers` | 最近 5 次遊戲的錯題（最多 50 題） |
| 查詢頁狀態 | sessionStorage | `bible_search_state` | 關鍵字、分頁位置 |
| 閱讀頁狀態 | sessionStorage | `bible_read_state` | 書卷、章節 |
| 遊戲頁狀態 | sessionStorage | `bible_game_state` | 當前遊戲進度（若中斷不保留） |

### Wrong Answers Storage Considerations
- **目的**：幫助使用者學習與回憶錯誤，強化聖經知識
- **保留策略**：最近 5 次遊戲的錯題，採用 FIFO（先進先出）
- **最大容量**：若 5 次遊戲全部答錯，共 5 × 10 = 50 題
- **估算大小**：約 15 KB（包含經文內容、出處、答案等）
- **獨立清除**：支援單獨清除錯題記錄，不影響遊戲分數記錄

### Alternatives Considered
| 方案 | 優點 | 缺點 | 結論 |
|------|------|------|------|
| 全部 localStorage | 所有狀態永久保留 | 頁面狀態不應永久保留 | ❌ 不採用 |
| 全部 sessionStorage | 關閉即清除 | 設定需永久保留 | ❌ 不採用 |
| **分層儲存** | 依資料特性選擇適當儲存 | 需區分兩種 Service | ✅ **採用** |
| IndexedDB | 支援大量結構化資料 | 過於複雜，資料量不大 | ❌ 不採用 |

---

## 4. 搜尋結果排序演算法

### Decision
採用 BibleData 內建的 **SearchTopRanked** 方法，依據相關性分數排序。

### Rationale
- BibleData 已實作排名搜尋，分數依據關鍵字出現位置與次數計算
- 無需自行實作排序邏輯，減少開發與維護成本
- 排名結果包含 `Score`、`MatchIndex`、`OccurrenceCount` 供進階應用

### SearchResult Properties
```csharp
public class SearchResult
{
    public VerseWithLocation Verse { get; set; }
    public double Score { get; set; }           // 相關性分數
    public int MatchIndex { get; set; }         // 首次出現位置
    public int OccurrenceCount { get; set; }    // 出現次數
}
```

### Usage
```csharp
// 取得前 20 筆排名結果
var results = _bibleIndex.SearchTopRanked("愛", 20);
```

---

## 5. 多關鍵字搜尋邏輯

### Decision
採用 **AND 邏輯**（所有關鍵字皆須包含），使用空格分隔關鍵字。

### Rationale
- spec 明確要求 AND 邏輯（FR-013）
- BibleData 提供 `SearchMultipleKeywords` 方法原生支援
- 空格分隔符合使用者直覺輸入習慣

### Implementation
```csharp
public List<VerseWithLocation> SearchMultiple(string input, int top = 20)
{
    var keywords = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (keywords.Length <= 1)
        return _bibleIndex.SearchTop(input, top);
    
    return _bibleIndex.SearchMultipleKeywords(keywords, top);
}
```

---

## 6. 經文導出格式實作

### Decision
採用 **策略模式 (Strategy Pattern)** 實作三種導出風格。

### Rationale
- 三種風格各有不同格式化邏輯，策略模式可清晰分離
- 便於日後新增其他導出風格
- 符合 SOLID 開放封閉原則

### Export Styles
| 風格 | 格式範例 | 說明 |
|------|----------|------|
| Style 1 | `(馬可福音 1:1) 經文內容` | 每節前加章節標記 |
| Style 2 | 標題 + 逐行經文 | `書卷 ( 第 X 章 Y ~ Z 節 )` |
| Style 3 | 標題 + 連續段落 | `《書卷》( 第 X 章 Y ~ Z 節 )` |

### Interface Design
```csharp
public interface IBibleExportFormatter
{
    string Format(IEnumerable<VerseWithLocation> verses, ExportOptions options);
}

public enum ExportStyle { Style1, Style2, Style3 }
```

---

## 7. 遊戲隨機選項生成

### Decision
採用 **隨機抽取 + 排除正確答案 + 避免重複** 策略。

### Rationale
- 確保 4 個選項中有且僅有 1 個正確答案
- 錯誤選項從 66 卷書中隨機抽取，避免重複
- 使用 `Random` 類別搭配 Fisher-Yates 洗牌演算法

### Implementation Pattern
```csharp
public List<string> GenerateOptions(string correctBook, int optionCount = 4)
{
    var allBooks = _bibleIndex.BookNames.ToList();
    allBooks.Remove(correctBook);
    
    var random = new Random();
    var wrongOptions = allBooks.OrderBy(_ => random.Next()).Take(optionCount - 1).ToList();
    
    var options = new List<string> { correctBook };
    options.AddRange(wrongOptions);
    
    return options.OrderBy(_ => random.Next()).ToList(); // 打亂順序
}
```

---

## 8. 導航選單擴展方式

### Decision
採用 **可展開子選單** 模式，於現有 NavMenu.razor 新增「聖經」分類。

### Rationale
- 符合 spec 要求的選單結構（FR-001）
- 沿用現有 Bootstrap 導航元件
- 子選項可收合，不佔用過多空間

### UI Structure
```
聖經 (可展開/收合)
├── 聖經查詢
├── 聖經閱讀
└── 聖經遊戲
```

---

## 9. 測試框架選擇

### Decision
沿用現有測試框架：**xUnit + NSubstitute**。

### Rationale
- 專案已使用 xUnit，保持一致性
- 已安裝 NSubstitute 用於 Mock
- **注意**：Constitution 要求 NUnit，但現有專案使用 xUnit。基於 Brownfield 原則，維持現有框架不變更。

### Deviation Note
Constitution VII 要求 NUnit 4.x + FluentAssertions 6.x，但現有專案使用 xUnit + NSubstitute。依據 Constitution I (Brownfield Project Respect) 原則，不變更現有測試框架。

---

## 10. 頁面狀態管理模式

### Decision
採用 **Service + sessionStorage** 模式管理頁面狀態。

### Rationale
- 每個聖經頁面有獨立狀態需求
- sessionStorage 自動於瀏覽器關閉時清除
- Service 提供統一 API，元件無需直接操作 Storage

### State Flow
```
Component → IBiblePageStateService → sessionStorage
                ↑
            OnInitializedAsync (Load)
            OnParametersSet (Load)
            StateChanged Event (Save)
```

---

## Research Conclusion

所有技術決策已完成評估，無待釐清項目。可進入 Phase 1 設計階段。

| 項目 | 狀態 |
|------|------|
| BibleData 整合 | ✅ 已決定 |
| 即時搜尋策略 | ✅ 已決定 |
| 儲存策略 | ✅ 已決定 |
| 搜尋排序 | ✅ 已決定 |
| 多關鍵字邏輯 | ✅ 已決定 |
| 導出格式 | ✅ 已決定 |
| 遊戲選項生成 | ✅ 已決定 |
| 導航擴展 | ✅ 已決定 |
| 測試框架 | ✅ 已決定（沿用現有） |
| 頁面狀態管理 | ✅ 已決定 |
