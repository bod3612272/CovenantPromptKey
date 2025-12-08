# BibleData 使用指南與注意事項

> 本文件說明 `BibleData` DLL 的資料結構設計、正確使用方式，以及常見誤解的澄清。

---

## 📋 目錄

- [核心概念](#核心概念)
- [Book.Number 編號系統說明](#booknumber-編號系統說明)
- [正確使用方式](#正確使用方式)
- [常見誤解與錯誤範例](#常見誤解與錯誤範例)
- [API 查詢建議](#api-查詢建議)
- [單元測試注意事項](#單元測試注意事項)
 - [搜尋 API 速覽](#搜尋-api-速覽)

---

## 核心概念

`BibleData` 提供兩種存取聖經資料的方式：

| 類別 | 用途 | 查詢複雜度 |
|------|------|-----------|
| `BibleStaticData.Bible` | 原始資料存取 | O(n) |
| `BibleIndex` | 索引化快速查詢 | O(1) |

---

## Book.Number 編號系統說明

### ⚠️ 重要：這是**舊約/新約分開編號**的系統

聖經書卷的 `Number` 屬性採用**舊約新約獨立編號**的設計：

| 類別 | 書卷範圍 | Number 範圍 | 範例 |
|------|---------|------------|------|
| 舊約 | 創世記 ~ 瑪拉基書 | 1 ~ 39 | 創世記 = 1, 詩篇 = 19, 瑪拉基書 = 39 |
| 新約 | 馬太福音 ~ 啟示錄 | 1 ~ 27 | 馬太福音 = 1, 羅馬書 = 6, 啟示錄 = 27 |

### 為什麼這樣設計？

此設計遵循傳統聖經索引慣例：
- 舊約書卷編號 1-39
- 新約書卷編號 1-27
- 方便與其他聖經研究資源對照

### 這代表什麼？

```
創世記.Number  = 1  (舊約第 1 卷)
馬太福音.Number = 1  (新約第 1 卷)
```

**兩本書的 `Number` 都是 `1`，但它們是不同的書卷！**

---

## 正確使用方式

### ✅ 推薦：使用 `書卷名稱` 查詢（絕對唯一）

```csharp
using BibleData;

var index = new BibleIndex();

// 正確：透過書卷名稱查詢（名稱是唯一的）
var genesis = index.GetBook("創世記");
var matthew = index.GetBook("馬太福音");

// 正確：透過書卷名稱取得經文
var verse = index.GetVerse("創世記", 1, 1);
Console.WriteLine(verse?.Content); // "起初，神創造天地。"
```

### ✅ 使用 `BookNames` 列表遍歷所有書卷

```csharp
var index = new BibleIndex();

// 正確：遍歷所有書卷（保證順序和完整性）
foreach (var bookName in index.BookNames)
{
    var book = index.GetBook(bookName);
    Console.WriteLine($"{book.Name}: {book.Chapters.Count} 章");
}
```

### ⚠️ 謹慎：使用 `書卷編號` 查詢

由於舊約新約編號重疊，使用 `GetBook(int bookNumber)` 時需注意：

```csharp
var index = new BibleIndex();

// 注意：這只會返回**後加入索引的書卷**（通常是新約）
var book = index.GetBook(1);
// book.Name 可能是 "馬太福音" 而非 "創世記"！
```

**目前 `BibleIndex._bookByNumber` 的行為**：當舊約和新約有相同編號時，新約書卷會覆蓋舊約書卷。

---

## 常見誤解與錯誤範例

### ❌ 錯誤：假設 Book.Number 是 1-66 的流水號

```csharp
// 錯誤假設：期望 Number 1-66 對應全部 66 卷書
var books = BibleStaticData.Bible.Books;
var bookNumbers = books.Select(b => b.Number).Distinct().ToList();

// ❌ 這個測試會失敗！
Assert.AreEqual(66, bookNumbers.Count); 
// 實際只有 39 個不重複編號（1-39 出現兩次）
```

### ❌ 錯誤：用 Number 建立唯一索引

```csharp
// 錯誤：這會導致資料覆蓋
var bookByNumber = new Dictionary<int, Book>();
foreach (var book in BibleStaticData.Bible.Books)
{
    bookByNumber[book.Number] = book; // 新約會覆蓋舊約！
}
// bookByNumber[1] = 馬太福音（創世記被覆蓋了）
```

### ✅ 正確：用 Name 建立唯一索引

```csharp
// 正確：書卷名稱是唯一的
var bookByName = new Dictionary<string, Book>();
foreach (var book in BibleStaticData.Bible.Books)
{
    bookByName[book.Name] = book; // 所有 66 卷都會被正確保存
}
```

---

## API 查詢建議

### 查詢優先順序

| 優先順序 | 方法 | 說明 |
|---------|------|------|
| 1️⃣ 最推薦 | `GetBook(string bookName)` | 名稱唯一，不會混淆 |
| 2️⃣ 推薦 | `GetVerse(string bookName, int chapter, int verse)` | 明確指定書卷名稱 |
| 3️⃣ 謹慎使用 | `GetBook(int bookNumber)` | 可能返回非預期結果 |
| 4️⃣ 謹慎使用 | `GetVerse(int bookNumber, int chapter, int verse)` | 可能返回非預期結果 |

### 搜尋功能（不受 Number 影響）

```csharp
var index = new BibleIndex();

// 搜尋功能正常運作，返回完整的書卷名稱和位置
var results = index.SearchTop("愛", 10);
foreach (var v in results)
{
    Console.WriteLine($"{v.BookName} {v.ChapterNumber}:{v.VerseNumber}");
    Console.WriteLine($"  {v.Content}");
}
```

---

## 搜尋 API 速覽

- `Search(keyword, comparisonType)` / `SearchIgnoreCase(keyword)`: 全掃描搜尋，回傳所有符合經文。
- `SearchInBook(bookName, keyword)`: 限定書卷的搜尋。
- `SearchTop(keyword, topN, comparisonType)` / `SearchTopIgnoreCase(keyword, topN)`: 回傳前 N 筆即時結果。
- `SearchTopWithCancellation(keyword, topN, cancellationToken, comparisonType)`: 邊輸入邊搜尋時可用取消權杖中止迴圈。
- `SearchTopRanked(keyword, topN, comparisonType)`: 依出現位置與次數計分，回傳前 N 筆排名結果。
- `SearchByPrefix(prefix, topN)`: 經文以指定前綴開頭的結果。
- `SearchMultipleKeywords(keywords, topN, comparisonType)`: AND 條件，需同時包含多個關鍵字。
- `SearchAnyKeyword(keywords, topN, comparisonType)`: OR 條件，任一關鍵字即可。
- `GetRandomVerse(random?)`: 隨機取一節，可傳入自訂 `Random` 以重現結果。

> 提示：即時搜尋（`SearchTop*`）本質仍是線性掃描，建議在應用啟動時先建立一次 `BibleIndex` 並重複使用，避免重建索引的成本。

---

## 單元測試注意事項

### 這個測試**會失敗**（但這是預期行為）

```csharp
[Test]
public void BibleIndex_ShouldHaveUniqueBookNumbers()
{
    var books = BibleStaticData.Bible.Books;
    var bookNumbers = books.Select(b => b.Number).ToList();
    var uniqueBookNumbers = bookNumbers.Distinct().ToList();
    
    // ❌ 會失敗：66 本書卷只有 39 個不重複編號
    Assert.AreEqual(books.Count, uniqueBookNumbers.Count);
}
```

**原因**：這不是 bug，是舊約新約分開編號的設計。

### 正確的測試方式

```csharp
[Test]
public void BibleIndex_BookNamesShouldBeUnique()
{
    var books = BibleStaticData.Bible.Books;
    var bookNames = books.Select(b => b.Name).ToList();
    var uniqueBookNames = bookNames.Distinct().ToList();
    
    // ✅ 會通過：66 本書卷有 66 個不重複名稱
    Assert.AreEqual(books.Count, uniqueBookNames.Count);
}

[Test]
public void BibleIndex_AllBooks_ShouldBeAccessibleByName()
{
    var index = new BibleIndex();
    
    foreach (var bookName in index.BookNames)
    {
        var book = index.GetBook(bookName);
        Assert.IsNotNull(book);
        Assert.AreEqual(bookName, book.Name);
    }
}
```

### 驗證資料完整性的正確測試

```csharp
[Test]
public void BibleStaticData_ShouldHave66Books()
{
    var books = BibleStaticData.Bible.Books;
    Assert.AreEqual(66, books.Count);
}

[Test]
public void BibleStaticData_OldTestament_ShouldHave39Books()
{
    // 舊約前 39 本，編號 1-39
    var books = BibleStaticData.Bible.Books.Take(39).ToList();
    Assert.AreEqual(39, books.Count);
    Assert.AreEqual(1, books.First().Number);
    Assert.AreEqual(39, books.Last().Number);
}

[Test]
public void BibleStaticData_NewTestament_ShouldHave27Books()
{
    // 新約後 27 本，編號 1-27
    var books = BibleStaticData.Bible.Books.Skip(39).ToList();
    Assert.AreEqual(27, books.Count);
    Assert.AreEqual(1, books.First().Number);
    Assert.AreEqual(27, books.Last().Number);
}
```

---

## 總結

| 項目 | 說明 |
|------|------|
| `Book.Number` | 舊約新約**分開編號**，不是 1-66 流水號 |
| 唯一識別符 | 請使用 `Book.Name`（書卷名稱） |
| 推薦查詢方式 | `GetBook(string bookName)` |
| `_bookByNumber` 行為 | 編號重疊時，後者覆蓋前者 |

### 核心原則

> **永遠使用書卷名稱作為主要識別符，而非書卷編號。**

---

## 版本資訊

- 文件版本：1.1
- 最後更新：2025-12-08
- 適用於：BibleData DLL

