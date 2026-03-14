using MauiApp1.Presentation.Pages.Habits;

namespace MauiApp1;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		// Shellルーティングの登録。
		// ここでは「画面の論理名(Route)」と「実際のページクラス」をひも付ける。
		// 画面遷移時は論理名で指定することで、View同士の結合度を下げる。
		Routing.RegisterRoute(nameof(OrderPage), typeof(OrderPage));
		Routing.RegisterRoute(nameof(TodoListPage), typeof(TodoListPage));
		// 習慣記録: ハブ画面と、習慣登録・今日のタスク・進捗レポート（Presentation/Pages/Habits 配下）
		Routing.RegisterRoute(nameof(HabitRecordHubPage), typeof(HabitRecordHubPage));
		Routing.RegisterRoute(nameof(HabitListPage), typeof(HabitListPage));
		Routing.RegisterRoute(nameof(TodayTasksPage), typeof(TodayTasksPage));
		Routing.RegisterRoute(nameof(ProgressReportPage), typeof(ProgressReportPage));
	}
}
