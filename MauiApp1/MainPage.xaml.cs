namespace MauiApp1;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object? sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}

	private async void OnOrderClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("OrderPage");
	}

	/// <summary>
	/// TODOリスト画面へ遷移するボタンのクリックイベント。
	/// 実際のTODO管理ロジックは、TodoListPage＋TodoListViewModel側に集約する。
	/// </summary>
	private async void OnTodoClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("TodoListPage");
	}

	/// <summary>
	/// 習慣記録のハブ画面へ遷移する。
	/// ハブ画面から習慣登録・今日のタスク・進捗レポートの各画面へ遷移できる。
	/// 遷移先は AppShell で登録したルート名で指定（View は Habits フォルダに集約済み）。
	/// </summary>
	private async void OnHabitRecordClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("HabitRecordHubPage");
	}
}
