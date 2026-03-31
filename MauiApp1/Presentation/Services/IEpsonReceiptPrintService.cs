namespace MauiApp1.Presentation.Services;

/// <summary>
/// TM 系レシートプリンター（APD5 ドライバー利用想定）へテキストを送り、用紙カットまで行う抽象。
/// Windows では WinSpool の RAW ジョブで ESC/POS を送出する。
/// </summary>
public interface IEpsonReceiptPrintService
{
    /// <param name="text">印字する本文（改行可）。Windows GDI 実装では <c>[B]</c> 太字、<c>[C]</c> 中央、<c>[R]</c> 右、<c>[L]</c> 大きめ、<c>[LR]</c> 左|右（併用時は <c>[B]</c> を先。例: <c>[B][C][L]</c>）。</param>
    /// <param name="printerName">
    /// Windows の「プリンター名」。null または空のときは OS の既定プリンターを使う。
    /// </param>
    /// <param name="logoPath">
    /// Windows GDI 実装のみ: 印字するロゴ画像のファイルパス（PNG/JPG/BMP 等）。null または空のときはロゴなし。本文の先頭（上端余白の直後）に、用紙幅に合わせて縮小して描画する。
    /// </param>
    Task PrintAndCutAsync(string text, string? printerName = null, string? logoPath = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Windows に登録されているプリンター名の一覧（アルファベット順）。
    /// Windows 以外では空。
    /// </summary>
    IReadOnlyList<string> GetInstalledPrinterNames();
}
