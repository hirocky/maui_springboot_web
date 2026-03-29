#if WINDOWS
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace MauiApp1.Infrastructure.Platform;

/// <summary>
/// Windows 向けウィンドウ配置。ホーム（ランチャー）は右上・狭幅、機能サブウィンドウは画面中央。
/// </summary>
public static class WindowsWindowLayout
{
    public const double Margin = 8;
    public const double HomeWidth = 420;
    public const double FeatureMaxWidth = 960;

    public static void PlaceHomeLauncher(Window window)
    {
        var (screenW, screenH) = GetScreenLogicalSize();
        var h = Math.Clamp(screenH * 0.88, 480, screenH - Margin * 2);
        window.Width = HomeWidth;
        window.Height = h;
        window.X = Math.Max(0, screenW - HomeWidth - Margin);
        window.Y = Margin;
    }

    public static void PlaceFeatureCentered(Window window)
    {
        var (screenW, screenH) = GetScreenLogicalSize();
        var w = Math.Min(FeatureMaxWidth, screenW - Margin * 2);
        var h = Math.Clamp(screenH * 0.88, 520, screenH - Margin * 2);
        window.Width = w;
        window.Height = h;
        window.X = Math.Max(Margin, (screenW - w) / 2);
        window.Y = Math.Max(Margin, (screenH - h) / 2);
    }

    private static (double W, double H) GetScreenLogicalSize()
    {
        var d = DeviceDisplay.MainDisplayInfo;
        var density = d.Density <= 0 ? 1 : d.Density;
        return (d.Width / density, d.Height / density);
    }
}
#endif
