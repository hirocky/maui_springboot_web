using MauiApp1.Domain.Printing;
using MauiApp1.Presentation.Services;

namespace MauiApp1.Presentation.Pages.Receipt;

public partial class CustomerDisplaySamplePage : ContentPage
{
    private readonly ICustomerDisplayService _customerDisplay;

    public CustomerDisplaySamplePage(ICustomerDisplayService customerDisplay)
    {
        InitializeComponent();
        _customerDisplay = customerDisplay;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnLoaded;

        var windows = OperatingSystem.IsWindows();
        PlatformHintLabel.IsVisible = !windows;
        SendButton.IsEnabled = windows;
    }

    private async void OnSendClicked(object? sender, EventArgs e)
    {
        if (!OperatingSystem.IsWindows())
        {
            await DisplayAlertAsync("未対応", "Windows 以外では送信を行いません。", "OK");
            return;
        }

        var line1 = Line1Entry.Text ?? string.Empty;
        var line2 = Line2Entry.Text ?? string.Empty;

        try
        {
            await _customerDisplay.SendTwoLinesAsync(line1, line2, new CustomerDisplaySendOptions());
            await DisplayAlertAsync("送信", "データを送りました。表示されない場合は Windows のプリンター設定と appsettings.windows.json を確認してください。", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("送信エラー", ex.Message, "OK");
        }
    }

    private void OnResetSampleClicked(object? sender, EventArgs e)
    {
        Line1Entry.Text = "いらっしゃいませ";
        Line2Entry.Text = "ありがとうございました";
    }
}
