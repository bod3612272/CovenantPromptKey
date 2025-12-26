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

- `dotnet publish CovenantPromptKeyWebAssembly/CovenantPromptKeyWebAssembly.csproj -c Release`
- 產物：純靜態資產（可上傳到 GitHub Pages 或 Azure Static Web Apps）。

## Deploy: GitHub Pages（設計目標）

最小要求：
- base href 支援 `/<repo>/`
- `.nojekyll` 存在
- `404.html` SPA fallback（deep link refresh 不 404）

## Deploy: Azure Static Web Apps（設計目標）

最小要求：
- `staticwebapp.config.json` 啟用 `navigationFallback` 到 `/index.html`
- 設定 `exclude` 避免 `/_framework/*` 被誤導
- 使用 response headers 套用 CSP（禁止 inline script）

## Offline test

- 使用 Published build 測試（service worker 通常只在 publish 生效）
- 在 DevTools（Application → Service Workers）確認 SW 註冊與 cache 建立
- 切換 Offline 後重新整理與 deep link 驗證
