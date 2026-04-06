using MauiApp1.Domain.Printing;
using MauiApp1.Presentation.Services;
#if WINDOWS
using System.IO.Ports;
#endif

namespace MauiApp1.Presentation.Pages.Receipt;

public partial class CustomerDisplaySamplePage : ContentPage
{
    private const string DisplayPrinterPickerPlaceholder = "（一覧から DM-D30 を選ぶ）";

    private static readonly int[] BaudRates = [9600, 19200, 38400, 57600, 115200];

    private readonly ICustomerDisplayService _customerDisplay;
    private readonly IPrinterDiscovery _printerDiscovery;

    private List<string?> _displayPrinterNameByPickerIndex = new();

    public CustomerDisplaySamplePage(ICustomerDisplayService customerDisplay, IPrinterDiscovery printerDiscovery)
    {
        InitializeComponent();
        _customerDisplay = customerDisplay;
        _printerDiscovery = printerDiscovery;

        TransportModePicker.ItemsSource = new List<string>
        {
            "Windows プリンター（スプーラ・RAW）",
            "COM ポート",
        };
        TransportModePicker.SelectedIndex = 0;

        BaudRatePicker.ItemsSource = BaudRates.Select(b => b.ToString()).ToList();
        BaudRatePicker.SelectedIndex = 0;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnLoaded;

        var windows = OperatingSystem.IsWindows();
        PlatformHintLabel.IsVisible = !windows;
        SendButton.IsEnabled = windows;

        var installed = _printerDiscovery.GetInstalledPrinterNames();
        var display = new List<string> { DisplayPrinterPickerPlaceholder };
        _displayPrinterNameByPickerIndex = new List<string?> { null };
        foreach (var name in installed)
        {
            display.Add(name);
            _displayPrinterNameByPickerIndex.Add(name);
        }

        DisplayPrinterPicker.ItemsSource = display;
        DisplayPrinterPicker.SelectedIndex = 0;

        for (var i = 1; i < _displayPrinterNameByPickerIndex.Count; i++)
        {
            var n = _displayPrinterNameByPickerIndex[i];
            if (n != null && n.Contains("DM-D30", StringComparison.OrdinalIgnoreCase))
            {
                DisplayPrinterPicker.SelectedIndex = i;
                break;
            }
        }

#if WINDOWS
        if (windows)
        {
            var names = SerialPort.GetPortNames();
            Array.Sort(names, StringComparer.OrdinalIgnoreCase);
            DetectedPortsLabel.Text = names.Length == 0
                ? "この PC で検出されたシリアルポート: なし（USB 接続・ドライバを確認）"
                : $"この PC で検出されたシリアルポート: {string.Join(", ", names)}";
            DetectedPortsLabel.IsVisible = true;
        }
#else
        DetectedPortsLabel.IsVisible = false;
#endif

        ApplyTransportModeUi();
    }

    private void OnTransportModeChanged(object? sender, EventArgs e)
        => ApplyTransportModeUi();

    private void ApplyTransportModeUi()
    {
        var idx = TransportModePicker.SelectedIndex;
        var useWindowsPrinter = idx <= 0;
        WindowsPrinterSection.IsVisible = useWindowsPrinter;
        ComPortSection.IsVisible = !useWindowsPrinter;
    }

    private string? GetSelectedDisplayPrinterName()
    {
        if (!string.IsNullOrWhiteSpace(DisplayPrinterNameEntry.Text))
            return DisplayPrinterNameEntry.Text.Trim();

        var idx = DisplayPrinterPicker.SelectedIndex;
        if (idx >= 0 && idx < _displayPrinterNameByPickerIndex.Count)
            return _displayPrinterNameByPickerIndex[idx];

        return null;
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

        CustomerDisplaySendOptions options;

        if (TransportModePicker.SelectedIndex == 0)
        {
            var printerName = GetSelectedDisplayPrinterName();
            if (string.IsNullOrWhiteSpace(printerName))
            {
                await DisplayAlertAsync("入力", "プリンター一覧で DM-D30 を選ぶか、下の欄にプリンター名を入力してください。", "OK");
                return;
            }

            options = new CustomerDisplaySendOptions(WindowsPrinterName: printerName);
        }
        else
        {
            var port = ComPortEntry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(port))
            {
                await DisplayAlertAsync("入力", "COM ポート名を入力してください。", "OK");
                return;
            }

            var baudText = BaudRatePicker.SelectedItem as string;
            if (!int.TryParse(baudText, out var baud) || baud <= 0)
            {
                await DisplayAlertAsync("入力", "ボーレートを選んでください。", "OK");
                return;
            }

            options = new CustomerDisplaySendOptions(ComPortName: port, BaudRate: baud);
        }

        try
        {
            await _customerDisplay.SendTwoLinesAsync(line1, line2, options);
            await DisplayAlertAsync("送信", "データを送りました。表示されない場合は接続方法（プリンター / COM）やドライバを確認してください。", "OK");
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
        BaudRatePicker.SelectedIndex = 0;
    }
}
