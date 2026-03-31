using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using MauiApp1.Presentation.Services;

namespace MauiApp1.Infrastructure.Platform;

#if WINDOWS
/// <summary>
/// APD5 でインストールされた TM プリンターへ、GDI 経由で印字する実装。
/// 日本語テキストは MS ゴシック（SHIFTJIS_CHARSET=128）、
/// 用紙カットは APD5 デバイスフォント「Control」+ "P" で行う。
/// </summary>
public sealed class WindowsEpsonReceiptPrintService : IEpsonReceiptPrintService
{
    private const string JapaneseFontName = "MS ゴシック";
    private const int ShiftJisCharset = 128;
    private const int FwNormal = 400;
    private const int FwBold = 700;
    /// <summary>行頭に付けると GDI 印字で太字（MS ゴシック Bold）になるマーカー。印字時に除去される。</summary>
    private const string BoldLinePrefix = "[B]";
    /// <summary>行頭に付けると用紙幅に対して中央寄せ。印字時に除去。太字と併用する場合は <c>[B]</c> を先に付ける。</summary>
    private const string CenterLinePrefix = "[C]";
    /// <summary>行頭に付けると用紙幅に対して右寄せ。印字時に除去。太字と併用する場合は <c>[B]</c> を先に付ける。</summary>
    private const string RightLinePrefix = "[R]";
    /// <summary>行頭に付けると用紙幅に対して左項目は左寄せ・右項目は右寄せ（形式: [LR]左|右）。太字と併用する場合は <c>[B]</c> を先に付ける。</summary>
    private const string LeftRightLinePrefix = "[LR]";
    private const char LeftRightSeparator = '|';
    /// <summary>行頭に付けると本文を大きめのポイントで印字（<c>[B]</c> / <c>[C]</c> / <c>[R]</c> と組み合わせ可。先頭の <c>[B]</c> の直後、または <c>[C]</c>/<c>[R]</c> の直後に付ける。</summary>
    private const string LargeLinePrefix = "[L]";
    private const int TextFontSizePt = 9;
    private const int LargeTextFontSizePt = 14;
    private const int LogPixelsY = 90;
    /// <summary>可印字領域の幅（ピクセル）を得るときの GetDeviceCaps nIndex（HORZRES）。</summary>
    private const int HorzRes = 8;
    /// <summary>上端余白（行数単位で指定）。増減で調整。</summary>
    private const int TopMarginLines = 1;
    /// <summary>下端余白（行数単位で指定）。増減で調整。</summary>
    private const int BottomMarginLines = 1;
    /// <summary>ロゴ描画後に本文とのあいだに空けるピクセル。</summary>
    private const int LogoBottomGapPixels = 4;

    public Task PrintAndCutAsync(string text, string? printerName = null, string? logoPath = null, CancellationToken cancellationToken = default)
        => Task.Run(() => PrintAndCutCore(text, printerName, logoPath), cancellationToken);

    public IReadOnlyList<string> GetInstalledPrinterNames() => EnumerateInstalledPrinterNames();

    // -------------------------------------------------------------------------
    // プリンター列挙
    // -------------------------------------------------------------------------

