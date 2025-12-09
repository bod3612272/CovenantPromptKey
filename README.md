# CovenantPromptKey

<div align="center"> 
  
  <img src="CovenantPromptKey/wwwroot/favicon.png" alt="CovenantPromptKey Logo" width="128" height="128">
  
  **互動式關鍵字替換介面 | 聖經查詢系統**
  
  [![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
  [![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor)](https://blazor.net/)
  [![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)
  [![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?logo=windows)](https://www.microsoft.com/windows)

</div>

---

## 📖 Overview

**CovenantPromptKey** 是一款專為保護敏感資訊而設計的桌面網頁應用程式，讓您在使用 AI 服務時能夠安全地遮罩機密內容。透過直覺的關鍵字映射系統，您可以將公司名稱、產品代號、個人資訊等敏感內容替換為安全的替代詞，待 AI 回覆後再將替代詞還原為原始內容。

此外，本應用程式亦整合了**聖經查詢系統**，提供聖經經文搜尋、閱讀及互動遊戲功能。

---

## ✨ Features

### 🔐 互動式關鍵字替換介面

- **智慧關鍵字偵測**：使用高效 Aho-Corasick 演算法進行多模式字串匹配
- **三欄式互動佈局**：原文 / 控制面板 / 結果區，支援即時視覺化標示
- **精準控制**：可選擇性勾選欲替換的關鍵字，避免誤傷內容
- **上下文語境警示**：自動偵測可能破壞中文詞組的替換並顯示警示
- **字典管理**：支援手動新增/編輯/刪除關鍵字，CSV 格式匯入/匯出
- **AI 回應還原**：將遮罩詞彙反向還原為原始機敏關鍵字
- **工作階段保存**：自動儲存工作狀態，避免刷新頁面導致資料遺失
- **Debug Log 追蹤**：完整的操作日誌記錄，方便問題排查

![CovenantPromptKey：您的 AI 對話隱私守護神](Image/imagePrompt.png)

### 📖 聖經查詢系統

- **經文搜尋**：支援關鍵字搜尋、模糊搜尋、多關鍵字 AND 搜尋
- **聖經閱讀**：書卷導航、章節跳轉、上下章切換
- **閱讀設定**：自訂字形、字體大小、文字顏色、背景顏色
- **書籤管理**：自動記錄最近閱讀的經文（最多 10 筆）
- **經文導出**：支援三種 Markdown 格式導出
- **經文猜猜遊戲**：透過遊戲熟悉聖經經文出處，含錯題複習功能

![CovenantPromptKey 聖經查詢系統功能一覽](Image/imageBible.png) 

---

## 🛠️ Technology Stack

| 技術 | 版本/說明 |
|------|----------|
| **Framework** | .NET 10.0 (Preview) |
| **UI Framework** | Blazor Server (Interactive Server Components) |
| **CSS Framework** | Bootstrap 5 |
| **CSV Processing** | CsvHelper 33.1.0 |
| **Markdown Parsing** | Markdig 0.44.0 |
| **Bible Data** | BibleData DLL (.NET Standard 2.1) |
| **Storage** | Browser localStorage / sessionStorage |

---

## 📦 Installation

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Preview)
- Windows 作業系統
- 現代瀏覽器（Chrome、Edge 或 Firefox 最新版）

### Build from Source

```bash
# Clone the repository
git clone https://github.com/bod3612272/CovenantPromptKey.git
cd CovenantPromptKey

# Restore dependencies
dotnet restore

# Run the application
cd CovenantPromptKey
dotnet run
```

應用程式啟動後會自動開啟預設瀏覽器。

### Publish Single File Executable

```bash
# Build release version (single file executable)
dotnet publish -c Release
```

發布後的單一執行檔位於 `bin/Release/net10.0/win-x64/publish/`。

---

## 🚀 Quick Start

### 關鍵字替換功能

1. **新增關鍵字**：前往「設定」頁面，新增機敏詞與對應的安全替代詞
2. **貼上文本**：在「替換與還原」頁面的「遮罩」頁籤貼上包含機敏資訊的文本
3. **選擇關鍵字**：系統會自動偵測並列出所有關鍵字，您可勾選欲替換的項目
4. **執行替換**：點擊「執行替換」按鈕，確認後獲得遮罩後的安全文本
5. **還原回應**：將 AI 回應貼入「還原」頁籤，系統會自動將替代詞還原

### 聖經查詢功能

1. 點選側邊欄「聖經」展開子選單
2. 選擇「聖經查詢」進行經文搜尋
3. 選擇「聖經閱讀」瀏覽特定書卷章節
4. 選擇「聖經遊戲」測試您對經文的熟悉度

---

## 📁 Project Structure

```
CovenantPromptKey/
├── CovenantPromptKey/           # Main Application
│   ├── Components/              # Blazor UI Components
│   │   ├── Layout/              # Layout Components
│   │   ├── Pages/               # Page Components
│   │   └── Shared/              # Shared Components
│   ├── Constants/               # Application Constants
│   ├── Models/                  # Domain Models
│   │   ├── Bible/               # Bible-related Models
│   │   └── Results/             # Operation Result Models
│   ├── Services/                # Business Logic Services
│   │   ├── Interfaces/          # Service Contracts
│   │   └── Implementations/     # Service Implementations
│   └── wwwroot/                 # Static Assets
├── CovenantPromptKey.Tests/     # xUnit Tests
├── CovenantPromptKey.NUnitTests/# NUnit Tests
├── Dll/                         # External DLL References
└── specs/                       # Feature Specifications
```

---

## 🔒 Privacy & Security

- **本機資料儲存**：所有關鍵字字典資料僅儲存在您的瀏覽器本機（localStorage）
- **零外部傳輸**：沒有任何資料會被傳送至外部伺服器
- **工作階段隔離**：工作階段資料儲存於 sessionStorage，關閉瀏覽器即清除
- **完全離線運作**：應用程式可完全離線運作，無需網路連線

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📋 Requirements

### System Requirements

- **Operating System**: Windows 10/11
- **Browser**: Chrome 90+, Edge 90+, Firefox 90+
- **Display**: 1366px 至 4K 解析度

### Performance Goals

| 指標 | 目標 |
|------|------|
| 1,000 字偵測 | < 200ms |
| UI 互動響應 | < 150ms |
| 還原處理 | < 100ms |
| 搜尋結果顯示 | < 2s |

---

## 🤝 Contributing

歡迎貢獻！請遵循以下步驟：

1. Fork 本專案
2. 建立功能分支 (`git checkout -b feature/amazing-feature`)
3. 提交變更 (`git commit -m 'Add some amazing feature'`)
4. 推送至分支 (`git push origin feature/amazing-feature`)
5. 開啟 Pull Request

---

## 📄 License

本專案採用 MIT 授權條款。詳見 [LICENSE.md](LICENSE.md)。

---

## 🙏 Acknowledgements

- [.NET](https://dotnet.microsoft.com/) - Microsoft 開源開發平台
- [Blazor](https://blazor.net/) - 互動式網頁 UI 框架
- [Bootstrap](https://getbootstrap.com/) - CSS 框架
- [CsvHelper](https://joshclose.github.io/CsvHelper/) - .NET CSV 處理函式庫
- [Markdig](https://github.com/xoofx/markdig) - Markdown 解析器

---

## 📞 Contact

- **GitHub Issues**: [提交問題或建議](https://github.com/bod3612272/CovenantPromptKey/issues)
- **Repository**: [bod3612272/CovenantPromptKey](https://github.com/bod3612272/CovenantPromptKey)

---

<div align="center">
  <sub>Built with ❤️ using .NET and Blazor</sub>
</div>
