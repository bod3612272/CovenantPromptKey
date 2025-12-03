# Tasks: 互動式關鍵字替換介面

**Input**: Design documents from `/specs/001-interactive-keyword-replace/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: 本功能規格已指定需要 TDD 開發流程，後端核心服務單元測試覆蓋率目標 ≥ 50%，前端 UI 採手動驗證。

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `CovenantPromptKey/` at repository root
- **Tests**: `CovenantPromptKey.Tests/` at repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization, dependencies, and basic structure

- [ ] T001 Install required NuGet packages (CsvHelper, Markdig) via `dotnet add package`
- [ ] T001a [P] Configure auto port selection and browser launch in `CovenantPromptKey/Properties/launchSettings.json` (FR-030, FR-031)
- [ ] T002 [P] Create constants file in `CovenantPromptKey/Constants/AppConstants.cs`
- [ ] T003 [P] Create reserved keywords file in `CovenantPromptKey/Constants/ReservedKeywords.cs`
- [ ] T004 Create test project `CovenantPromptKey.Tests/CovenantPromptKey.Tests.csproj` with xUnit

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Models (Core Entities)

- [ ] T005 [P] Create `KeywordMapping` model in `CovenantPromptKey/Models/KeywordMapping.cs`
- [ ] T006 [P] Create `DetectedKeyword` and `KeywordOccurrence` models in `CovenantPromptKey/Models/DetectedKeyword.cs`
- [ ] T007 [P] Create `WorkSession` and `WorkMode` enum in `CovenantPromptKey/Models/WorkSession.cs`
- [ ] T008 [P] Create `LogEntry` and `LogLevel` enum in `CovenantPromptKey/Models/LogEntry.cs`
- [ ] T009 [P] Create `MaskResult`, `RestoreResult`, `ReplacementDetail` in `CovenantPromptKey/Models/Results/MaskResult.cs`
- [ ] T010 [P] Create `CsvImportResult` in `CovenantPromptKey/Models/Results/CsvImportResult.cs`
- [ ] T011 [P] Create `ValidationResult<T>`, `CsvValidationResult`, `CsvError` in `CovenantPromptKey/Models/Results/ValidationResult.cs`
- [ ] T012 [P] Create `MarkdownStructure` and `TextRange` in `CovenantPromptKey/Models/MarkdownStructure.cs`

### Service Interfaces

- [ ] T013 [P] Create `IKeywordService` interface in `CovenantPromptKey/Services/Interfaces/IKeywordService.cs`
- [ ] T014 [P] Create `IDictionaryService` interface in `CovenantPromptKey/Services/Interfaces/IDictionaryService.cs`
- [ ] T015 [P] Create `ICsvService` interface in `CovenantPromptKey/Services/Interfaces/ICsvService.cs`
- [ ] T016 [P] Create `ISessionStorageService` interface in `CovenantPromptKey/Services/Interfaces/ISessionStorageService.cs`
- [ ] T017 [P] Create `IWorkSessionService` interface in `CovenantPromptKey/Services/Interfaces/IWorkSessionService.cs`
- [ ] T018 [P] Create `IDebugLogService` interface in `CovenantPromptKey/Services/Interfaces/IDebugLogService.cs`
- [ ] T019 [P] Create `IMarkdownService` interface in `CovenantPromptKey/Services/Interfaces/IMarkdownService.cs`
- [ ] T020 [P] Create `IKeywordValidationService` interface in `CovenantPromptKey/Services/Interfaces/IKeywordValidationService.cs`

### Core Infrastructure Services

- [ ] T021 Unit tests for `DebugLogService` in `CovenantPromptKey.Tests/Services/DebugLogServiceTests.cs`
- [ ] T022 Implement `DebugLogService` in `CovenantPromptKey/Services/Implementations/DebugLogService.cs`
- [ ] T023 Unit tests for `SessionStorageService` in `CovenantPromptKey.Tests/Services/SessionStorageServiceTests.cs`
- [ ] T024 Implement `SessionStorageService` in `CovenantPromptKey/Services/Implementations/SessionStorageService.cs`
- [ ] T025 Create JS interop file for clipboard/scroll in `CovenantPromptKey/wwwroot/js/interop.js`

### Dependency Injection Registration

- [ ] T026 Update `CovenantPromptKey/Program.cs` to register all services with appropriate lifetimes

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - 核心替換流程 (Priority: P1) 🎯 MVP

**Goal**: 使用者可貼上文本，系統偵測關鍵字並高亮，可選擇性替換，產出安全文本

**Independent Test**: 貼上包含已知關鍵字的文本，驗證偵測、選擇、替換流程，確認統計資訊正確

### Tests for User Story 1 ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T027 [P] [US1] Unit tests for `KeywordValidationService` in `CovenantPromptKey.Tests/Services/KeywordValidationServiceTests.cs`
- [ ] T028 [P] [US1] Unit tests for `MarkdownService` in `CovenantPromptKey.Tests/Services/MarkdownServiceTests.cs`
- [ ] T029 [P] [US1] Unit tests for `KeywordService.DetectKeywordsAsync` in `CovenantPromptKey.Tests/Services/KeywordServiceTests.cs`
- [ ] T030 [P] [US1] Unit tests for `KeywordService.ApplyMaskAsync` in `CovenantPromptKey.Tests/Services/KeywordServiceTests.cs`

### Implementation for User Story 1

- [ ] T031 [US1] Implement `KeywordValidationService` in `CovenantPromptKey/Services/Implementations/KeywordValidationService.cs`
- [ ] T032 [US1] Implement `MarkdownService` (Markdig integration) in `CovenantPromptKey/Services/Implementations/MarkdownService.cs`
- [ ] T033 [US1] Implement Aho-Corasick algorithm helper in `CovenantPromptKey/Services/Implementations/AhoCorasickMatcher.cs`
- [ ] T034 [US1] Implement `KeywordService` (detect + mask) in `CovenantPromptKey/Services/Implementations/KeywordService.cs`
- [ ] T035 [US1] Implement `WorkSessionService` in `CovenantPromptKey/Services/Implementations/WorkSessionService.cs`

### UI Components for User Story 1

- [ ] T036 [P] [US1] Create `ThreeColumnLayout.razor` component in `CovenantPromptKey/Components/Shared/ThreeColumnLayout.razor`
- [ ] T037 [P] [US1] Create `ThreeColumnLayout.razor.css` styles in `CovenantPromptKey/Components/Shared/ThreeColumnLayout.razor.css`
- [ ] T038 [P] [US1] Create `ToastNotification.razor` component in `CovenantPromptKey/Components/Shared/ToastNotification.razor`
- [ ] T039 [P] [US1] Create `ConfirmationDialog.razor` component in `CovenantPromptKey/Components/Shared/ConfirmationDialog.razor`
- [ ] T040 [US1] Create `SourceTextEditor.razor` component in `CovenantPromptKey/Components/Shared/SourceTextEditor.razor`
- [ ] T041 [US1] Create `KeywordControlPanel.razor` component in `CovenantPromptKey/Components/Shared/KeywordControlPanel.razor`
- [ ] T042 [US1] Create `StatisticsDashboard.razor` component in `CovenantPromptKey/Components/Shared/StatisticsDashboard.razor`
- [ ] T043 [US1] Create `ResultViewer.razor` component in `CovenantPromptKey/Components/Shared/ResultViewer.razor`
- [ ] T044 [US1] Create `MaskRestorePage.razor` (Mask tab only) in `CovenantPromptKey/Components/Pages/MaskRestorePage.razor`
- [ ] T045 [US1] Update `NavMenu.razor` to include new navigation items in `CovenantPromptKey/Components/Layout/NavMenu.razor`
- [ ] T046 [US1] Update global styles in `CovenantPromptKey/wwwroot/app.css` for keyword highlighting

**Checkpoint**: User Story 1 should be fully functional - mask workflow complete

---

## Phase 4: User Story 2 - 關鍵字字典管理 (Priority: P1)

**Goal**: 使用者可手動 CRUD 關鍵字映射，可 CSV 匯入/匯出字典

**Independent Test**: 在關鍵字管理頁面新增、修改、刪除關鍵字，驗證匯入/匯出功能正常

### Tests for User Story 2 ⚠️

- [ ] T047 [P] [US2] Unit tests for `DictionaryService` in `CovenantPromptKey.Tests/Services/DictionaryServiceTests.cs`
- [ ] T048 [P] [US2] Unit tests for `CsvService` in `CovenantPromptKey.Tests/Services/CsvServiceTests.cs`

### Implementation for User Story 2

- [ ] T049 [US2] Implement `DictionaryService` (CRUD + localStorage) in `CovenantPromptKey/Services/Implementations/DictionaryService.cs`
- [ ] T050 [US2] Implement `CsvService` (CsvHelper integration) in `CovenantPromptKey/Services/Implementations/CsvService.cs`

### UI Components for User Story 2

- [ ] T051 [P] [US2] Create `ColorPicker.razor` component in `CovenantPromptKey/Components/Shared/ColorPicker.razor`
- [ ] T052 [P] [US2] Create `KeywordForm.razor` component in `CovenantPromptKey/Components/Shared/KeywordForm.razor`
- [ ] T053 [US2] Create `KeywordList.razor` component in `CovenantPromptKey/Components/Shared/KeywordList.razor`
- [ ] T054 [US2] Create `CsvImportExport.razor` component in `CovenantPromptKey/Components/Shared/CsvImportExport.razor`
- [ ] T055 [US2] Create `SettingsPage.razor` page in `CovenantPromptKey/Components/Pages/SettingsPage.razor`

**Checkpoint**: User Story 2 should be fully functional - dictionary management complete

---

## Phase 5: User Story 3 - 詳細互動與定位 (Priority: P2)

**Goal**: 使用者可檢視關鍵字位置分佈，點擊行號跳轉並高亮，直接點擊原文關鍵字切換狀態

**Independent Test**: 使用長文本（>50 行）測試，驗證計數、行號定位、平滑滾動與視覺回饋

### Implementation for User Story 3

- [ ] T056 [US3] Enhance `SourceTextEditor.razor` with line numbers and scroll-to-line in `CovenantPromptKey/Components/Shared/SourceTextEditor.razor`
- [ ] T057 [US3] Enhance `KeywordControlPanel.razor` with expandable position list in `CovenantPromptKey/Components/Shared/KeywordControlPanel.razor`
- [ ] T058 [US3] Add click-to-toggle functionality on highlighted keywords in `SourceTextEditor.razor`
- [ ] T059 [US3] Update JS interop for smooth scrolling and flash highlight in `CovenantPromptKey/wwwroot/js/interop.js`

**Checkpoint**: User Story 3 should be fully functional - detailed interaction complete

---

## Phase 6: User Story 4 - AI 回應還原 (Priority: P2)

**Goal**: 使用者可貼上 AI 回應，系統自動將安全替代字還原為原始機敏詞

**Independent Test**: 準備包含已知 SafeKey 的文本，驗證還原流程正確

### Tests for User Story 4 ⚠️

- [ ] T060 [P] [US4] Unit tests for `KeywordService.RestoreTextAsync` in `CovenantPromptKey.Tests/Services/KeywordServiceTests.cs`

### Implementation for User Story 4

- [ ] T061 [US4] Implement restore logic in `KeywordService.RestoreTextAsync` in `CovenantPromptKey/Services/Implementations/KeywordService.cs`
- [ ] T062 [US4] Add Restore tab to `MaskRestorePage.razor` in `CovenantPromptKey/Components/Pages/MaskRestorePage.razor`

**Checkpoint**: User Story 4 should be fully functional - restore workflow complete

---

## Phase 7: User Story 5 - 上下文語境警示 (Priority: P3)

**Goal**: 系統偵測關鍵字前後緊鄰中文字元時顯示警示圖示

**Independent Test**: 構造 "武科電的規範" 與 "武科電路板" 測試案例，驗證警示標記

### Implementation for User Story 5

- [ ] T063 [US5] Add context warning detection logic in `KeywordService.DetectKeywordsAsync` in `CovenantPromptKey/Services/Implementations/KeywordService.cs`
- [ ] T064 [US5] Update `KeywordControlPanel.razor` to display warning icons in `CovenantPromptKey/Components/Shared/KeywordControlPanel.razor`
- [ ] T065 [US5] Update `SourceTextEditor.razor` to show warning visual style in `CovenantPromptKey/Components/Shared/SourceTextEditor.razor`

**Checkpoint**: User Story 5 should be fully functional - context warnings complete

---

## Phase 8: User Story 6 - Debug Log 追蹤與問題排查 (Priority: P2)

**Goal**: 提供專用 Debug Log 頁面，記錄所有重要程式碼動作，支援一鍵複製

**Independent Test**: 執行各種操作，驗證 Log 正確記錄，測試複製功能

### Implementation for User Story 6

- [ ] T066 [P] [US6] Create `LogViewer.razor` component in `CovenantPromptKey/Components/Shared/LogViewer.razor`
- [ ] T067 [US6] Create `DebugLogPage.razor` page in `CovenantPromptKey/Components/Pages/DebugLogPage.razor`
- [ ] T068 [US6] Add logging calls throughout services (KeywordService, DictionaryService, CsvService)
- [ ] T069 [US6] Add copy-to-clipboard functionality in JS interop for logs in `CovenantPromptKey/wwwroot/js/interop.js`

**Checkpoint**: User Story 6 should be fully functional - debug log complete

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Additional pages, improvements that affect multiple user stories

- [ ] T070 [P] Create `HelpPage.razor` in `CovenantPromptKey/Components/Pages/HelpPage.razor`
- [ ] T071 [P] Create `AboutPage.razor` in `CovenantPromptKey/Components/Pages/AboutPage.razor`
- [ ] T072 [P] Create `ReportIssuePage.razor` in `CovenantPromptKey/Components/Pages/ReportIssuePage.razor`
- [ ] T073 [P] Create `ChangelogPage.razor` in `CovenantPromptKey/Components/Pages/ChangelogPage.razor`
- [ ] T074 Add keyboard shortcuts (Ctrl+V, Ctrl+A, Ctrl+Enter, Ctrl+C, Escape) in `CovenantPromptKey/wwwroot/js/interop.js`
- [ ] T075 Performance optimization: verify 1000-char detection < 200ms, 100K-char < 1000ms
- [ ] T076 Run quickstart.md validation - verify all test scenarios pass

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-8)**: All depend on Foundational phase completion
  - US1 & US2 (P1): Can proceed in parallel after Phase 2
  - US3, US4, US6 (P2): Can proceed after Phase 2, prefer after US1 for context
  - US5 (P3): Can proceed after Phase 2, prefer after US1 for context
- **Polish (Phase 9)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - Core mask workflow
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) - Dictionary management (independent of US1)
- **User Story 3 (P2)**: Depends on US1 `SourceTextEditor` and `KeywordControlPanel` existing
- **User Story 4 (P2)**: Depends on US1 `MaskRestorePage` existing
- **User Story 5 (P3)**: Depends on US1 detection logic existing
- **User Story 6 (P2)**: Mostly independent, integrates with all services

### Within Each User Story

- Tests (TDD) MUST be written and FAIL before implementation
- Models before services
- Services before UI components
- Core components before page components

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Model creation tasks (T005-T012) can run in parallel
- All Interface creation tasks (T013-T020) can run in parallel
- All test tasks within a story marked [P] can run in parallel
- US1 and US2 can be worked on in parallel after Phase 2

---

## Parallel Example: Phase 2 Foundational

```powershell
# Launch all models in parallel:
# T005: KeywordMapping.cs
# T006: DetectedKeyword.cs
# T007: WorkSession.cs
# T008: LogEntry.cs
# T009: MaskResult.cs
# T010: CsvImportResult.cs
# T011: ValidationResult.cs
# T012: MarkdownStructure.cs

