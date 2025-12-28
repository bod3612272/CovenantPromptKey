# Quickstart: Dual-hosting（Server + browser-hosted）

**Date**: 2025-12-26  
**Branch**: `001-add-wasm-hosting`

本 quickstart 針對「設計後」預期的 repo 形態（新增 client-side host + shared libraries）提供操作指引；實作前若你尚未批准新增專案/修改 build/deploy，請先停在 Phase 2 的 approval gate。

## Prerequisites

- .NET SDK：需支援 `net10.0`
- Git

## Run Server host（現況）

- `dotnet run --project CovenantPromptKey/CovenantPromptKey.csproj`
- 預期行為：自動選 port 並開啟瀏覽器（現況 `Program.cs`）。

## Build browser-hosted client-side host（設計目標）

> 需要先完成 Phase 2 並取得 explicit approval 才會落地新增專案。

- Command: `dotnet publish CovenantPromptKeyWebAssembly/CovenantPromptKeyWebAssembly.csproj -c Release`
- Output: `CovenantPromptKeyWebAssembly/bin/Release/net10.0/publish/`（static artefacts root；deploy this directory）
- Notes: This is a deterministic, serverless build output suitable for GitHub Pages / Azure Static Web Apps.

## Publish for Static Hosting（實作狀態）

本 repo 提供一個可重複、可驗證（輸出 hashes）的 publish script：

- Script: `specs/001-add-wasm-hosting/scripts/publish-browser-hosted.ps1`
- Output root: `ReleaseDownload/browser-hosted/{platform}/{configuration}/wwwroot/`

### Publish: GitHub Pages

- Command:
	- `powershell -NoProfile -ExecutionPolicy Bypass -File specs/001-add-wasm-hosting/scripts/publish-browser-hosted.ps1 -Platform github-pages -BasePath "/CovenantPromptKey/" -Configuration Release`
- Notes:
	- `BasePath` 必須是 `/<repo>/` 形式（需以 `/` 開頭與結尾）。
	- Script 會在 publish output 內改寫 `index.html` 與 `404.html` 的 `<base href="..." />`。
	- GitHub Pages 需要 `.nojekyll` 與 `404.html` 才能支援 `/_framework/*` 與 deep-link refresh。

### Publish: Azure Static Web Apps

- Command:
	- `powershell -NoProfile -ExecutionPolicy Bypass -File specs/001-add-wasm-hosting/scripts/publish-browser-hosted.ps1 -Platform azure-swa -Configuration Release`
- Notes:
	- Script 會把 `CovenantPromptKeyWebAssembly/staticwebapp.config.json` 複製到 publish output root。
	- `navigationFallback` 的 `exclude` 規則必須保護 `/_framework/*` 與 `/_content/*`，避免載入 .wasm/.dll 時拿到 HTML。

## Deploy: GitHub Pages（設計目標）

最小要求：
- base href 支援 `/<repo>/`
- `.nojekyll` 存在
- `404.html` SPA fallback（deep link refresh 不 404）
- CSP：GitHub Pages 無法可靠設定 response headers，CSP baseline 以 publish output 內的 `<meta http-equiv="Content-Security-Policy" ...>` 套用。

## Deploy: Azure Static Web Apps（設計目標）

最小要求：
- `staticwebapp.config.json` 啟用 `navigationFallback` 到 `/index.html`
- 設定 `exclude` 避免 `/_framework/*` 被誤導
- 使用 response headers 套用 CSP（禁止 inline script）

## Offline test

- 使用 Published build 測試（service worker 通常只在 publish 生效）
- 在 DevTools（Application → Service Workers）確認 SW 註冊與 cache 建立
- 切換 Offline 後重新整理與 deep link 驗證

## Security validation（US3）

### 1) Malicious input corpus（XSS / injection smoke test）

- Corpus: `specs/001-add-wasm-hosting/security/malicious-input-cases.md`
- 目標：所有輸入都必須「以文字顯示」，不可被瀏覽器當成 HTML/JS 執行。
- 建議做法（最小可重複流程）：
	- 在 UI 中把 corpus 內的 payloads 複製貼上到所有會顯示 user-controlled text 的入口（例如：搜尋/高亮、任何會回顯輸入的區塊）。
	- 預期結果：頁面不應跳 alert、不應執行 script、不應產生可互動的注入 DOM。

### 2) Static publish output secrets scan

此掃描用來確保「client-side 靜態檔案」中不包含常見 secrets/token patterns。

- Script: `specs/001-add-wasm-hosting/scripts/scan-static-artifacts.ps1`
- Example（先產出 publish output，再掃描）：
	- `powershell -NoProfile -ExecutionPolicy Bypass -File specs/001-add-wasm-hosting/scripts/publish-browser-hosted.ps1 -Platform github-pages -BasePath "/CovenantPromptKey/" -Configuration Release`
	- `powershell -NoProfile -ExecutionPolicy Bypass -File specs/001-add-wasm-hosting/scripts/scan-static-artifacts.ps1 -PublishRoot "ReleaseDownload/browser-hosted/github-pages/Release/wwwroot"`

PASS/FAIL：
- PASS：腳本回傳 0 並顯示 `PASS: No secret patterns detected.`
- FAIL：腳本回傳非 0，並列出命中的檔案與 pattern 名稱（match 內容會被遮罩）。
