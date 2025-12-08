# Tasks: 聖經查詢系統 (Bible Query System)

**Input**: Design documents from `/specs/2-bible-query-system/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: 依據 plan.md 規範，新功能將包含單元測試（NUnit 4.x + NSubstitute + FluentAssertions，服務測試覆蓋率 ≥ 50%）

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

---

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Based on plan.md project structure:
- **Models**: `CovenantPromptKey/Models/Bible/`
- **Services**: `CovenantPromptKey/Services/Interfaces/` and `CovenantPromptKey/Services/Implementations/`
- **Pages**: `CovenantPromptKey/Components/Pages/Bible/`
- **Shared Components**: `CovenantPromptKey/Components/Shared/Bible/`
- **Tests**: `CovenantPromptKey.Tests/Services/`
- **Styles**: `CovenantPromptKey/wwwroot/css/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization, DLL integration, and shared model creation

- [X] T001 Create `libs/` directory and add BibleData.dll reference
- [X] T002 Update `CovenantPromptKey/CovenantPromptKey.csproj` to include BibleData DLL reference
- [X] T003 [P] Create directory structure: `Models/Bible/`, `Components/Pages/Bible/`, `Components/Shared/Bible/`
- [X] T004 [P] Create enum types in `CovenantPromptKey/Models/Bible/BibleEnums.cs` (FontFamily, TextColor, BackgroundColor, ExportStyle)
- [X] T005 [P] Create `CovenantPromptKey/Models/Bible/BibleSettings.cs` model
- [X] T006 [P] Create `CovenantPromptKey/Models/Bible/BibleBookmark.cs` model
- [X] T007 [P] Create `CovenantPromptKey/Models/Bible/BibleGameRecord.cs` model (BibleGameRecordCollection, BibleGameSession, BibleGameAnswer)
- [X] T008 [P] Create `CovenantPromptKey/Models/Bible/BibleWrongAnswer.cs` model (BibleWrongAnswerCollection, WrongAnswerSession, WrongAnswerRecord)
- [X] T009 [P] Create `CovenantPromptKey/Models/Bible/BiblePageState.cs` model (BibleSearchPageState, BibleReadPageState, BibleGamePageState)
- [X] T010 [P] Create `CovenantPromptKey/Models/Bible/ExportModels.cs` (ExportOptions, ExportRange)
- [X] T011 [P] Create `CovenantPromptKey/Models/Bible/SearchResultItem.cs` model
- [X] T012 [P] Create `CovenantPromptKey/Models/Bible/BibleGameQuestion.cs` model
- [X] T013 [P] Create `CovenantPromptKey/Models/Bible/BibleStyleHelper.cs` static helper class

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core services that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T014 Register `BibleIndex` as Singleton in `CovenantPromptKey/Program.cs`
- [X] T015 [P] Create `CovenantPromptKey/Services/Interfaces/IBibleSettingsService.cs` interface
- [X] T016 [P] Create `CovenantPromptKey/Services/Interfaces/IBiblePageStateService.cs` interface
- [X] T017 Create `CovenantPromptKey/Services/Implementations/BibleSettingsService.cs` implementation (depends on ISessionStorageService)
- [X] T018 Create `CovenantPromptKey/Services/Implementations/BiblePageStateService.cs` implementation (depends on ISessionStorageService)
- [X] T019 Register Bible settings and page state services in `CovenantPromptKey/Program.cs`
- [X] T020 [P] Create `CovenantPromptKey/wwwroot/css/bible.css` base stylesheet
- [X] T021 [P] Create `CovenantPromptKey/Components/Shared/Bible/BibleSubNavigation.razor` component
- [X] T022 [P] Create `CovenantPromptKey/Components/Shared/Bible/BibleSettingsPanel.razor` component
- [X] T023 [P] Create `CovenantPromptKey/Components/Shared/Bible/BibleVerseDisplay.razor` component

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 7 - 側邊選單導航 (Priority: P1) 🎯 MVP Entry Point

**Goal**: Enable users to access Bible features from the side navigation menu

**Independent Test**: Users can see "聖經" option in side menu, expand to see sub-options, and navigate to corresponding pages

### Tests for User Story 7

- [X] T024 [P] [US7] Create `CovenantPromptKey.Tests/Services/BibleSettingsServiceTests.cs` unit tests
- [X] T025 [P] [US7] Create `CovenantPromptKey.Tests/Services/BiblePageStateServiceTests.cs` unit tests

### Implementation for User Story 7

