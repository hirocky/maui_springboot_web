namespace MauiApp1.Domain.Printing;

/// <summary>
/// レシートプリンターへの出力を抽象化するポート（依存性逆転の原則）。
/// Infrastructure 層に具体実装を持つ。
/// </summary>
public interface IReceiptPrinter
{
    Task PrintAndCutAsync(
        ReceiptDocument document,
        string? printerName = null,
        CancellationToken cancellationToken = default);
}
