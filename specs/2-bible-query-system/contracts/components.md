# Component Contracts: 聖經查詢系統

**Date**: 2025-12-06  
**Related Plan**: [../plan.md](../plan.md)

---

## Overview

本文件定義聖經查詢系統的 Blazor 元件規格，包含頁面元件與共用元件。

---

## Page Components

### 1. BibleHomePage.razor（聖經主頁）

**路由**: `/bible`

**職責**: 聖經功能入口頁面，顯示子功能選項並提供快速導航。

```razor
@page "/bible"
@rendermode InteractiveServer

<PageTitle>聖經 - CovenantPromptKey</PageTitle>

<div class="bible-home-container">
    <!-- 子功能導航列 -->
    <BibleSubNavigation ActivePage="Home" />
    
    <!-- 主要內容區 -->
    <div class="bible-home-content">
        <h1>聖經查詢系統</h1>
        
        <!-- 功能卡片 -->
        <div class="feature-cards">
            <FeatureCard 
                Title="聖經查詢" 
                Description="使用關鍵字搜尋經文"
                Icon="bi-search"
                Link="/bible/search" />
            
            <FeatureCard 
                Title="聖經閱讀" 
                Description="逐章逐節閱讀聖經"
                Icon="bi-book"
                Link="/bible/read" />
            
            <FeatureCard 
                Title="聖經遊戲" 
                Description="經文猜猜遊戲"
                Icon="bi-controller"
                Link="/bible/game" />
        </div>
        
        <!-- 最近閱讀書籤 -->
        <BibleBookmarkList MaxDisplay="5" ShowMore="true" />
    </div>
</div>
```

**Parameters**: 無

**Services Injected**:
- `IBibleBookmarkService`

---

### 2. BibleSearchPage.razor（聖經查詢頁）

**路由**: `/bible/search`

**職責**: 提供關鍵字搜尋經文功能。

```razor
@page "/bible/search"
@rendermode InteractiveServer

<PageTitle>聖經查詢 - CovenantPromptKey</PageTitle>

<div class="bible-search-container">
    <!-- 子功能導航列 -->
    <BibleSubNavigation ActivePage="Search" />
    
    <!-- 設定面板 (可展開/收合) -->
    <BibleSettingsPanel />
    
    <!-- 搜尋輸入區 -->
    <div class="search-input-area">
        <input type="text" 
               @bind-value="SearchKeyword" 
               @bind-value:event="oninput"
               @onkeyup="OnSearchInput"
               placeholder="輸入關鍵字搜尋經文..." />
        <button @onclick="ExecuteSearch">
            <span class="bi bi-search"></span> 搜尋
        </button>
    </div>
    
    <!-- 搜尋結果區 -->
    <BibleSearchResults 
        Results="SearchResults" 
        IsLoading="IsSearching"
        Keyword="SearchKeyword"
        OnPageChange="OnPageChange" />
</div>
```

**Parameters**: 無

**State**:
- `SearchKeyword: string` - 當前搜尋關鍵字
- `SearchResults: List<SearchResultItem>` - 搜尋結果
- `IsSearching: bool` - 是否正在搜尋
- `CurrentPage: int` - 當前頁碼

**Services Injected**:
- `IBibleSearchService`
- `IBibleSettingsService`
- `IBiblePageStateService`

**Lifecycle**:
- `OnInitializedAsync`: 載入頁面狀態與設定
- `OnAfterRenderAsync`: 恢復捲動位置
- `Dispose`: 儲存頁面狀態

---

### 3. BibleReadPage.razor（聖經閱讀頁）

**路由**: `/bible/read` 或 `/bible/read/{BookNumber:int}/{ChapterNumber:int}`

**職責**: 提供書卷章節閱讀功能。

