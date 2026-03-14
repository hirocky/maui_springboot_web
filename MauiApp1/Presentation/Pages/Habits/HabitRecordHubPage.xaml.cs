namespace MauiApp1.Presentation.Pages.Habits;

/// <summary>
/// 習慣記録のハブ画面（コードビハインド）。
/// ナビゲーションのみ担当し、各機能画面へ遷移する。
/// </summary>
public partial class HabitRecordHubPage : ContentPage
{
	public HabitRecordHubPage()
	{
		InitializeComponent();
	}

	private async void OnHabitListClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(HabitListPage));
	}

	private async void OnTodayTasksClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(TodayTasksPage));
	}

	private async void OnProgressReportClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(ProgressReportPage));
	}
}
