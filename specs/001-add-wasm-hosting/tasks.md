---

description: "Task list for 001-add-wasm-hosting implementation (Option A: incremental slices)"
---

# Tasks: 001-add-wasm-hosting（Dual-hosting + Browser-hosted + Security Baseline）

**Input**: 設計文件位於 `specs/001-add-wasm-hosting/`（plan.md, spec.md, research.md, contracts/, quickstart.md）  
**Goal**: 以 minimal-change 方式保留 Server host，新增 browser-hosted（Blazor WebAssembly）版本；同時達成 offline-first + GitHub Pages/Azure SWA 部署 + CSP baseline + strict parity。  
**Approach**: Option A（incremental slices）：避免「整包搬移/重組資料夾」，改採逐 workflow/逐頁 thin-slice 遷移，降低 brownfield 風險並符合 constitution。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 可平行處理（different files / no dependency）
- **[Story]**: 僅用於 user story tasks（`[US1]`, `[US2]`, `[US3]`）
- 每一條 task **必須**包含明確 file path（或將新增的檔案路徑）。
- **Constitution note**: 任何 `.csproj` / `.slnx` / DI / CI 改動皆需 explicit approval gate（拆細，不做 blanket approval）。

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 先把 approvals、驗收規格、測試資料集與驗收腳本框架準備好；不動 production code。

- [ ] T001 Create approval log in specs/001-add-wasm-hosting/approvals.md
- [ ] T002 [P] Create parity suite schema + testcases doc in specs/001-add-wasm-hosting/parity/testcases.md (include: initial state, steps, expected output, expected storage delta, allowed deltas)
- [ ] T003 [P] Create malicious input corpus doc in specs/001-add-wasm-hosting/security/malicious-input-cases.md (map each case to a UI surface)
- [ ] T004 [P] Create publish helper script skeleton in specs/001-add-wasm-hosting/scripts/publish-browser-hosted.ps1
- [ ] T005 [P] Create static artifact scan script skeleton in specs/001-add-wasm-hosting/scripts/scan-static-artifacts.ps1

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 建立 WASM host skeleton + 少量 shared libraries（只做必要骨架，不做大規模搬移）。

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### Approval Gates (Granular)

- [ ] T006 Record decision options and approvals in specs/001-add-wasm-hosting/approvals.md (add a table with: change-type, scope, approved?, rationale, rollback)
- [ ] T007 Approval Gate: add new projects + modify CovenantPromptKey.slnx (Domain/UI/Browser/WASM) → record in specs/001-add-wasm-hosting/approvals.md
- [ ] T008 Approval Gate: allow DI changes in CovenantPromptKey/Program.cs and new DI in CovenantPromptKeyWebAssembly/Program.cs → record in specs/001-add-wasm-hosting/approvals.md
- [ ] T009 Approval Gate: allow CI/deploy workflow changes under .github/workflows/ → record in specs/001-add-wasm-hosting/approvals.md

### Minimal Project Skeletons (No folder reorganising)

- [ ] T010 Create WASM host project at CovenantPromptKeyWebAssembly/CovenantPromptKeyWebAssembly.csproj
- [ ] T011 Add WASM host to solution in CovenantPromptKey.slnx
- [ ] T012 Create WASM host bootstrap files: CovenantPromptKeyWebAssembly/Program.cs, CovenantPromptKeyWebAssembly/App.razor, CovenantPromptKeyWebAssembly/Routes.razor, CovenantPromptKeyWebAssembly/wwwroot/index.html

### Shared libraries (Skeleton first; move only when needed per slice)

- [ ] T013 Create Domain library project at CovenantPromptKey.Domain/CovenantPromptKey.Domain.csproj (empty skeleton)
- [ ] T014 Create Browser boundary library project at CovenantPromptKey.Browser/CovenantPromptKey.Browser.csproj (empty skeleton)
- [ ] T015 Create shared UI RCL project at CovenantPromptKey.UI/CovenantPromptKey.UI.csproj (empty skeleton; do NOT move existing Components/ yet)
- [ ] T016 Add Domain/Browser/UI projects to solution in CovenantPromptKey.slnx

### Shared DI hooks (Skeleton)

