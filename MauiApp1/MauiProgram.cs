using Microsoft.Extensions.Logging;

namespace MauiApp1;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
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

#if WINDOWS
		// プレゼンテーション層サービス
		// - OS 依存のメッセージボックス表示を抽象化したサービスを DI に登録する。
		// - ViewModel からは IMessageBoxService 経由で利用し、user32.dll には依存しない。
		builder.Services.AddSingleton<Presentation.Services.IMessageBoxService, Infrastructure.Platform.WindowsMessageBoxService>();
#endif

		// ViewModel
		builder.Services.AddTransient<Presentation.ViewModels.TodoListViewModel>();

		// View（ページ）
		builder.Services.AddTransient<TodoListPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
