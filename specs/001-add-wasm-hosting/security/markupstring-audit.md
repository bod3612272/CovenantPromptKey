# MarkupString / HTML Injection Audit (US3)

本文件盤點在 Browser-hosted / WASM output 中，任何可能造成「使用者可控內容被當作 HTML 解譯」的渲染入口點（entrypoints），並記錄現況與緩解措施（mitigations）。

## Scope

- 以 repo 內 `MarkupString` 使用點為主（快速落地、可測）。
- 目標：**code-as-text**（永遠顯示、不可執行），並符合 plan 的 **CSP baseline（禁止 inline script）**。

## Inventory

| File path | Surface | Input source | Current render method | Risk | Mitigation |
|---|---|---|---|---|---|
| CovenantPromptKey/Components/Shared/Bible/BibleVerseDisplay.razor | 單章閱讀/多節顯示的「關鍵字高亮」 | `HighlightKeyword`（使用者在 UI 輸入的搜尋/高亮字串）+ 經文內容（資料集） | 先用字串 Replace 拼出 `<span class="bible-verse-highlight">...`，再以 `MarkupString` 輸出 | 若 keyword 或 content 被注入 HTML，`MarkupString` 會繞過 encoding，造成 XSS 風險 | 改為 `RenderFragment`：以 index/range 計算，並用 `builder.AddContent(...)` 輸出（由 Blazor 自動 HTML-encode），只用受控的 `<mark class="bible-verse-highlight">` 元素包住 matched substring |
| CovenantPromptKey/Components/Shared/Bible/BibleSearchResults.razor | 搜尋結果列表中的「高亮經文片段」 | `SearchKeyword`（使用者輸入）+ `SearchResultItem.Content`（資料集） | 直接將 `result.HighlightedContent` cast 成 `MarkupString` 輸出（HighlightedContent 由 service 產生 HTML） | 雖然目前 service 會先 HtmlEncode，但 UI 仍存在 raw HTML injection surface（若未來換來源或 bug，風險放大） | 改為 `RenderFragment`：完全基於 `result.Content` + `SearchKeyword` 進行安全高亮渲染，不再依賴/輸出 `HighlightedContent` 的 HTML |
| CovenantPromptKey/Components/Shared/LogViewer.razor | Log level badge（含 icon） | `LogEntry.Level`（程式內 enum） | `GetLevelBadge()` 回傳 `MarkupString` 內含 `<span>`/`<i>` | 目前 input 非 user-controlled，風險較低，但仍屬 HTML injection pattern（不利於安全基線一致性） | 改為 `RenderFragment`：以 `builder.OpenElement/AddAttribute` 組出固定 DOM，不再拼接 HTML 字串 |

## Notes

- 本次修正的目標是把 UI 層的 `MarkupString` 使用 **歸零**，讓「任何使用者可控文字」都走 Blazor 的 encoding pipeline。
- `BibleSearchService.HighlightKeywords(...)` 目前仍回傳 HTML 字串（並進行 HtmlEncode），但 UI 不再使用該 HTML 進行渲染；後續若要進一步收斂 API，可在不破壞契約的前提下逐步淘汰 `HighlightedContent`。
