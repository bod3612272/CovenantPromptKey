# Data Model (Conceptual): Browser-hosted Dual-hosting

**Date**: 2025-12-26  
**Branch**: `001-add-wasm-hosting`  

本 feature 的 data model 以「既有 app 已存在的 browser storage state」為中心，重點是：
- 兩個 host（Server / browser-hosted）對同一份資料結構的讀寫語意要一致（strict parity）。
- 所有 browser storage 資料都視為可被使用者檢視/竄改（untrusted at rest），因此必須具備 validation 與復原能力。

## Entities

### Browser Storage Record

代表存放於瀏覽器端（`localStorage`/`sessionStorage`）的任意資料紀錄。

**Core fields (conceptual)**
- `Key`: storage key（namespace + version）
- `Value`: serialised payload（JSON 文字）
- `SchemaVersion`: 用來做 migration / backward compatibility

**Validation rules**
- 反序列化失敗或 schema 不符：必須回復到可操作的 safe default（FR-007）。
- 任何從 storage 還原後的文字內容仍屬 untrusted input：render 時必須安全輸出（FR-006/FR-008a）。

### Dictionary

使用者自訂關鍵字/替換規則集合。

**Key attributes**
- entries（keyword mappings）
- lastUpdated（用於同步/避免多分頁覆寫的最低限度策略）

### Work Session

使用者當前工作階段狀態（例如輸入、處理結果、模式）。

**Key attributes**
- current input / output
- selected options
- timestamps

### Bible Settings / Bible Page State

Bible 模組的 UI state（篩選、目前章節、顯示模式等）。

### Bible Bookmark

Bible 模組的書籤集合。

### Bible Game State

Bible 遊戲/互動模式的狀態與紀錄。

## Relationships (conceptual)

- Dictionary / Work Session / Bible Settings / Bookmark / Game State 都是 Browser Storage Record 的 payload 類型。
- Parity suite 會以「同樣的 payload」在兩 host 上重放操作，驗證輸出與狀態變更一致（SC-005）。

## State transitions

- `Initial` → `LoadedFromStorage` → `UserMutates` → `Persisted`
- `Persisted` → `CorruptedOrOldSchema` → `RecoveredToDefault`（可操作狀態）
