namespace CovenantPromptKey.Constants;

/// <summary>
/// 應用程式版本資訊。
/// 
/// 語意化版本控制 (Semantic Versioning) 規則：
/// 
/// X.Y.Z 三碼版本號定義：
/// - X (Major)：主版號 - 不相容的 API 變更，需重置 Y 與 Z 為 0
/// - Y (Minor)：次版號 - 向下相容的新功能，需重置 Z 為 0  
/// - Z (Patch)：修訂號 - 向下相容的錯誤修正
/// 
/// 更新範例：
/// - Bug 修正：1.2.3 → 1.2.4
/// - 新增功能：1.2.3 → 1.3.0
/// - 重大變更：1.2.3 → 2.0.0
/// 
/// 開發版本可附加後綴：X.Y.Z-dev / X.Y.Z-beta / X.Y.Z-rc1
/// 
/// ⚠️ 每次發布前請務必同步更新 CovenantPromptKey.csproj 中的 Version 屬性！
/// </summary>
public static class VersionInfo
{
    /// <summary>
    /// 應用程式版本號。
    /// 
    /// ⚠️ 更新此版本號時，請同步更新 CovenantPromptKey.csproj 中的 &lt;Version&gt; 屬性。
    /// </summary>
    public const string Version = "1.0.0";
    
    /// <summary>
    /// 應用程式完整名稱與版本。
    /// </summary>
    public const string FullVersion = $"CovenantPromptKey v{Version}";
}
