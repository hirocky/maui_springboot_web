using MauiApp1.Application.Printing;
using Microsoft.Maui.Storage;

namespace MauiApp1.Presentation.Pages.Receipt;

public partial class ReceiptPrintPage : ContentPage
{
    private readonly PrintReceiptUseCase _printReceiptUseCase;
    public ReceiptPrintPage(PrintReceiptUseCase printReceiptUseCase)
    {
        InitializeComponent();
        _printReceiptUseCase = printReceiptUseCase;
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
        var logoPath = string.IsNullOrWhiteSpace(LogoPathEntry.Text)
            ? null
            : LogoPathEntry.Text.Trim();

        try
        {
            await _printReceiptUseCase.ExecuteAsync(text, logoPath);
            await DisplayAlertAsync("印刷", "送信しました。表示されない場合は Windows のプリンター設定と appsettings.windows.json を確認してください。", "OK");
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
