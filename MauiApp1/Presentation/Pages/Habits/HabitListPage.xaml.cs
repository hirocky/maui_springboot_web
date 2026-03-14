using MauiApp1.Presentation.ViewModels.Habits;

namespace MauiApp1.Presentation.Pages.Habits;

/// <summary>
/// 習慣登録画面の View（コードビハインド）。
/// ViewModel を BindingContext にセットし、表示時に一覧・カテゴリをロードする。
/// </summary>
public partial class HabitListPage : ContentPage
{
    public HabitListPage(HabitListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        Loaded += async (_, _) =>
        {
            await viewModel.LoadAsync();
        };
    }
}
