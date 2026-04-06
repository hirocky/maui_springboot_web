using MauiApp1.Presentation.Pages.Habits;
using MauiApp1.Presentation.Pages.Order;
using MauiApp1.Presentation.Pages.Receipt;
using MauiApp1.Presentation.Pages.Todos;
using MauiApp1.Presentation.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MauiApp1;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private static IFeatureWindowService FeatureWindows =>
		Microsoft.Maui.Controls.Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<IFeatureWindowService>();

	private async void OnOrderClicked(object? sender, EventArgs e)
	{
		await FeatureWindows.OpenFeatureAsync<OrderPage>();
	}

	private async void OnTodoClicked(object? sender, EventArgs e)
	{
		await FeatureWindows.OpenFeatureAsync<TodoListPage>();
	}

	private async void OnHabitRecordClicked(object? sender, EventArgs e)
	{
		await FeatureWindows.OpenFeatureAsync<HabitRecordHubPage>();
	}

	private async void OnReceiptPrintClicked(object? sender, EventArgs e)
	{
		await FeatureWindows.OpenFeatureAsync<ReceiptPrintPage>();
	}

	private async void OnCustomerDisplayClicked(object? sender, EventArgs e)
	{
		await FeatureWindows.OpenFeatureAsync<CustomerDisplaySamplePage>();
	}
}