# Then launch all interfaces in parallel:
# T013-T020: All interface files
```

---

## Parallel Example: User Story 1

```powershell
# Launch all tests for User Story 1 together:
# T027: KeywordValidationServiceTests.cs
# T028: MarkdownServiceTests.cs
# T029-T030: KeywordServiceTests.cs

# Launch shared UI components in parallel:
# T036: ThreeColumnLayout.razor
# T038: ToastNotification.razor
# T039: ConfirmationDialog.razor
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (Core Mask Workflow)
4. Complete Phase 4: User Story 2 (Dictionary Management)
5. **STOP and VALIDATE**: Test US1 + US2 independently
6. Deploy/demo if ready - basic mask/restore with dictionary management

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → MVP Release (mask workflow)
3. Add User Story 2 → Test independently → Full CRUD dictionary
4. Add User Story 4 → Test independently → Restore workflow complete
5. Add User Story 3 → Test independently → Enhanced UX
6. Add User Story 6 → Test independently → Debug capabilities
7. Add User Story 5 → Test independently → Context warnings
8. Polish phase → Production ready

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Core Mask)
   - Developer B: User Story 2 (Dictionary)
3. After US1 basics complete:
   - Developer A: User Story 3 (Detailed Interaction)
   - Developer B: User Story 4 (Restore)
4. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- TDD required: verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
