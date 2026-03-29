namespace MauiApp1.Presentation.Services;

/// <summary>
/// TM 系レシートプリンター（APD5 ドライバー利用想定）へテキストを送り、用紙カットまで行う抽象。
/// Windows では WinSpool の RAW ジョブで ESC/POS を送出する。
/// </summary>
public interface IEpsonReceiptPrintService
{
    /// <param name="text">印字する本文（改行可）</param>
    /// <param name="printerName">
    /// Windows の「プリンター名」。null または空のときは OS の既定プリンターを使う。
    /// </param>
    Task PrintAndCutAsync(string text, string? printerName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Windows に登録されているプリンター名の一覧（アルファベット順）。
    /// Windows 以外では空。
    /// </summary>
    IReadOnlyList<string> GetInstalledPrinterNames();
}
