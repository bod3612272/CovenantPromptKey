using System.Text.RegularExpressions;
using CovenantPromptKey.Models;
using CovenantPromptKey.Services.Interfaces;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// Markdown 解析服務實作
/// 使用 Markdig 函式庫分析 Markdown 結構，識別需保護區域
/// </summary>
public partial class MarkdownService : IMarkdownService
{
    [GeneratedRegex(@"https?://[^\s<>\[\]\""']+", RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();

    /// <inheritdoc />
    public MarkdownStructure Analyze(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new MarkdownStructure();
        }

        var result = new MarkdownStructure
        {
            ProtectedUrls = [],
            CodeBlocks = [],
            InlineCode = []
        };

        try
        {
            // Parse with Markdig
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            var document = Markdown.Parse(text, pipeline);

            // Extract code blocks
            foreach (var block in document.Descendants<FencedCodeBlock>())
            {
                var span = block.Span;
                result.CodeBlocks.Add(new TextRange(span.Start, span.End + 1));
            }

            // Extract inline code
            foreach (var inline in document.Descendants<CodeInline>())
            {
                var span = inline.Span;
                // Adjust for backticks
                result.InlineCode.Add(new TextRange(span.Start, span.End + 1));
            }

            // Extract URLs from links
            foreach (var link in document.Descendants<LinkInline>())
            {
                if (!string.IsNullOrEmpty(link.Url))
                {
                    // Find URL position in original text
                    var urlStart = text.IndexOf(link.Url, StringComparison.Ordinal);
                    if (urlStart >= 0)
                    {
                        result.ProtectedUrls.Add(new TextRange(urlStart, urlStart + link.Url.Length));
                    }
                }
            }

            // Also detect raw URLs not in markdown link syntax
            var urlMatches = UrlRegex().Matches(text);
            foreach (Match match in urlMatches)
            {
                var range = new TextRange(match.Index, match.Index + match.Length);

                // Only add if not already covered by a link
                if (!result.ProtectedUrls.Any(existing =>
                    existing.Start <= range.Start && existing.End >= range.End))
                {
                    // Also check it's not inside a code block
                    if (!IsInCodeRegion(range.Start, result.CodeBlocks) &&
                        !IsInCodeRegion(range.Start, result.InlineCode))
                    {
                        result.ProtectedUrls.Add(range);
                    }
                }
            }

            // Filter out inline code that's inside code blocks
            result.InlineCode = result.InlineCode
                .Where(ic => !IsInCodeRegion(ic.Start, result.CodeBlocks))
                .ToList();
        }
        catch
        {
            // If Markdig fails, return empty structure
            // The main keyword detection will still work
        }

        return result;
    }

    private static bool IsInCodeRegion(int position, List<TextRange> codeRegions)
    {
        return codeRegions.Any(region =>
            position >= region.Start && position < region.End);
    }
}
