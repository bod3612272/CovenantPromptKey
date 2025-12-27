namespace CovenantPromptKey.Constants;

/// <summary>
/// 程式語言保留字清單 (FR-028, FR-029)
/// </summary>
public static class ReservedKeywords
{
    public static readonly HashSet<string> CSharp = new(StringComparer.OrdinalIgnoreCase)
    {
        "public", "private", "protected", "internal", "class", "struct",
        "interface", "enum", "namespace", "using", "static", "void", "int",
        "string", "bool", "var", "const", "readonly", "new", "null", "true",
        "false", "if", "else", "switch", "case", "default", "for", "foreach",
        "while", "do", "break", "continue", "return", "throw", "try", "catch",
        "finally", "async", "await", "override", "virtual", "abstract", "sealed",
        "this", "base", "get", "set", "value", "where", "select", "from"
    };

    public static readonly HashSet<string> Python = new(StringComparer.OrdinalIgnoreCase)
    {
        "def", "class", "import", "from", "as", "if", "elif", "else", "for",
        "while", "try", "except", "finally", "with", "return", "yield", "lambda",
        "pass", "break", "continue", "and", "or", "not", "in", "is", "True",
        "False", "None", "global", "nonlocal", "async", "await", "raise"
    };

    public static readonly HashSet<string> JavaScript = new(StringComparer.OrdinalIgnoreCase)
    {
        "function", "const", "let", "var", "if", "else", "for", "while",
        "switch", "case", "break", "continue", "return", "class", "extends",
        "import", "export", "default", "async", "await", "try", "catch",
        "finally", "throw", "new", "this", "null", "undefined", "true", "false",
        "typeof", "instanceof", "delete", "void", "yield"
    };

    public static readonly HashSet<string> Java = new(StringComparer.OrdinalIgnoreCase)
    {
        "public", "private", "protected", "class", "interface", "extends",
        "implements", "static", "final", "void", "int", "boolean", "new",
        "return", "if", "else", "for", "while", "switch", "case", "break",
        "continue", "try", "catch", "finally", "throw", "throws", "import",
        "package", "this", "super", "null", "true", "false", "abstract"
    };

    /// <summary>
    /// 所有語言的保留字聯集
    /// </summary>
    public static HashSet<string> All => CSharp
        .Union(Python)
        .Union(JavaScript)
        .Union(Java)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 檢查是否為保留字
    /// </summary>
    public static bool IsReserved(string keyword) =>
        All.Contains(keyword);

    /// <summary>
    /// 檢查是否為保留字的子字串（長度 &lt; 4 時警告）
    /// </summary>
    public static bool IsSubstringOfReserved(string keyword, out string matchedKeyword)
    {
        matchedKeyword = All.FirstOrDefault(r =>
            r.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
            !r.Equals(keyword, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
        return !string.IsNullOrEmpty(matchedKeyword);
    }
}
