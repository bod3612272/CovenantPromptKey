# Research Notes: Browser-hosted 版本（可免費 Static Hosting）與資安基線

**Date**: 2025-12-26  
**Branch**: `001-add-wasm-hosting`  
**Spec**: [spec.md](spec.md)

本文件用 evidence-based 口徑整理 Phase 0 research 結果，將技術風險轉成明確 decision，並列出替代方案（alternatives）與取捨（trade-offs）。

## Decision 1: Dual-hosting 架構（Server + browser-hosted）

**Decision**: 採 dual-hosting：保留現有 `CovenantPromptKey`（Server host），新增 `CovenantPromptKeyWebAssembly`（browser-hosted client-side host）。

**Rationale**: 
- 直接滿足 FR-001/FR-009（保留 fallback 路徑 + 另提供純靜態輸出）。
- 對 brownfield 風險最低：不需要 big-bang 替換既有 host。

**Alternatives considered**:
- 單一 host 同時支援多 render modes（同專案內切換）：可行但容易引入 hosting-specific 條件式與發佈複雜度；且「免費 static hosting」的發佈路線不如獨立 client-side host straightforward。

## Decision 2: Shared UI 與 shared logic 的抽離方式

**Decision**: 建議（需你明確批准後才實作）新增：
- `CovenantPromptKey.UI`：Razor Class Library (RCL)，承載共享 Razor components + 靜態資產。
- `CovenantPromptKey.Domain`（或 `Core`）：純 .NET class library，承載 models/constants/pure logic services。
- `CovenantPromptKey.Browser`（可選）：集中 JS interop + browser storage boundary，讓兩個 host 共用同一份實作，降低行為分歧。

**Rationale**:
- strict parity（SC-005）要落地，核心行為需共用，而不是複製兩份。
- 把 browser-boundary（IJSRuntime、storage）集中，能讓 unit tests 在不依賴 JS 的情況下驗證規則與轉換。

**Alternatives considered**:
- 不抽 RCL：兩個 host 各自維護 UI（高 duplicated code 風險，且 parity drift 機率高）。

## Decision 3: GitHub Pages 子路徑（/<repo>/）與 deep link

**Decision**: 以「base href 改寫 + `.nojekyll` + `404.html` SPA fallback」作為 GitHub Pages 的最小可靠策略。

**Rationale**:
- GitHub Pages 缺乏伺服端 rewrite 規則，因此 deep link refresh 需要 `404.html` workaround。
- `.nojekyll` 是為了保留 `/_framework` 路徑資產。

**Alternatives considered**:
- Hash routing：可避開 404，但 URL 形態改變，且不符合一般使用者對 deep link 的期待。

## Decision 4: Azure Static Web Apps (SWA) routing

**Decision**: 使用 `staticwebapp.config.json` 的 `navigationFallback` rewrite `/index.html`，並用 `exclude` 避免靜態資源誤導。

**Rationale**:
- SWA 原生支援 SPA fallback，能比 GitHub Pages 更乾淨地支援 deep link。

**Alternatives considered**:
- `routes.json`（deprecated）：不採用。

## Decision 5: CSP baseline（禁止 inline script）

**Decision**: 採 CSP 禁止 inline script，並以 `script-src 'self' 'wasm-unsafe-eval'` 作為 client-side host 的最低可用基線；同時保證「任何程式碼只能純文字顯示，不得執行」。

**Rationale**:
- 符合 FR-008/FR-008a：降低 XSS 可利用性，並確保 code snippet 顯示不會變成可執行內容。
- WASM runtime 常需要 `wasm-unsafe-eval` 才能正常運作；相較開 `unsafe-eval` 風險面更小。

**Alternatives considered**:
- Nonce/hash CSP：更嚴格但維護成本高；純靜態託管對 nonce 不友善。
- 不做 CSP：不符合 spec 的 security baseline。

## Decision 6: Offline-first（PWA / Service Worker）

**Decision**: 以 PWA 的「App shell precache」為最小可行離線策略：
- 使用者需至少成功載入一次（first-load online），之後可離線啟動與操作核心流程。
- 更新策略採預設 waiting/activate 流程，避免混版。

**Rationale**:
- 符合 FR-003，且維持 minimal change。

**Alternatives considered**:
- Runtime caching API responses：一致性與 invalidation 複雜度顯著提升，不作為 MVP。

## Risk Register（對應 spec 的 must-fix / must-validate）

- **R-001**: `BibleData.dll` 在 browser runtime 的相容性
  - Mitigation: Phase 2 早期建立最小 WASM host + Bible module smoke test；若不相容，需另立 design decision（可能涉及資料外部化），並回到你做明確批准。
- **R-002**: CSP 與 framework runtime 相容性
  - Mitigation: 以 `wasm-unsafe-eval` 作為基線，並在兩平台分別驗證（GH Pages meta vs SWA headers）。
- **R-003**: strict parity 的可驗收性
  - Mitigation: 建立 parity suite（inputs/outputs/state transitions），並明確界定 host bootstrap 不納入 parity。
