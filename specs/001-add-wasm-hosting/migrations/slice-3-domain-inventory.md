# US1 — Slice 3 Domain Extraction: Migration Inventory

> Scope: targeted moves only (minimal-change), whilst keeping strict parity.

## Objective

將「可跨 host 重用」且與 keyword/dictionary workflow 直接相關的檔案，從 Server host 專案搬移到 `CovenantPromptKey.Domain`，以便 Server host 與 WASM host 共用同一份實作。

## In-Scope (Files to Move)

### Matcher algorithm

- Source: `CovenantPromptKey/Services/Implementations/AhoCorasickMatcher.cs`
- Target: `CovenantPromptKey.Domain/Algorithms/AhoCorasickMatcher.cs`
- Notes: Pure algorithm; no host dependencies.

### Dictionary services

- Source: `CovenantPromptKey/Services/Interfaces/IDictionaryService.cs`
- Target: `CovenantPromptKey.Domain/Services/Interfaces/IDictionaryService.cs`

- Source: `CovenantPromptKey/Services/Implementations/DictionaryService.cs`
- Target: `CovenantPromptKey.Domain/Services/Implementations/DictionaryService.cs`

- Dependencies observed:
  - `Microsoft.JSInterop.IJSRuntime` (storage persistence)
  - `IKeywordValidationService`, `IDebugLogService`
  - `Models`, `Constants`, `Models.Results`

### Keyword services

- Source: `CovenantPromptKey/Services/Interfaces/IKeywordService.cs`
- Target: `CovenantPromptKey.Domain/Services/Interfaces/IKeywordService.cs`

- Source: `CovenantPromptKey/Services/Implementations/KeywordService.cs`
- Target: `CovenantPromptKey.Domain/Services/Implementations/KeywordService.cs`

- Dependencies observed:
  - `IMarkdownService` (markdown structure analysis)
  - `IDebugLogService`
  - `Constants`, `Models`, `Models.Results`
  - `AhoCorasickMatcher`

## Out-of-Scope (Explicitly Not Doing Here)

- No mass-moving of `Components/` or UI extraction.
- No CI/deploy workflow changes.
- No redesign or behavioural change to keyword matching rules.
- No refactor to re-home `IMarkdownService` / `ICsvService` / `IWorkSessionService` (handled in Slice 4).

## Required Follow-ups

- Update DI:
  - Ensure moved services are registered for both Server host and WASM host.
  - Update any project-level references if required.

- Fix compilation breakages:
  - Update namespaces/usings in all call sites.
  - Update unit tests expecting old namespaces.

## Verification Checklist

- `dotnet build -c Release` succeeds.
- `dotnet test -c Release` succeeds for:
  - `CovenantPromptKey.Tests`
  - `CovenantPromptKey.NUnitTests`

## Rollback Plan

- Revert the moved files back to `CovenantPromptKey/Services/...` and undo DI/usings changes.
