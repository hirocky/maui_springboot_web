using MauiApp1.Presentation.Services;

namespace MauiApp1.Presentation.Pages.Receipt;

public partial class ReceiptPrintPage : ContentPage
{
    private const string DefaultPrinterDisplay = "（Windows の既定プリンターを使う）";

    private readonly IEpsonReceiptPrintService _receiptPrintService;

    /// <summary>
    /// <see cref="PrinterPicker"/> の選択インデックスに対応する WinSpool 用プリンター名。先頭は null（既定）。
    /// </summary>
    private List<string?> _printerNameByPickerIndex = new();

    public ReceiptPrintPage(IEpsonReceiptPrintService receiptPrintService)
    {
        InitializeComponent();
        _receiptPrintService = receiptPrintService;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnLoaded;

        var installed = _receiptPrintService.GetInstalledPrinterNames();
        var display = new List<string> { DefaultPrinterDisplay };
        _printerNameByPickerIndex = new List<string?> { null };
        foreach (var name in installed)
        {
            display.Add(name);
            _printerNameByPickerIndex.Add(name);
        }

        PrinterPicker.ItemsSource = display;
        PrinterPicker.SelectedIndex = 0;

        if (installed.Count == 0)
        {
            // Windows 以外・列挙失敗時は説明だけ残し、手動入力に任せる
            ReceiptPrinterNameEntry.Placeholder =
                "例: EPSON TM-T88V Receipt（設定アプリのプリンター名と同じ表記）";
        }
    }

    private async void OnReceiptPrintClicked(object? sender, EventArgs e)
    {
        var text = ReceiptTextEditor.Text ?? string.Empty;

        string? printerName = null;
        if (!string.IsNullOrWhiteSpace(ReceiptPrinterNameEntry.Text))
            printerName = ReceiptPrinterNameEntry.Text.Trim();
        else
        {
            var idx = PrinterPicker.SelectedIndex;
            if (idx >= 0 && idx < _printerNameByPickerIndex.Count)
                printerName = _printerNameByPickerIndex[idx];
        }

        try
        {
            await _receiptPrintService.PrintAndCutAsync(text, printerName);
            await DisplayAlert("印刷", "送信しました。プリンターが応答しない場合はドライバー名や RAW 可否を確認してください。", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("印刷エラー", ex.Message, "OK");
        }
    }
}
