# Parity Suite Testcases: 001-add-wasm-hosting

**Date**: 2025-12-27  
**Contract**: `specs/001-add-wasm-hosting/contracts/parity.md`  

本文件定義 parity suite 的 testcase schema 與 MVP 測試案例（strict parity）。

## Schema

每個 testcase 必須包含：

- **Initial state**: local/session storage 初始內容（key/value 或「empty」）
- **Steps**: 明確操作步驟（UI actions / inputs）
- **Expected output**: 使用者可觀測輸出（畫面文字、結果內容、錯誤訊息）
- **Expected storage delta**: storage 變更（新增/修改/刪除的 keys）
- **Allowed deltas**: 允許的 host-specific 差異（通常應為空；若非空需先記錄原因）

> Scope reminder: parity 不包含 host bootstrap（port probing、auto-open browser、server middleware）。

## Naming & Storage Recording

- Storage keys 請以「namespace + version」描述（如 `cpk:work-session:v1`）。
- JSON payload 請以 pretty-printed 示例提供。

## MVP Core Operations (US1 checkpoint)

### TC-001 Dictionary / Keyword workflow

- **Initial state**: empty
- **Steps**:
  1. Open the keyword/dictionary page.
  2. Add one mapping: `foo` → `bar`.
  3. Save.
  4. Reload the app.
- **Expected output**:
  - Mapping is visible after reload.
- **Expected storage delta**:
  - `cpk:dictionary:*` created/updated
- **Allowed deltas**:
  - None

### TC-002 Markdown rendering workflow

- **Initial state**: empty
- **Steps**:
  1. Open the markdown workflow.
  2. Input a markdown sample containing headings, lists, and code block.
  3. Render/preview.
- **Expected output**:
  - Rendered output matches between hosts (textual content and structure).
  - Code content remains plain text (no execution).
- **Expected storage delta**:
  - `cpk:work-session:*` updated (if applicable)
- **Allowed deltas**:
  - None

### TC-003 CSV import/export workflow

- **Initial state**: empty
- **Steps**:
  1. Prepare a small CSV file with 2–3 mappings.
  2. Import into the app.
  3. Export back to CSV.
- **Expected output**:
  - Imported entries exist and exported CSV content is equivalent.
- **Expected storage delta**:
  - `cpk:dictionary:*` updated
- **Allowed deltas**:
  - None
