
# CovenantPromptKey 系統設計文件 - 完整需求規格

**專案名稱:** CovenantPromptKey (去識別化工具)  
**版本:** 1.0.0  
**目標作業系統:** Windows 11  
**技術棧:** .NET 10, Blazor (Server-side rendering via Local Kestrel)

---

## 1. 專案概述

本工具旨在為開發者提供一個安全的「AI 對話中介層」。透過本地端執行的 Web 介面,將機敏關鍵字自動遮罩(Masking)後再發送給 AI,並將 AI 回覆的遮罩內容還原(Unmasking)。系統強調「高情緒價值」的視覺反饋與極致的文字排版優化。

### 核心價值主張

| 功能面向 | 價值描述 |
|---------|---------|
| **隱私保護** | 機敏資訊不外洩至 AI 服務 |
| **使用者體驗** | 高情緒價值的視覺反饋 |
| **文字優化** | 自動中英文排版美化 |
| **本地執行** | 無需網路,資料不上傳 |

---

## 2. 架構與技術棧

### 2.1 技術選型

| 技術層 | 選用技術 | 理由 |
|--------|---------|------|
| **Framework** | .NET 10 SDK | 最新 LTS 版本,效能優異 |
| **UI Framework** | Blazor Web App (Interactive Server) | 本地響應式互動,無需前後端分離 |
| **Hosting** | ASP.NET Core Kestrel | 自託管主控台應用程式 |
| **Data Storage** | CSV (Local File System) | 輕量級,易於編輯與版本控制 |
| **Styling** | Tailwind CSS / MudBlazor | 快速 UI 元件開發 + 自訂 CSS 動畫 |

### 2.2 應用程式生命週期

**啟動流程:**

1. **Port Scavenger (連接埠掃描器):** 應用程式啟動時,使用 `System.Net.Sockets` 偵測 localhost 上的可用 Port (從 5000 開始)
2. **Browser Launch (瀏覽器啟動):** 確定 Port 後,透過 `System.Diagnostics.Process.Start` 自動呼叫 Windows 預設瀏覽器開啟 `http://localhost:{port}`
3. **Config Load (設定載入):** 初始化時載入 `dictionary.csv` 至記憶體 (`Dictionary<string, string>`)

---

## 3. 核心功能規格

### 3.1 去識別化引擎 (De-identification Engine)

**輸入 (Input):**
- 原始文字(包含 Markdown, Code Blocks)

**處理流程 (Process):**

1. **Segmentation (文本切分):** 將文本依據 Markdown Code Block (` ``` ... ``` `) 與 Inline Code (`` ` ... ` ``) 進行切分,**僅針對非程式碼區域**進行處理

2. **Replacement (關鍵字替換):** 掃描 CSV 定義的 `SensitiveKey`,替換為 `SafeKey`

**輸出 (Output):**
- 處理後的安全文本

**關鍵技術要點:**

| 處理項目 | 技術策略 |
|---------|---------|
| **Markdown 保護** | 解析並標記 Code Block、Inline Code、URL 區域 |
| **最長匹配優先** | 按關鍵字長度降序處理,避免「武科電」被「台積」誤判 |
| **邊界檢測** | 檢查前後字元是否為中文/英文/數字,確保完整匹配 |
| **重疊避免** | 記錄已處理範圍,防止重複替換 |

### 3.2 還原引擎 (Re-identification Engine)

**輸入 (Input):**
- AI 回覆的文本(包含 `SafeKey`)

**處理流程 (Process):**
- 反向查找字典,將 `SafeKey` 還原為 `SensitiveKey`

**輸出 (Output):**
- 還原後的原始文本

### 3.3 智慧排版優化 (Smart Formatting)

**功能描述:**
在「中文」與「英文/數字」交界處插入半形空白 (` `)

**安全限制條件:**

| 限制項目 | 說明 |
|---------|------|
| **Markdown 語法保護** | 嚴禁修改 `[Link]`, `**Bold**`, `# Header` 內部的連續性 |
| **URL 保護** | 嚴禁在 URL 或路徑 (如 `C:\Windows`) 中插入空白 |
| **Code Block 保護** | 嚴禁修改程式碼區塊內容 |
| **標點符號處理** | 句點、逗號等標點符號前後不插入空白 |

**範例:**

| 原始文字 | 優化後文字 |
|---------|-----------|
| `這是Windows系統` | `這是 Windows 系統` |
| `版本號3.14.159` | `版本號 3.14.159` |
| `[連結](https://example.com)` | `[連結](https://example.com)` (不變) |

### 3.4 設定檔管理 (Configuration Management)

**檔案格式:** `dictionary.csv`

```
SensitiveKey,SafeKey,ColorHex
Eason,User_A,#FF5733
ProjectX,Project_Omega,#33FF57
武科電,T-Company,#3498DB
```

**欄位說明:**