```razor
@page "/bible/read"
@page "/bible/read/{BookNumber:int}/{ChapterNumber:int}"
@rendermode InteractiveServer

<PageTitle>@GetPageTitle() - CovenantPromptKey</PageTitle>

<div class="bible-read-container">
    <!-- 子功能導航列 -->
    <BibleSubNavigation ActivePage="Read" />
    
    <!-- 設定面板 -->
    <BibleSettingsPanel />
    
    <!-- 書卷/章節選擇器 -->
    <div class="chapter-selector-area">
        <BibleBookSelector 
            SelectedBook="CurrentBookNumber"
            OnBookSelected="OnBookSelected" />
        
        <BibleChapterNavigator 
            BookNumber="CurrentBookNumber"
            ChapterNumber="CurrentChapterNumber"
            OnChapterChange="OnChapterChange" />
    </div>
    
    <!-- 章節標題 -->
    <div class="chapter-header">
        <h2>@CurrentBookName 第 @CurrentChapterNumber 章</h2>
    </div>
    
    <!-- 經文顯示區 -->
    <BibleVerseDisplay 
        Verses="CurrentVerses"
        Settings="CurrentSettings" />
    
    <!-- 上下章導航 -->
    <div class="chapter-navigation">
        <button @onclick="GoPreviousChapter" disabled="@(!HasPreviousChapter)">
            <span class="bi bi-chevron-left"></span> 上一章
        </button>
        <button @onclick="GoNextChapter" disabled="@(!HasNextChapter)">
            下一章 <span class="bi bi-chevron-right"></span>
        </button>
    </div>
    
    <!-- 書籤列表 (側邊面板) -->
    <BibleBookmarkList OnBookmarkClick="OnBookmarkClick" />
</div>
```

**Route Parameters**:
- `BookNumber: int` - 書卷編號 (可選)
- `ChapterNumber: int` - 章節編號 (可選)

**State**:
- `CurrentBookNumber: int` - 當前書卷編號
- `CurrentChapterNumber: int` - 當前章節編號
- `CurrentBookName: string` - 當前書卷名稱
- `CurrentVerses: List<VerseWithLocation>` - 當前章節經文
- `CurrentSettings: BibleSettings` - 當前顯示設定

**Services Injected**:
- `IBibleReadingService`
- `IBibleSettingsService`
- `IBibleBookmarkService`
- `IBiblePageStateService`
- `NavigationManager`

---

### 4. BibleGamePage.razor（聖經遊戲頁）

**路由**: `/bible/game`

**職責**: 提供經文猜猜遊戲功能，包含遊戲記錄與錯題複習。

