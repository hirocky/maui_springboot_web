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
    private const int TextFontSizePt = 9;
    private const int LogPixelsY = 90;
    /// <summary>上端余白（行数単位で指定）。増減で調整。</summary>
    private const int TopMarginLines = 1;
    /// <summary>下端余白（行数単位で指定）。増減で調整。</summary>
    private const int BottomMarginLines = 1;

    public Task PrintAndCutAsync(string text, string? printerName = null, CancellationToken cancellationToken = default)
        => Task.Run(() => PrintAndCutCore(text, printerName), cancellationToken);

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

    private static void PrintAndCutCore(string text, string? printerName)
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
                    RenderReceipt(hDC, text);
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

    private static void RenderReceipt(IntPtr hDC, string text)
    {
        int dpiY       = GetDeviceCaps(hDC, LogPixelsY);
        int fontHeight = -(int)Math.Round(TextFontSizePt * dpiY / 72.0);

        // 日本語フォント: SHIFTJIS_CHARSET=128 を明示することで全角文字化けを防ぐ
        var hFont = CreateFont(
            fontHeight, 0, 0, 0, FwNormal, 0, 0, 0,
            ShiftJisCharset, 0, 0, 0, 0, JapaneseFontName);
        if (hFont == IntPtr.Zero)
            throw new InvalidOperationException($"フォント「{JapaneseFontName}」の作成に失敗しました。");

        var hOldFont = SelectObject(hDC, hFont);

        GetTextMetrics(hDC, out TEXTMETRIC tm);
        int lineHeight = tm.tmHeight + tm.tmExternalLeading;

        int y = lineHeight * TopMarginLines;
        var lines = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Split('\n');

        foreach (var line in lines)
        {
            _ = TextOut(hDC, 0, y, line, line.Length);
            y += lineHeight;
        }

        // 下端マージン: この位置まで空白を描くことでページ高さを確保する
        _ = TextOut(hDC, 0, y + lineHeight * BottomMarginLines, " ", 1);

        _ = SelectObject(hDC, hOldFont);
        _ = DeleteObject(hFont);
        // 用紙カットは EndPage/EndDoc 時に APD5 が自動で行うため、明示的なカット処理は不要
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

    // -------------------------------------------------------------------------
    // P/Invoke: winspool.drv（既定プリンター取得）
    // -------------------------------------------------------------------------

    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetDefaultPrinter(StringBuilder? pszBuffer, ref int pcchBuffer);
}
#endif
