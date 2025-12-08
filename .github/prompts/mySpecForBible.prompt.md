
# 需求說明

我要在現在網頁架構下，建立一個聖經查詢系統。
請幫我設計一個詳細的規格說明文件，內容需包含以下幾個部分：

系統介紹：
這會是此系統的另一個子功能。
必須在 Web 左側的 Menu 中，新增一個「聖經」的選項，點選後會顯示更多子選項，包括「聖經查詢」、「聖經閱讀」及「聖經遊戲」三個主要功能。
子選項位於方便閱讀切換的位置，並且在使用者點選後，會載入對應的功能頁面。
子選項在「聖經」主畫面的最上方顯示，並以清晰的字體和適當的間距呈現，確保使用者能夠輕鬆辨識和選擇所需功能。

系統功能：
1. 聖經查詢功能
   - 使用者可以輸入關鍵字進行搜尋，系統會回傳包含該關鍵字的經文列表。
   - 搜尋結果需顯示經文所在的書卷、章節及節數。
   - 支援模糊搜尋及多關鍵字搜尋功能。
   - 提供即時搜尋建議功能，使用者在輸入關鍵字時，系統會即時顯示相關經文建議。
   - 搜尋結果可依相關性排序，並提供分頁功能。
   - 搜尋結果提供是否過長的經文自動換行的選項，提升閱讀體驗。
   - 可以設定顯示的字形、字體大小及顏色。
   - 字形有三種選擇：標楷體、微軟正黑體
   - 字體大小可調整，範圍從 12px 到 24px。
   - 顏色可選擇全黑，深灰(#333333)，淺灰(#666666)。
   - 背景顏色可選擇白色、米色(#FFF8DC)、淺灰(#F5F5F5)、淺綠(#F0FFF0)，還有夜間模式(黑色背景 #000000，白色字體 #FFFFFF)。
2. 聖經閱讀功能
   - 使用者可以選擇書卷、章節進行閱讀。並有上下章節的導航按鈕。
   - 提供書卷目錄，方便使用者快速導航至特定書卷。
   - 顯示方式是一行一句經文，並標示節數。書名跟章會在最上方顯示。
   - 支援章節跳轉功能，使用者可直接輸入章節號碼跳轉。
   - 支援書籤功能，使用者可標記喜愛的經文，並在下次閱讀時快速訪問。
   - 利用原本 CovenantPromptKey 紀錄 config 的方式，是採用瀏覽器內建的 Web Storage API 來儲存的字典資料的。這邊也請沿用這個方式來儲存使用者的設定與書籤資料。書籤就是最近閱讀過的經文清單。保留 10 筆。
   - 搜尋結果提供是否過長的經文自動換行的選項，提升閱讀體驗。
   - 可以設定顯示的字形、字體大小及顏色。
   - 字形有三種選擇：標楷體、微軟正黑體
   - 字體大小可調整，範圍從 12px 到 24px。
   - 顏色可選擇全黑，深灰(#333333)，淺灰(#666666)。
   - 背景顏色可選擇白色、米色(#FFF8DC)、淺灰(#F5F5F5)、淺綠(#F0FFF0)，還有夜間模式(黑色背景 #000000，白色字體 #FFFFFF)。
3. 聖經遊戲功能
   - 遊戲 1 : 經文猜猜遊戲：系統隨機選取一段經文，使用者需根據系統提供的 4 個選項找出這經文的正確書卷，不需要到章節。遊戲結束後，顯示正確答案並提供重新挑戰的選項。每一輪有 10 題，每次結束系統會自動記憶使用者的分數，並顯示歷史最高分數跟時間，另有最近 5 次的遊戲分數跟時間。儲存在 Web Storage API 中。
   - 遊戲 2 : 寫敬請期待，歡迎提供新的遊戲點子給我。
   - 遊戲紀錄查看功能：使用者可以查看過去的遊戲紀錄，包括每次遊戲的分數、時間及答題情況。可以清除遊戲紀錄。遊戲紀錄也儲存在 Web Storage API 中。

當使用者切換到聖經查詢或聖經閱讀的頁面時，系統會自動載入並套用使用者先前儲存的設定（如字形、字體大小、顏色等），以提供一致的使用體驗。
也會記憶每一個畫面目前的狀態，例如聖經閱讀的目前章節，或是聖經查詢的最後搜尋關鍵字等，當使用者下次回到該頁面時，可以恢復到先前的狀態。除非程式被重新啟動，才會清除這些狀態資訊。

聖經的顯示方式可以設定：
- 經文顯示格式：使用者可以選擇經文的顯示格式，如純文字、帶有章節標號或帶有書卷標題等。
- 目前語言只有 BibleData 中提供的合和本新舊約版本。
- 字體與顏色設定：使用者可以自訂經文的字體、字體大小及顏色，以符合個人閱讀習慣。
- 導出功能：使用者可以將選定的經文導出為 Markdown 的格式，方便分享或保存。
- 導出功能：使用者可以設定導出的經文範圍，如單一章節、多個章節或整本書卷。可以勾選是否包含書卷標題。
- 導出功能：使用者可以選擇導出的經文格式。設計三種風格。
    例如以下的風格
    - 風格 1 :
(馬可福音 1:1) 神的兒子，耶穌基督福音的起頭。
(馬可福音 1:2) 正如先知以賽亞（有古卷無以賽亞三個字）書上記著說：看哪，我要差遣我的使者在你前面，預備道路。
(馬可福音 1:3) 在曠野有人聲喊著說：預備主的道，修直他的路。
    - 風格 2 :
馬可福音 ( 第 1 章 1 ~ 3 節 )
神的兒子，耶穌基督福音的起頭。
正如先知以賽亞（有古卷無以賽亞三個字）書上記著說：看哪，我要差遣我的使者在你前面，預備道路。
在曠野有人聲喊著說：預備主的道，修直他的路。
    - 風格 3 :
《馬可福音》 ( 第 1 章 1 ~ 3 節 )
神的兒子，耶穌基督福音的起頭。正如先知以賽亞（有古卷無以賽亞三個字）書上記著說：看哪，我要差遣我的使者在你前面，預備道路。在曠野有人聲喊著說：預備主的道，修直他的路。




    - 
---

附件：
我要求你使用既有的 Bible DLL (BibleData) 來實作這些功能。
他有完整的新舊約經文資料，並且有提供快速查詢的 C# Methods。
使用說明跟介紹如下：

----

# BibleData

一個提供聖經靜態資料的 .NET 類別庫，資料在編譯時即存在於程式碼中，無需讀取外部檔案。

## ✨ 特色

- **跨平台相容**：使用 .NET Standard 2.1，支援 .NET Core 3.0+、.NET 5+、.NET Framework 4.8+、Xamarin、Unity 等
- **零 I/O 延遲**：聖經資料編譯時即嵌入程式碼，無需讀取外部檔案
- **O(1) 快速查詢**：透過 `BibleIndex` 類別使用字典索引，實現常數時間查詢
- **完整型別支援**：提供強型別模型與 Nullable 支援
- **即時搜尋**：支援邊輸入邊搜尋、可取消搜尋、排名搜尋等功能，適合 Web/App 開發
- **靈活查詢方式**：支援書卷名稱或編號查詢、關鍵字搜尋、多關鍵字搜尋、隨機經文等功能

## 📦 安裝與參考

### 方式一：專案參考（推薦用於同一解決方案）

在您的 `.csproj` 檔案中加入：

```xml
<ItemGroup>
  <ProjectReference Include="..\BibleData\BibleData.csproj" />
</ItemGroup>
```

### 方式二：DLL 參考

1. 建置 `BibleData` 專案取得 `BibleData.dll`
2. 在您的專案中加入 DLL 參考：

```xml
<ItemGroup>
  <Reference Include="BibleData">
    <HintPath>path\to\BibleData.dll</HintPath>
  </Reference>
</ItemGroup>
```

## 🏗️ 資料結構

### 類別階層圖

```
Bible
└── Books : List<Book>
    ├── Number : int          (書卷編號，從 1 開始)
    ├── Name : string         (書卷名稱，如「創世記」)
    └── Chapters : List<Chapter>
        ├── Number : int      (章節編號，從 1 開始)
        └── Verses : List<Verse>
            ├── Number : int  (節數編號，從 1 開始)
            └── Content : string (經文內容)
```

### 索引結構（BibleIndex）

```
BibleIndex
├── _bookByName      : Dictionary<string, Book>                    # 書卷名稱索引
├── _bookByNumber    : Dictionary<int, Book>                       # 書卷編號索引
├── _chapterIndex    : Dictionary<(int, int), Chapter>             # (書卷, 章) 索引
├── _verseIndex      : Dictionary<(int, int, int), Verse>          # (書卷, 章, 節) 索引
├── _verseByNameIndex: Dictionary<(string, int, int), Verse>       # (書名, 章, 節) 索引
└── _allVerses       : List<VerseWithLocation>                     # 扁平化經文列表
```

### 模型定義

```csharp
namespace BibleData.Models
{
    public class Bible
    {
        public List<Book> Books { get; set; } = new List<Book>();
    }

    public class Book
    {
        public int Number { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public List<Chapter> Chapters { get; set; } = new List<Chapter>();
    }

    public class Chapter
    {
        public int Number { get; set; } = 0;
        public List<Verse> Verses { get; set; } = new List<Verse>();
    }

    public class Verse
    {
        public int Number { get; set; } = 0;
        public string Content { get; set; } = string.Empty;
    }
}

namespace BibleData
{
    /// <summary>
    /// 包含完整位置資訊的經文（用於搜尋結果）
    /// </summary>
    public class VerseWithLocation
    {
        public int BookNumber { get; set; }
        public string BookName { get; set; } = string.Empty;
        public int ChapterNumber { get; set; }
        public int VerseNumber { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Reference => $"{BookName} {ChapterNumber}:{VerseNumber}";
    }

    /// <summary>
    /// 搜尋結果（含排名分數）
    /// </summary>
    public class SearchResult
    {
        public VerseWithLocation Verse { get; set; }
        public double Score { get; set; }           // 相關性分數
        public int MatchIndex { get; set; }         // 首次出現位置
        public int OccurrenceCount { get; set; }    // 出現次數
    }
}
```

## 🚀 使用方式

### 方式一：直接使用 BibleStaticData（簡單但較慢）

```csharp
using BibleData;
using BibleData.Models;

// 取得聖經資料
Bible bible = BibleStaticData.Bible;

// 查詢經文（O(n) 時間複雜度）
var verse = bible.Books
    .FirstOrDefault(b => b.Name == "創世記")?
    .Chapters.FirstOrDefault(c => c.Number == 1)?
    .Verses.FirstOrDefault(v => v.Number == 1);
```

### 方式二：使用 BibleIndex（推薦，O(1) 快速查詢）

```csharp
using BibleData;

// 建立索引（建議在應用程式啟動時建立一次，之後重複使用）
var index = new BibleIndex();

// 快速查詢經文（O(1) 時間複雜度）
var verse = index.GetVerse("創世記", 1, 1);
Console.WriteLine(verse?.Content);
// 輸出: 起初，神創造天地。
```

## 📖 BibleIndex API 參考

### 建構函式

```csharp
// 使用預設的 BibleStaticData.Bible
var index = new BibleIndex();

// 使用自訂的 Bible 資料
var index = new BibleIndex(customBible);
```

### 屬性

| 屬性 | 型別 | 說明 |
|------|------|------|
| `BookNames` | `IReadOnlyList<string>` | 書卷名稱列表（按編號排序） |
| `TotalBooks` | `int` | 總書卷數 |
| `TotalChapters` | `int` | 總章數 |
| `TotalVerses` | `int` | 總節數 |

### 書卷查詢方法

```csharp
// 透過名稱取得書卷 - O(1)
Book? book = index.GetBook("創世記");

// 透過編號取得書卷 - O(1)
Book? book = index.GetBook(1);

// 檢查書卷是否存在
bool exists = index.BookExists("創世記");
bool exists = index.BookExists(1);
```

### 章節查詢方法

```csharp
// 取得章節 - O(1)
Chapter? chapter = index.GetChapter("創世記", 1);
Chapter? chapter = index.GetChapter(1, 1);  // 書卷編號, 章編號

// 取得書卷的章數
int count = index.GetChapterCount("創世記");
```

### 經文查詢方法

```csharp
// 取得經文 - O(1)
Verse? verse = index.GetVerse("創世記", 1, 1);
Verse? verse = index.GetVerse(1, 1, 1);  // 書卷編號, 章, 節

// 直接取得經文內容 - O(1)
string? content = index.GetVerseContent("創世記", 1, 1);

// 取得章節的節數
int count = index.GetVerseCount("創世記", 1);
```

### 搜尋方法

```csharp
// 關鍵字搜尋（大小寫敏感）
List<VerseWithLocation> results = index.Search("愛");

// 關鍵字搜尋（不區分大小寫）
List<VerseWithLocation> results = index.SearchIgnoreCase("god");

// 自訂比較方式
List<VerseWithLocation> results = index.Search("愛", StringComparison.CurrentCulture);

// 在特定書卷中搜尋
List<VerseWithLocation> results = index.SearchInBook("約翰福音", "愛");
```

### 即時搜尋方法（適合 Web/App 邊輸入邊搜尋）

```csharp
// 回傳前 N 筆結果（預設 10 筆）
List<VerseWithLocation> results = index.SearchTop("神", 10);

// 不區分大小寫
List<VerseWithLocation> results = index.SearchTopIgnoreCase("god", 5);

// 支援取消（適合快速輸入時取消前一次搜尋）
var cts = new CancellationTokenSource();
List<VerseWithLocation> results = index.SearchTopWithCancellation("愛", 10, cts.Token);

// 帶排名分數（依關鍵字出現位置和次數排序）
List<SearchResult> results = index.SearchTopRanked("愛", 10);
foreach (var r in results)
{
    Console.WriteLine($"[分數:{r.Score:F1}] {r.Verse.Reference} - {r.Verse.Content}");
}

// 前綴搜尋（經文以指定文字開頭）
List<VerseWithLocation> results = index.SearchByPrefix("耶和華", 10);

// 多關鍵字搜尋（AND，必須全部包含）
List<VerseWithLocation> results = index.SearchMultipleKeywords(new[] { "神", "愛" }, 10);

// 任一關鍵字搜尋（OR，包含任一即可）
List<VerseWithLocation> results = index.SearchAnyKeyword(new[] { "信", "望", "愛" }, 10);
```

### 其他方法

```csharp
// 取得所有經文（含位置資訊）
IReadOnlyList<VerseWithLocation> allVerses = index.GetAllVerses();

// 取得隨機經文
VerseWithLocation randomVerse = index.GetRandomVerse();

// 使用自訂隨機數產生器
var random = new Random(42);
VerseWithLocation randomVerse = index.GetRandomVerse(random);
```

## 📝 範例程式碼

### 完整範例：搜尋並顯示結果

```csharp
using BibleData;

var index = new BibleIndex();

// 顯示統計資訊
Console.WriteLine($"聖經共有 {index.TotalBooks} 卷, {index.TotalChapters} 章, {index.TotalVerses} 節");

// 搜尋包含「愛」的經文
var results = index.Search("愛");
Console.WriteLine($"\n找到 {results.Count} 節包含「愛」的經文：\n");

foreach (var verse in results.Take(10))
{
    Console.WriteLine($"{verse.Reference} - {verse.Content}");
}
```

### 效能比較

```csharp
// ❌ 較慢的方式 - O(n) 每次查詢都要遍歷
for (int i = 0; i < 1000; i++)
{
    var verse = BibleStaticData.Bible.Books
        .FirstOrDefault(b => b.Name == "創世記")?
        .Chapters.FirstOrDefault(c => c.Number == 1)?
        .Verses.FirstOrDefault(v => v.Number == 1);
}

// ✅ 較快的方式 - O(1) 字典查詢
var index = new BibleIndex();  // 只需建立一次
for (int i = 0; i < 1000; i++)
{
    var verse = index.GetVerse("創世記", 1, 1);
}
```

## ⚙️ 技術規格

| 項目 | 值 |
|------|-----|
| Target Framework | .NET Standard 2.1 |
| 支援平台 | .NET Core 3.0+, .NET 5/6/7/8+, .NET Framework 4.8+, Xamarin, Unity |
| Nullable | 啟用 |
| Namespace | `BibleData`, `BibleData.Models` |

### 時間複雜度比較

| 操作 | BibleStaticData | BibleIndex |
|------|-----------------|------------|
| 取得書卷 | O(n) | O(1) |
| 取得章節 | O(n) | O(1) |
| 取得經文 | O(n) | O(1) |
| 關鍵字搜尋 | O(n) | O(n) |
| 即時搜尋前 N 筆 | O(n) | O(n) 但提前終止 |
| 取得統計 | O(n) | O(1) |

## 🌐 Web/App 即時搜尋使用範例

### ASP.NET Core Web API

```csharp
[ApiController]
[Route("api/[controller]")]
public class BibleSearchController : ControllerBase
{
    private static readonly BibleIndex _index = new BibleIndex();

    [HttpGet("search")]
    public IActionResult Search([FromQuery] string keyword, [FromQuery] int top = 10)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return Ok(Array.Empty<object>());

        var results = _index.SearchTop(keyword, top);
        return Ok(results.Select(v => new 
        {
            reference = v.Reference,
            content = v.Content
        }));
    }

    [HttpGet("search-ranked")]
    public IActionResult SearchRanked([FromQuery] string keyword, [FromQuery] int top = 10)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return Ok(Array.Empty<object>());

        var results = _index.SearchTopRanked(keyword, top);
        return Ok(results.Select(r => new 
        {
            reference = r.Verse.Reference,
            content = r.Verse.Content,
            score = r.Score
        }));
    }
}
```

### Blazor 元件（邊輸入邊搜尋）

```razor
@using BibleData

<input @oninput="OnSearchInput" placeholder="輸入關鍵字搜尋經文..." />

@if (searchResults.Any())
{
    <ul>
        @foreach (var verse in searchResults)
        {
            <li><strong>@verse.Reference</strong> @verse.Content</li>
        }
    </ul>
}

@code {
    private static readonly BibleIndex _index = new BibleIndex();
    private List<VerseWithLocation> searchResults = new();
    private CancellationTokenSource? _cts;

    private async Task OnSearchInput(ChangeEventArgs e)
    {
        // 取消前一次搜尋
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        var keyword = e.Value?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(keyword))
        {
            searchResults.Clear();
            return;
        }

        // 延遲執行（防抖動）
        try
        {
            await Task.Delay(200, _cts.Token);
            searchResults = _index.SearchTopWithCancellation(keyword, 10, _cts.Token);
            StateHasChanged();
        }
        catch (TaskCanceledException)
        {
            // 搜尋被取消，忽略
        }
    }
}
```

### JavaScript 前端整合範例

```javascript
// 搭配上方的 Web API 使用
let debounceTimer;
const searchInput = document.getElementById('searchInput');
const resultsList = document.getElementById('resultsList');

searchInput.addEventListener('input', (e) => {
    clearTimeout(debounceTimer);
    
    debounceTimer = setTimeout(async () => {
        const keyword = e.target.value;
        if (!keyword) {
            resultsList.innerHTML = '';
            return;
        }

        const response = await fetch(`/api/biblesearch/search?keyword=${encodeURIComponent(keyword)}&top=10`);
        const results = await response.json();
        
        resultsList.innerHTML = results
            .map(v => `<li><strong>${v.reference}</strong> ${v.content}</li>`)
            .join('');
    }, 200); // 200ms 防抖動
});
```

## 📝 注意事項

1. **延遲初始化**：`BibleStaticData.Bible` 使用延遲初始化模式，第一次存取時才會建立資料物件。
2. **索引建立時機**：建議在應用程式啟動時建立 `BibleIndex` 實例（可設為 static），之後重複使用。
3. **記憶體考量**：`BibleIndex` 會額外使用記憶體來儲存索引字典，換取查詢速度。
4. **即時搜尋建議**：使用 `SearchTop` 系列方法時，建議搭配防抖動（debounce）減少不必要的搜尋。
5. **自動產生**：`BibleStaticData.cs` 由 `BibleCodeGenerator` 自動產生，請勿手動修改。
6. **資料語言**：聖經資料為**繁體中文**（和合本）。
7. **.NET Standard 2.1**：本專案使用 .NET Standard 2.1，可在多種 .NET 執行環境中使用。

## 📂 檔案結構

```
BibleData/
├── BibleData.csproj          # 專案檔
├── BibleStaticData.cs        # 聖經靜態資料（自動產生）
├── BibleIndex.cs             # 快速索引查詢類別
├── README.md                 # 本文件
└── Models/
    └── Bible.cs              # 資料模型定義
```

## 🔗 相關專案

- **BibleCodeGenerator** - 用於產生 `BibleStaticData.cs` 的程式碼產生器
- **BibleConsole** - 使用本類別庫的主控台應用程式，提供搜尋與測驗功能
