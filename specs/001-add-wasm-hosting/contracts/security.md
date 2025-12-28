# Contract: Security Baseline (CSP + Safe Rendering)

**Date**: 2025-12-26  
**Feature**: `001-add-wasm-hosting`

本 contract 定義 browser-hosted 版本的最低 security baseline（FR-005/FR-006/FR-008/FR-008a, SC-003/SC-004）。

## No secrets in client

- MUST NOT embed secrets (API keys, private endpoints, credentials) in any shipped static artifact
- MUST treat browser storage as user-readable and user-modifiable

## Safe rendering (no execution)

- Any user-provided content MUST be rendered as text (encoded/escaped)
- MUST NOT render user-provided content via raw HTML injection
- Any "code" pasted by users MUST remain plain text and MUST NOT be executable

## CSP baseline

Goal: disable inline scripts and reduce XSS exploitability.

### Client-side host baseline

- `default-src 'self'`
- `base-uri 'self'`
- `object-src 'none'`
- `script-src 'self' 'wasm-unsafe-eval'` (avoid `unsafe-inline`)
- `style-src 'self'`

### Platform differences

- GitHub Pages: CSP typically via `<meta http-equiv>`; cannot enforce certain directives (e.g., `frame-ancestors`) via meta.
- Azure SWA: CSP via response headers is supported and preferred.
