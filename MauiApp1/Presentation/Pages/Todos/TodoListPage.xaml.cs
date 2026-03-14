using MauiApp1.Presentation.ViewModels.Todos;

namespace MauiApp1.Presentation.Pages.Todos;

/// <summary>
/// TODOリスト画面のView。
///
/// - XAMLで定義したレイアウトに対応するコードビハインド。
/// - MVVMでは、ここには「ナビゲーションやライフサイクルなど、Viewに密接な処理」のみを書く。
/// - ビジネスロジックやDBアクセスはViewModelやサービスに委譲する。
/// </summary>
public partial class TodoListPage : ContentPage
{
    public TodoListPage(TodoListViewModel viewModel)
    {
        InitializeComponent();

        // DIコンテナから受け取ったViewModelをBindingContextにセットすることで、
        // XAML側のBindingが有効になる。
        BindingContext = viewModel;

        // 画面表示時にTODO一覧をロードする。
        Loaded += async (_, _) =>
        {
            await viewModel.LoadAsync();
        };
    }
}