- [ ] T017 Create DI extension placeholders: CovenantPromptKey.Domain/DependencyInjection.cs and CovenantPromptKey.Browser/DependencyInjection.cs
- [ ] T018 Wire Server host DI hooks (minimal) in CovenantPromptKey/Program.cs (call extension methods; no behavioural change)
- [ ] T019 Wire WASM host DI hooks (minimal) in CovenantPromptKeyWebAssembly/Program.cs (add services; add HttpClient base address)

**Checkpoint**: Foundation ready - user story implementation can now begin (in incremental slices).

---

## Phase 3: User Story 1 - 以免費 Static Hosting 使用並可離線工作 (Priority: P1) 🎯 MVP

**Goal**: 交付可完全在 browser 執行的 app（WASM），在 successful first-load 後可 offline 啟動並完成核心 workflows；同時維持 strict parity（以 parity suite 定義的可觀測行為一致）。  
**Strategy**: 先讓 WASM host 可跑、可 publish；再用 slices 逐步把「可共享且可測」的邏輯移入 Domain/Browser；UI 共用採 “page-by-page extraction” 而不是整包搬移。

**Independent Test**: 只啟動 WASM（不跑 Server），完成 first-load online → offline reopen → 完成 specs/001-add-wasm-hosting/parity/testcases.md 定義的 3 個核心操作。

### US1 — Slice 0: WASM smoke test + publishable shell

- [ ] T020 [US1] Implement minimal navigable pages in CovenantPromptKeyWebAssembly/Components/Pages/ (e.g., Home + Diagnostics) to validate routing works
- [ ] T021 [US1] Validate `dotnet publish` produces static output for WASM host and record commands in specs/001-add-wasm-hosting/quickstart.md

### US1 — Slice 1: Storage boundary (shared) + “interactive ready” gating

