namespace CovenantPromptKey.Models.Bible;

/// <summary>
/// 聖經顯示設定模型
/// </summary>
public class BibleSettings
{
    /// <summary>
    /// 字形選擇
    /// </summary>
    public FontFamily FontFamily { get; set; } = FontFamily.MicrosoftJhengHei;

    /// <summary>
    /// 字體大小 (px)，範圍 12-24
    /// </summary>
    public int FontSize { get; set; } = 16;

    /// <summary>
    /// 文字顏色
    /// </summary>
    public TextColor TextColor { get; set; } = TextColor.Black;

    /// <summary>
    /// 背景顏色
    /// </summary>
    public BackgroundColor BackgroundColor { get; set; } = BackgroundColor.White;

    /// <summary>
    /// 是否自動換行
    /// </summary>
    public bool AutoWrap { get; set; } = true;
}
