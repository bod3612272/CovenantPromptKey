# Feature Specification: Browser-hosted 版本（可免費 Static Hosting）與資安基線

**Feature Branch**: `001-add-wasm-hosting`  
**Created**: 2025-12-26  
**Status**: Draft  
**Input**: User description: "新增可部署至免費 static hosting 的 browser-hosted（client-side）版本，並以資安基線確保安全（避免 XSS/HTML injection、避免將 secrets 置於前端），同時保留現有 Server 版本作為 fallback（dual-hosting）。"

## Cross-links

- Implementation plan: [specs/001-add-wasm-hosting/plan.md](specs/001-add-wasm-hosting/plan.md)
- Task tracker: [specs/001-add-wasm-hosting/tasks.md](specs/001-add-wasm-hosting/tasks.md)
- Quickstart (run/publish): [specs/001-add-wasm-hosting/quickstart.md](specs/001-add-wasm-hosting/quickstart.md)
- Contracts (acceptance requirements): [specs/001-add-wasm-hosting/contracts/](specs/001-add-wasm-hosting/contracts/)
- Validation evidence: [specs/001-add-wasm-hosting/validation-notes.md](specs/001-add-wasm-hosting/validation-notes.md)

## Clarifications

### Session 2025-12-26

- Q: 主要目標部署平台是哪一個？ → A: 兩者都必須支援（以同一套產物/腳本兼容兩邊）
- Q: browser-hosted 版本的 MVP parity 範圍？ → A: 追求與 Server 版完全一致（所有功能與行為都要一致）
- Q: Security Baseline Policy（CSP）最低要求？ → A: 設定 CSP，禁止 inline script；同時允許貼上/顯示任何程式碼（必須以純文字安全呈現，不得被執行）

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - 以免費 Static Hosting 使用並可離線工作 (Priority: P1)

作為一般使用者，我希望可以直接透過瀏覽器開啟並使用 CovenantPromptKey（不需要任何後端或長連線），並在首次載入後即使離線也能完成核心工作流程，以符合 offline-first 的產品期待。

**Why this priority**: 這是本升級提案的 primary value：降低託管門檻並提供 genuinely offline 的使用體驗。

**Independent Test**: 只要提供一個可用的 browser-hosted（client-side）版本並具備離線快取，使用者即可在沒有伺服器的情境下完成核心操作，獨立可測。

**Acceptance Scenarios**:

1. **Given** 使用者已成功載入一次 browser-hosted 版本且已完成必要資源快取，**When** 使用者切換到離線狀態並重新開啟應用程式，**Then** 應用程式仍可啟動並提供核心功能介面。
2. **Given** 使用者在離線狀態，**When** 使用者執行至少一項核心流程（例如：關鍵字處理、內容檢視/搜尋、匯出/下載），**Then** 系統應能完成該流程或提供清晰可理解的限制說明（不得出現無限重試或「必須連線」的誤導）。

---

### User Story 2 - 維運者可用免費平台順利部署 (Priority: P2)

作為維運者（repo maintainer），我希望可以把 browser-hosted 版本以靜態資產的形式部署到免費的 static hosting（例如 GitHub Pages / Azure Static Web Apps 類型的平台），並確保在子路徑與 deep link 情境下仍能正常開啟與導覽。

**Why this priority**: 若無法穩定部署到免費平台，本功能的 operational benefit 會大幅下降。

**Independent Test**: 只要提供一份可部署的靜態輸出，並在測試環境驗證 base-path 與 deep-link 行為即可獨立完成測試。

**Acceptance Scenarios**:

1. **Given** browser-hosted 版本已部署到一個「非 root」的站點子路徑，**When** 使用者從該子路徑進入應用程式並進行頁面導覽，**Then** 所有導覽應維持在同一站點範圍且不發生資源載入失敗。
2. **Given** 使用者手動輸入（或由書籤打開）一個應用程式的 deep link，**When** 使用者重新整理頁面，**Then** 應用程式仍能載入並正確顯示該 deep link 的內容（不得回傳 404 造成使用者被阻斷）。

