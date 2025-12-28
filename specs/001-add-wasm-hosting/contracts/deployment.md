# Contract: Deployment (GitHub Pages + Azure Static Web Apps)

**Date**: 2025-12-26  
**Feature**: `001-add-wasm-hosting`

本 contract 定義「browser-hosted 靜態產物」在兩個免費平台上必須滿足的可部署條件與驗收行為（FR-004/FR-010, SC-001）。

## Artifact Contract (static output)

- MUST be deployable as static files (no server runtime required)
- MUST include framework/runtime static assets required to start the app
- MUST support subpath hosting where applicable (GitHub Pages: `/<repo>/`)

## GitHub Pages Requirements

- `base href` MUST be `/<repo>/` (trailing slash required)
- MUST include `.nojekyll` at site root (to allow `/_framework`)
- MUST include `404.html` SPA fallback so deep-link refresh does not 404

## Azure Static Web Apps Requirements

- MUST include `staticwebapp.config.json` with `navigationFallback.rewrite=/index.html`
- MUST exclude static assets from fallback (at least `/_framework/*` and common extensions)
- SHOULD apply CSP via response headers (meta CSP is acceptable only when headers are impossible)

## Acceptance Checklist

- [ ] GitHub Pages: initial load works under `/<repo>/`
- [ ] GitHub Pages: deep link refresh does not 404
- [ ] Azure SWA: deep link refresh does not 404
- [ ] Both: navigation works without broken asset loads