- [X] T026 [US7] Update `CovenantPromptKey/Components/Layout/NavMenu.razor` to add "聖經" expandable menu with sub-items
- [X] T027 [US7] Create `CovenantPromptKey/Components/Pages/Bible/BibleHomePage.razor` (route: /bible)
- [X] T028 [US7] Create `CovenantPromptKey/Components/Pages/Bible/BibleHomePage.razor.css` styles

**Checkpoint**: At this point, User Story 7 should be fully functional - users can navigate to Bible section

---

## Phase 4: User Story 1 - 聖經關鍵字搜尋 (Priority: P1) 🎯 MVP Core Feature

**Goal**: Enable users to search Bible verses by keywords with real-time suggestions and ranked results

**Independent Test**: Users can enter keywords (e.g., "愛"), see real-time suggestions, and get ranked search results with pagination

### Tests for User Story 1

- [X] T029 [P] [US1] Create `CovenantPromptKey.Tests/Services/BibleSearchServiceTests.cs` unit tests

### Implementation for User Story 1

- [X] T030 [P] [US1] Create `CovenantPromptKey/Services/Interfaces/IBibleSearchService.cs` interface
- [X] T031 [US1] Create `CovenantPromptKey/Services/Implementations/BibleSearchService.cs` implementation (depends on BibleIndex)
- [X] T032 [US1] Register IBibleSearchService in `CovenantPromptKey/Program.cs`
- [X] T033 [P] [US1] Create `CovenantPromptKey/Components/Shared/Bible/BibleSearchResults.razor` component
- [X] T034 [US1] Create `CovenantPromptKey/Components/Pages/Bible/BibleSearchPage.razor` (route: /bible/search)
- [X] T035 [US1] Create `CovenantPromptKey/Components/Pages/Bible/BibleSearchPage.razor.css` styles
- [X] T036 [US1] Implement debounce search with CancellationToken in BibleSearchPage.razor (250ms delay)
- [X] T037 [US1] Implement keyword highlighting in search results

**Checkpoint**: At this point, User Story 1 should be fully functional - users can search and find verses

---

## Phase 5: User Story 2 - 聖經章節閱讀 (Priority: P1) 🎯 MVP Core Feature

**Goal**: Enable users to read Bible chapters with book/chapter navigation

**Independent Test**: Users can select any book and chapter, read verses line by line, and navigate between chapters

### Tests for User Story 2

- [X] T038 [P] [US2] Create `CovenantPromptKey.Tests/Services/BibleReadingServiceTests.cs` unit tests

### Implementation for User Story 2

- [X] T039 [P] [US2] Create `CovenantPromptKey/Services/Interfaces/IBibleReadingService.cs` interface
- [X] T040 [US2] Create `CovenantPromptKey/Services/Implementations/BibleReadingService.cs` implementation (depends on BibleIndex)
- [X] T041 [US2] Register IBibleReadingService in `CovenantPromptKey/Program.cs`
- [X] T042 [P] [US2] Create `CovenantPromptKey/Components/Shared/Bible/BibleBookSelector.razor` component
- [X] T043 [P] [US2] Create `CovenantPromptKey/Components/Shared/Bible/BibleChapterNavigator.razor` component
- [X] T044 [US2] Create `CovenantPromptKey/Components/Pages/Bible/BibleReadPage.razor` (routes: /bible/read, /bible/read/{BookNumber:int}/{ChapterNumber:int})
- [X] T045 [US2] Create `CovenantPromptKey/Components/Pages/Bible/BibleReadPage.razor.css` styles
- [X] T046 [US2] Implement chapter navigation (prev/next) and direct jump in BibleReadPage.razor
- [X] T047 [US2] Implement page state persistence (restore last read position) in BibleReadPage.razor

**Checkpoint**: At this point, User Stories 1, 2, and 7 should all work independently - MVP complete

---

## Phase 6: User Story 3 - 閱讀設定自訂 (Priority: P2)

**Goal**: Enable users to customise font family, size, text colour, and background colour

**Independent Test**: Users can change settings and see immediate effect on verse display; settings persist across sessions

### Implementation for User Story 3

- [X] T048 [US3] Enhance `CovenantPromptKey/Components/Shared/Bible/BibleSettingsPanel.razor` with full font/colour options
- [X] T049 [US3] Integrate settings with BibleVerseDisplay.razor for real-time style updates
- [X] T050 [US3] Implement settings persistence via BibleSettingsService in search and read pages
- [X] T051 [US3] Add night mode special handling (auto white text on black background)

**Checkpoint**: At this point, User Story 3 should be fully functional - reading experience is customisable

---

## Phase 7: User Story 4 - 書籤管理 (Priority: P2)