```razor
@page "/bible/game"
@rendermode InteractiveServer

<PageTitle>聖經遊戲 - CovenantPromptKey</PageTitle>

<div class="bible-game-container">
    <!-- 子功能導航列 -->
    <BibleSubNavigation ActivePage="Game" />
    
    <!-- 遊戲選擇區 -->
    <div class="game-selection">
        <div class="game-card active">
            <h3>經文猜猜遊戲</h3>
            <p>猜猜這段經文來自哪卷書</p>
        </div>
        <div class="game-card disabled">
            <h3>遊戲 2</h3>
            <p>敬請期待，歡迎提供新的遊戲點子給我</p>
        </div>
    </div>
    
    @if (GameState == GameStateEnum.NotStarted)
    {
        <!-- 遊戲開始畫面 -->
        <div class="game-start">
            <button @onclick="StartGame" class="btn-start">開始遊戲</button>
            
            <!-- 遊戲記錄區塊 -->
            <div class="game-history">
                <h4>遊戲記錄</h4>
                <p>歷史最高分: @HighScore 分</p>
                <h5>最近 5 次遊戲</h5>
                @foreach (var record in RecentRecords)
                {
                    <div class="record-item">
                        <span>@record.PlayedAt.ToLocalTime().ToString("yyyy/MM/dd HH:mm")</span>
                        <span>@record.Score / @record.TotalQuestions 分</span>
                    </div>
                }
                <button @onclick="ClearRecords" class="btn-clear">清除記錄</button>
            </div>
            
            <!-- 錯題複習區塊 -->
            <div class="wrong-answers-section">
                <h4>錯題複習</h4>
                @if (WrongAnswerCount > 0)
                {
                    <p>共 @WrongAnswerCount 道錯題待複習</p>
                    <button @onclick="ToggleWrongAnswersPanel" class="btn-review">
                        @(ShowWrongAnswersPanel ? "收起錯題" : "查看錯題")
                    </button>
                    <button @onclick="ClearWrongAnswers" class="btn-clear-wrong">清除錯題記錄</button>
                    
                    @if (ShowWrongAnswersPanel)
                    {
                        <div class="wrong-answers-list">
                            @foreach (var wrongAnswer in WrongAnswers)
                            {
                                <div class="wrong-answer-card">
                                    <div class="verse-content">
                                        <p>@wrongAnswer.VerseContent</p>
                                        <small class="verse-reference">@wrongAnswer.VerseReference</small>
                                    </div>
                                    <div class="answer-detail">
                                        <span class="wrong-label">❌ 您的答案: @wrongAnswer.SelectedAnswer</span>
                                        <span class="correct-label">✓ 正確答案: @wrongAnswer.CorrectAnswer</span>
                                    </div>
                                </div>
                            }
                        </div>
                    }
                }
                else
                {
                    <p class="no-wrong-answers">暫無錯題記錄，繼續保持！</p>
                }
            </div>
        </div>
    }
    else if (GameState == GameStateEnum.Playing)
    {
        <!-- 遊戲進行中 -->
        <div class="game-playing">
            <div class="progress-bar">
                <span>第 @CurrentQuestionNumber / @TotalQuestions 題</span>
                <span>目前分數: @CurrentScore</span>
            </div>
            
            <div class="question-area">
                <div class="verse-content">
                    <p>@CurrentQuestion?.Verse.Content</p>
                </div>
                
                <div class="options">
                    @foreach (var option in CurrentQuestion?.Options ?? new())
                    {
                        <button @onclick="() => SelectAnswer(option)"
                                class="option-btn @GetOptionClass(option)">
                            @option
                        </button>
                    }
                </div>
            </div>
            
            @if (ShowAnswer)
            {
                <div class="answer-feedback @(IsLastAnswerCorrect ? "correct" : "wrong")">
                    @if (IsLastAnswerCorrect)
                    {
                        <p>✓ 答對了！</p>
                    }
                    else
                    {
                        <p>✗ 答錯了！正確答案是: @CurrentQuestion?.CorrectAnswer</p>
                    }
                    <button @onclick="NextQuestion">下一題</button>
                </div>
            }
        </div>
    }
    else if (GameState == GameStateEnum.Finished)
    {
        <!-- 遊戲結束 -->
        <div class="game-finished">
            <h3>遊戲結束！</h3>
            <p class="final-score">最終分數: @CurrentScore / @TotalQuestions</p>
            
            @if (IsNewHighScore)
            {
                <p class="new-high-score">🎉 新的最高分！</p>
            }
            
            @if (CurrentSessionWrongCount > 0)
            {
                <p class="wrong-count-hint">本次答錯 @CurrentSessionWrongCount 題，已加入錯題記錄</p>
            }
            
            <div class="action-buttons">
                <button @onclick="StartGame">再玩一次</button>
                <button @onclick="ViewRecords">查看記錄</button>
                @if (CurrentSessionWrongCount > 0)
                {
                    <button @onclick="ReviewWrongAnswers">複習本次錯題</button>
                }
            </div>
        </div>
    }
</div>
```

