using Microsoft.Maui.Controls;

namespace MauiApp1.Presentation.Services;

/// <summary>
/// 機能画面をサブウィンドウで開く（Windows ではサブは常に 1 つ、別ボタンで中身を差し替え。ホームはメイン右上狭幅。それ以外は Shell 遷移）。
/// </summary>
public interface IFeatureWindowService
{
    Task OpenFeatureAsync<TPage>() where TPage : ContentPage;
}
