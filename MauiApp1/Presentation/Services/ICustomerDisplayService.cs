namespace MauiApp1.Presentation.Services;

/// <summary>
/// カスタマーディスプレイ（例: Epson DM-D30）へテキストを送るための抽象。
/// Windows では (1) プリンターとして登録されている場合はスプーラへ RAW（ESC/POS）、
/// (2) 仮想 COM のみの場合はシリアル送信、のいずれか。
/// </summary>
public interface ICustomerDisplayService
{
    /// <summary>
    /// 2行表示領域に文字列を送る。DM-D30 は 20 桁×2 行を想定し、行ごとに Shift_JIS で最大 20 バイトに収める。
    /// </summary>
    Task SendTwoLinesAsync(
        string line1,
        string line2,
        CustomerDisplaySendOptions options,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// <see cref="ComPortName"/> と <see cref="WindowsPrinterName"/> のどちらか一方だけを指定する。
/// </summary>
/// <param name="ComPortName">例: COM5（シリアル経路）</param>
/// <param name="BaudRate">COM 使用時のみ有効</param>
/// <param name="WindowsPrinterName">設定アプリに表示されるプリンター名（スプーラ RAW 経路）</param>
public sealed record CustomerDisplaySendOptions(
    string? ComPortName = null,
    int BaudRate = 9600,
    string? WindowsPrinterName = null);