**State**:
- `GameState: GameStateEnum` - 遊戲狀態 (NotStarted/Playing/Finished)
- `CurrentQuestion: BibleGameQuestion?` - 當前題目
- `CurrentQuestionNumber: int` - 當前題號
- `CurrentScore: int` - 當前分數
- `TotalQuestions: int` - 總題數 (固定 10)
- `Questions: List<BibleGameQuestion>` - 所有題目
- `HighScore: int` - 歷史最高分
- `RecentRecords: List<BibleGameSession>` - 最近 5 次記錄
- `ShowAnswer: bool` - 是否顯示答案
- `IsLastAnswerCorrect: bool` - 上一題是否答對
- `SelectedAnswer: string?` - 選擇的答案
- `WrongAnswers: List<WrongAnswerRecord>` - 錯題列表
- `WrongAnswerCount: int` - 錯題總數
- `ShowWrongAnswersPanel: bool` - 是否展開錯題面板
- `CurrentSessionWrongCount: int` - 本次遊戲答錯題數

**Services Injected**:
- `IBibleGameService`
- `IBiblePageStateService`

---

## Shared Components

### 1. BibleSubNavigation.razor（子功能導航列）

**職責**: 顯示聖經子功能導航選項。

```razor
<div class="bible-sub-nav">
    <NavLink href="/bible/search" class="sub-nav-item @GetActiveClass("Search")">
        <span class="bi bi-search"></span> 聖經查詢
    </NavLink>
    <NavLink href="/bible/read" class="sub-nav-item @GetActiveClass("Read")">
        <span class="bi bi-book"></span> 聖經閱讀
    </NavLink>
    <NavLink href="/bible/game" class="sub-nav-item @GetActiveClass("Game")">
        <span class="bi bi-controller"></span> 聖經遊戲
    </NavLink>
</div>
```

**Parameters**:
- `ActivePage: string` - 當前頁面名稱 ("Home"/"Search"/"Read"/"Game")

---

### 2. BibleSettingsPanel.razor（顯示設定面板）

**職責**: 提供經文顯示設定的 UI 控制項。

```razor
<div class="bible-settings-panel @(IsExpanded ? "expanded" : "collapsed")">
    <button class="toggle-btn" @onclick="ToggleExpand">
        <span class="bi bi-gear"></span> 顯示設定
        <span class="bi @(IsExpanded ? "bi-chevron-up" : "bi-chevron-down")"></span>
    </button>
    
    @if (IsExpanded)
    {
        <div class="settings-content">
            <!-- 字形選擇 -->
            <div class="setting-item">
                <label>字形</label>
                <select @bind="Settings.FontFamily" @bind:after="OnSettingsChanged">
                    <option value="@FontFamily.MicrosoftJhengHei">微軟正黑體</option>
                    <option value="@FontFamily.DFKai">標楷體</option>
                </select>
            </div>
            
            <!-- 字體大小 -->
            <div class="setting-item">
                <label>字體大小: @(Settings.FontSize)px</label>
                <input type="range" min="12" max="24" 
                       @bind="Settings.FontSize" @bind:after="OnSettingsChanged" />
            </div>
            
            <!-- 文字顏色 -->
            <div class="setting-item">
                <label>文字顏色</label>
                <div class="color-options">
                    <button class="color-btn @GetColorClass(TextColor.Black)"
                            style="background-color: #000000"
                            @onclick="() => SetTextColor(TextColor.Black)"></button>
                    <button class="color-btn @GetColorClass(TextColor.DarkGray)"
                            style="background-color: #333333"
                            @onclick="() => SetTextColor(TextColor.DarkGray)"></button>
                    <button class="color-btn @GetColorClass(TextColor.LightGray)"
                            style="background-color: #666666"
                            @onclick="() => SetTextColor(TextColor.LightGray)"></button>
                </div>
            </div>
            
            <!-- 背景顏色 -->
            <div class="setting-item">
                <label>背景顏色</label>
                <div class="color-options">
                    <button class="bg-btn @GetBgClass(BackgroundColor.White)"
                            style="background-color: #FFFFFF"
                            @onclick="() => SetBackgroundColor(BackgroundColor.White)">白</button>
                    <button class="bg-btn @GetBgClass(BackgroundColor.Beige)"
                            style="background-color: #FFF8DC"
                            @onclick="() => SetBackgroundColor(BackgroundColor.Beige)">米</button>
                    <button class="bg-btn @GetBgClass(BackgroundColor.LightGray)"
                            style="background-color: #F5F5F5"
                            @onclick="() => SetBackgroundColor(BackgroundColor.LightGray)">灰</button>
                    <button class="bg-btn @GetBgClass(BackgroundColor.LightGreen)"
                            style="background-color: #F0FFF0"
                            @onclick="() => SetBackgroundColor(BackgroundColor.LightGreen)">綠</button>
                    <button class="bg-btn night-mode @GetBgClass(BackgroundColor.NightMode)"
                            @onclick="() => SetBackgroundColor(BackgroundColor.NightMode)">夜</button>
                </div>
            </div>
            
            <!-- 自動換行 -->
            <div class="setting-item">
                <label>
                    <input type="checkbox" @bind="Settings.AutoWrap" @bind:after="OnSettingsChanged" />
                    經文自動換行
                </label>
            </div>
        </div>
    }
</div>
```

