# Component Contracts: 互動式關鍵字替換介面

**Branch**: `001-interactive-keyword-replace` | **Date**: 2025-12-03

本文件定義 Blazor 元件的契約與責任範圍。

---

## 頁面層級元件 (Page Components)

### 1. MaskRestorePage.razor

主要替換與還原頁面，採用頁籤切換。

```razor
@page "/mask-restore"
@page "/"

@* 元件責任 *@
@* - 提供頁籤切換介面 (遮罩/還原) *@
@* - 管理當前工作模式狀態 *@
@* - 協調三欄式佈局元件 *@
```

**Props/Parameters**: 無

**State**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| CurrentMode | WorkMode | 當前模式 (Mask/Restore) |

**Events**:
| 名稱 | 觸發時機 |
|------|---------|
| OnModeChanged | 切換頁籤時 |

---

### 2. SettingsPage.razor

關鍵字字典管理頁面。

```razor
@page "/settings"

@* 元件責任 *@
@* - 顯示關鍵字列表 *@
@* - 提供新增/編輯/刪除介面 *@
@* - 處理 CSV 匯入/匯出 *@
```

**子元件**:
- `KeywordList`
- `KeywordForm`
- `CsvImportExport`

---

### 3. DebugLogPage.razor

除錯日誌頁面。

```razor
@page "/debug-log"

@* 元件責任 *@
@* - 顯示所有日誌項目 *@
@* - 提供複製全部功能 *@
@* - 自動捲動至最新 *@
```

---

## 佈局元件 (Layout Components)

### 4. ThreeColumnLayout.razor

三欄式佈局容器。

```razor
@* 元件責任 *@
@* - 提供響應式三欄結構 *@
@* - 各欄獨立捲動 *@
@* - 支援拖曳調整欄寬（可選） *@

<div class="three-column-layout">
    <div class="column source-column">
        @SourceContent
    </div>
    <div class="column control-column">
        @ControlContent
    </div>
    <div class="column result-column">
        @ResultContent
    </div>
</div>
```

**Parameters**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| SourceContent | RenderFragment | 左欄內容 |
| ControlContent | RenderFragment | 中欄內容 |
| ResultContent | RenderFragment | 右欄內容 |

---

## 功能元件 (Feature Components)

### 5. SourceTextEditor.razor

原文輸入與顯示元件。

```razor
@* 元件責任 *@
@* - 接收使用者貼上/輸入的文本 *@
@* - 顯示高亮標記的關鍵字 *@
@* - 支援點擊高亮區切換狀態 *@
@* - 顯示行號 *@
@* - 支援平滑滾動至指定位置 *@
```

**Parameters**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| Text | string | 原始文本 |
| DetectedKeywords | List<DetectedKeyword> | 偵測結果 |
| IsEditable | bool | 是否可編輯 |

**Events**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| OnTextChanged | EventCallback<string> | 文本變更時 |
| OnKeywordClicked | EventCallback<KeywordOccurrence> | 點擊高亮區時 |

**Methods** (透過 @ref 呼叫):
| 名稱 | 說明 |
|------|------|
| ScrollToLine(int lineNumber) | 滾動至指定行 |

---

### 6. KeywordControlPanel.razor

關鍵字控制面板元件。

```razor
@* 元件責任 *@
@* - 列出所有偵測到的關鍵字 *@
@* - 顯示出現次數與警示狀態 *@
@* - 提供勾選/取消勾選功能 *@
@* - 支援展開查看行號位置 *@
@* - 提供全選/全不選/反選功能 *@
```

**Parameters**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| DetectedKeywords | List<DetectedKeyword> | 偵測結果 |

**Events**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| OnSelectionChanged | EventCallback<DetectedKeyword> | 勾選狀態變更時 |
| OnLineClicked | EventCallback<int> | 點擊行號時 |
| OnSelectAll | EventCallback | 全選時 |
| OnDeselectAll | EventCallback | 全不選時 |
| OnInvertSelection | EventCallback | 反選時 |

---

### 7. ResultViewer.razor

結果顯示元件。

```razor
@* 元件責任 *@
@* - 顯示處理後的結果文本 *@
@* - 提供複製按鈕 *@
@* - 顯示處理統計 *@
```

**Parameters**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| ResultText | string | 結果文本 |
| Statistics | ResultStatistics | 統計資訊 |

**Events**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| OnCopy | EventCallback | 複製按鈕點擊時 |

---

### 8. StatisticsDashboard.razor

即時統計儀表板元件。

```razor
@* 元件責任 *@
@* - 顯示偵測總數 *@
@* - 顯示已選擇數量 *@
@* - 顯示警示數量 *@
@* - 即時更新 *@
```

**Parameters**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| DetectedKeywords | List<DetectedKeyword> | 偵測結果 |

