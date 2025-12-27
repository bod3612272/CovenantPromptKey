# Implementation Plan: 001-add-wasm-hosting

**Branch**: `001-add-wasm-hosting` | **Date**: 2025-12-26 | **Spec**: [specs/001-add-wasm-hosting/spec.md](specs/001-add-wasm-hosting/spec.md)

## Cross-links

- Task tracker: [specs/001-add-wasm-hosting/tasks.md](specs/001-add-wasm-hosting/tasks.md)
- Quickstart (run/publish): [specs/001-add-wasm-hosting/quickstart.md](specs/001-add-wasm-hosting/quickstart.md)
- Contracts (acceptance requirements): [specs/001-add-wasm-hosting/contracts/](specs/001-add-wasm-hosting/contracts/)
- Security audit (MarkupString/injection surfaces): [specs/001-add-wasm-hosting/security/markupstring-audit.md](specs/001-add-wasm-hosting/security/markupstring-audit.md)
- Validation evidence: [specs/001-add-wasm-hosting/validation-notes.md](specs/001-add-wasm-hosting/validation-notes.md)

## Summary

目標是「雙宿主」：保留現有 Server host（目前是 Blazor Web App + Interactive Server），另外新增一個 client-side host（Blazor WebAssembly）。

最佳實務建議把共享切成兩層：
1) **共享 UI**：用 Razor Class Library (RCL) 承載可重用的 Razor components + 靜態資產（CSS/JS/圖片）。
2) **共享領域/商業邏輯**：用純 .NET Class Library（不依賴 ASP.NET Core/Blazor hosting）放 models、規則、解析、計算與可測試的服務。

同時，針對「兩種 host 行為一致」的重點（JS interop、Local/Session Storage），採用：
- 在共享層只定義 **抽象（interfaces）與跨 host 安全的用法**（以 `IJSRuntime` 為主）。
- host 專案各自負責 DI 註冊與 host-specific 的啟動差異。
- Storage/JS interop 實作盡量共用一份（放在可被兩個 host 引用的 library），並處理 Server prerender/未互動期 JS 不可用的情況。

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`)  
**Current Host**: Blazor Web App（Server 專案）已啟用 `.AddInteractiveServerComponents()` / `.AddInteractiveServerRenderMode()`  
**Primary Dependencies**: `CsvHelper`, `Markdig`, `BibleData.dll`  
**Testing**: `CovenantPromptKey.NUnitTests`（NUnit 4.x 為本 repo 的規範）；`CovenantPromptKey.Tests`（xUnit，既存專案，不在此 feature 範圍內調整）  
**Target Platform**: Windows（Server host 可單檔發布；新增 WASM host 為瀏覽器執行）  

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **I. Brownfield Project Respect (NON-NEGOTIABLE)**: PASS（規劃階段不改 production code；實作時只改與 dual-hosting / security baseline / deploy 直接相關的部分）
- **II. Minimal Change Principle**: PASS（採 incremental 落地；先讓 WASM host 可跑、可部署，再逐步收斂共用層）
- **III. Explicit Approval Principle**: PASS WITH CONDITIONS（實作會涉及新增專案/修改 `.csproj`/新增 CI 部署設定；在開始動到 repo structure 或 build/deploy 前，會逐項提出選項並等待你明確批准）
- **IV. Technology Stack Stability**: PASS（維持 `net10.0`；不調整 TargetFramework / Nullable / LangVersion 等全域設定）
- **VII. Testing Discipline**: PASS（新功能優先補 NUnit 測試；不修改既有測試框架配置）

## Recommended Solution Structure

### 專案切分（建議）

```text
CovenantPromptKey.slnx

# Host projects
CovenantPromptKey/                       # ✅現有：Server host（保留，必要時可日後更名為 .Server）
CovenantPromptKeyWebAssembly/            # ➕新增：Blazor WebAssembly host（browser-hosted static publish output）

# Shared libraries
CovenantPromptKey.UI/                    # ➕新增：Razor Class Library (RCL) 放共享 Razor components + 靜態資產
CovenantPromptKey.Domain/                # ➕新增：Class library 放共享 domain models/logic（不碰 IJSRuntime/HttpContext）

# (可選) 若你想更乾淨地隔離 Browser-only 的東西
CovenantPromptKey.Browser/               # ➕新增：Class library 放 JS interop wrappers / storage 實作（依賴 Microsoft.JSInterop）

# Tests
CovenantPromptKey.Tests/
CovenantPromptKey.NUnitTests/
```

### 依賴方向（避免循環）

```text
CovenantPromptKey.Domain
  ↑
CovenantPromptKey.UI  (引用 Domain；只放 UI 與 UI-用的 abstractions)
  ↑                ↑
  |                |
