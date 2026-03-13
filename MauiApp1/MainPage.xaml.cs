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
		await Shell.Current.GoToAsync(nameof(OrderPage));
	}

	/// <summary>
	/// TODOリスト画面へ遷移するボタンのクリックイベント。
	/// 
	/// - View（MainPage）は、単に「どの画面へ遷移するか」を決めるだけ。
	/// - 実際のTODO管理ロジックは、TodoListPage＋TodoListViewModel側に集約する。
	/// </summary>
	private async void OnTodoClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(TodoListPage));
	}
}