**Parameters**:
- `OnSettingsChanged: EventCallback<BibleSettings>` - 設定變更回呼

**Services Injected**:
- `IBibleSettingsService`

---

### 3. BibleVerseDisplay.razor（經文顯示元件）

**職責**: 以指定樣式顯示經文列表。

```razor
<div class="bible-verse-display" style="@GetContainerStyle()">
    @foreach (var verse in Verses)
    {
        <div class="verse-line" style="@GetVerseStyle()">
            <span class="verse-number">@verse.VerseNumber</span>
            <span class="verse-content">@verse.Content</span>
        </div>
    }
</div>

@code {
    [Parameter] public List<VerseWithLocation> Verses { get; set; } = new();
    [Parameter] public BibleSettings Settings { get; set; } = new();
    
    private string GetContainerStyle()
    {
        var bgColor = BibleStyleHelper.GetBackgroundColorHex(Settings.BackgroundColor);
        return $"background-color: {bgColor};";
    }
    
    private string GetVerseStyle()
    {
        var fontFamily = BibleStyleHelper.GetFontFamilyCss(Settings.FontFamily);
        var textColor = Settings.BackgroundColor == BackgroundColor.NightMode 
            ? "#FFFFFF" 
            : BibleStyleHelper.GetTextColorHex(Settings.TextColor);
        var wrap = Settings.AutoWrap ? "normal" : "nowrap";
        
        return $"font-family: {fontFamily}; font-size: {Settings.FontSize}px; color: {textColor}; white-space: {wrap};";
    }
}
```

**Parameters**:
- `Verses: List<VerseWithLocation>` - 經文列表
- `Settings: BibleSettings` - 顯示設定

---

### 4. BibleBookSelector.razor（書卷選擇器）

**職責**: 提供書卷選擇下拉選單。

```razor
<div class="bible-book-selector">
    <select @bind="SelectedBook" @bind:after="OnSelectionChanged">
        @foreach (var (name, index) in BookNamesWithIndex)
        {
            <option value="@(index + 1)">@name</option>
        }
    </select>
</div>

@code {
    [Parameter] public int SelectedBook { get; set; } = 1;
    [Parameter] public EventCallback<int> OnBookSelected { get; set; }
    
    [Inject] private IBibleReadingService BibleReadingService { get; set; } = null!;
    
    private IEnumerable<(string Name, int Index)> BookNamesWithIndex => 
        BibleReadingService.GetBookNames().Select((name, index) => (name, index));
    
    private async Task OnSelectionChanged()
    {
        await OnBookSelected.InvokeAsync(SelectedBook);
    }
}
```

**Parameters**:
- `SelectedBook: int` - 選中的書卷編號
- `OnBookSelected: EventCallback<int>` - 選擇變更回呼

---

### 5. BibleChapterNavigator.razor（章節導航器）

**職責**: 提供章節選擇與跳轉功能。

