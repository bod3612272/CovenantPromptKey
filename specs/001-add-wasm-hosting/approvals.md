# Approvals Log: 001-add-wasm-hosting

**Date**: 2025-12-27  
**Feature**: `specs/001-add-wasm-hosting/`  

本文件用於記錄所有需要 explicit approval 的變更（granular approvals），以符合 constitution 的 **Explicit Approval Principle**。

## Approval Register

| Change Type | Scope | Approved? | Rationale (British English, concise) | Rollback Plan |
|---|---|---:|---|---|
| Add new projects | Add `CovenantPromptKeyWebAssembly`, `CovenantPromptKey.Domain`, `CovenantPromptKey.Browser`, `CovenantPromptKey.UI` | ☑ | Approved on 2025-12-27; proceed with skeletons only to minimise brownfield risk. | Revert added projects and remove from solution. |
| Modify solution | Update `CovenantPromptKey.slnx` to include new projects | ☑ | Approved on 2025-12-27; necessary for build orchestration and local developer experience. | Revert `.slnx` changes. |
| DI changes (server) | Update `CovenantPromptKey/Program.cs` to call shared DI extension methods | ☑ | Approved on 2025-12-27; intended to be behaviour-preserving while enabling incremental extraction. | Revert DI wiring; keep original registrations. |
| DI changes (WASM) | New `CovenantPromptKeyWebAssembly/Program.cs` DI graph | ☑ | Approved on 2025-12-27; required to bootstrap the client-side host. | Revert WASM bootstrap; remove project. |
| Modify project files | Update `CovenantPromptKey.Domain/CovenantPromptKey.Domain.csproj`, `CovenantPromptKey/CovenantPromptKey.csproj` (exclude duplicate Compile items), and test project references for Slice 3 (Domain extraction) | ☑ | Approved on 2025-12-27; required to compile moved services and keep tests building without relying on transitive references. | Revert `.csproj` changes and keep types in the Server host project. |
| Modify project files | Slice 4: extend `CovenantPromptKey.Domain/CovenantPromptKey.Domain.csproj` (add Markdig/CsvHelper and reference Browser lib) and extend `CovenantPromptKey/CovenantPromptKey.csproj` Compile excludes | ☑ | Approved on 2025-12-27; pragmatic extraction to keep cross-host parity while preventing duplicate-type collisions. | Revert Slice 4 `.csproj` deltas; restore host-compiled service/model files. |
| DI changes (slices) | Slice 4: register Markdown/Csv/WorkSession + DebugLog via `AddCovenantPromptKeyDomain()` and remove redundant Server registrations | ☑ | Approved on 2025-12-27; keeps composition consistent across hosts and reduces drift. | Revert Domain DI registrations and re-add host registrations. |
| Modify project files | US1 PWA: update `CovenantPromptKeyWebAssembly/CovenantPromptKeyWebAssembly.csproj` to generate `service-worker-assets.js` and publish service worker scripts | ☑ | Approved on 2025-12-27; required for offline-first publish output on free static hosting. | Revert WASM `.csproj` PWA properties/items; remove service worker assets. |
| CI/deploy workflows | Add `.github/workflows/deploy-gh-pages.yml` and `.github/workflows/deploy-azure-swa.yml` | ☑ | Approved on 2025-12-27; required for free static hosting deployment automation (GitHub Pages + Azure SWA). | Revert workflows and related config. |

## Notes

- Tick **Approved?** only after you explicitly confirm in chat.
- Each approval should be as small and reversible as reasonably possible.
