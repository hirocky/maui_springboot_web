using MauiApp1.Domain.Printing;

namespace MauiApp1.Application.Printing;

/// <summary>
/// レシート印刷ユースケース。
/// テキストを解析して <see cref="ReceiptDocument"/> を作り、<see cref="IReceiptPrinter"/> に委譲する。
/// </summary>
public sealed class PrintReceiptUseCase
{
    private readonly IReceiptPrinter _printer;

    public PrintReceiptUseCase(IReceiptPrinter printer)
        => _printer = printer;

    public Task ExecuteAsync(
        string text,
        string? logoPath = null,
        CancellationToken cancellationToken = default)
    {
        var document = ReceiptTextParser.Parse(text, logoPath);
        return _printer.PrintAndCutAsync(document, null, cancellationToken);
    }
}