| 欄位名稱 | 資料型別 | 說明 |
|---------|---------|------|
| **SensitiveKey** | string | 機敏關鍵字(如「武科電」) |
| **SafeKey** | string | 安全替代字(如「T-Company」) |
| **ColorHex** | string | 高亮顏色(如「#FF5733」,若空白則自動生成) |

**CRUD 介面功能:**

- **新增:** 輸入三個欄位,即時寫入 CSV
- **修改:** 選擇現有項目,編輯後儲存
- **刪除:** 選擇項目後移除,並更新 CSV
- **即時生效:** 所有變更立即反映至記憶體字典

---

## 4. UI/UX 設計

### 4.1 版面配置 (Layout)

**Split View (分割視圖):**

| 區域 | 功能 | 位置 |
|------|------|------|
| **原始/還原區** | 顯示原始文本或還原後文本 | 左側 50% |
| **去識別化/AI 溝通區** | 顯示遮罩後的安全文本 | 右側 50% |
| **Dashboard (儀表板)** | 顯示關鍵字統計數據 | 頂部 |

**Dashboard 統計項目:**

- 當前偵測到的關鍵字總數
- 本次替換的關鍵字數量
- 字典中定義的關鍵字總數

### 4.2 視覺反饋 (情緒價值設計)

**Highlighting (高亮顯示):**

- 使用 `MarkupString` 渲染文本
- 對每個關鍵字包裹 `<span style="background-color: {ColorHex}">`
- 支援自訂顏色,提升視覺辨識度

**Animations (動畫效果):**

| 觸發時機 | 動畫效果 | 持續時間 |
|---------|---------|---------|
| **轉換成功** | 全螢幕微型紙花 (Confetti) | 2 秒 |
| **統計數字更新** | 數字跳動效果 (Counting up) | 1 秒 |
| **關鍵字高亮** | 螢光流動效果 | 持續 |
| **複製成功** | Toast 通知 "Copied! ✨" | 3 秒 |

### 4.3 剪貼簿整合 (Clipboard Integration)

由於無法直接使用瀏覽器擴充,設計 **"One-Click Copy/Paste"** 按鈕:

**功能按鈕:**

| 按鈕名稱 | 功能描述 |
|---------|---------|
| **[貼上並轉換]** | 讀取剪貼簿 → 執行去識別化 → 渲染結果 |
| **[複製結果]** | 將結果寫入剪貼簿 → 顯示 Toast 通知 |
| **[還原並複製]** | 執行還原 → 寫入剪貼簿 |

---

## 5. 資料結構

### 5.1 CSV 資料模型

**KeywordMapping 類別:**

| 屬性名稱 | 資料型別 | 範例值 | 說明 |
|---------|---------|--------|------|
| **SensitiveKey** | string | "武科電" | 機敏關鍵字 |
| **SafeKey** | string | "T-Company" | 安全替代字 |
| **ColorHex** | string | "#F0A3FF" | 高亮顏色(若空白則自動生成) |

### 5.2 受保護區域資料結構

**ProtectedRange 類別:**

| 屬性名稱 | 資料型別 | 說明 |
|---------|---------|------|
| **Start** | int | 起始位置索引 |
| **End** | int | 結束位置索引 |
| **Type** | enum | CodeBlock / InlineCode / URL |

---

## 6. 限制與安全性

### 6.1 解析安全性 (Parsing Safety)

**技術策略:**

| 策略 | 實作方式 |
|------|---------|
| **State Pattern** | 使用狀態機追蹤當前解析狀態 |
| **Regex 負向斷言** | 使用 Negative Lookbehind/Lookahead 確保不破壞結構 |
| **範圍標記** | 預先標記所有受保護區域,替換時跳過 |

### 6.2 效能考量 (Performance)

**最佳化策略:**

| 文本大小 | 策略 | 預期效能 |
|---------|------|---------|
| **< 1k 字** | 直接字串操作 | < 10ms |
| **1k-10k 字** | StringBuilder | < 50ms |
| **> 10k 字** | StringBuilder + 分段處理 | < 200ms |

### 6.3 邊界條件處理

**需特別處理的情況:**

1. **英文單字內包含關鍵字:** `AbcMjokdefg` 不應替換為 `AbcMMMMdefg`
2. **中文詞組部分匹配:** `武科電路板` 不應替換為 `T-Company路板`，因為是指電路板，若不確定要列出提示使用者
3. **標點符號後的關鍵字:** `.Mjok` 應替換為 `.MMMM`
3. **標點符號後的關鍵字:** `.Mjok.Task` 應替換為 `.MMMM.Task`
4. **URL 中的關鍵字:** `https://Mjok.com/Mjok` 應替換 `https://MMMM.com/MMMM`
5. **Markdown 連結文字:** `[Mjok](url)` 應替換為 `[MMMM](url)`

---

## 7. 使用流程

### 7.1 典型工作流程

1. **啟動應用程式** → 自動開啟瀏覽器
2. **貼上原始文本** → 點擊「去識別化」按鈕
3. **檢視高亮結果** → 確認關鍵字已正確替換
4. **複製安全文本** → 貼至 AI 對話介面
5. **取得 AI 回覆** → 貼回 CovenantPromptKey
6. **點擊還原** → 取得包含原始關鍵字的回覆
7. **複製最終結果** → 完成工作流程

### 7.2 設定管理流程

1. **開啟設定頁面** → 檢視現有關鍵字清單
2. **新增關鍵字** → 輸入機敏字與替代字
3. **選擇顏色** → 自訂高亮顏色(可選)
4. **儲存變更** → 即時寫入 CSV 檔案
5. **測試驗證** → 返回主頁面測試新關鍵字

---

## 8. 非功能性需求

### 8.1 效能需求

| 指標 | 目標值 |
|------|--------|
| **啟動時間** | < 3 秒 |
| **處理 1k 字文本** | < 50ms |
| **UI 響應時間** | < 100ms |
| **記憶體使用** | < 100MB |

### 8.2 可用性需求

- **直覺操作:** 無需閱讀說明即可上手
- **視覺反饋:** 每個操作都有明確的視覺回饋
- **錯誤處理:** 友善的錯誤訊息與復原建議

### 8.3 可維護性需求

- **模組化設計:** 引擎、UI、資料層分離
- **單元測試覆蓋率:** > 80%
- **文件完整性:** 所有公開 API 都有 XML 註解



