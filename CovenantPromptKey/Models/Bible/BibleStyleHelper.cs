namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經樣式輔助類別
/// </summary>
public static class BibleStyleHelper
{
    /// <summary>
    /// 取得字形對應的 CSS font-family 值
    /// </summary>
    /// <param name="font">字形列舉</param>
    /// <returns>CSS font-family 字串</returns>
    public static string GetFontFamilyCss(FontFamily font) => font switch
    {
        FontFamily.DFKai => "'DFKai-SB', '標楷體', serif",
        FontFamily.MicrosoftJhengHei => "'Microsoft JhengHei', '微軟正黑體', sans-serif",
        _ => "'Microsoft JhengHei', sans-serif"
    };

    /// <summary>
    /// 取得文字顏色的十六進位色碼
    /// </summary>
    /// <param name="color">文字顏色列舉</param>
    /// <returns>十六進位色碼字串</returns>
    public static string GetTextColorHex(TextColor color) => color switch
    {
        TextColor.Black => "#000000",
        TextColor.DarkGray => "#333333",
        TextColor.LightGray => "#666666",
        _ => "#000000"
    };

    /// <summary>
    /// 取得背景顏色的十六進位色碼
    /// </summary>
    /// <param name="color">背景顏色列舉</param>
    /// <returns>十六進位色碼字串</returns>
    public static string GetBackgroundColorHex(BackgroundColor color) => color switch
    {
        BackgroundColor.White => "#FFFFFF",
        BackgroundColor.Beige => "#FFF8DC",
        BackgroundColor.LightGray => "#F5F5F5",
        BackgroundColor.LightGreen => "#F0FFF0",
        BackgroundColor.NightMode => "#000000",
        _ => "#FFFFFF"
    };

    /// <summary>
    /// 根據背景顏色取得適合的文字顏色
    /// </summary>
    /// <param name="bg">背景顏色列舉</param>
    /// <returns>適合的文字顏色十六進位色碼</returns>
    public static string GetTextColorForBackground(BackgroundColor bg) =>
        bg == BackgroundColor.NightMode ? "#FFFFFF" : GetTextColorHex(TextColor.Black);
}
