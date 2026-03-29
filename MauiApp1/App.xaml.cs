namespace MauiApp1;

/// <summary>
/// アプリケーション全体のエントリポイントとなるクラス。
/// 
/// - MAUI の Application 基底クラスを継承する。
/// - 名前解決の競合を避けるため、明示的に完全修飾名を指定している。
/// </summary>
public partial class App : Microsoft.Maui.Controls.Application
{
	public App()
	{
		InitializeComponent();
	}

#if WINDOWS
	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
		window.Created += (_, _) => Infrastructure.Platform.WindowsWindowLayout.PlaceHomeLauncher(window);
		return window;
	}
#else
	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
#endif
}