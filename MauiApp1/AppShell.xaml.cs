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
	}
}