```razor
<div class="bible-chapter-navigator">
    <div class="chapter-selector">
        <label>章節:</label>
        <select @bind="ChapterNumber" @bind:after="OnChapterSelected">
            @for (int i = 1; i <= TotalChapters; i++)
            {
                <option value="@i">第 @i 章</option>
            }
        </select>
    </div>
    
    <div class="chapter-jump">
        <input type="number" min="1" max="@TotalChapters" 
               @bind="JumpChapter" placeholder="跳轉" />
        <button @onclick="JumpToChapter">Go</button>
    </div>
</div>

@code {
    [Parameter] public int BookNumber { get; set; }
    [Parameter] public int ChapterNumber { get; set; }
    [Parameter] public EventCallback<int> OnChapterChange { get; set; }
    
    [Inject] private IBibleReadingService BibleReadingService { get; set; } = null!;
    
    private int TotalChapters => BibleReadingService.GetChapterCount(BookNumber);
    private int JumpChapter { get; set; }
    
    private async Task OnChapterSelected()
    {
        await OnChapterChange.InvokeAsync(ChapterNumber);
    }
    
    private async Task JumpToChapter()
    {
        if (JumpChapter >= 1 && JumpChapter <= TotalChapters)
        {
            await OnChapterChange.InvokeAsync(JumpChapter);
        }
    }
}
```

**Parameters**:
- `BookNumber: int` - 書卷編號
- `ChapterNumber: int` - 章節編號
- `OnChapterChange: EventCallback<int>` - 章節變更回呼

---

### 6. BibleSearchResults.razor（搜尋結果列表）

**職責**: 顯示搜尋結果並提供分頁功能。

```razor
<div class="bible-search-results">
    @if (IsLoading)
    {
        <div class="loading">搜尋中...</div>
    }
    else if (!Results.Any())
    {
        <div class="no-results">
            @if (!string.IsNullOrWhiteSpace(Keyword))
            {
                <p>查無結果，請嘗試其他關鍵字</p>
            }
            else
            {
                <p>請輸入關鍵字開始搜尋</p>
            }
        </div>
    }
    else
    {
        <div class="results-header">
            <span>找到 @TotalCount 筆結果</span>
        </div>
        
        <div class="results-list">
            @foreach (var result in PagedResults)
            {
                <div class="result-item" @onclick="() => OnResultClick(result)">
                    <div class="result-reference">@result.Reference</div>
                    <div class="result-content">@((MarkupString)result.HighlightedContent)</div>
                </div>
            }
        </div>
        
        <!-- 分頁 -->
        @if (TotalPages > 1)
        {
            <div class="pagination">
                <button @onclick="PreviousPage" disabled="@(CurrentPage <= 1)">上一頁</button>
                <span>第 @CurrentPage / @TotalPages 頁</span>
                <button @onclick="NextPage" disabled="@(CurrentPage >= TotalPages)">下一頁</button>
            </div>
        }
    }
</div>

@code {
    [Parameter] public List<SearchResultItem> Results { get; set; } = new();
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public string Keyword { get; set; } = string.Empty;
    [Parameter] public int PageSize { get; set; } = 20;
    [Parameter] public EventCallback<int> OnPageChange { get; set; }
    [Parameter] public EventCallback<SearchResultItem> OnResultSelected { get; set; }
    
    private int CurrentPage { get; set; } = 1;
    private int TotalCount => Results.Count;
    private int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    private IEnumerable<SearchResultItem> PagedResults => 
        Results.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
    
    private async Task PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await OnPageChange.InvokeAsync(CurrentPage);
        }
    }
    
    private async Task NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await OnPageChange.InvokeAsync(CurrentPage);
        }
    }
    
    private async Task OnResultClick(SearchResultItem result)
    {
        await OnResultSelected.InvokeAsync(result);
    }
}
```

**Parameters**:
- `Results: List<SearchResultItem>` - 搜尋結果
- `IsLoading: bool` - 載入狀態
- `Keyword: string` - 搜尋關鍵字
- `PageSize: int` - 每頁筆數 (預設 20)
- `OnPageChange: EventCallback<int>` - 分頁變更回呼
- `OnResultSelected: EventCallback<SearchResultItem>` - 結果點擊回呼