    private static IReadOnlyList<string> EnumerateInstalledPrinterNames()
    {
        const uint printerEnumLocal = 0x0002;
        const uint printerEnumConnections = 0x0004;
        uint flags = printerEnumLocal | printerEnumConnections;
        uint cbNeeded = 0;
        uint cReturned = 0;

        _ = EnumPrinters(flags, null, 4, IntPtr.Zero, 0, ref cbNeeded, ref cReturned);
        if (cbNeeded == 0)
            return Array.Empty<string>();

        var buffer = Marshal.AllocCoTaskMem((int)cbNeeded);
        try
        {
            if (!EnumPrinters(flags, null, 4, buffer, cbNeeded, ref cbNeeded, ref cReturned) || cReturned == 0)
                return Array.Empty<string>();

            var structSize = Marshal.SizeOf<PRINTER_INFO_4>();
            var names = new List<string>();
            for (uint i = 0; i < cReturned; i++)
            {
                var row = IntPtr.Add(buffer, (int)(i * structSize));
                var info = Marshal.PtrToStructure<PRINTER_INFO_4>(row);
                if (!string.IsNullOrWhiteSpace(info.pPrinterName))
                    names.Add(info.pPrinterName);
            }

            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }
        finally
        {
            Marshal.FreeCoTaskMem(buffer);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct PRINTER_INFO_4
    {
        public string pPrinterName;
        public string pServerName;
        public uint Attributes;
    }

    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool EnumPrinters(
        uint flags, string? name, uint level, IntPtr pPrinterEnum,
        uint cbBuf, ref uint pcbNeeded, ref uint pcReturned);

    // -------------------------------------------------------------------------
    // 印字コア（GDI）
    // -------------------------------------------------------------------------

    private static void PrintAndCutCore(string text, string? printerName, string? logoPath)
    {
        var name = string.IsNullOrWhiteSpace(printerName) ? GetDefaultPrinterNameOrThrow() : printerName.Trim();

        var hDC = CreateDC(null, name, null, IntPtr.Zero);
        if (hDC == IntPtr.Zero)
            throw new InvalidOperationException(
                $"プリンター「{name}」の DC を作成できませんでした。APD5 インストール済みか、名前が正しいか確認してください。");

        try
        {
            var docInfo = new DOCINFO
            {
                cbSize = Marshal.SizeOf<DOCINFO>(),
                lpszDocName = "MauiApp1 Receipt"
            };

            if (StartDoc(hDC, ref docInfo) <= 0)
                throw new InvalidOperationException("StartDoc に失敗しました。");

            try
            {
                if (StartPage(hDC) <= 0)
                    throw new InvalidOperationException("StartPage に失敗しました。");

                try
                {
                    RenderReceipt(hDC, text, logoPath);
                }
                finally
                {
                    _ = EndPage(hDC);
                }
            }
            finally
            {
                _ = EndDoc(hDC);
            }
        }
        finally
        {
            _ = DeleteDC(hDC);
        }
    }

    private static void RenderReceipt(IntPtr hDC, string text, string? logoPath)
    {
        int dpiY = GetDeviceCaps(hDC, LogPixelsY);

        // 日本語フォント: SHIFTJIS_CHARSET=128 を明示することで全角文字化けを防ぐ
        var hFontNormal = CreateReceiptFont(dpiY, TextFontSizePt, FwNormal);
        var hFontBold = CreateReceiptFont(dpiY, TextFontSizePt, FwBold);
        var hFontNormalLarge = CreateReceiptFont(dpiY, LargeTextFontSizePt, FwNormal);
        var hFontBoldLarge = CreateReceiptFont(dpiY, LargeTextFontSizePt, FwBold);
        if (hFontNormal == IntPtr.Zero || hFontBold == IntPtr.Zero
            || hFontNormalLarge == IntPtr.Zero || hFontBoldLarge == IntPtr.Zero)
            throw new InvalidOperationException($"フォント「{JapaneseFontName}」の作成に失敗しました。");

        var hOldFont = SelectObject(hDC, hFontNormal);

        GetTextMetrics(hDC, out TEXTMETRIC tmBase);
        int lineHeightNormal = tmBase.tmHeight + tmBase.tmExternalLeading;
        int pageWidth = Math.Max(0, GetDeviceCaps(hDC, HorzRes));

        int y = lineHeightNormal * TopMarginLines;

        if (!string.IsNullOrWhiteSpace(logoPath))
        {
            var path = logoPath.Trim();
            if (!File.Exists(path))
                throw new FileNotFoundException($"ロゴ画像が見つかりません: {path}");
            DrawLogoScaledToPageWidth(hDC, path, pageWidth, ref y);
            y += LogoBottomGapPixels;
        }

        var lines = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Split('\n');

        foreach (var line in lines)
        {
            ParseReceiptLine(
                line,
                out var draw,
                out var bold,
                out var center,
                out var right,
                out var leftText,
                out var rightText,
                out var leftRight,
                out var large);

            var hFontLine = PickReceiptFont(bold, large, hFontNormal, hFontBold, hFontNormalLarge, hFontBoldLarge);
            _ = SelectObject(hDC, hFontLine);
            GetTextMetrics(hDC, out TEXTMETRIC tmLine);
            int lineHeightThis = tmLine.tmHeight + tmLine.tmExternalLeading;

            if (leftRight)
            {
                var leftStr = leftText ?? string.Empty;
                var rightStr = rightText ?? string.Empty;

                int xLeft = 0;
                int xRight = 0;

                if (pageWidth > 0 && rightStr.Length > 0)
                {
                    if (!GetTextExtentPoint32(hDC, rightStr, rightStr.Length, out var rightExtent))
                        rightExtent = default;

                    xRight = Math.Max(0, pageWidth - rightExtent.cx);

                    if (leftStr.Length > 0)
                    {
                        if (!GetTextExtentPoint32(hDC, leftStr, leftStr.Length, out var leftExtent))
                            leftExtent = default;

                        // 過度な縮退時（左が長すぎる等）に右側が左側に食い込むのを少しでも避ける
                        xRight = Math.Max(xRight, xLeft + leftExtent.cx);
                    }
                }

                if (leftStr.Length > 0)
                    _ = TextOut(hDC, xLeft, y, leftStr, leftStr.Length);

                if (rightStr.Length > 0)
                    _ = TextOut(hDC, xRight, y, rightStr, rightStr.Length);
            }
            else
            {
                int x = 0;
                if (pageWidth > 0 && draw.Length > 0 && (center || right))
                {
                    if (!GetTextExtentPoint32(hDC, draw, draw.Length, out var extent))
                        extent = default;
                    if (center)
                        x = Math.Max(0, (pageWidth - extent.cx) / 2);
                    else if (right)
                        x = Math.Max(0, pageWidth - extent.cx);
                }

                _ = TextOut(hDC, x, y, draw, draw.Length);
            }

            y += lineHeightThis;
        }

        // 下端マージン: この位置まで空白を描くことでページ高さを確保する
        _ = SelectObject(hDC, hFontNormal);
        GetTextMetrics(hDC, out TEXTMETRIC tmBottom);
        int lineHeightBottom = tmBottom.tmHeight + tmBottom.tmExternalLeading;
        _ = TextOut(hDC, 0, y + lineHeightBottom * BottomMarginLines, " ", 1);

        _ = SelectObject(hDC, hOldFont);
        _ = DeleteObject(hFontNormal);
        _ = DeleteObject(hFontBold);
        _ = DeleteObject(hFontNormalLarge);
        _ = DeleteObject(hFontBoldLarge);
        // 用紙カットは EndPage/EndDoc 時に APD5 が自動で行うため、明示的なカット処理は不要
    }

    /// <summary>
    /// ロゴを可印字幅に合わせて横方向いっぱいに縮小し、現在の <paramref name="y"/> 位置に描画する。
    /// </summary>
    private static void DrawLogoScaledToPageWidth(IntPtr hDC, string path, int pageWidth, ref int y)
    {
        using var src = new Bitmap(path);
        if (pageWidth <= 0)
        {
            using var g = Graphics.FromHdc(hDC);
            g.DrawImage(src, 0, y);
            y += src.Height;
            return;
        }

        int targetW = pageWidth;
        int targetH = Math.Max(1, (int)Math.Round(src.Height * (double)targetW / Math.Max(1, src.Width)));

        using var scaled = new Bitmap(targetW, targetH, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(scaled))
        {
            g.Clear(System.Drawing.Color.White);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(src, 0, 0, targetW, targetH);
        }

        using (var g = Graphics.FromHdc(hDC))
        {
            g.PageUnit = GraphicsUnit.Pixel;
            g.DrawImage(scaled, 0, y);
        }

        y += targetH;
    }

    private static IntPtr CreateReceiptFont(int dpiY, int fontSizePt, int weight)
    {
        int fontHeight = -(int)Math.Round(fontSizePt * dpiY / 72.0);
        return CreateFont(
            fontHeight, 0, 0, 0, weight, 0, 0, 0,
            ShiftJisCharset, 0, 0, 0, 0, JapaneseFontName);
    }

    private static IntPtr PickReceiptFont(
        bool bold,
        bool large,
        IntPtr hFontNormal,
        IntPtr hFontBold,
        IntPtr hFontNormalLarge,
        IntPtr hFontBoldLarge)
    {
        if (large)
            return bold ? hFontBoldLarge : hFontNormalLarge;
        return bold ? hFontBold : hFontNormal;
    }

    private static void ParseReceiptLine(
        string line,
        out string draw,
        out bool bold,
        out bool center,
        out bool right,
        out string? leftText,
        out string? rightText,
        out bool leftRight,
        out bool large)
    {
        draw = line;
        bold = false;
        center = false;
        right = false;
        large = false;
        leftText = null;
        rightText = null;
        leftRight = false;

        if (draw.StartsWith(BoldLinePrefix, StringComparison.Ordinal))
        {
            bold = true;
            draw = draw.Substring(BoldLinePrefix.Length);
        }

        if (draw.StartsWith(LargeLinePrefix, StringComparison.Ordinal))
        {
            large = true;
            draw = draw.Substring(LargeLinePrefix.Length);
        }

        // [LR] は [B] の次に来る想定（[B] を先に付けるのがルール）
        if (draw.StartsWith(LeftRightLinePrefix, StringComparison.Ordinal))
        {
            leftRight = true;
            var rest = draw.Substring(LeftRightLinePrefix.Length);
            var parts = rest.Split(LeftRightSeparator, 2);
            leftText = parts.Length > 0 ? parts[0].Trim() : string.Empty;
            rightText = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            draw = string.Empty; // leftText/rightText を使うため
            return;
        }

        if (draw.StartsWith(CenterLinePrefix, StringComparison.Ordinal))
        {
            center = true;
            draw = draw.Substring(CenterLinePrefix.Length);
        }
        else if (draw.StartsWith(RightLinePrefix, StringComparison.Ordinal))
        {
            right = true;
            draw = draw.Substring(RightLinePrefix.Length);
        }

        if (draw.StartsWith(LargeLinePrefix, StringComparison.Ordinal))
        {
            large = true;
            draw = draw.Substring(LargeLinePrefix.Length);
        }
    }

    private static string GetDefaultPrinterNameOrThrow()
    {
        int size = 0;
        if (GetDefaultPrinter(null, ref size))
            throw new InvalidOperationException("既定プリンターの取得に失敗しました（想定外の応答）。");

        const int errorInsufficientBuffer = 122;
        if (Marshal.GetLastWin32Error() != errorInsufficientBuffer || size <= 0)
            throw new InvalidOperationException(
                "既定のプリンターが設定されていません。Windows の設定で既定プリンターを指定するか、プリンター名を入力してください。");

        var sb = new StringBuilder(size);
        if (!GetDefaultPrinter(sb, ref size))
            throw new InvalidOperationException("既定プリンター名の取得に失敗しました。");

        return sb.ToString();
    }

    // -------------------------------------------------------------------------
    // 構造体
    // -------------------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DOCINFO
    {
        public int cbSize;
        public string? lpszDocName;
        public string? lpszOutput;
        public string? lpszDatatype;
        public uint fwType;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SIZE
    {
        public int cx;
        public int cy;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct TEXTMETRIC
    {
        public int tmHeight;
        public int tmAscent;
        public int tmDescent;
        public int tmInternalLeading;
        public int tmExternalLeading;
        public int tmAveCharWidth;
        public int tmMaxCharWidth;
        public int tmWeight;
        public int tmOverhang;
        public int tmDigitizedAspectX;
        public int tmDigitizedAspectY;
        public char tmFirstChar;
        public char tmLastChar;
        public char tmDefaultChar;
        public char tmBreakChar;
        public byte tmItalic;
        public byte tmUnderlined;
        public byte tmStruckOut;
        public byte tmPitchAndFamily;
        public byte tmCharSet;
    }

    // -------------------------------------------------------------------------
    // P/Invoke: gdi32.dll
    // -------------------------------------------------------------------------

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateDC(
        string? lpszDriver, string? lpszDevice, string? lpszOutput, IntPtr lpInitData);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteDC(IntPtr hDC);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int StartDoc(IntPtr hdc, ref DOCINFO lpdi);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int EndDoc(IntPtr hDC);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int StartPage(IntPtr hDC);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int EndPage(IntPtr hDC);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateFont(
        int nHeight, int nWidth, int nEscapement, int nOrientation,
        int fnWeight, int fdwItalic, int fdwUnderline, int fdwStrikeOut,
        int fdwCharSet, int fdwOutputPrecision, int fdwClipPrecision,
        int fdwQuality, int fdwPitchAndFamily, string lpszFace);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart, string lpString, int cbString);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetTextMetrics(IntPtr hdc, out TEXTMETRIC lptm);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetTextExtentPoint32(IntPtr hdc, string lpString, int c, out SIZE lpSize);

    // -------------------------------------------------------------------------
    // P/Invoke: winspool.drv（既定プリンター取得）
    // -------------------------------------------------------------------------

    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetDefaultPrinter(StringBuilder? pszBuffer, ref int pcchBuffer);
}
#endif
