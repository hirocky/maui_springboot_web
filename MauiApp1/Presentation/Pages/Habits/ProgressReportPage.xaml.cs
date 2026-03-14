using MauiApp1.Presentation.ViewModels.Habits;

namespace MauiApp1.Presentation.Pages.Habits;

/// <summary>
/// 進捗レポート画面の View（コードビハインド）。
///
/// BindingContext に ProgressReportViewModel をセットし、
/// 表示時に LoadAsync で進捗データを取得する。ナビゲーションは Home または今日タスク画面から行う。
/// </summary>
public partial class ProgressReportPage : ContentPage
{
    public ProgressReportPage(ProgressReportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        Loaded += async (_, _) =>
        {
            await viewModel.LoadAsync();
        };
    }
}
