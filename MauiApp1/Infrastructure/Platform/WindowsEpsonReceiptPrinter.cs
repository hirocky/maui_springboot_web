using MauiApp1.Domain.Printing;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MauiApp1.Infrastructure.Platform;

#if WINDOWS
/// <summary>
/// APD5 でインストールされた TM プリンターへ、GDI 経由で印字する <see cref="IReceiptPrinter"/> 実装。
/// テキスト解析は Application 層の <see cref="MauiApp1.Application.Printing.ReceiptTextParser"/> が担い、
/// このクラスは <see cref="ReceiptDocument"/> の描画に専念する。
/// </summary>
public sealed class WindowsEpsonReceiptPrinter : IReceiptPrinter
{
    private const string JapaneseFontName = "MS ゴシック";
    private const int ShiftJisCharset = 128;
    private const int FwNormal = 400;
    private const int FwBold = 700;
    private const int TextFontSizePt = 9;
    private const int LargeTextFontSizePt = 14;
    private const int LogPixelsY = 90;
    private const int HorzRes = 8;
    private const int TopMarginLines = 1;
    private const int BottomMarginLines = 1;
    private const int LogoBottomGapPixels = 4;

    public Task PrintAndCutAsync(
        ReceiptDocument document,
        string? printerName = null,
        CancellationToken cancellationToken = default)
        => Task.Run(() => PrintAndCutCore(document, printerName), cancellationToken);

    private static void PrintAndCutCore(ReceiptDocument document, string? printerName)
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

                try { RenderDocument(hDC, document); }
                finally { _ = EndPage(hDC); }
            }
            finally { _ = EndDoc(hDC); }
        }
        finally { _ = DeleteDC(hDC); }
    }

    private static void RenderDocument(IntPtr hDC, ReceiptDocument document)
    {
        int dpiY = GetDeviceCaps(hDC, LogPixelsY);

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

        if (!string.IsNullOrWhiteSpace(document.LogoPath))
        {
            var path = document.LogoPath.Trim();
            if (!File.Exists(path))
                throw new FileNotFoundException($"ロゴ画像が見つかりません: {path}");
            DrawLogoScaledToPageWidth(hDC, path, pageWidth, ref y);
            y += LogoBottomGapPixels;
        }

        foreach (var line in document.Lines)
        {
            var hFontLine = PickFont(line.Bold, line.Large, hFontNormal, hFontBold, hFontNormalLarge, hFontBoldLarge);
            _ = SelectObject(hDC, hFontLine);
            GetTextMetrics(hDC, out TEXTMETRIC tmLine);
            int lineHeight = tmLine.tmHeight + tmLine.tmExternalLeading;

            if (line.IsLeftRight)
            {
                var leftStr = line.LeftText;
                var rightStr = line.RightText;
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
                if (pageWidth > 0 && line.Draw.Length > 0 && (line.Center || line.Right))
                {
                    if (!GetTextExtentPoint32(hDC, line.Draw, line.Draw.Length, out var extent))
                        extent = default;
                    if (line.Center)
                        x = Math.Max(0, (pageWidth - extent.cx) / 2);
                    else if (line.Right)
                        x = Math.Max(0, pageWidth - extent.cx);
                }
                _ = TextOut(hDC, x, y, line.Draw, line.Draw.Length);
            }

            y += lineHeight;
        }

        _ = SelectObject(hDC, hFontNormal);
        GetTextMetrics(hDC, out TEXTMETRIC tmBottom);
        int lineHeightBottom = tmBottom.tmHeight + tmBottom.tmExternalLeading;
        _ = TextOut(hDC, 0, y + lineHeightBottom * BottomMarginLines, " ", 1);

        _ = SelectObject(hDC, hOldFont);
        _ = DeleteObject(hFontNormal);
        _ = DeleteObject(hFontBold);
        _ = DeleteObject(hFontNormalLarge);
        _ = DeleteObject(hFontBoldLarge);
    }

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

    private static IntPtr PickFont(bool bold, bool large, IntPtr fN, IntPtr fB, IntPtr fNL, IntPtr fBL)
    {
        if (large) return bold ? fBL : fNL;
        return bold ? fB : fN;
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
    // P/Invoke
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

    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetDefaultPrinter(StringBuilder? pszBuffer, ref int pcchBuffer);
}
#endif
