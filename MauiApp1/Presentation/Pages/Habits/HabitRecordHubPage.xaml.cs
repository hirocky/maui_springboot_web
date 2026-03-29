using Microsoft.Extensions.DependencyInjection;

namespace MauiApp1.Presentation.Pages.Habits;

/// <summary>
/// 習慣記録のハブ画面（コードビハインド）。
/// ナビゲーションのみ担当し、各機能画面へ遷移する。
/// サブウィンドウ内では Shell ではなく <see cref="NavigationPage"/> スタックで遷移する。
/// </summary>
public partial class HabitRecordHubPage : ContentPage
{
	private readonly IServiceProvider _services;

	public HabitRecordHubPage(IServiceProvider services)
	{
		_services = services;
		InitializeComponent();
	}

	private async void OnHabitListClicked(object? sender, EventArgs e)
	{
		if (OperatingSystem.IsWindows())
			await Navigation.PushAsync(_services.GetRequiredService<HabitListPage>());
		else
			await Shell.Current.GoToAsync(nameof(HabitListPage));
	}

	private async void OnTodayTasksClicked(object? sender, EventArgs e)
	{
		if (OperatingSystem.IsWindows())
			await Navigation.PushAsync(_services.GetRequiredService<TodayTasksPage>());
		else
			await Shell.Current.GoToAsync(nameof(TodayTasksPage));
	}

	private async void OnProgressReportClicked(object? sender, EventArgs e)
	{
		if (OperatingSystem.IsWindows())
			await Navigation.PushAsync(_services.GetRequiredService<ProgressReportPage>());
		else
			await Shell.Current.GoToAsync(nameof(ProgressReportPage));
	}
}