---

### 7. BibleBookmarkList.razor（書籤列表）

**職責**: 顯示書籤列表並提供快速跳轉。

```razor
<div class="bible-bookmark-list">
    <h4>
        <span class="bi bi-bookmark"></span> 最近閱讀
    </h4>
    
    @if (!Bookmarks.Any())
    {
        <p class="no-bookmarks">尚無閱讀記錄</p>
    }
    else
    {
        <ul>
            @foreach (var bookmark in DisplayedBookmarks)
            {
                <li @onclick="() => OnClick(bookmark)">
                    <span class="bookmark-reference">@bookmark.DisplayReference</span>
                    <span class="bookmark-time">@bookmark.CreatedAt.ToLocalTime().ToString("MM/dd HH:mm")</span>
                </li>
            }
        </ul>
        
        @if (ShowMore && Bookmarks.Count > MaxDisplay)
        {
            <button class="show-more" @onclick="ToggleShowAll">
                @(IsShowingAll ? "收合" : $"顯示全部 ({Bookmarks.Count})")
            </button>
        }
    }
</div>

@code {
    [Parameter] public int MaxDisplay { get; set; } = 10;
    [Parameter] public bool ShowMore { get; set; } = false;
    [Parameter] public EventCallback<BibleBookmark> OnBookmarkClick { get; set; }
    
    [Inject] private IBibleBookmarkService BookmarkService { get; set; } = null!;
    
    private List<BibleBookmark> Bookmarks { get; set; } = new();
    private bool IsShowingAll { get; set; } = false;
    
    private IEnumerable<BibleBookmark> DisplayedBookmarks => 
        IsShowingAll ? Bookmarks : Bookmarks.Take(MaxDisplay);
    
    protected override async Task OnInitializedAsync()
    {
        Bookmarks = await BookmarkService.LoadBookmarksAsync();
    }
    
    private async Task OnClick(BibleBookmark bookmark)
    {
        await OnBookmarkClick.InvokeAsync(bookmark);
    }
    
    private void ToggleShowAll() => IsShowingAll = !IsShowingAll;
}
```

**Parameters**:
- `MaxDisplay: int` - 最大顯示筆數 (預設 10)
- `ShowMore: bool` - 是否顯示「顯示更多」按鈕
- `OnBookmarkClick: EventCallback<BibleBookmark>` - 書籤點擊回呼

---

### 8. BibleExportDialog.razor（導出對話框）

**職責**: 提供經文導出設定與預覽。

```razor
<div class="bible-export-dialog @(IsVisible ? "visible" : "hidden")">
    <div class="dialog-overlay" @onclick="Close"></div>
    <div class="dialog-content">
        <div class="dialog-header">
            <h3>導出經文</h3>
            <button class="close-btn" @onclick="Close">×</button>
        </div>
        
        <div class="dialog-body">
            <!-- 導出範圍 -->
            <div class="export-range">
                <h4>導出範圍</h4>
                <!-- 範圍選擇器元件 -->
            </div>
            
            <!-- 導出風格 -->
            <div class="export-style">
                <h4>導出風格</h4>
                <div class="style-options">
                    <label>
                        <input type="radio" name="style" value="Style1" 
                               @onchange="() => Options.Style = ExportStyle.Style1" />
                        風格 1: (書卷 章:節) 經文
                    </label>
                    <label>
                        <input type="radio" name="style" value="Style2"
                               @onchange="() => Options.Style = ExportStyle.Style2" />
                        風格 2: 標題 + 逐行經文
                    </label>
                    <label>
                        <input type="radio" name="style" value="Style3"
                               @onchange="() => Options.Style = ExportStyle.Style3" />
                        風格 3: 標題 + 連續段落
                    </label>
                </div>
            </div>
            
            <!-- 導出選項 -->
            <div class="export-options">
                <label>
                    <input type="checkbox" @bind="Options.IncludeBookTitle" />
                    包含書卷標題
                </label>
                <label>
                    <input type="checkbox" @bind="Options.OneFilePerBook" />
                    一本書一個檔案
                </label>
            </div>
            
            <!-- 預覽 -->
            <div class="export-preview">
                <h4>預覽</h4>
                <pre>@PreviewText</pre>
            </div>
        </div>
        
        <div class="dialog-footer">
            <button @onclick="Close">取消</button>
            <button @onclick="Export" class="btn-primary">導出</button>
        </div>
    </div>
</div>
```

