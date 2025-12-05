# Implementation Plan: 聖經查詢系統 (Bible Query System)

**Branch**: `2-bible-query-system` | **Date**: 2025-12-06 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-bible-query-system/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

為 CovenantPromptKey 網頁應用程式新增「聖經」子系統，提供聖經查詢、閱讀及遊戲互動體驗。系統採用 Blazor Server 架構搭配 .NET 10.0，整合 BibleData DLL（.NET Standard 2.1）提供 O(1) 快速經文查詢。核心功能包含：關鍵字即時搜尋（防抖動 + 排名）、書卷章節閱讀（導航 + 書籤）、顯示設定自訂（字形/大小/顏色）、Markdown 經文導出（三種風格）、以及經文猜猜遊戲（含錯題複習功能，記錄最近 5 次遊戲的錯題供學習回顧）。所有使用者設定與狀態透過 Web Storage API 持久化。

## Technical Context

**Language/Version**: C# / .NET 10.0 (Preview)  
**Primary Dependencies**: 
- Blazor Server (Interactive Server Components)
- BibleData DLL (.NET Standard 2.1) - 聖經經文資料與 O(1) 索引查詢
- Bootstrap 5 (UI 框架，沿用現有)
- CsvHelper (現有依賴)
- Markdig (Markdown 解析，現有依賴)

**Storage**: Browser localStorage (設定/書籤/遊戲記錄持久化) + sessionStorage (頁面狀態)  
**Testing**: xUnit + NSubstitute（沿用現有測試框架）；新增服務單元測試覆蓋率 ≥ 50%；前端 UI 採手動驗證  
**Target Platform**: Windows (Chromium-based browsers: Chrome, Edge)  
**Project Type**: Web Application (Single Project - Blazor Server，擴展現有架構)  
**Performance Goals**: 
- 關鍵字搜尋 < 2 秒（SC-001）
- 即時搜尋建議 < 300ms（SC-002）
- UI 互動響應 < 150ms
- BibleIndex 查詢 O(1) 時間複雜度

**Constraints**: 
- 搜尋防抖動延遲 200-300ms
- 書籤上限 10 筆
- 遊戲記錄保留最近 5 次 + 歷史最高分
- 錯題記錄保留最近 5 次遊戲的錯題（最多 50 題）
- 所有資料處理於用戶端完成，無外部伺服器傳輸
- 必須遵循 Brownfield Project 原則，僅新增功能，不修改現有架構

**Scale/Scope**: 
- 單一使用者本地工具
- 新增 4 個主要頁面（聖經主頁、查詢、閱讀、遊戲）
- 新增約 4-6 個 Services
- 新增約 4-6 個 Models

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Phase 0 Gate Evaluation

| 原則 | 狀態 | 說明 |
|------|------|------|
| **I. Brownfield Project Respect** | ✅ PASS | 本功能為新增子系統，不修改現有程式碼架構 |
| **II. Minimal Change Principle** | ✅ PASS | 僅新增聖經相關檔案，不觸及現有功能 |
| **III. Explicit Approval Principle** | ⏳ PENDING | 需新增 BibleData DLL 參考，待使用者核准 |
| **IV. Technology Stack Stability** | ✅ PASS | 維持 .NET 10.0，BibleData 為 .NET Standard 2.1 相容 |
| **V. Change Classification** | ✅ PASS | 屬於「Feature Addition」許可類別 |
| **VI. Code Quality Excellence** | ✅ PASS | 遵循 SOLID 原則與現有 Service 設計模式 |
| **VII. Testing Discipline** | ✅ PASS | 新功能將包含單元測試 |
| **VIII. User Experience Consistency** | ✅ PASS | 沿用現有 UI 框架與導航模式 |
| **IX. Performance Requirements** | ✅ PASS | 已定義明確效能目標 |
| **X. Documentation Synchronisation** | ⏳ PENDING | 完成後須更新 docs/getting-started.md |

### 需要使用者核准的變更

