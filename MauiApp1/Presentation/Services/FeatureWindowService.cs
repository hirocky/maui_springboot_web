using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MauiApp1.Presentation.Services;

/// <summary>
/// Windows では機能用サブウィンドウを常に 1 つだけ保持し、別ボタンが押されたら同一ウィンドウの内容を差し替える。
/// 初回のみ <see cref="Microsoft.Maui.Controls.Application.OpenWindow(Window)"/> で開き中央配置。ホームはメインウィンドウで右上狭幅。
/// その他のプラットフォームでは Shell へ遷移する。
/// </summary>
public sealed class FeatureWindowService : IFeatureWindowService
{
    private readonly IServiceProvider _services;

    /// <summary>機能サブウィンドウ（閉じると null になる）。</summary>
    private Window? _featureWindow;

    public FeatureWindowService(IServiceProvider services)
    {
        _services = services;
    }

    public Task OpenFeatureAsync<TPage>() where TPage : ContentPage
    {
#if WINDOWS
        var page = _services.GetRequiredService<TPage>();
        var nav = CreateFeatureNavigation(page);
        var title = page.Title ?? typeof(TPage).Name;

        if (_featureWindow is null)
        {
            var w = new Window(nav) { Title = title };
            w.Created += (_, _) => Infrastructure.Platform.WindowsWindowLayout.PlaceFeatureCentered(w);
            w.Destroying += (_, _) => _featureWindow = null;
            _featureWindow = w;
            Microsoft.Maui.Controls.Application.Current!.OpenWindow(w);
        }
        else
        {
            _featureWindow.Page = nav;
            _featureWindow.Title = title;
        }

        return Task.CompletedTask;
#else
        return Shell.Current.GoToAsync(typeof(TPage).Name);
#endif
    }

    private static NavigationPage CreateFeatureNavigation(ContentPage page) =>
        new(page)
        {
            BarBackgroundColor = Colors.White,
            BarTextColor = Colors.Black
        };
}
