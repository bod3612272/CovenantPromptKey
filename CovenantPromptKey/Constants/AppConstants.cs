namespace CovenantPromptKey.Constants;

/// <summary>
/// 應用程式常數設定
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// 最大關鍵字數量限制 (FR-032)
    /// </summary>
    public const int MaxKeywordCount = 500;

    /// <summary>
    /// 最大文本長度限制 (FR-033)
    /// </summary>
    public const int MaxTextLength = 100_000;

    /// <summary>
    /// 最大日誌行數 (Story 6)
    /// </summary>
    public const int MaxLogEntries = 5_000;

    /// <summary>
    /// 關鍵字最大長度
    /// </summary>
    public const int MaxKeywordLength = 200;

    /// <summary>
    /// 預設高亮顏色調色盤 (FR-036)
    /// </summary>
    public static readonly string[] DefaultColors =
    [
        "#FF6B6B", // 珊瑚紅
        "#4ECDC4", // 青綠
        "#45B7D1", // 天藍
        "#96CEB4", // 薄荷綠
        "#FFEAA7", // 檸檬黃
        "#DDA0DD", // 梅紫
        "#98D8C8", // 淺綠
        "#F7DC6F", // 金黃
        "#BB8FCE", // 淡紫
        "#85C1E9"  // 淺藍
    ];

    /// <summary>
    /// SessionStorage 鍵名
    /// </summary>
    public static class StorageKeys
    {
        public const string WorkSession = "cpk_work_session";
        public const string KeywordDictionary = "cpk_keyword_dict";
    }
}