| 類別 | 變更項目 | 理由 |
|------|----------|------|
| **Package Management** | 新增 BibleData DLL 參考 | 提供聖經經文資料與查詢功能 |
| **Project Configuration** | 修改 CovenantPromptKey.csproj 加入 DLL 參考 | 整合 BibleData 類別庫 |
| **DI Configuration** | 新增 BibleIndex Singleton 註冊 | 應用程式啟動時建立一次索引 |

## Project Structure

### Documentation (this feature)

```text
specs/2-bible-query-system/
├── plan.md              # This file (implementation plan)
├── research.md          # Phase 0: Technical research & decisions
├── data-model.md        # Phase 1: Domain models & entities
├── quickstart.md        # Phase 1: Quick start guide
├── contracts/           # Phase 1: Service & component contracts
│   ├── services.md      # Service interface definitions
│   └── components.md    # Blazor component specifications
└── tasks.md             # Phase 2 output (NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
CovenantPromptKey/
├── Program.cs                        # DI configuration (新增 BibleIndex 註冊)
├── CovenantPromptKey.csproj          # 新增 BibleData DLL 參考
│
├── Models/                           # Domain entities
│   ├── Bible/                        # 聖經相關模型 (新增)
│   │   ├── BibleSettings.cs          # 顯示設定模型
│   │   ├── BibleBookmark.cs          # 書籤模型
│   │   ├── BibleGameRecord.cs        # 遊戲記錄模型
│   │   └── BiblePageState.cs         # 頁面狀態模型
│   └── [existing models...]
│
├── Services/                         # Business logic services
│   ├── Interfaces/                   # Service contracts
│   │   ├── IBibleSearchService.cs    # 聖經搜尋服務介面 (新增)
│   │   ├── IBibleReadingService.cs   # 聖經閱讀服務介面 (新增)
│   │   ├── IBibleExportService.cs    # 經文導出服務介面 (新增)
│   │   ├── IBibleGameService.cs      # 聖經遊戲服務介面 (新增)
│   │   ├── IBibleSettingsService.cs  # 設定管理服務介面 (新增)
│   │   └── [existing interfaces...]
│   └── Implementations/              # Service implementations
│       ├── BibleSearchService.cs     # (新增)
│       ├── BibleReadingService.cs    # (新增)
│       ├── BibleExportService.cs     # (新增)
│       ├── BibleGameService.cs       # (新增)
│       ├── BibleSettingsService.cs   # (新增)
│       └── [existing implementations...]
│
├── Components/
│   ├── Layout/
│   │   └── NavMenu.razor             # 新增「聖經」選單項目
│   ├── Pages/
│   │   ├── Bible/                    # 聖經頁面 (新增目錄)
│   │   │   ├── BibleHomePage.razor   # 聖經主頁
│   │   │   ├── BibleSearchPage.razor # 聖經查詢頁面
│   │   │   ├── BibleReadPage.razor   # 聖經閱讀頁面
│   │   │   └── BibleGamePage.razor   # 聖經遊戲頁面
│   │   └── [existing pages...]
│   └── Shared/
│       ├── Bible/                    # 聖經共用元件 (新增目錄)
│       │   ├── BibleSettingsPanel.razor    # 顯示設定面板
│       │   ├── BibleVerseDisplay.razor     # 經文顯示元件
│       │   ├── BibleBookSelector.razor     # 書卷選擇器
│       │   ├── BibleChapterNavigator.razor # 章節導航器
│       │   ├── BibleSearchResults.razor    # 搜尋結果列表
│       │   ├── BibleBookmarkList.razor     # 書籤列表
│       │   └── BibleExportDialog.razor     # 導出對話框
│       └── [existing shared components...]
│
└── wwwroot/
    └── css/
        └── bible.css                 # 聖經專用樣式 (新增)

CovenantPromptKey.Tests/
├── Services/
│   ├── BibleSearchServiceTests.cs    # (新增)
│   ├── BibleReadingServiceTests.cs   # (新增)
│   ├── BibleExportServiceTests.cs    # (新增)
│   ├── BibleGameServiceTests.cs      # (新增)
│   ├── BibleSettingsServiceTests.cs  # (新增)
│   └── [existing tests...]
└── [existing test files...]
```