---

### User Story 3 - 安全輸出與資料保護（降低 XSS 影響面） (Priority: P3)

作為重視資安的使用者，我希望系統在顯示高亮、搜尋結果、或任何由使用者輸入/匯入資料衍生出的內容時，都能以安全的方式呈現，避免 script injection 或 HTML injection，並降低瀏覽器端儲存資料被 XSS 直接讀取的風險。

**Why this priority**: 本專案大量依賴瀏覽器端儲存以支援 offline-first；因此一旦 XSS 成立，impact 會特別直接且嚴重。此 user story 提供必要的 security baseline。

**Independent Test**: 可透過一組惡意輸入（例如：關鍵字、匯入內容、被高亮的片段）進行測試，驗證頁面不會執行注入腳本且內容以文字呈現。

**Acceptance Scenarios**:

1. **Given** 使用者輸入包含 HTML/JavaScript 特徵的字串（例如 `<script>...</script>` 或事件屬性片段），**When** 該字串被用於高亮或顯示在任何 UI 區塊，**Then** 畫面應僅顯示其文字（已被安全轉義），不得執行任何腳本。
2. **Given** 使用者在瀏覽器端已保存偏好/工作狀態等資料，**When** 使用者觸發可能造成渲染的特殊字元輸入，**Then** 系統不得發生 DOM 注入導致資料被前端腳本讀取或外送（以可觀測行為驗證：無腳本執行、無非預期網路請求）。
3. **Given** 使用者貼上包含任意程式碼的內容（可能含 `<`、`>`、`"` 等字元），**When** 該內容被顯示於 UI（例如預覽、結果、或歷史紀錄），**Then** 系統 MUST 以純文字安全呈現該內容（完成轉義/encoding），不得被執行，且不得因 CSP 或安全策略導致內容無法顯示。

---

[Add more user stories as needed, each with an assigned priority]

### Edge Cases

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right edge cases.
-->

- 首次載入就處於離線狀態：系統應提示需要先完成一次線上載入，並提供可理解的行動指引。
- 站點以子路徑託管：資源與路由不得因 base-path 不一致而失效。
- 使用者以 deep link 開啟/重新整理：不得因 static hosting 的 fallback 行為而被 404 阻斷。
- 使用者輸入或匯入超長文本：系統不得因輸入大小而直接崩潰；必要時以清楚訊息提示限制。
- 瀏覽器儲存空間不足或被清除：系統應能回復到可操作狀態，並避免資料結構錯誤導致無限錯誤迴圈。
- 多分頁同時開啟：狀態更新不應造成資料互相覆蓋到不可理解的程度；至少應具備明確且一致的行為。
- 版本升級後快取混淆：若快取導致行為異常，系統應能以使用者可理解的方式恢復（例如提示重新載入）。

## Requirements *(mandatory)*

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right functional requirements.
-->

### Functional Requirements

