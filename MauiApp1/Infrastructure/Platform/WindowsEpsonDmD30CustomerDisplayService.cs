using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using MauiApp1.Presentation.Services;

namespace MauiApp1.Infrastructure.Platform;

#if WINDOWS
/// <summary>
/// Epson DM-D30 向け: ESC/POS を (1) Windows スプーラ RAW、または (2) COM シリアルで送る。
/// </summary>
public sealed class WindowsEpsonDmD30CustomerDisplayService : ICustomerDisplayService
{
    /// <summary>DM-D30 の標準は 20 桁×2 行（本サンプルは行あたり Shift_JIS で最大 20 バイト）。</summary>
    private const int MaxBytesPerLine = 20;

    static WindowsEpsonDmD30CustomerDisplayService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public Task SendTwoLinesAsync(
        string line1,
        string line2,
        CustomerDisplaySendOptions options,
        CancellationToken cancellationToken = default)
    {
        ValidateOptions(options);
        return Task.Run(() => SendCore(line1, line2, options), cancellationToken);
    }

    private static void ValidateOptions(CustomerDisplaySendOptions options)
    {
        var hasCom = !string.IsNullOrWhiteSpace(options.ComPortName);
        var hasPrinter = !string.IsNullOrWhiteSpace(options.WindowsPrinterName);
        if (hasCom == hasPrinter)
        {
            throw new ArgumentException(
                "COM ポート名と Windows プリンター名のどちらか一方だけを指定してください。",
                nameof(options));
        }
    }

    private static void SendCore(string line1, string line2, CustomerDisplaySendOptions options)
    {
        var payload = BuildPayload(line1, line2);

        if (!string.IsNullOrWhiteSpace(options.WindowsPrinterName))
        {
            WriteRawBytesToWindowsPrinter(options.WindowsPrinterName.Trim(), payload);
            return;
        }

        var portName = ResolveExistingPortNameOrThrow(options.ComPortName!.Trim());

        using var port = new SerialPort(portName, options.BaudRate, Parity.None, 8, StopBits.One)
        {
            NewLine = "\n",
            ReadTimeout = 3000,
            WriteTimeout = 3000,
            DtrEnable = true,
            RtsEnable = true,
        };

        try
        {
            port.Open();
        }
        catch (FileNotFoundException ex)
        {
            throw new InvalidOperationException(
                $"ポート「{portName}」を開けませんでした。COM 番号が間違っているか、デバイスが認識されていない可能性があります。", ex);
        }

        port.Write(payload, 0, payload.Length);
        port.BaseStream.Flush();
    }

    private static void WriteRawBytesToWindowsPrinter(string printerName, byte[] payload)
    {
        if (!OpenPrinter(printerName, out var hPrinter, IntPtr.Zero))
        {
            var err = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"プリンター「{printerName}」を開けませんでした（Win32: {err}）。設定アプリのプリンター名と完全一致するか確認してください。");
        }

        try
        {
            var docInfo = new DOC_INFO_1
            {
                pDocName = "MauiApp1 Customer Display",
                pOutputFile = null,
                pDatatype = "RAW",
            };

            var jobId = StartDocPrinter(hPrinter, 1, ref docInfo);
            if (jobId == 0)
            {
                var err = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"StartDocPrinter に失敗しました（Win32: {err}）。ドライバが RAW ジョブを受け付けられるか確認してください。");
            }

            try
            {
                if (!StartPagePrinter(hPrinter))
                {
                    var err = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"StartPagePrinter に失敗しました（Win32: {err}）。");
                }

                try
                {
                    var handle = GCHandle.Alloc(payload, GCHandleType.Pinned);
                    try
                    {
                        var ptr = handle.AddrOfPinnedObject();
                        if (!WritePrinter(hPrinter, ptr, payload.Length, out var written))
                        {
                            var err = Marshal.GetLastWin32Error();
                            throw new InvalidOperationException($"WritePrinter に失敗しました（Win32: {err}）。");
                        }

                        if (written != payload.Length)
                            throw new InvalidOperationException($"送信バイト数が一致しません: {written}/{payload.Length}");
                    }
                    finally
                    {
                        handle.Free();
                    }
                }
                finally
                {
                    _ = EndPagePrinter(hPrinter);
                }
            }
            finally
            {
                _ = EndDocPrinter(hPrinter);
            }
        }
        finally
        {
            _ = ClosePrinter(hPrinter);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DOC_INFO_1
    {
        public string pDocName;
        public string? pOutputFile;
        public string pDatatype;
    }

    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int StartDocPrinter(IntPtr hPrinter, int level, [In] ref DOC_INFO_1 pDocInfo);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBuf, int cbBuf, out int pcWritten);

    private static string ResolveExistingPortNameOrThrow(string requested)
    {
        var trimmed = requested.Trim();
        var available = SerialPort.GetPortNames();
        var match = Array.Find(available, p => string.Equals(p, trimmed, StringComparison.OrdinalIgnoreCase));
        if (match is not null)
            return match;

        var sorted = (string[])available.Clone();
        Array.Sort(sorted, StringComparer.OrdinalIgnoreCase);
        var list = sorted.Length == 0
            ? "（シリアルポートが検出されていません。USB 接続とドライバを確認してください。）"
            : string.Join(", ", sorted);

        throw new InvalidOperationException(
            $"ポート「{trimmed}」が見つかりません。デバイスマネージャーで COM 番号を確認し、次のいずれかを指定してください: {list}");
    }

    private static byte[] BuildPayload(string line1, string line2)
    {
        using var ms = new MemoryStream();

        ms.WriteByte(0x1B);
        ms.WriteByte(0x40);

        // 既定は JIS 系の解釈のため、Shift_JIS バイト列を送ると文字化けする。
        // DM-D30: US ( G fn=96 m=1 で漢字モード、fn=97 m=1 で SHIFT-JIS を指定（EPSON ESC/POS 顧客表示器リファレンス）。
        ReadOnlySpan<byte> kanjiShiftJisSetup =
        [
            0x1F, 0x28, 0x47, 0x02, 0x00, 0x60, 0x01,
            0x1F, 0x28, 0x47, 0x02, 0x00, 0x61, 0x01,
        ];
        ms.Write(kanjiShiftJisSetup);

        var b1 = TrimLineToShiftJisBytes(line1);
        var b2 = TrimLineToShiftJisBytes(line2);
        ms.Write(b1);
        ms.WriteByte(0x0A);
        ms.Write(b2);

        return ms.ToArray();
    }

    private static byte[] TrimLineToShiftJisBytes(string? line)
    {
        var enc = Encoding.GetEncoding(932);
        var bytes = enc.GetBytes(line ?? string.Empty);
        if (bytes.Length <= MaxBytesPerLine)
            return bytes;

        int end = 0;
        int i = 0;
        while (i < bytes.Length && end < MaxBytesPerLine)
        {
            int charLen = IsShiftJisLeadByte(bytes[i]) ? 2 : 1;
            if (charLen == 2 && i + 1 >= bytes.Length)
                break;
            if (end + charLen <= MaxBytesPerLine)
            {
                end += charLen;
                i += charLen;
            }
            else
                break;
        }

        return bytes.AsSpan(0, end).ToArray();
    }

    private static bool IsShiftJisLeadByte(byte b)
        => (b >= 0x81 && b <= 0x9F) || (b >= 0xE0 && b <= 0xFC);
}
#endif