**Parameters**:
- `IsVisible: bool` - 對話框可見性
- `Range: ExportRange` - 導出範圍
- `OnExport: EventCallback<(ExportOptions, string)>` - 導出回呼

**Services Injected**:
- `IBibleExportService`

---

## Navigation Menu Update

### NavMenu.razor 修改

在現有 `NavMenu.razor` 中新增「聖經」選單項目：

```razor
<!-- 在現有選單項目後新增 -->
<hr class="nav-divider mx-3 my-2" />

<div class="nav-item px-3">
    <div class="nav-group">
        <button class="nav-link nav-group-toggle @(IsBibleExpanded ? "expanded" : "")" 
                @onclick="ToggleBibleMenu">
            <span class="bi bi-book" aria-hidden="true"></span> 聖經
            <span class="bi @(IsBibleExpanded ? "bi-chevron-up" : "bi-chevron-down") toggle-icon"></span>
        </button>
        
        @if (IsBibleExpanded)
        {
            <div class="nav-group-items">
                <NavLink class="nav-link sub-item" href="bible/search">
                    <span class="bi bi-search" aria-hidden="true"></span> 聖經查詢
                </NavLink>
                <NavLink class="nav-link sub-item" href="bible/read">
                    <span class="bi bi-journal-text" aria-hidden="true"></span> 聖經閱讀
                </NavLink>
                <NavLink class="nav-link sub-item" href="bible/game">
                    <span class="bi bi-controller" aria-hidden="true"></span> 聖經遊戲
                </NavLink>
            </div>
        }
    </div>
</div>

@code {
    private bool IsBibleExpanded { get; set; } = false;
    
    private void ToggleBibleMenu()
    {
        IsBibleExpanded = !IsBibleExpanded;
    }
}
```

---

## CSS Styling Reference

所有聖經相關元件樣式統一放置於 `wwwroot/css/bible.css`：

```css
/* 主要容器 */
.bible-home-container,
.bible-search-container,
.bible-read-container,
.bible-game-container {
    padding: 1rem;
    max-width: 1200px;
    margin: 0 auto;
}

/* 子功能導航列 */
.bible-sub-nav {
    display: flex;
    gap: 1rem;
    margin-bottom: 1.5rem;
    padding: 0.5rem;
    background: var(--bs-light);
    border-radius: 0.5rem;
}

.bible-sub-nav .sub-nav-item {
    padding: 0.5rem 1rem;
    text-decoration: none;
    color: var(--bs-dark);
    border-radius: 0.25rem;
    transition: background-color 0.2s;
}

.bible-sub-nav .sub-nav-item:hover,
.bible-sub-nav .sub-nav-item.active {
    background-color: var(--bs-primary);
    color: white;
}

/* 經文顯示 */
.bible-verse-display {
    padding: 1rem;
    border-radius: 0.5rem;
}

.verse-line {
    display: flex;
    gap: 0.5rem;
    margin-bottom: 0.5rem;
    line-height: 1.8;
}

.verse-number {
    color: var(--bs-secondary);
    font-weight: bold;
    min-width: 2rem;
    text-align: right;
}

/* 設定面板 */
.bible-settings-panel {
    margin-bottom: 1rem;
    border: 1px solid var(--bs-border-color);
    border-radius: 0.5rem;
}

/* 更多樣式... */
```
