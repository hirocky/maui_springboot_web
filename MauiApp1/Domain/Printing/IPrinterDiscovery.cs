namespace MauiApp1.Domain.Printing;

/// <summary>
/// インストール済みプリンターを列挙するポート。
/// </summary>
public interface IPrinterDiscovery
{
    IReadOnlyList<string> GetInstalledPrinterNames();
}
