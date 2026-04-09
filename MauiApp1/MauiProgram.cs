using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MauiApp1.Infrastructure.Configuration;

namespace MauiApp1;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

#if WINDOWS
		builder.Configuration.AddJsonFile("appsettings.windows.json", optional: true, reloadOnChange: false);
		var receiptPrinterSettings = builder.Configuration
			.GetSection(ReceiptPrinterSettings.SectionName)
			.Get<ReceiptPrinterSettings>() ?? new ReceiptPrinterSettings();
		var customerDisplaySettings = builder.Configuration
			.GetSection(CustomerDisplaySettings.SectionName)
			.Get<CustomerDisplaySettings>() ?? new CustomerDisplaySettings();
		builder.Services.AddSingleton(receiptPrinterSettings);
		builder.Services.AddSingleton(customerDisplaySettings);
#endif

		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// 依存性注入（DI）コンテナへの登録。
		// MVVM＋レイヤード構成では、上位レイヤー（ViewやViewModel）が
		// 下位レイヤー（リポジトリなど）の実装クラスを直接newしないようにするため、
		// ここでインターフェースと実装の組み合わせを宣言しておく。

		// ドメイン＋インフラ＋アプリケーション層
		// ITodoRepository の実装として、MySQL 版のリポジトリを使用する。
		// ここを書き換えるだけで、アプリケーション層や ViewModel のコードは変更せずに
		// 永続化先を差し替えられるのがレイヤード構成のメリット。
		builder.Services.AddSingleton<Domain.Repositories.ITodoRepository, Infrastructure.Data.MySqlTodoRepository>();
		builder.Services.AddSingleton<Application.Services.TodoService>();

		// 習慣（ルーチン）機能: カテゴリ・習慣・チェックインのリポジトリとアプリケーションサービス。
		builder.Services.AddSingleton<Domain.Repositories.ICategoryRepository, Infrastructure.Data.MySqlCategoryRepository>();
		builder.Services.AddSingleton<Domain.Repositories.IHabitRepository, Infrastructure.Data.MySqlHabitRepository>();
		builder.Services.AddSingleton<Domain.Repositories.ICheckInRepository, Infrastructure.Data.MySqlCheckInRepository>();
		builder.Services.AddSingleton<Application.Services.HabitService>();

		// 機能画面をサブウィンドウで開く（Windows ではサブは 1 つだけ・内容差し替え。ホームは App.CreateWindow で右上狭幅。他は Shell 遷移）。
		builder.Services.AddSingleton<Presentation.Services.IFeatureWindowService, Presentation.Services.FeatureWindowService>();

#if WINDOWS
		// プレゼンテーション層サービス
		// - OS 依存のメッセージボックス表示を抽象化したサービスを DI に登録する。
		// - ViewModel からは IMessageBoxService 経由で利用し、user32.dll には依存しない。
		builder.Services.AddSingleton<Presentation.Services.IMessageBoxService, Infrastructure.Platform.WindowsMessageBoxService>();
		// レシート印刷（クリーンアーキテクチャ: Domain ポート → Infrastructure 実装）
		builder.Services.AddSingleton<Domain.Printing.IReceiptPrinter, Infrastructure.Platform.WindowsEpsonReceiptPrinter>();
		builder.Services.AddSingleton<Domain.Printing.IPrinterDiscovery, Infrastructure.Platform.WindowsPrinterDiscovery>();
		// カスタマーディスプレイ（DM-D30 等・COM + ESC/POS）
		builder.Services.AddSingleton<Presentation.Services.ICustomerDisplayService, Infrastructure.Platform.WindowsEpsonDmD30CustomerDisplayService>();
#else
		builder.Services.AddSingleton<Domain.Printing.IReceiptPrinter, Infrastructure.Platform.NullReceiptPrinter>();
		builder.Services.AddSingleton<Domain.Printing.IPrinterDiscovery, Infrastructure.Platform.NullPrinterDiscovery>();
		builder.Services.AddSingleton<Presentation.Services.ICustomerDisplayService, Infrastructure.Platform.NullCustomerDisplayService>();
#endif
		// Application 層ユースケース
		builder.Services.AddTransient<Application.Printing.PrintReceiptUseCase>();

		// ViewModel
		builder.Services.AddTransient<Presentation.ViewModels.Todos.TodoListViewModel>();
		builder.Services.AddTransient<Presentation.ViewModels.Habits.TodayTasksViewModel>();
		builder.Services.AddTransient<Presentation.ViewModels.Habits.ProgressReportViewModel>();
		builder.Services.AddTransient<Presentation.ViewModels.Habits.HabitListViewModel>();

		// View（ページ）
		builder.Services.AddTransient<Presentation.Pages.Todos.TodoListPage>();
		builder.Services.AddTransient<Presentation.Pages.Order.OrderPage>();
		builder.Services.AddTransient<Presentation.Pages.Receipt.ReceiptPrintPage>();
		builder.Services.AddTransient<Presentation.Pages.Receipt.CustomerDisplaySamplePage>();
		builder.Services.AddTransient<Presentation.Pages.Habits.TodayTasksPage>();
		builder.Services.AddTransient<Presentation.Pages.Habits.ProgressReportPage>();
		builder.Services.AddTransient<Presentation.Pages.Habits.HabitListPage>();
		builder.Services.AddTransient<Presentation.Pages.Habits.HabitRecordHubPage>();
#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
