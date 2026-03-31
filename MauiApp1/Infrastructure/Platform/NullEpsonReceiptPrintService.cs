using MauiApp1.Presentation.Services;

namespace MauiApp1.Infrastructure.Platform;

/// <summary>
/// Windows 以外のターゲット向けのスタブ。呼び出すと未対応であることを示す。
/// </summary>
public sealed class NullEpsonReceiptPrintService : IEpsonReceiptPrintService
{
    public Task PrintAndCutAsync(string text, string? printerName = null, string? logoPath = null, CancellationToken cancellationToken = default)
        => Task.FromException(new PlatformNotSupportedException(
            "レシート印刷（APD5 / ESC-POS）は Windows ターゲットでのみ利用できます。"));

    public IReadOnlyList<string> GetInstalledPrinterNames() => Array.Empty<string>();
}