**Goal**: Enable automatic bookmark recording of recently read chapters (max 10)

**Independent Test**: Users can read chapters, see them auto-added to bookmark list, and click to quick-jump

### Tests for User Story 4

- [X] T052 [P] [US4] Create `CovenantPromptKey.NUnitTests/Services/BibleBookmarkServiceTests.cs` unit tests

### Implementation for User Story 4

- [X] T053 [P] [US4] Create `CovenantPromptKey/Services/Interfaces/IBibleBookmarkService.cs` interface
- [X] T054 [US4] Create `CovenantPromptKey/Services/Implementations/BibleBookmarkService.cs` implementation
- [X] T055 [US4] Register IBibleBookmarkService in `CovenantPromptKey/Program.cs`
- [X] T056 [P] [US4] Create `CovenantPromptKey/Components/Shared/Bible/BibleBookmarkList.razor` component
- [X] T057 [US4] Integrate bookmark auto-add in BibleReadPage.razor (on chapter change)
- [X] T058 [US4] Add bookmark quick-jump in BibleReadPage.razor and BibleHomePage.razor

**Checkpoint**: At this point, User Story 4 should be fully functional - reading history is tracked

---

## Phase 8: User Story 5 - 經文導出 (Priority: P2)

**Goal**: Enable users to export selected verses to Markdown format with three style options

**Independent Test**: Users can select verses, choose export style, and get correctly formatted Markdown text

### Tests for User Story 5

- [X] T059 [P] [US5] Create `CovenantPromptKey.NUnitTests/Services/BibleExportServiceTests.cs` unit tests

### Implementation for User Story 5

- [X] T060 [P] [US5] Create `CovenantPromptKey/Services/Interfaces/IBibleExportService.cs` interface
- [X] T061 [US5] Create `CovenantPromptKey/Services/Implementations/BibleExportService.cs` implementation
- [X] T062 [US5] Register IBibleExportService in `CovenantPromptKey/Program.cs`
- [X] T063 [P] [US5] Create `CovenantPromptKey/Components/Shared/Bible/BibleExportDialog.razor` component
- [X] T064 [US5] Integrate export dialog in BibleReadPage.razor
- [X] T065 [US5] Implement "one file per book" export option (multiple .md files)
- [X] T066 [US5] Implement copy-to-clipboard and download functionality

**Checkpoint**: At this point, User Story 5 should be fully functional - verses can be exported

---

## Phase 9: User Story 6 - 經文猜猜遊戲 (Priority: P3)

**Goal**: Enable verse guessing game with score history and wrong answer review

**Independent Test**: Users can play 10-question game, see scores, review wrong answers

### Tests for User Story 6

- [X] T067 [P] [US6] Create `CovenantPromptKey.NUnitTests/Services/BibleGameServiceTests.cs` unit tests

### Implementation for User Story 6

- [X] T068 [P] [US6] Create `CovenantPromptKey/Services/Interfaces/IBibleGameService.cs` interface
- [X] T069 [US6] Create `CovenantPromptKey/Services/Implementations/BibleGameService.cs` implementation
- [X] T070 [US6] Register IBibleGameService in `CovenantPromptKey/Program.cs`
- [X] T071 [US6] Create `CovenantPromptKey/Components/Pages/Bible/BibleGamePage.razor` (route: /bible/game)
- [X] T072 [US6] Create `CovenantPromptKey/Components/Pages/Bible/BibleGamePage.razor.css` styles
- [X] T073 [US6] Implement game flow: start → questions → answer feedback → results
- [X] T074 [US6] Implement game record storage (high score + recent 5 games)
- [X] T075 [US6] Implement wrong answer recording and review panel
- [X] T076 [US6] Implement separate clear functions for game records and wrong answers
- [X] T077 [US6] Add "Game 2" placeholder card with "敬請期待" message

**Checkpoint**: All user stories should now be independently functional

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T078 [P] Update BibleHomePage.razor with feature cards and recent bookmarks display
- [X] T079 [P] Add responsive styles for mobile/tablet in `bible.css`
- [X] T080 Code cleanup and ensure consistent error handling across all services
- [X] T081 [P] Add loading states and empty states for all components
- [X] T082 Performance validation: verify search < 2s, suggestions < 300ms
- [X] T083 Run quickstart.md validation - verify all steps work correctly
- [X] T084 Final integration test - navigate through all features end-to-end

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-9)**: All depend on Foundational phase completion
  - User stories can proceed in priority order (P1 → P2 → P3)
  - Multiple P1 stories can be parallelised if staffed
