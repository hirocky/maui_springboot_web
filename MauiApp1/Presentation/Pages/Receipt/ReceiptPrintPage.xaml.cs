using MauiApp1.Application.Printing;
using MauiApp1.Domain.Printing;
using Microsoft.Maui.Storage;

namespace MauiApp1.Presentation.Pages.Receipt;

public partial class ReceiptPrintPage : ContentPage
{
    private const string DefaultPrinterDisplay = "（Windows の既定プリンターを使う）";

    private readonly PrintReceiptUseCase _printReceiptUseCase;
    private readonly IPrinterDiscovery _printerDiscovery;

    /// <summary>
    /// <see cref="PrinterPicker"/> の選択インデックスに対応するプリンター名。先頭は null（既定）。
    /// </summary>
    private List<string?> _printerNameByPickerIndex = new();

    public ReceiptPrintPage(PrintReceiptUseCase printReceiptUseCase, IPrinterDiscovery printerDiscovery)
    {
        InitializeComponent();
        _printReceiptUseCase = printReceiptUseCase;
        _printerDiscovery = printerDiscovery;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnLoaded;

        var installed = _printerDiscovery.GetInstalledPrinterNames();
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
            ReceiptPrinterNameEntry.Placeholder =
                "例: EPSON TM-T88V Receipt（設定アプリのプリンター名と同じ表記）";
        }
    }

    private string? GetSelectedPrinterName()
    {
        if (!string.IsNullOrWhiteSpace(ReceiptPrinterNameEntry.Text))
            return ReceiptPrinterNameEntry.Text.Trim();

        var idx = PrinterPicker.SelectedIndex;
        if (idx >= 0 && idx < _printerNameByPickerIndex.Count)
            return _printerNameByPickerIndex[idx];

        return null;
    }

    private async void OnReceiptPrintClicked(object? sender, EventArgs e)
    {
        var text = ReceiptTextEditor.Text ?? string.Empty;
        await PrintReceiptAsync(text);
    }

    private async void OnSampleKaikatsuReceiptClicked(object? sender, EventArgs e)
    {
        await PrintReceiptAsync(KaikatsuSampleReceiptText.Build());
    }

    private async Task PrintReceiptAsync(string text)
    {
        var printerName = GetSelectedPrinterName();
        var logoPath = string.IsNullOrWhiteSpace(LogoPathEntry.Text)
            ? null
            : LogoPathEntry.Text.Trim();

        try
        {
            await _printReceiptUseCase.ExecuteAsync(text, printerName, logoPath);
            await DisplayAlertAsync("印刷", "送信しました。プリンターが応答しない場合はドライバー名や RAW 可否を確認してください。", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("印刷エラー", ex.Message, "OK");
        }
    }

    private async void OnLogoPickClicked(object? sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "ロゴ画像を選択",
                FileTypes = FilePickerFileType.Images,
            });
            if (result == null)
                return;

            var path = result.FullPath;
            if (string.IsNullOrEmpty(path))
                path = result.FileName;

            LogoPathEntry.Text = path;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("ファイル選択", ex.Message, "OK");
        }
    }
}
