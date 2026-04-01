using MauiApp1.Domain.Printing;

namespace MauiApp1.Infrastructure.Platform;

/// <summary>
/// Windows 以外のターゲット向けスタブ。プリンター一覧は常に空を返す。
/// </summary>
public sealed class NullPrinterDiscovery : IPrinterDiscovery
{
    public IReadOnlyList<string> GetInstalledPrinterNames() => Array.Empty<string>();
}