- **Polish (Phase 10)**: Depends on all desired user stories being complete

### User Story Dependencies

| Story | Priority | Can Start After | Dependencies on Other Stories |
|-------|----------|-----------------|-------------------------------|
| US7 - Navigation | P1 | Phase 2 | None (entry point) |
| US1 - Search | P1 | Phase 2 | None |
| US2 - Reading | P1 | Phase 2 | None |
| US3 - Settings | P2 | Phase 2 | Enhances US1/US2 but independent |
| US4 - Bookmarks | P2 | Phase 2 | Works with US2 but independent |
| US5 - Export | P2 | Phase 2 | Works with US2 but independent |
| US6 - Game | P3 | Phase 2 | None |

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Interfaces before implementations
- Implementations before components
- Components before page integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel (T003-T013)
- All Foundational tasks marked [P] can run in parallel (T015-T016, T020-T023)
- Once Foundational phase completes, US7/US1/US2 can start in parallel
- All tests for a user story marked [P] can run in parallel
- Interfaces marked [P] can run in parallel within a story

---

## Parallel Example: Phase 1 Setup

```bash
# All these can run in parallel:
T003 - Create directory structure
T004 - Create enum types
T005 - Create BibleSettings model
T006 - Create BibleBookmark model
T007 - Create BibleGameRecord model
T008 - Create BibleWrongAnswer model
T009 - Create BiblePageState model
T010 - Create ExportModels
T011 - Create SearchResultItem model
T012 - Create BibleGameQuestion model
T013 - Create BibleStyleHelper
```

## Parallel Example: P1 User Stories (After Phase 2)

```bash
# These user stories can proceed in parallel if staffed:
[Team A] US7 - Navigation (T026-T028)
[Team B] US1 - Search (T030-T037)
[Team C] US2 - Reading (T039-T047)
```

---

## Implementation Strategy

### MVP Scope (Minimum Viable Product)

Complete these phases for initial release:
1. **Phase 1**: Setup
2. **Phase 2**: Foundational
3. **Phase 3**: US7 - Navigation (entry point)
4. **Phase 4**: US1 - Search (core feature)
5. **Phase 5**: US2 - Reading (core feature)

**MVP Deliverables**: Users can navigate to Bible section, search verses by keyword, and read chapters.

### Incremental Delivery

After MVP, deliver in priority order:
- **Increment 1**: US3 (Settings) + US4 (Bookmarks) - enhances reading experience
- **Increment 2**: US5 (Export) - extends functionality
- **Increment 3**: US6 (Game) - entertainment feature
- **Final**: Phase 10 Polish

---

## Task Summary

| Phase | Description | Task Range | Task Count |
|-------|-------------|------------|------------|
| 1 | Setup | T001-T013 | 13 |
| 2 | Foundational | T014-T023 | 10 |
| 3 | US7 Navigation | T024-T028 | 5 |
| 4 | US1 Search | T029-T037 | 9 |
| 5 | US2 Reading | T038-T047 | 10 |
| 6 | US3 Settings | T048-T051 | 4 |
| 7 | US4 Bookmarks | T052-T058 | 7 |
| 8 | US5 Export | T059-T066 | 8 |
| 9 | US6 Game | T067-T077 | 11 |
| 10 | Polish | T078-T084 | 7 |
| **Total** | | | **84** |

### Tasks Per User Story

| User Story | Description | Task Count | Has Tests |
|------------|-------------|------------|-----------|
| US7 | 側邊選單導航 | 5 | ✅ |
| US1 | 聖經關鍵字搜尋 | 9 | ✅ |
| US2 | 聖經章節閱讀 | 10 | ✅ |
| US3 | 閱讀設定自訂 | 4 | ❌ (UI-focused) |
| US4 | 書籤管理 | 7 | ✅ |
| US5 | 經文導出 | 8 | ✅ |
| US6 | 經文猜猜遊戲 | 11 | ✅ |

### Parallel Opportunities Identified

- **Phase 1**: 11 tasks can run in parallel (T003-T013)
- **Phase 2**: 6 tasks can run in parallel (T015-T016, T020-T023)
- **P1 Stories**: US7, US1, US2 can be parallelised after Phase 2
- **P2 Stories**: US3, US4, US5 can be parallelised after P1 completion

### Format Validation ✅

All tasks follow the required checklist format:
- ✅ Checkbox (`- [ ]`)
- ✅ Task ID (T001, T002, etc.)
- ✅ [P] marker for parallelisable tasks
- ✅ [Story] label for user story phase tasks
- ✅ File paths in descriptions
