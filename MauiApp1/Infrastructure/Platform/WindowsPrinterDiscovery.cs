using MauiApp1.Domain.Printing;
using System.Runtime.InteropServices;

namespace MauiApp1.Infrastructure.Platform;

#if WINDOWS
/// <summary>
/// Windows の WinSpool API を使ってインストール済みプリンターを列挙する実装。
/// </summary>
public sealed class WindowsPrinterDiscovery : IPrinterDiscovery
{
    public IReadOnlyList<string> GetInstalledPrinterNames() => EnumerateInstalledPrinterNames();

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
}
#endif
