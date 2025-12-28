# Malicious Input Corpus: 001-add-wasm-hosting

**Date**: 2025-12-27  
**Contract**: `specs/001-add-wasm-hosting/contracts/security.md`  

本文件整理可重複的惡意輸入測試資料集（malicious corpus），並將每一個 case 映射到 UI surface，以驗證：

- 使用者提供的內容永遠以 text-only 呈現（encoded/escaped）
- 不得觸發腳本執行（no execution）

## How to Use

- 在 Server host 與 browser-hosted host 分別重放相同輸入。
- 驗收標準：畫面可顯示文字，但不得執行；不得彈窗、不得外連、不得產生 DOM event-driven JS。

## Corpus

| ID | Payload | Attack class | Target UI surface (map in implementation) | Expected behaviour |
|---|---|---|---|---|
| MI-001 | `<script>alert('xss')</script>` | Script injection | Any user text input surface | Render as text; no alert. |
| MI-002 | `<img src=x onerror=alert('xss')>` | Event handler injection | Any user text input surface | Render as text; no alert/network. |
| MI-003 | `<svg onload=alert('xss')></svg>` | SVG/onload | Any user text input surface | Render as text; no alert. |
| MI-004 | `<a href="javascript:alert('xss')">click</a>` | JS URL | Any rendered output area | Render as text; not clickable JS. |
| MI-005 | `</textarea><script>alert(1)</script>` | Context break-out | Any textarea-backed UI | Render as text; no DOM break-out. |
| MI-006 | "```html\n<script>alert(1)</script>\n```" | Markdown code fencing | Markdown preview | Code block shows raw text only. |
| MI-007 | "[x](javascript:alert(1))" | Markdown link JS scheme | Markdown preview | Link is sanitised or rendered as plain text (no JS). |
| MI-008 | "<b onclick=alert(1)>bold</b>" | Inline handler | Any output | Render as text; no click handler. |

## Mapping Notes

- 若某 UI surface 使用 `MarkupString` 或 raw HTML insertion，必須先列入 audit（見 US3 tasks）。
- 任何差異（例如某 host 的 default escaping 行為不同）必須被視為 parity defect，除非在 Allowed deltas 明確豁免。
