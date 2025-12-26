# Contract: Strict Parity Suite

**Date**: 2025-12-26  
**Feature**: `001-add-wasm-hosting`

本 contract 定義 SC-005 所需的 parity suite：用一致的輸入與狀態，在 Server host 與 browser-hosted host 上比對輸出與行為。

## Scope

- Includes: in-app user-visible features/behaviours (pages, actions, outputs, state persistence)
- Excludes: host bootstrap behaviour (auto-open browser, port probing, server middleware)

## Parity Test Dimensions

- Inputs → outputs: 동일輸入在兩 host 的結果一致
- State transitions: storage 還原、操作、保存後再還原的一致性
- Error handling: 同樣的錯誤/限制情境要顯示一致訊息（文案差異需先定義）

## Minimum Parity Suite (MVP)

- Dictionary / keyword workflows
- Markdown rendering workflows
- CSV import/export workflows
- Bible module: search/reading/export/bookmark/game workflows
