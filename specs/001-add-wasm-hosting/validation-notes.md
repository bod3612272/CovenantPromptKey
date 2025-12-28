# Validation Notes: 001-add-wasm-hosting

**Date**: 2025-12-27  
**Scope**: Phase 6 quickstart validation evidence (build/tests + publish artefacts + security scan).  

## Build & Test (Release)

- Command: `dotnet test -c Release`
- Result: PASS (Total: 286, Failed: 0, Passed: 286)

## Publish Validation (Static Hosting Outputs)

### GitHub Pages

- Command:
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/publish-browser-hosted.ps1 -Platform github-pages -BasePath "/CovenantPromptKey/" -Configuration Release`
- Output:
  - `ReleaseDownload/browser-hosted/github-pages/Release/wwwroot/`
- Base path:
  - `index.html` includes `<base href="/CovenantPromptKey/" />`
  - `404.html` includes `<base href="/CovenantPromptKey/" />`
- Deterministic hashes (from script output):
  - `IndexHtmlSha256=953154AD62C07B3946096EB45A350294C5E1200EE32B0B51AE6AB0E1A2793110`
  - `ServiceWorkerAssetsSha256=CB833BE916ABC580E9D8ED3364F9066114E735783A39F65276F2A67EC0BB9606`

### Azure Static Web Apps (SWA)

- Command:
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/publish-browser-hosted.ps1 -Platform azure-swa -Configuration Release`
- Output:
  - `ReleaseDownload/browser-hosted/azure-swa/Release/wwwroot/`
- Config:
  - `ReleaseDownload/browser-hosted/azure-swa/Release/staticwebapp.config.json` exists and includes `globalHeaders.Content-Security-Policy` baseline.
- Deterministic hashes (from script output):
  - `IndexHtmlSha256=EF9F715149BE36C6FEC278895AF4803D836567D2506FBB33F0969006F7137F35`
  - `ServiceWorkerAssetsSha256=CB833BE916ABC580E9D8ED3364F9066114E735783A39F65276F2A67EC0BB9606`

## Security Validation

### Malicious input corpus

- Corpus: `specs/001-add-wasm-hosting/security/malicious-input-cases.md`
- Manual expectation: any payload renders as text (no script execution), aligning with code-as-text requirement.

### Static artefact secrets scan

- GitHub Pages output:
  - Command: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/scan-static-artifacts.ps1 -PublishRoot "ReleaseDownload/browser-hosted/github-pages/Release/wwwroot"`
  - Result: PASS
- Azure SWA output:
  - Command: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/scan-static-artifacts.ps1 -PublishRoot "ReleaseDownload/browser-hosted/azure-swa/Release/wwwroot"`
  - Result: PASS

## Notes (Operational)

- Publish logs may warn about `wasm-tools` workload for optimisations; this validation focuses on correctness and determinism rather than size optimisation.
