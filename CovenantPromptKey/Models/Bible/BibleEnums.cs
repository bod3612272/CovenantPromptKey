namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 字形列舉
/// </summary>
public enum FontFamily
{
    /// <summary>標楷體</summary>
    DFKai,
    /// <summary>微軟正黑體</summary>
    MicrosoftJhengHei
}

/// <summary>
/// 文字顏色列舉
/// </summary>
public enum TextColor
{
    /// <summary>全黑 (#000000)</summary>
    Black,
    /// <summary>深灰 (#333333)</summary>
    DarkGray,
    /// <summary>淺灰 (#666666)</summary>
    LightGray
}

/// <summary>
/// 背景顏色列舉
/// </summary>
public enum BackgroundColor
{
    /// <summary>白色 (#FFFFFF)</summary>
    White,
    /// <summary>米色 (#FFF8DC)</summary>
    Beige,
    /// <summary>淺灰 (#F5F5F5)</summary>
    LightGray,
    /// <summary>淺綠 (#F0FFF0)</summary>
    LightGreen,
    /// <summary>夜間模式 (黑底白字)</summary>
    NightMode
}

/// <summary>
/// 導出風格列舉
/// </summary>
public enum ExportStyle
{
    /// <summary>
    /// 風格 1：(馬可福音 1:1) 經文內容
    /// </summary>
    Style1,

    /// <summary>
    /// 風格 2：書卷 ( 第 X 章 Y ~ Z 節 ) + 逐行經文
    /// </summary>
    Style2,

    /// <summary>
    /// 風格 3：《書卷》( 第 X 章 Y ~ Z 節 ) + 連續段落
    /// </summary>
    Style3
}
