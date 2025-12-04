# Quick Start Guide: 互動式關鍵字替換介面

**Branch**: `001-interactive-keyword-replace` | **Date**: 2025-12-03

本指南提供快速開始開發互動式關鍵字替換介面功能所需的所有資訊。

---

## 環境需求 (Prerequisites)

| 項目 | 版本 | 備註 |
|------|------|------|
| .NET SDK | 10.0 Preview | 專案目標框架 |
| Visual Studio 2022 | 17.12+ | 或 VS Code + C# Dev Kit |
| Node.js | 18+ | (可選) 若需前端工具鏈 |
| Browser | Chrome/Edge 最新版 | Blazor Server 支援 |

---

## 快速開始 (Getting Started)

### 1. 複製專案與切換分支

```powershell
git clone https://github.com/bod3612272/CovenantPromptKey.git
cd CovenantPromptKey
git checkout 001-interactive-keyword-replace
```

### 2. 安裝相依套件

```powershell
cd CovenantPromptKey
dotnet restore
dotnet add package CsvHelper
dotnet add package Markdig
```

### 3. 執行應用程式

```powershell
dotnet run
```

應用程式將自動選擇可用 port 並開啟預設瀏覽器。

### 4. 驗證安裝

開啟瀏覽器導覽至 `https://localhost:5001` (或應用程式顯示的 URL)，確認首頁正常載入。

---

## 專案結構速覽 (Project Structure Overview)

```
CovenantPromptKey/
├── Models/           # 領域實體 (KeywordMapping, DetectedKeyword, etc.)
├── Services/         # 業務邏輯服務
│   ├── Interfaces/   # 服務介面定義
│   └── Implementations/ # 服務實作
├── Constants/        # 常數定義 (限制值、保留字)
├── Components/       # Blazor UI 元件
│   ├── Layout/       # 佈局元件 (MainLayout, NavMenu)
│   ├── Pages/        # 頁面元件
│   └── Shared/       # 共用元件
└── wwwroot/          # 靜態資源
```

---

## 核心服務說明 (Core Services)

| 服務 | 責任 | 生命週期 |
|------|------|---------|
| `IKeywordService` | 關鍵字偵測與替換核心邏輯 | Transient |
| `IDictionaryService` | 關鍵字字典 CRUD 操作 | Scoped |
| `ICsvService` | CSV 匯入/匯出處理 | Transient |
| `ISessionStorageService` | 瀏覽器 SessionStorage 互動 | Scoped |
| `IWorkSessionService` | 工作階段狀態管理 | Scoped |
| `IDebugLogService` | 除錯日誌記錄 | Singleton |
| `IMarkdownService` | Markdown 結構解析 | Transient |
| `IKeywordValidationService` | 關鍵字驗證規則 | Transient |

---

## 服務註冊範例 (DI Registration)

在 `Program.cs` 中加入以下服務註冊：

```csharp
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

## 開發工作流程 (Development Workflow)

### 步驟 1: 建立 Models

先從 `data-model.md` 實作領域實體：
- `KeywordMapping` - 關鍵字映射
- `DetectedKeyword` / `KeywordOccurrence` - 偵測結果
- `WorkSession` - 工作階段
- `LogEntry` - 日誌項目
- `MaskResult` / `RestoreResult` - 操作結果

### 步驟 2: 實作 Services

依照 `contracts/services.md` 實作服務介面：
1. 先實作基礎設施服務 (`SessionStorageService`, `DebugLogService`)
2. 再實作核心服務 (`KeywordValidationService`, `DictionaryService`)
3. 最後實作主要功能服務 (`KeywordService`, `CsvService`, `MarkdownService`)

### 步驟 3: 建立 Components

依照 `contracts/components.md` 實作 Blazor 元件：
1. 佈局元件 (`ThreeColumnLayout`)
2. 共用元件 (`ColorPicker`, `ConfirmationDialog`, `ToastNotification`)
3. 功能元件 (`SourceTextEditor`, `KeywordControlPanel`, `ResultViewer`)
4. 頁面元件 (`MaskRestorePage`, `SettingsPage`, `DebugLogPage`)

### 步驟 4: TDD 開發與手動驗證

**後端核心服務** - 採用 TDD (Red-Green-Refactor) 流程：
1. **Red**: 先撰寫失敗的測試案例
2. **Green**: 實作最小可行程式碼使測試通過
3. **Refactor**: 重構程式碼，確保測試仍通過

**測試覆蓋率目標**: 核心服務 ≥ 50%

應測試的核心服務：
- `IKeywordService` - 關鍵字偵測演算法正確性
- `IKeywordValidationService` - 字典驗證邏輯
- `ICsvService` - CSV 匯入/匯出解析
- `IMarkdownService` - Markdown 結構解析
- `IDictionaryService` - 字典 CRUD 操作

**前端 UI** - 採手動驗證（不計入覆蓋率）：
- 三欄式佈局顯示
- 勾選/取消勾選互動
- 捲動與高亮行為

---

## 常用指令 (Common Commands)

```powershell
# 執行應用程式
dotnet run

# 執行測試
dotnet test

# 建置發佈版本
dotnet publish -c Release

# 監視模式開發
dotnet watch run

# 新增 NuGet 套件
dotnet add package <PackageName>
```

---

## 關鍵配置 (Key Configurations)

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### launchSettings.json

```json
{
  "profiles": {
    "CovenantPromptKey": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 相關文件 (Related Documents)

| 文件 | 說明 |
|------|------|
| [spec.md](./spec.md) | 功能規格書（需求與驗收條件） |
| [plan.md](./plan.md) | 實作計畫（技術決策與架構） |
| [research.md](./research.md) | 技術研究（演算法與相依套件選擇） |
| [data-model.md](./data-model.md) | 領域模型（實體定義與驗證規則） |
| [contracts/services.md](./contracts/services.md) | 服務契約（介面定義） |
| [contracts/components.md](./contracts/components.md) | 元件契約（UI 規格） |

---

## 常見問題 (FAQ)

### Q: 為何選擇 Blazor Server 而非 Blazor WASM?

A: Blazor Server 適合需要即時互動的工具類應用，無需等待 WASM 下載，且開發除錯更為便利。

### Q: 如何測試關鍵字偵測效能?

A: 使用 `BenchmarkDotNet` 建立效能測試，驗證 SC-001 (1,000 字 < 200ms) 目標。

### Q: sessionStorage 與 localStorage 的使用時機?

A: 
- `sessionStorage`: 工作階段狀態（刷新頁面可還原，關閉分頁清除）
- `localStorage`: 關鍵字字典（持久化儲存，跨分頁共享）

---

## 聯絡與支援 (Support)

如有問題，請透過以下方式回報：
- GitHub Issues: [CovenantPromptKey Issues](https://github.com/bod3612272/CovenantPromptKey/issues)
- 內建「回報問題」頁面