- **FR-001**: System MUST 提供一個 browser-hosted（client-side）版本，可作為「純靜態資產」部署與提供服務（不得依賴伺服器常駐進程或長連線才能互動）。
- **FR-002**: System MUST 維持完整 feature parity：browser-hosted 版本與既有 Server 版本在所有使用者可見功能與行為上 MUST 完全一致（不得以平台差異作為功能缺漏或行為差異的理由）。
- **FR-003**: System MUST 支援 offline-first：在完成一次成功載入後，使用者在離線狀態仍能啟動並使用核心功能；若使用者尚未完成首次載入，系統 MUST 清楚提示離線限制。
- **FR-004**: System MUST 支援子路徑託管（non-root hosting）並確保導覽與資源載入一致；deep link 在重新整理後 MUST 仍能回到正確畫面。
- **FR-005**: System MUST 明確遵循「no secrets in client」原則：browser-hosted 版本不得內含任何敏感憑證、金鑰、或需保密的端點資訊。
- **FR-006**: System MUST 對所有可能源自使用者輸入/匯入/儲存資料的顯示內容採用安全輸出策略：預設以文字呈現並進行適當轉義；任何高亮/標記效果 MUST 在安全轉義後再套用標記，避免 HTML injection。
- **FR-007**: System MUST 在瀏覽器端保存必要的使用者資料以支援離線體驗（例如偏好、字典、工作狀態等），並具備資料異常時的復原能力（例如：資料結構不符時能回復到可操作狀態）。
- **FR-008**: System MUST 提供最低限度的前端 security baseline 以降低 XSS 可利用性：必須設定 CSP 並禁止 inline script；同時不得造成核心功能不可用。
- **FR-008a**: System MUST 允許使用者貼上/顯示任何程式碼作為內容的一部分，並保證其永遠以純文字安全呈現（不得被執行、不得被當成可執行 script 或可解譯的 HTML）。
- **FR-009**: System MUST 保留既有 Server 版本作為 fallback 路徑（dual-hosting）。（Parity 規範由 FR-002 與 SC-005 定義。）
- **FR-010**: System MUST 支援部署到 GitHub Pages 與 Azure Static Web Apps（或同等 static hosting 平台），並確保同一套輸出產物與自動化流程可在兩者上達成可用狀態與驗收。

### Key Entities *(include if feature involves data)*

- **Browser-hosted Release**: 代表可被 static hosting 提供的應用程式輸出版本；具備可被快取以支援離線的資產集合。
- **Server-hosted Release**: 代表既有可在本機/伺服器上啟動的版本，作為 fallback 與相容性保留。
- **Browser Storage Record**: 代表保存於瀏覽器端的使用者資料（例如偏好、字典、工作狀態、書籤）；必須視為可被使用者檢視/修改的資料。
- **User-provided Content**: 代表任何可由使用者輸入、匯入或由儲存資料還原後再渲染的文字內容；屬於不可信輸入（untrusted input）。
- **Security Baseline Policy**: 代表針對前端輸出、腳本來源、與渲染策略的最低限度安全規範，用以降低 XSS/注入的可利用性與 impact。

## Success Criteria *(mandatory)*

<!--
  ACTION REQUIRED: Define measurable success criteria.
  These must be technology-agnostic and measurable.
-->

### Measurable Outcomes

- **SC-001**: 維運者可在不新增付費後端資源的情況下，將 browser-hosted 版本部署到 GitHub Pages 與 Azure Static Web Apps，並在驗收腳本中 100% 通過兩平台的「子路徑載入」與「deep link 重新整理」測試。
- **SC-002**: 在驗收流程中，使用者於完成一次線上載入後，可在離線狀態下完成至少 3 個核心操作（例如：關鍵字處理、內容檢視/搜尋、匯出），成功率達 95% 以上（以既定測試案例計算）。
- **SC-003**: 針對一組明確定義的惡意輸入測試集（例如含 HTML/JS 片段的關鍵字、匯入內容），所有案例均不發生腳本執行或 DOM 注入（通過率 100%）。
- **SC-004**: browser-hosted 版本的公開輸出中不得包含任何敏感憑證或金鑰類資料；以自動化掃描規則（maintainer 定義的 token pattern 清單）檢測結果為 0 命中。
- **SC-005**: 對一組定義好的 parity 測試案例（涵蓋所有使用者可見功能與行為），browser-hosted 與 Server 版在輸入/輸出/狀態變更上比較結果為 100% 一致。

## Assumptions

- 本功能不引入使用者登入/帳號系統；所有資料均屬本機（瀏覽器端）使用者自主管理。
- 本功能不依賴任何需要保密的第三方服務金鑰；若未來需要外部 API，必須另立功能並重新評估 trust boundary。
- 目標使用環境以現代瀏覽器為主；若瀏覽器不支援必要能力（例如離線快取或儲存空間），系統允許以清楚訊息降級（graceful degradation）。

## Dependencies

- GitHub Pages 與 Azure Static Web Apps（或同等 static hosting 平台）需支援 HTTPS 與靜態檔案發佈。
- 使用者端瀏覽器需允許離線快取與足夠的儲存空間以保存必要資料。
