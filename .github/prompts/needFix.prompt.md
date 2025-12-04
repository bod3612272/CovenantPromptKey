
說明測試方式：
設定 AAAA 為關鍵字，需要遮罩轉換成 VVVV。


目前有以下問題:

1. 原文輸入後，檢查到整個原文有三個位置出現 AAAA 關鍵字，但在畫面中央的偵測到的關鍵字內容卻顯示 AAA 有 3 處，沒有用編號標示是哪三處。這樣會造成不符合 SPEC 要求的，可以讓使用者點選哪些關鍵字要轉換，哪些不轉換。

需要修正的項目
1. 必須標記顏色或是框線，讓使用者可以清楚知道 AAAA 關鍵字出現在哪三個位置。
2. 偵測到的關鍵字內容，必須有數字，像是第幾行第幾個字，讓使用者可以清楚知道是哪三處 AAAA 關鍵字。
3. 中間控制區列出的關鍵字清單內要顯示位置資訊，例如：AAAA (行1, 字5)、AAAA (行2, 字10)、AAAA (行3, 字15)。
4. 使用者可以點選關鍵字清單內的項目，來切換該位置的關鍵字是否要遮罩轉換。
5. 當使用者點選關鍵字清單內的項目時，原文中對應位置的關鍵字也要有明顯的選取狀態變化，例如變色或是加框線。

---

## 錯誤日誌記錄 (Error Log Records)

以下為測試過程中 Debug Log 頁面記錄的錯誤訊息：

### JavaScript Interop Prerender Error (2025-12-04)

```
08:56:47.548 Error
載入字典時發生錯誤: JavaScript interop calls cannot be issued at this time. This is because the component is being statically rendered. When prerendering is enabled, JavaScript interop calls can only be performed during the OnAfterRenderAsync lifecycle method.

08:56:47.548 Error
Failed to load session: JavaScript interop calls cannot be issued at this time. This is because the component is being statically rendered. When prerendering is enabled, JavaScript interop calls can only be performed during the OnAfterRenderAsync lifecycle method.

08:55:08.322 Error
載入字典時發生錯誤: JavaScript interop calls cannot be issued at this time. This is because the component is being statically rendered. When prerendering is enabled, JavaScript interop calls can only be performed during the OnAfterRenderAsync lifecycle method.
```

**問題分析**: 在 Blazor Server 預渲染模式下，`OnInitializedAsync` 生命週期方法中呼叫了 JavaScript interop (如 sessionStorage/localStorage 存取)。這些呼叫必須移至 `OnAfterRenderAsync` 方法中執行，因為在預渲染階段尚未建立與瀏覽器的連線。

**修正方案**: 將所有 JS interop 呼叫移至 `OnAfterRenderAsync` 生命週期方法，並使用 `firstRender` 參數確保只在首次渲染後執行一次。




