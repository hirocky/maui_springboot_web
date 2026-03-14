using MauiApp1.Presentation.ViewModels.Habits;

namespace MauiApp1.Presentation.Pages.Habits;

/// <summary>
/// 今日タスク画面の View（コードビハインド）。
///
/// MVVM では、ナビゲーションやライフサイクルなど View に密接な処理のみをここに記述する。
/// ビジネスロジックやデータ取得は TodayTasksViewModel に委譲する。
/// - BindingContext に DI で受け取った ViewModel をセット
/// - Loaded 時に ViewModel.LoadAsync を呼び出して一覧を表示
/// - 「進捗レポートを見る」は Shell ナビゲーションで遷移
/// </summary>
public partial class TodayTasksPage : ContentPage
{
    public TodayTasksPage(TodayTasksViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        Loaded += async (_, _) =>
        {
            await viewModel.LoadAsync();
            // 習慣が0件のときのメッセージ表示（オプション）
            EmptyMessageLabel.IsVisible = viewModel.TodayItems.Count == 0;
        };
    }

    /// <summary>
    /// 進捗レポート画面へ遷移する。遷移先は AppShell で登録したルート名で指定する。
    /// </summary>
    private async void OnOpenProgressReportClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProgressReportPage));
    }
}