- [ ] T022 [US1] Move storage interfaces to Browser library (targeted only): move CovenantPromptKey/Services/Interfaces/ILocalStorageService.cs to CovenantPromptKey.Browser/Interfaces/ILocalStorageService.cs
- [ ] T023 [US1] Move storage interfaces to Browser library (targeted only): move CovenantPromptKey/Services/Interfaces/ISessionStorageService.cs to CovenantPromptKey.Browser/Interfaces/ISessionStorageService.cs
- [ ] T024 [US1] Move storage implementations to Browser library (targeted only): move CovenantPromptKey/Services/Implementations/LocalStorageService.cs to CovenantPromptKey.Browser/Implementations/LocalStorageService.cs
- [ ] T025 [US1] Move storage implementations to Browser library (targeted only): move CovenantPromptKey/Services/Implementations/SessionStorageService.cs to CovenantPromptKey.Browser/Implementations/SessionStorageService.cs
- [ ] T026 [US1] Implement cross-host “interactive ready” gating inside CovenantPromptKey.Browser/Implementations/*StorageService.cs (avoid Server prerender JS calls)
- [ ] T027 [US1] Update DI registrations to use Browser library in CovenantPromptKey/Program.cs and CovenantPromptKeyWebAssembly/Program.cs

### US1 — Slice 2: Shared assets (minimal) without mass-moving UI folders

- [ ] T028 [P] [US1] Create shared interop module in RCL at CovenantPromptKey.UI/wwwroot/js/interop.js (initially only storage helpers used by Browser lib)
- [ ] T029 [P] [US1] Create shared CSS baseline in RCL at CovenantPromptKey.UI/wwwroot/css/app.css (minimal; no redesign)
- [ ] T030 [US1] Reference RCL assets from Server host in CovenantPromptKey/wwwroot/app.css and CovenantPromptKey/Components/_Imports.razor (via _content path)
- [ ] T031 [US1] Reference RCL assets from WASM host in CovenantPromptKeyWebAssembly/wwwroot/index.html (via _content path)

### US1 — Slice 3: Domain extraction (keyword/dictionary) — move only required files

- [ ] T032 [US1] Create “migration inventory” for Slice 3 in specs/001-add-wasm-hosting/migrations/slice-3-domain-inventory.md (list types/files required; keep scope tight)
- [ ] T033 [US1] Move matcher algorithm into Domain (targeted): move CovenantPromptKey/Services/Implementations/AhoCorasickMatcher.cs to CovenantPromptKey.Domain/Algorithms/AhoCorasickMatcher.cs
- [ ] T034 [US1] Move dictionary services into Domain (targeted): move CovenantPromptKey/Services/Interfaces/IDictionaryService.cs to CovenantPromptKey.Domain/Services/Interfaces/IDictionaryService.cs
- [ ] T035 [US1] Move dictionary services into Domain (targeted): move CovenantPromptKey/Services/Implementations/DictionaryService.cs to CovenantPromptKey.Domain/Services/Implementations/DictionaryService.cs
- [ ] T036 [US1] Move keyword services into Domain (targeted): move CovenantPromptKey/Services/Interfaces/IKeywordService.cs to CovenantPromptKey.Domain/Services/Interfaces/IKeywordService.cs
- [ ] T037 [US1] Move keyword services into Domain (targeted): move CovenantPromptKey/Services/Implementations/KeywordService.cs to CovenantPromptKey.Domain/Services/Implementations/KeywordService.cs
- [ ] T038 [US1] Update DI for moved Domain services in CovenantPromptKey/Program.cs and CovenantPromptKeyWebAssembly/Program.cs

### US1 — Slice 4: Markdown + CSV + Work session (targeted, one-by-one)

- [ ] T039 [US1] Move markdown service into Domain (targeted): move CovenantPromptKey/Services/Interfaces/IMarkdownService.cs and CovenantPromptKey/Services/Implementations/MarkdownService.cs to CovenantPromptKey.Domain/Services/
- [ ] T040 [US1] Move CSV service into Domain (targeted): move CovenantPromptKey/Services/Interfaces/ICsvService.cs and CovenantPromptKey/Services/Implementations/CsvService.cs to CovenantPromptKey.Domain/Services/
- [ ] T041 [US1] Move work session service into Domain (targeted): move CovenantPromptKey/Services/Interfaces/IWorkSessionService.cs and CovenantPromptKey/Services/Implementations/WorkSessionService.cs to CovenantPromptKey.Domain/Services/
- [ ] T042 [US1] Update DI for Slice 4 in CovenantPromptKey/Program.cs and CovenantPromptKeyWebAssembly/Program.cs

### US1 — Offline-first (PWA) enablement

- [ ] T043 [US1] Implement PWA wiring in CovenantPromptKeyWebAssembly/wwwroot/index.html (register service worker with updateViaCache: 'none')
- [ ] T044 [US1] Add PWA assets using existing icon source: CovenantPromptKeyWebAssembly/wwwroot/manifest.webmanifest and CovenantPromptKeyWebAssembly/wwwroot/icon-192.png and CovenantPromptKeyWebAssembly/wwwroot/icon-512.png (derived from CovenantPromptKey/wwwroot/favicon.png)
- [ ] T045 [US1] Add service worker scripts: CovenantPromptKeyWebAssembly/wwwroot/service-worker.js and CovenantPromptKeyWebAssembly/wwwroot/service-worker.published.js
- [ ] T046 [US1] Ensure publish generates service-worker-assets.js (configure in CovenantPromptKeyWebAssembly/CovenantPromptKeyWebAssembly.csproj)
- [ ] T047 [US1] Add offline-first UX guidance (first-load required) in CovenantPromptKeyWebAssembly/Components/Pages/Diagnostics.razor (minimal copy; no extra UX features)

**Checkpoint**: US1 browser-hosted MVP is runnable, publishable, offline-capable after first load, and passes the 3 core operations defined in specs/001-add-wasm-hosting/parity/testcases.md.

---

## Phase 4: User Story 2 - 維運者可用免費平台順利部署 (Priority: P2)

**Goal**: 同一套 source + automation 可靠部署到 GitHub Pages 與 Azure Static Web Apps；支援 subpath + deep link refresh。  
**Independent Test**: 以 publish script 產出兩種 deployment-ready outputs，並在兩平台完成 base-path + deep-link refresh 驗收。

- [ ] T048 [US2] Implement publish script (platform parameter) in specs/001-add-wasm-hosting/scripts/publish-browser-hosted.ps1 (must emit deterministic validation output)
- [ ] T049 [US2] Add GitHub Pages artefacts to WASM project: CovenantPromptKeyWebAssembly/wwwroot/.nojekyll and CovenantPromptKeyWebAssembly/wwwroot/404.html
- [ ] T050 [US2] Ensure base href is rewritten for GitHub Pages subpath in specs/001-add-wasm-hosting/scripts/publish-browser-hosted.ps1 (no manual post-edit of publish output)

- [ ] T051 [US2] Add Azure SWA config file at CovenantPromptKeyWebAssembly/staticwebapp.config.json (copied to publish root)
- [ ] T052 [US2] Configure SWA navigationFallback + exclude rules in CovenantPromptKeyWebAssembly/staticwebapp.config.json

- [ ] T053 [P] [US2] Add GitHub Actions workflow for GitHub Pages deploy in .github/workflows/deploy-gh-pages.yml
- [ ] T054 [P] [US2] Add GitHub Actions workflow for Azure SWA deploy in .github/workflows/deploy-azure-swa.yml
- [ ] T055 [US2] Document deployment steps + validation commands in specs/001-add-wasm-hosting/quickstart.md

**Checkpoint**: Both platforms deploy successfully; deep link refresh works; no broken static asset loads.

---

## Phase 5: User Story 3 - 安全輸出與資料保護（降低 XSS 影響面） (Priority: P3)

**Goal**: 落實 safe rendering（no execution）與 CSP baseline（no inline script），並提供可重複的惡意輸入驗證與 secrets scan。  
**Independent Test**: 以 malicious input corpus 測試所有相關 UI，確認無腳本執行；並以 scan script 檢查 publish output 無 secrets patterns。

- [ ] T056 [US3] Inventory all user-provided render entrypoints and document in specs/001-add-wasm-hosting/security/markupstring-audit.md (include: file path, surface, input source, current render method, mitigation)
- [ ] T057 [US3] Fix Bible highlight rendering without raw HTML injection in CovenantPromptKey/Components/Shared/Bible/BibleVerseDisplay.razor (or the extracted RCL equivalent when it exists)
- [ ] T058 [US3] Fix Bible highlight rendering without raw HTML injection in CovenantPromptKey/Components/Shared/Bible/BibleSearchResults.razor (or the extracted RCL equivalent when it exists)
- [ ] T059 [US3] Audit and mitigate MarkupString usage in CovenantPromptKey/Components/Shared/LogViewer.razor (document why safe; ensure no user-controlled HTML is injected)

- [ ] T060 [US3] Add CSP meta tag baseline (no inline scripts) to CovenantPromptKeyWebAssembly/wwwroot/index.html
- [ ] T061 [US3] Add CSP headers baseline to Azure SWA config in CovenantPromptKeyWebAssembly/staticwebapp.config.json

- [ ] T062 [US3] Implement static artifact scan (no secrets in client) in specs/001-add-wasm-hosting/scripts/scan-static-artifacts.ps1 (token patterns must be defined in the script)
- [ ] T063 [US3] Add security validation instructions in specs/001-add-wasm-hosting/quickstart.md (how to run malicious corpus and scan script)

**Checkpoint**: CSP is applied (platform-appropriate), malicious inputs render as text only, and publish output passes secrets scan.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: 收斂文件、驗收、與 cross-cutting hardening（不新增未在 spec 要求的 UX/features）。

- [ ] T064 [P] Update docs cross-links (plan/spec/contracts) in specs/001-add-wasm-hosting/plan.md and specs/001-add-wasm-hosting/spec.md
- [ ] T065 Run end-to-end quickstart validation and capture results in specs/001-add-wasm-hosting/validation-notes.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies
- **Foundational (Phase 2)**: Depends on Setup; BLOCKS all user stories
- **User Stories (Phase 3–5)**: All depend on Foundational completion
- **Polish (Phase 6)**: Depends on desired user stories complete

### User Story Dependencies (expected)

- **US1 (P1)**: Can start after Foundational; delivers MVP value
- **US2 (P2)**: Requires US1 publish output; focuses on platform deployment
- **US3 (P3)**: Best verified once US1 UI is running in WASM; inventory can start earlier

---

## Parallel Execution Examples

- [P] Documentation/testcase work (T002–T005)
- [P] CI workflows for GH Pages and SWA (T053–T054) once approved