**Structure Decision**: 採用現有 Blazor Server 單一專案架構進行擴展。新增聖經相關檔案皆放置於獨立子目錄（`Models/Bible/`、`Components/Pages/Bible/`、`Components/Shared/Bible/`），確保與現有功能隔離且易於維護。遵循現有 Services 設計模式（Interface + Implementation 分離）。

## Complexity Tracking

> 本功能為純新增功能，無需違反 Constitution 原則

| 項目 | 說明 |
|------|------|
| 新增 DLL 依賴 | BibleData DLL 為獨立類別庫，不影響現有依賴 |
| 新增 DI 註冊 | BibleIndex 設為 Singleton，符合最佳實踐 |
| 導航選單修改 | NavMenu.razor 新增選單項目，屬於許可的「Feature Addition」 |

---

## Post-Phase 1 Constitution Check

*Re-evaluation after design completion*

### Design Review Gate

| 原則 | 狀態 | 設計驗證 |
|------|------|----------|
| **I. Brownfield Project Respect** | ✅ PASS | 所有新增檔案皆放置於獨立子目錄，不修改現有程式碼結構 |
| **II. Minimal Change Principle** | ✅ PASS | NavMenu.razor 僅新增選單區塊，不影響現有項目 |
| **III. Explicit Approval Principle** | ⏳ PENDING | 需使用者核准：BibleData DLL 參考、DI 註冊、csproj 修改 |
| **IV. Technology Stack Stability** | ✅ PASS | 維持 .NET 10.0，BibleData (.NET Standard 2.1) 完全相容 |
| **V. Change Classification** | ✅ PASS | 所有變更屬於「Feature Addition」許可類別 |
| **VI. Code Quality Excellence** | ✅ PASS | Service 介面遵循 SOLID，使用 DI 與 Nullable Reference Types |
| **VII. Testing Discipline** | ✅ PASS | 已規劃 5 個服務測試檔案，將使用 xUnit + NSubstitute（沿用現有框架） |
| **VIII. User Experience Consistency** | ✅ PASS | UI 元件沿用 Bootstrap 5，導航模式與現有一致 |
| **IX. Performance Requirements** | ✅ PASS | 使用 BibleIndex O(1) 查詢，防抖動搜尋，符合效能目標 |
| **X. Documentation Synchronisation** | ⏳ PENDING | 功能完成後須更新 docs/getting-started.md |

### 設計符合性摘要

| 檢查項目 | 結果 |
|----------|------|
| 資料模型完整定義 | ✅ 6 個核心模型 + 3 個輔助模型 |
| Service 介面契約 | ✅ 7 個服務介面已定義 |
| 元件規格 | ✅ 4 個頁面 + 8 個共用元件 |
| 儲存策略 | ✅ localStorage + sessionStorage 分層 |
| 效能設計 | ✅ O(1) 查詢 + 防抖動 + 可取消搜尋 |

### 待使用者核准項目

在進入 Phase 2 (tasks) 之前，需使用者明確核准以下變更：

1. **✅ 新增 BibleData DLL 參考**
   - 路徑：`libs/BibleData.dll`
   - 影響：`CovenantPromptKey.csproj` 新增 `<Reference>` 項目

2. **✅ DI 服務註冊**
   - 影響：`Program.cs` 新增 8 行服務註冊程式碼
   - BibleIndex (Singleton) + 7 個 Scoped Services

3. **✅ NavMenu.razor 修改**
   - 影響：新增約 20 行程式碼（聖經選單區塊）
   - 不影響現有選單項目

---

**Phase 1 完成狀態**: ✅ 所有設計文件已生成

**下一步**: 執行 `/speckit.tasks` 命令產生 tasks.md