Server host     WASM host
```

## Key Best Practices (重點準則)

## Strict Parity Definition（如何解讀「完全一致」）

依照 spec 的 strict parity（FR-002/FR-009），本 plan 的實作與驗收以「使用者可見的 in-app 功能與行為」為比較單位：

- 包含：頁面/功能、輸入→輸出、狀態保存/還原、匯入/匯出、Bible 模組行為、錯誤訊息與限制提示。
- 不包含：host bootstrap（例如 Server 版自動開啟瀏覽器、動態 port 探測、Kestrel/SignalR 管線等）。這些屬於 host 專屬啟動行為，browser-hosted static hosting 亦無對應概念。

（此定義用來讓 SC-005 的 parity suite 可落地且可驗收。）

### 1) RCL 放什麼、不放什麼

建議放在 `CovenantPromptKey.UI`：
- `Components/Pages/Layout/Shared` 等 Razor components（可 routable）
- CSS、圖片、JS（放 `wwwroot`）

不建議放：
- host-specific 啟動/管線（`Program.cs` / middleware）
- 只在 Server 才能用的服務（例如直接碰檔案系統、EF DbContext、Windows-only API）

（Microsoft 指引要點）
- `index.html`（WASM）與 host page（Server 的 `App.razor` 這類 root）通常是 host-specific，不要硬共享。
- 若要共享 routable components，可用 Router 的 `AdditionalAssemblies` 或調整 `AppAssembly` 指到含 routes 的 assembly。

### 2) RCL 靜態資產（CSS/JS）引用方式

RCL 的靜態資產請放在 `CovenantPromptKey.UI/wwwroot/...`，host 引用時使用：

`_content/{PackageId-or-AssemblyName}/{path}`

例如：
- `_content/CovenantPromptKey.UI/js/interop.js`
- `_content/CovenantPromptKey.UI/css/app.css`

### 3) JS interop：用「可跨 host」的形狀

共享層（RCL/Browser library）優先只依賴：
- `IJSRuntime` / `IJSObjectReference`（兩種 host 都支援）

WASM 專屬的同步 interop（`IJSInProcessRuntime`）如果要用，建議：
- 只做「可選最佳化」：以 `as IJSInProcessRuntime` 條件轉型，有就同步、沒有就走 async fallback。

另外：如果你會用 `[JSImport]`/`[JSExport]` 新式 interop，請注意 RCL 的 collocated JS 不支援；把 JS 放 RCL 的 `wwwroot`，並以 `_content/...` 路徑載入。

### 4) Storage services：兩種 host 一致行為的設計

你目前已有 `ILocalStorageService`/`ISessionStorageService`，而且在 Server host 以 Scoped 註冊。

建議調整成：
1. **Interface 放共享層**（例如 `CovenantPromptKey.Domain` 或 `CovenantPromptKey.Browser.Abstractions`）。
2. **實作放可共用的一個 library**（例如 `CovenantPromptKey.Browser`），兩個 host 都引用同一個實作，減少行為分歧。
3. **處理 Server prerender/互動未就緒**：
  - Server 的互動元件在「互動完成前」呼叫 JS 會失敗（JS runtime 尚未可用/電路尚未建立）。
  - 做法 A（最簡單、最一致）：Storage service 在第一次需要 JS 時，等待一個 `InteractiveReady` 的訊號；該訊號由任何共享 Layout/根元件在 `OnAfterRenderAsync(firstRender)` 觸發。
  - 做法 B（較嚴格）：若未就緒就拋出自訂例外，並要求呼叫端只能在 `OnAfterRenderAsync` 後使用（容易踩雷，不推薦）。

JS 端（共用一份）提供：
- `localStorage.getItem/setItem/removeItem`
- `sessionStorage.getItem/setItem/removeItem`
- 需要 JSON 序列化就由 .NET 端處理（可測試且一致）。

## Security Baseline（CSP + safe rendering）

### CSP minimum（禁止 inline script）

目標：符合 spec（FR-008），禁止 inline script，同時讓 WASM runtime 正常運作。

- **Azure Static Web Apps（可用 response headers）**：以 HTTP header 設定 CSP（可支援 `frame-ancestors`）。
- **GitHub Pages（通常只能用 `<meta http-equiv>`）**：可用 meta 設定 CSP，但 `frame-ancestors` 類指令無法透過 meta 生效（限制需在風險章節中明確記錄）。

WASM 常見最低需求（保守基線）：

```text
default-src 'self';
base-uri 'self';
object-src 'none';
script-src 'self' 'wasm-unsafe-eval';
style-src 'self';
```

### Code-as-text（任何程式碼只能顯示、不可執行）

依照 spec（FR-008a / User Story 3），所有使用者貼上的任何程式碼內容必須：

- 永遠以純文字呈現（encoding / escaping）
- 不得透過 `MarkupString` 直接注入
- 不得允許以 HTML/JS 被解譯與執行

## Deployment Targets（GitHub Pages + Azure SWA）

### GitHub Pages

- `base href` 必須支援 `/<repo>/` 子路徑（可在 CI 以 token replacement 改寫）
- 需要 `.nojekyll` 以確保 `_framework` 目錄可被服務
- 需要 `404.html` SPA fallback（deep link refresh 不 404）

### Azure Static Web Apps

- 使用 `staticwebapp.config.json` 的 `navigationFallback` rewrite 到 `/index.html`
- 設定 `exclude` 避免把 `/_framework/*` 與靜態資源誤導到 HTML

## Migration Steps (from current repo)

1. **新增 `CovenantPromptKey.Domain`**
  - 移入純模型/純邏輯：例如 `Models/` 中不依賴 UI/JS 的型別、Markdown parsing 的純函式、Keyword/Dictionary 的規則與演算法。
  - 原本在 `CovenantPromptKey` 專案內、但不需要 ASP.NET Core 的 services，改成放 Domain 並用 DI 註冊在兩個 host。

2. **新增 `CovenantPromptKey.UI` (RCL)**
  - 把可共享的 `Components/Pages/Layout/Shared` 逐步搬進 RCL（先從最少依賴的頁面開始）。
  - RCL 內建立 `wwwroot/js/...`、`wwwroot/css/...`（把想共享的 interop.js / css 搬過去）。
  - Host（Server/WASM）改用 `_content/CovenantPromptKey.UI/...` 引用資產。

3. **在 Server host 保留 host page + Router 組態，但指向 RCL pages**
  - 保留現有 `CovenantPromptKey/Components/App.razor`（這是 host page；通常不共享）。
  - 調整 Router（`Routes.razor`）讓它能路由到 RCL 的 routable components：
    - 若 RCL 內放 routable pages，使用 `AdditionalAssemblies`（或把 `AppAssembly` 換成 RCL assembly，再把 host assembly 加到 AdditionalAssemblies）。

4. **新增 `CovenantPromptKeyWebAssembly`（Blazor WebAssembly host）**
  - 建立新的 WASM 專案，引用 `CovenantPromptKey.UI` + `CovenantPromptKey.Domain`（以及 `CovenantPromptKey.Browser` 若有）。
  - 建立 `wwwroot/index.html`（WASM host 專屬，不共享）。
  - 設定它要用同一套路由（`App.razor/Router` 可以是 WASM 版，指向 RCL pages）。

5. **JS interop + Storage 一致化**
  - 把 `ILocalStorageService`/`ISessionStorageService` interface 移到共享層。
  - 把 JS interop（storage/clipboard/whatever）集中成「一個 JS module」放在 RCL `wwwroot`。
  - 在 .NET 端提供單一實作（兩 host 共用），並加入「互動就緒」保護，避免 Server prerender 期間呼叫 JS。

6. **組態與發布策略確認（很重要）**
  - 你目前 Release 設了 `GenerateStaticWebAssetsManifest=false` 以支援單檔發布；但 RCL/WASM host 會強依賴 static web assets 管線。
  - 建議在導入 RCL/WASM 後，重新驗證 Release 發布流程是否仍可單檔；若 `_content/...` 找不到資產，可能需要調整 publish 策略（例如改非單檔、或恢復 manifest/使用支援的 static assets 作法）。

7. **（可選）WASM host 的離線優先（PWA / Service Worker）**
  - 目標：在「純靜態 hosting」下，讓 WASM host 在使用者「至少成功載入一次」後可離線啟動（App shell offline）。
  - 採用 Blazor WebAssembly PWA 的預設模型：build 產生 `service-worker-assets.js`（含 hash），`service-worker.published.js` 以 cache-first 建立「一致性快取快照」。
  - 注意：離線支援只在 *published* 狀態啟用；開發模式通常不啟用，避免干擾 hot reload/開發迭代。

8. **測試策略**
  - Domain library 的服務/規則優先補 unit tests（快速、穩定、跨 host 共用）。
  - Storage/JS interop 以 interface mock 測試 .NET 端序列化/行為；host 整合測試可以晚一點做。

## Offline-first (PWA) Notes（針對 Browser-hosted WASM；最小可行做法）

> 這一段以「WASM host 可靜態部署」為前提（例如 Nginx/Static Web hosting/CDN）。
> 離線能力主要靠 Service Worker + Cache Storage，並遵循 Blazor PWA 預設的 hash-based 一致性快取模型。

### 快取策略（Caching strategy）

- **App Shell precache（最小且建議）**：只確保應用程式本身可離線啟動。
  - 使用 Blazor PWA 預設：`service-worker-assets.js` 列出 app 需要的靜態資產（`.wasm`、assemblies、`.js`、`.css`、圖片等）與內容 hash。
  - `service-worker.published.js` 的 `onInstall` 會建立新 cache 並把 manifest 內的資產抓下來。
  - **避免把大量/不必要內容放進 `wwwroot`**：因為 manifest 會把 publish 到 `wwwroot` 的東西都列進去，會導致首次安裝快取時間過長甚至失敗。

- **Runtime caching（先不做/保守做）**：對 API 回應做離線快取通常會大幅增加同步/一致性複雜度。
  - 最小做法是：API 仍走 network；離線時由 app 端（Razor components）對 `HttpClient` 失敗做 graceful handling（顯示離線提示、使用本機狀態或最後一次結果）。

### 更新策略（Update strategy）

- **預設背景更新模型**（Blazor PWA template）：每次使用者造訪且網路正常時，瀏覽器會重新抓 `service-worker.js` 與 `service-worker-assets.js`，若內容變動就開始安裝新 worker。
- **啟用較可靠的 SW 更新**：在 WASM host 的 `wwwroot/index.html` 註冊 service worker 時使用 `updateViaCache: 'none'`，避免瀏覽器用 HTTP cache 拿舊的 `service-worker-assets.js`/SW script，降低部署後暫時性 integrity 問題。
- **等待啟用（waiting）是正常現象**：新 service worker 安裝完成後，通常會進入 waiting，直到使用者把 app 的所有分頁/視窗都關閉，才會 activate。
  - 測試時可在 DevTools 的 Application → Service Workers 看到 waiting 狀態並手動點選 `skipWaiting` 來快速切換。
  - 若你把 `skipWaiting/clientsClaim` 自動化，會放棄「同一個分頁生命週期內資產一致」的保證；最小方案建議先沿用預設 waiting 行為。

### 首次離線行為（First-load offline behavior）

- **第一次造訪無法離線啟動**：離線優先 PWA 仍需要「至少一次在線上完整載入」才能把 app shell 快取進 Cache Storage。
- **最小 UX 建議**：
  - 如果使用者在完全沒快取前就離線進站，只能顯示瀏覽器的網路錯誤頁（除非你有額外做 navigation fallback/自訂 offline page）。
  - 一旦快取完成，離線重新整理/深連結（例如 `/counter`）會由 service worker 以快取的 `/index.html` 回應，WASM app 仍可啟動。

### 靜態 hosting 的最小要求（Minimal static hosting）

- **必須 HTTPS**：service worker 需要安全環境（localhost 開發例外）。
- **不要修改 publish 產物**：CDN/中介層如果會對檔案做動態改寫（例如自動 minify HTML/JS），可能導致 hash/integrity 與快取一致性問題。
- **路由回寫（SPA fallback）**：靜態 hosting 需把未知路徑導向 `/index.html`（讓深連結可啟動），同時要確保 `/_framework/*`、`_content/*` 等靜態資源路徑不被誤導到 `index.html`（否則會出現 .dll/.wasm 取到 HTML 而造成 integrity 失敗）。

### 離線測試方式（How to test offline behavior）

最小且有效的測試流程：

1. **用 Published 版本測**：因為離線支援通常只在 publish 後啟用。
2. **打開 DevTools → Application**：
  - 確認 Service Worker 已註冊（Application → Service Workers）。
  - 看 Cache Storage 是否出現 app 的 cache 並含資產。
3. **切離線**：
  - Network tab 設為 Offline，或 Application → Service Workers 勾選 Offline。
4. **刷新與深連結**：
  - 在 Offline 狀態重新整理頁面，確認 app shell 仍可啟動。
  - 直接進 `/some/route`（深連結）確認仍能載入（會回 `/index.html`）。
5. **測更新**：
  - 勾選 Application → Service Workers 的 **Update on reload** 來強制每次 reload 嘗試更新。
  - 部署新版本後觀察 worker 進入 waiting；關閉所有分頁再開，或在 DevTools 手動 `skipWaiting`（測試用）。

（如果遇到 integrity 類錯誤）
- 先用 Network tab 確認 `.wasm`/`.dll` 請求回來的是正確二進位、HTTP 200，且不是被錯誤地回寫成 `index.html`。
- 若你確定 hosting 無法提供一致回應、或部署過程可能導致混版快取，可評估是否要關掉 `service-worker.published.js` 的 `integrity` 參數（但這會失去安全保證，最小方案不建議預設關閉）。

## Notes / Open Questions

- `BibleData.dll` 是否可在 WASM 執行：若該 DLL 依賴檔案系統/非瀏覽器可用 API，WASM 可能不可用；需要決定 WASM 版資料載入策略（例如改成從 `wwwroot` 下載資料、或透過 API）。
- 若未來想用同一個 Server 專案同時支援 `InteractiveServer` + `InteractiveWebAssembly` render modes（不是「兩個獨立 host」），可再評估改成 Blazor Web App 的 `.Client` 專案模式；但這與「獨立 WASM host」的部署/路由模型不同。
