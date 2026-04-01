using MauiApp1.Domain.Printing;

namespace MauiApp1.Infrastructure.Platform;

/// <summary>
/// Windows 以外のターゲット向けスタブ。呼び出すと未対応であることを示す。
/// </summary>
public sealed class NullReceiptPrinter : IReceiptPrinter
{
    public Task PrintAndCutAsync(
        ReceiptDocument document,
        string? printerName = null,
        CancellationToken cancellationToken = default)
        => Task.FromException(new PlatformNotSupportedException(
            "レシート印刷（APD5 / ESC-POS）は Windows ターゲットでのみ利用できます。"));
}
