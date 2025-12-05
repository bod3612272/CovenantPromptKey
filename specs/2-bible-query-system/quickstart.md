# Quickstart Guide: 聖經查詢系統

**Date**: 2025-12-06  
**Related Plan**: [plan.md](./plan.md)

---

## Prerequisites

確保以下環境已就緒：

- ✅ .NET 10.0 SDK (Preview)
- ✅ Visual Studio 2022 / VS Code with C# Dev Kit
- ✅ 現有 CovenantPromptKey 專案可正常建置執行
- ✅ BibleData.dll 檔案（.NET Standard 2.1）

---

## Step 1: 取得 BibleData DLL

將 `BibleData.dll` 放置於專案目錄：

```
CovenantPromptKey/
├── libs/                    # 新建此目錄
│   └── BibleData.dll        # 放置 DLL
├── CovenantPromptKey/
│   └── CovenantPromptKey.csproj
└── ...
```

---

## Step 2: 新增 DLL 參考

編輯 `CovenantPromptKey/CovenantPromptKey.csproj`，加入 BibleData 參考：

```xml
<ItemGroup>
  <Reference Include="BibleData">
    <HintPath>..\libs\BibleData.dll</HintPath>
  </Reference>
</ItemGroup>
```

驗證參考是否正確：

```powershell
cd CovenantPromptKey
dotnet build
```

---

## Step 3: 註冊 DI 服務

編輯 `Program.cs`，加入 BibleIndex 與聖經服務註冊：

```csharp
using BibleData;
using CovenantPromptKey.Services.Interfaces;
using CovenantPromptKey.Services.Implementations;

// ... existing code ...

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

// ... existing code ...
```

---

## Step 4: 建立 Model 檔案

在 `Models/Bible/` 目錄下建立以下檔案：

```
Models/
└── Bible/
    ├── BibleSettings.cs
    ├── BibleBookmark.cs
    ├── BibleGameRecord.cs
    ├── BiblePageState.cs
    ├── ExportOptions.cs
    ├── ExportRange.cs
    └── SearchResultItem.cs
```

參考 [data-model.md](./data-model.md) 取得完整模型定義。

---

## Step 5: 建立 Service 介面與實作

### 介面 (`Services/Interfaces/`)

```
Services/Interfaces/
├── IBibleSearchService.cs
├── IBibleReadingService.cs
├── IBibleExportService.cs
├── IBibleGameService.cs
├── IBibleSettingsService.cs
├── IBibleBookmarkService.cs
└── IBiblePageStateService.cs
```

### 實作 (`Services/Implementations/`)

```
Services/Implementations/
├── BibleSearchService.cs
├── BibleReadingService.cs
├── BibleExportService.cs
├── BibleGameService.cs
├── BibleSettingsService.cs
├── BibleBookmarkService.cs
└── BiblePageStateService.cs
```

參考 [contracts/services.md](./contracts/services.md) 取得完整介面定義。

---

## Step 6: 建立 Blazor 元件

### 頁面元件 (`Components/Pages/Bible/`)

```
Components/Pages/Bible/
├── BibleHomePage.razor
├── BibleSearchPage.razor
├── BibleReadPage.razor
└── BibleGamePage.razor
```

### 共用元件 (`Components/Shared/Bible/`)

```
Components/Shared/Bible/
├── BibleSubNavigation.razor
├── BibleSettingsPanel.razor
├── BibleVerseDisplay.razor
├── BibleBookSelector.razor
├── BibleChapterNavigator.razor
├── BibleSearchResults.razor
├── BibleBookmarkList.razor
└── BibleExportDialog.razor
```

參考 [contracts/components.md](./contracts/components.md) 取得完整元件規格。

---

## Step 7: 更新導航選單

編輯 `Components/Layout/NavMenu.razor`，加入聖經選單：

```razor
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

## Step 8: 新增 CSS 樣式

建立 `wwwroot/css/bible.css` 並在 `App.razor` 中引用：

```html
<link rel="stylesheet" href="css/bible.css" />
```

---

## Step 9: 執行驗證

### 建置專案

```powershell
cd CovenantPromptKey
dotnet build
```

### 執行專案

```powershell
dotnet run
```

### 驗證清單

- [ ] 首頁左側選單顯示「聖經」選項
- [ ] 點擊展開顯示三個子選項
- [ ] 點擊「聖經查詢」進入查詢頁面
- [ ] 輸入關鍵字（如「愛」）可搜尋到結果
- [ ] 點擊「聖經閱讀」進入閱讀頁面
- [ ] 可選擇書卷與章節閱讀經文
- [ ] 上下章導航正常運作
- [ ] 顯示設定變更立即生效
- [ ] 點擊「聖經遊戲」進入遊戲頁面
- [ ] 遊戲流程正常（開始→答題→結束）
- [ ] 遊戲記錄正確儲存

---

## Development Workflow

### 建議開發順序

```
1. Models (資料模型)
   ↓
2. Service Interfaces (服務介面)
   ↓
3. Service Implementations (服務實作)
   ↓
4. Unit Tests (單元測試)
   ↓
5. Shared Components (共用元件)
   ↓
6. Page Components (頁面元件)
   ↓
7. NavMenu Update (選單更新)
   ↓
8. CSS Styling (樣式調整)
   ↓
9. Integration Testing (整合測試)
```

### 優先級實作順序

依據 spec 優先級：

**Phase 1 (P1 - 核心功能)**
1. IBibleReadingService + BibleReadPage（閱讀功能）
2. IBibleSearchService + BibleSearchPage（查詢功能）
3. NavMenu 更新（導航入口）

**Phase 2 (P2 - 重要功能)**
4. IBibleSettingsService + BibleSettingsPanel（設定功能）
5. IBibleBookmarkService + BibleBookmarkList（書籤功能）
6. IBibleExportService + BibleExportDialog（導出功能）

**Phase 3 (P3 - 延伸功能)**
7. IBibleGameService + BibleGamePage（遊戲功能）

---

## Troubleshooting

### BibleData DLL 載入失敗

**症狀**: 建置時出現「找不到 BibleData」錯誤

**解決方案**:
1. 確認 DLL 路徑正確
2. 確認 DLL 為 .NET Standard 2.1 版本
3. 嘗試清理並重建：`dotnet clean && dotnet build`

### BibleIndex 建立緩慢

**症狀**: 應用程式啟動時間過長

**說明**: BibleIndex 首次建立需要解析所有經文資料，約需 1-2 秒，屬正常現象。後續查詢為 O(1) 時間複雜度。

### Storage 存取錯誤

**症狀**: 設定或書籤無法儲存

**解決方案**:
1. 確認瀏覽器允許 localStorage
2. 清除瀏覽器快取並重試
3. 檢查 Console 錯誤訊息

---

## Related Documents

- [plan.md](./plan.md) - 實作計畫
- [research.md](./research.md) - 技術研究
- [data-model.md](./data-model.md) - 資料模型
- [contracts/services.md](./contracts/services.md) - 服務契約
- [contracts/components.md](./contracts/components.md) - 元件規格
- [spec.md](./spec.md) - 功能規格