---

### 9. ConfirmationDialog.razor

確認對話框元件。

```razor
@* 元件責任 *@
@* - 顯示替換預覽資訊 *@
@* - 提供確認/取消按鈕 *@
```

**Parameters**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| Title | string | 對話框標題 |
| Message | string | 訊息內容 |
| ReplacementPreview | List<ReplacementDetail> | 替換預覽 |
| IsVisible | bool | 是否顯示 |

**Events**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| OnConfirm | EventCallback | 確認時 |
| OnCancel | EventCallback | 取消時 |

---

### 10. KeywordForm.razor

關鍵字編輯表單元件。

```razor
@* 元件責任 *@
@* - 提供新增/編輯關鍵字的表單 *@
@* - 驗證輸入 *@
@* - 顏色選擇器 *@
```

**Parameters**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| Mapping | KeywordMapping? | 編輯時的現有映射 |
| Mode | FormMode | 新增/編輯模式 |

**Events**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| OnSave | EventCallback<KeywordMapping> | 儲存時 |
| OnCancel | EventCallback | 取消時 |

---

### 11. ColorPicker.razor

顏色選擇器元件。

```razor
@* 元件責任 *@
@* - 顯示 8-10 種預設顏色 *@
@* - 允許選擇顏色 *@
```

**Parameters**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| SelectedColor | string | 當前選擇的顏色 |
| Colors | string[] | 可選顏色列表 |

**Events**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| OnColorSelected | EventCallback<string> | 選擇顏色時 |

---

### 12. CsvImportExport.razor

CSV 匯入/匯出元件。

```razor
@* 元件責任 *@
@* - 提供匯出按鈕 *@
@* - 提供檔案選擇匯入介面 *@
@* - 顯示匯入結果/錯誤 *@
@* - 提供合併/覆蓋選項 *@
```

**Parameters**: 無

**Events**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| OnImportComplete | EventCallback<CsvImportResult> | 匯入完成時 |

---

### 13. LogViewer.razor

日誌檢視元件。

```razor
@* 元件責任 *@
@* - 顯示日誌列表 *@
@* - 顏色區分日誌等級 *@
@* - 自動滾動至最新 *@
@* - 提供複製全部按鈕 *@
```

**Parameters**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| Logs | IReadOnlyList<LogEntry> | 日誌列表 |

**Events**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| OnCopyAll | EventCallback | 複製全部時 |
| OnClear | EventCallback | 清空時 |

---

### 14. ToastNotification.razor

短暫提示訊息元件。

```razor
@* 元件責任 *@
@* - 顯示操作結果提示 *@
@* - 自動消失 *@
@* - 支援不同樣式（成功/警告/錯誤） *@
```

**Parameters**:
| 名稱 | 類型 | 說明 |
|------|------|------|
| Message | string | 訊息內容 |
| Type | ToastType | 提示類型 |
| Duration | int | 顯示時長 (ms) |

---

## 元件層級結構

```
App.razor
└── MainLayout.razor
    ├── NavMenu.razor
    └── @Body
        ├── MaskRestorePage.razor
        │   ├── TabControl (遮罩/還原)
        │   └── ThreeColumnLayout.razor
        │       ├── SourceTextEditor.razor
        │       ├── KeywordControlPanel.razor
        │       │   └── StatisticsDashboard.razor
        │       └── ResultViewer.razor
        │
        ├── SettingsPage.razor
        │   ├── KeywordList.razor
        │   ├── KeywordForm.razor
        │   │   └── ColorPicker.razor
        │   └── CsvImportExport.razor
        │
        └── DebugLogPage.razor
            └── LogViewer.razor

(Global)
├── ConfirmationDialog.razor
└── ToastNotification.razor
```

---

## CSS 類別命名規範

採用 BEM (Block-Element-Modifier) 命名法：

```css
/* Block */
.keyword-panel { }

/* Element */
.keyword-panel__item { }
.keyword-panel__count { }
.keyword-panel__warning { }

/* Modifier */
.keyword-panel__item--selected { }
.keyword-panel__item--warning { }

/* 高亮顏色 */
.highlight--color-1 { background-color: #FF6B6B; }
.highlight--color-2 { background-color: #4ECDC4; }
```

---

## 快捷鍵定義 (FR-015)

| 快捷鍵 | 功能 | 適用頁面 |
|--------|------|---------|
| Ctrl+V | 貼上並偵測 | 遮罩/還原 |
| Ctrl+A | 全選關鍵字 | 遮罩 |
| Ctrl+Shift+A | 全不選 | 遮罩 |
| Ctrl+Enter | 執行替換/還原 | 遮罩/還原 |
| Ctrl+C | 複製結果 | 遮罩/還原 |
| Escape | 取消/關閉對話框 | 全域 |
