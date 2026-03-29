namespace MauiApp1;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private async void OnOrderClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("OrderPage");
	}

	private async void OnTodoClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("TodoListPage");
	}

	private async void OnHabitRecordClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("HabitRecordHubPage");
	}

	private async void OnReceiptPrintClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("ReceiptPrintPage");
	}
}
