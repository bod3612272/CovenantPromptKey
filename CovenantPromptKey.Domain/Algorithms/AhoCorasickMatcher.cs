using System.Collections.Generic;
using System.Linq;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// Aho-Corasick 多模式字串匹配演算法實作
/// 用於高效能偵測文本中的多個關鍵字
/// 時間複雜度: O(n + m + z) 其中 n=文本長度, m=所有模式總長, z=匹配數
/// </summary>
public class AhoCorasickMatcher
{
    private readonly Node _root;
    private readonly Dictionary<int, (string Pattern, int PatternIndex)> _outputByNodeId;
    private int _nodeIdCounter;

    public AhoCorasickMatcher()
    {
        _root = new Node { Id = _nodeIdCounter++ };
        _outputByNodeId = new Dictionary<int, (string, int)>();
    }

    /// <summary>
    /// 新增要匹配的模式
    /// </summary>
    /// <param name="pattern">模式字串</param>
    /// <param name="patternIndex">模式索引（用於識別是哪個關鍵字）</param>
    public void AddPattern(string pattern, int patternIndex)
    {
        var current = _root;

        foreach (var ch in pattern.ToLowerInvariant())
        {
            if (!current.Children.TryGetValue(ch, out var child))
            {
                child = new Node { Id = _nodeIdCounter++ };
                current.Children[ch] = child;
            }
            current = child;
        }

        current.IsEndOfPattern = true;
        _outputByNodeId[current.Id] = (pattern, patternIndex);
    }

    /// <summary>
    /// 建構失敗函數（Failure links）
    /// 必須在所有模式新增完畢後呼叫
    /// </summary>
    public void Build()
    {
        var queue = new Queue<Node>();

        // Initialize first level
        foreach (var child in _root.Children.Values)
        {
            child.Failure = _root;
            queue.Enqueue(child);
        }

        // BFS to build failure links
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var (ch, child) in current.Children)
            {
                queue.Enqueue(child);

                // Follow failure links to find longest proper suffix
                var failure = current.Failure;
                while (failure != null && !failure.Children.ContainsKey(ch))
                {
                    failure = failure.Failure;
                }

                child.Failure = failure?.Children.GetValueOrDefault(ch) ?? _root;

                // If child.Failure points to end of a pattern, inherit that output
                if (child.Failure != _root && child.Failure.IsEndOfPattern)
                {
                    // We could track multiple outputs per node for overlapping patterns
                    // For simplicity, we'll handle this in Search
                }
            }
        }
    }

    /// <summary>
    /// 在文本中搜尋所有匹配
    /// </summary>
    /// <param name="text">要搜尋的文本</param>
    /// <returns>所有匹配結果，包含結束位置、模式和模式索引</returns>
    public IEnumerable<Match> Search(string text)
    {
        var current = _root;
        var lowerText = text.ToLowerInvariant();
        var matches = new List<Match>();

        for (int i = 0; i < lowerText.Length; i++)
        {
            var ch = lowerText[i];

            // Follow failure links until we find a match or reach root
            while (current != _root && !current.Children.ContainsKey(ch))
            {
                current = current.Failure ?? _root;
            }

            if (current.Children.TryGetValue(ch, out var next))
            {
                current = next;
            }

            // Check for matches at current node and through failure links
            var temp = current;
            while (temp != _root)
            {
                if (temp.IsEndOfPattern && _outputByNodeId.TryGetValue(temp.Id, out var output))
                {
                    var startIndex = i - output.Pattern.Length + 1;
                    var originalText = text.Substring(startIndex, output.Pattern.Length);

                    matches.Add(new Match
                    {
                        StartIndex = startIndex,
                        EndIndex = i + 1,
                        Pattern = output.Pattern,
                        PatternIndex = output.PatternIndex,
                        OriginalText = originalText
                    });
                }
                temp = temp.Failure ?? _root;
            }
        }

        return matches;
    }

    /// <summary>
    /// 過濾匹配結果，處理重疊情況（最長匹配優先）
    /// </summary>
    /// <param name="matches">所有原始匹配</param>
    /// <returns>過濾後的非重疊匹配</returns>
    public static IEnumerable<Match> FilterOverlaps(IEnumerable<Match> matches)
    {
        // Sort by start position, then by length descending (longest first)
        var sorted = matches
            .OrderBy(m => m.StartIndex)
            .ThenByDescending(m => m.EndIndex - m.StartIndex)
            .ToList();

        var result = new List<Match>();
        var lastEnd = -1;

        foreach (var match in sorted)
        {
            // Skip if this match overlaps with the previous one
            if (match.StartIndex >= lastEnd)
            {
                result.Add(match);
                lastEnd = match.EndIndex;
            }
            // If new match starts at same position but is longer, it was already added first
            // due to sorting, so we skip shorter overlapping ones
        }

        return result;
    }

    private class Node
    {
        public int Id { get; init; }
        public Dictionary<char, Node> Children { get; } = new();
        public Node? Failure { get; set; }
        public bool IsEndOfPattern { get; set; }
    }

    public class Match
    {
        public int StartIndex { get; init; }
        public int EndIndex { get; init; }
        public required string Pattern { get; init; }
        public int PatternIndex { get; init; }
        public required string OriginalText { get; init; }
    }
}
