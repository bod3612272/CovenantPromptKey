# Implementation Plan: 互動式關鍵字替換介面

**Branch**: `001-interactive-keyword-replace` | **Date**: 2025-12-03 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-interactive-keyword-replace/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

建構一個互動式關鍵字替換工具，提供三欄式佈局（原文/控制面板/結果）的文字遮罩與還原介面。系統採用 Blazor Server 架構搭配 .NET 10.0，使用 Aho-Corasick 演算法進行高效能多模式字串匹配，支援最多 500 組關鍵字與 100,000 字元文本處理。核心功能包含：關鍵字字典管理（CRUD + CSV 匯入匯出）、即時偵測與高亮、選擇性替換、AI 回應還原、以及內建 Debug Log 追蹤機制。

## Technical Context

**Language/Version**: C# / .NET 10.0 (Preview)  
**Primary Dependencies**: 
- Blazor Server (Interactive Server Components)
- CsvHelper (CSV 匯入/匯出)
- Markdig (Markdown 解析)
- Bootstrap 5 (UI 框架)

**Storage**: Browser localStorage (字典持久化) + sessionStorage (工作階段狀態)  
**Testing**: xUnit + TDD (Red-Green-Refactor)；後端核心服務單元測試覆蓋率 ≥ 50%；前端 UI 採手動驗證  
**Target Platform**: Windows (Chromium-based browsers: Chrome, Edge)  
**Project Type**: Web Application (Single Project - Blazor Server)  
**Performance Goals**: 
- 1,000 字偵測 < 200ms
- UI 互動響應 < 150ms
- 長文本 (100K 字元) 偵測 < 1,000ms

**Constraints**: 
- 最大關鍵字數量: 500 組
- 最大文本長度: 100,000 字元
- Debug Log 最大行數: 5,000 行
- 所有文本處理於用戶端完成，無外部伺服器傳輸

**Scale/Scope**: 
- 單一使用者本地工具
- 7 個主要頁面/功能區

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

由於 `constitution.md` 目前為範本狀態，未定義具體核心原則。以下採用最佳實踐進行自我檢核：

| 檢核項目 | 狀態 | 備註 |
|---------|------|------|
| Service Layer 設計符合 SOLID | ✅ PASS | 參見 `.github/instructions/CreateService.instructions.md` |
| 元件職責單一化 | ✅ PASS | 每個 Blazor 元件專注單一功能 |
| 依賴注入配置正確 | ✅ PASS | Singleton/Scoped/Transient 適當分配 |
| 無外部資料傳輸 | ✅ PASS | 所有處理於瀏覽器端完成 |
| 效能目標明確可測量 | ✅ PASS | 已定義 SC-001 ~ SC-007 成功標準 |

## Project Structure

### Documentation (this feature)

```text
specs/001-interactive-keyword-replace/
├── plan.md              # This file (implementation plan)
├── research.md          # Phase 0: Technical research & decisions
├── data-model.md        # Phase 1: Domain models & entities
├── quickstart.md        # Phase 1: Quick start guide
├── contracts/           # Phase 1: Service & component contracts
│   ├── services.md      # Service interface definitions
│   └── components.md    # Blazor component specifications
├── checklists/
│   └── requirements.md  # Requirements tracking checklist
└── tasks.md             # Phase 2 output (NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
CovenantPromptKey/
├── Program.cs                    # Application entry point & DI configuration
├── CovenantPromptKey.csproj      # Project file (.NET 10.0)
├── appsettings.json              # Configuration
│
├── Models/                       # Domain entities
│   ├── KeywordMapping.cs         # Keyword mapping entity
│   ├── DetectedKeyword.cs        # Detection result models
│   ├── WorkSession.cs            # Session state model
│   ├── LogEntry.cs               # Debug log entry
│   └── Results/                  # Operation result models
│       ├── MaskResult.cs
│       ├── RestoreResult.cs
│       └── CsvImportResult.cs
│
├── Services/                     # Business logic services
│   ├── Interfaces/               # Service contracts
│   │   ├── IKeywordService.cs
│   │   ├── IDictionaryService.cs
│   │   ├── ICsvService.cs
│   │   ├── ISessionStorageService.cs
│   │   ├── IWorkSessionService.cs
│   │   ├── IDebugLogService.cs
│   │   ├── IMarkdownService.cs
│   │   └── IKeywordValidationService.cs
│   └── Implementations/          # Service implementations
│       ├── KeywordService.cs
│       ├── DictionaryService.cs
│       ├── CsvService.cs
│       ├── SessionStorageService.cs
│       ├── WorkSessionService.cs
│       ├── DebugLogService.cs
│       ├── MarkdownService.cs
│       └── KeywordValidationService.cs
│
├── Constants/                    # Application constants
│   ├── AppConstants.cs           # Max limits, storage keys
│   └── ReservedKeywords.cs       # Programming language keywords
│
├── Components/                   # Blazor components
│   ├── _Imports.razor
│   ├── App.razor
│   ├── Routes.razor
│   ├── Layout/
│   │   ├── MainLayout.razor      # Main application layout
│   │   ├── NavMenu.razor         # Navigation menu (7 items)
│   │   └── ReconnectModal.razor  # Connection handling
│   ├── Pages/
│   │   ├── MaskRestorePage.razor # Main mask/restore page with tabs
│   │   ├── SettingsPage.razor    # Keyword dictionary management
│   │   ├── DebugLogPage.razor    # Debug log viewer
│   │   ├── HelpPage.razor        # Help documentation
│   │   ├── AboutPage.razor       # About information
│   │   ├── ReportIssuePage.razor # Issue reporting
│   │   └── ChangelogPage.razor   # Version history
│   └── Shared/                   # Reusable UI components
│       ├── ThreeColumnLayout.razor
│       ├── SourceTextEditor.razor
│       ├── KeywordControlPanel.razor
│       ├── ResultViewer.razor
│       ├── StatisticsDashboard.razor
│       ├── ConfirmationDialog.razor
│       ├── KeywordForm.razor
│       ├── KeywordList.razor
│       ├── ColorPicker.razor
│       ├── CsvImportExport.razor
│       ├── LogViewer.razor
│       └── ToastNotification.razor
│
├── wwwroot/
│   ├── app.css                   # Global styles
│   ├── js/
│   │   └── interop.js            # JS interop for clipboard, scroll
│   └── lib/
│       └── bootstrap/            # Bootstrap 5
│
└── Properties/
    └── launchSettings.json       # Auto port selection (FR-030) & browser launch (FR-031)
```

**Structure Decision**: 採用 Single Project 結構，因為此為純前端 Blazor Server 應用程式，所有業務邏輯與 UI 均在同一專案內。Models/Services/Components 分層清晰，符合關注點分離原則。

## Complexity Tracking

> 無 Constitution 違規需要說明。設計已遵循最簡化原則：
> - 無引入額外資料庫（使用瀏覽器儲存）
> - 無引入複雜狀態管理庫（使用 Service Layer + Cascading Parameter）
> - 無引入額外 UI 元件庫（使用現有 Bootstrap）

## Phase 1 Artifacts Generated

| 文件 | 狀態 | 說明 |
|------|------|------|
| `research.md` | ✅ 完成 | 技術決策與相依套件選擇 |
| `data-model.md` | ✅ 完成 | 領域模型、實體定義、狀態轉換 |
| `contracts/services.md` | ✅ 完成 | 8 個服務介面契約 |
| `contracts/components.md` | ✅ 完成 | 14 個 Blazor 元件規格 |
| `quickstart.md` | ✅ 完成 | 快速開始指南 |
