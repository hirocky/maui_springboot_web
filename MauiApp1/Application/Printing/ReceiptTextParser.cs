using MauiApp1.Domain.Printing;

namespace MauiApp1.Application.Printing;

/// <summary>
/// レシートテキスト（マーカー付き文字列）を <see cref="ReceiptDocument"/> に変換するパーサー。
/// <para>対応マーカー: [B] 太字, [L] 大字, [C] 中央, [R] 右寄せ, [LR]左テキスト|右テキスト</para>
/// </summary>
public static class ReceiptTextParser
{
    private const string BoldPrefix = "[B]";
    private const string LargePrefix = "[L]";
    private const string CenterPrefix = "[C]";
    private const string RightPrefix = "[R]";
    private const string LeftRightPrefix = "[LR]";
    private const char LeftRightSep = '|';

    public static ReceiptDocument Parse(string text, string? logoPath = null)
    {
        var lines = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Split('\n')
            .Select(ParseLine)
            .ToList();

        return new ReceiptDocument(lines, logoPath);
    }

    public static ReceiptLine ParseLine(string raw)
    {
        var s = raw;
        var bold = false;
        var large = false;
        var center = false;
        var right = false;
        var isLR = false;
        var leftText = string.Empty;
        var rightText = string.Empty;

        if (s.StartsWith(BoldPrefix, StringComparison.Ordinal))
        {
            bold = true;
            s = s.Substring(BoldPrefix.Length);
        }

        if (s.StartsWith(LargePrefix, StringComparison.Ordinal))
        {
            large = true;
            s = s.Substring(LargePrefix.Length);
        }

        if (s.StartsWith(LeftRightPrefix, StringComparison.Ordinal))
        {
            isLR = true;
            var rest = s.Substring(LeftRightPrefix.Length);
            var parts = rest.Split(LeftRightSep, 2);
            leftText = parts.Length > 0 ? parts[0].Trim() : string.Empty;
            rightText = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            s = string.Empty;
        }
        else if (s.StartsWith(CenterPrefix, StringComparison.Ordinal))
        {
            center = true;
            s = s.Substring(CenterPrefix.Length);
            if (s.StartsWith(LargePrefix, StringComparison.Ordinal))
            {
                large = true;
                s = s.Substring(LargePrefix.Length);
            }
        }
        else if (s.StartsWith(RightPrefix, StringComparison.Ordinal))
        {
            right = true;
            s = s.Substring(RightPrefix.Length);
            if (s.StartsWith(LargePrefix, StringComparison.Ordinal))
            {
                large = true;
                s = s.Substring(LargePrefix.Length);
            }
        }

        return new ReceiptLine(s, bold, large, center, right, isLR, leftText, rightText);
    }
}
