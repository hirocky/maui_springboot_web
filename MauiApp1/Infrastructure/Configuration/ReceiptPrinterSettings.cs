namespace MauiApp1.Infrastructure.Configuration;

public sealed class ReceiptPrinterSettings
{
    public const string SectionName = "ReceiptPrinter";

    public string WindowsPrinterName { get; init; } = "EPSON TM-T88V Receipt";
}
