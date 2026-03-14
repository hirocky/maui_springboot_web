using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp1.Application.DTOs;
using MauiApp1.Application.Services;

namespace MauiApp1.Presentation.ViewModels;

/// <summary>
/// 「今日タスク」画面用の ViewModel。
///
/// プレゼンテーション層に属し、次の責務を持つ。
/// - 今日やるべき習慣のチェックリストを HabitService から取得して保持する。
/// - ユーザーがチェック／未チェックを切り替えたときに HabitService を呼び出し、
///   画面の状態（IsCheckedToday）を更新する。
/// - View はこの ViewModel のプロパティとコマンドにバインドするだけで、
///   DB やドメインの詳細を知らなくてよい（MVVM の分離）。
/// </summary>
public class TodayTasksViewModel : BaseViewModel
{
    private readonly HabitService _habitService;

    /// <summary>
    /// 今日の習慣タスク一覧。各要素は習慣と「今日チェック済みか」の情報を持つ。
    /// View の CollectionView の ItemsSource にバインドする。
    /// </summary>
    public ObservableCollection<TodayHabitItem> TodayItems { get; } = new();

    /// <summary>
    /// 画面表示時に一覧を再読み込みするコマンド。
    /// ページの Loaded や「更新」ボタンから呼び出す。
    /// </summary>
    public ICommand LoadCommand { get; }

    /// <summary>
    /// ある習慣について「今日の達成」をトグルするコマンド。
    /// チェック済みなら未達成に、未チェックなら達成に更新する。
    /// パラメータは TodayHabitItem（Binding で CommandParameter に渡す）。
    /// </summary>
    public ICommand ToggleCheckCommand { get; }

    private DateTime _targetDate = DateTime.Today;

    /// <summary>
    /// 表示対象の日付（「今日」）。将来、日付ピッカーで変更できるようにする場合はこれをバインドする。
    /// </summary>
    public DateTime TargetDate
    {
        get => _targetDate;
        set => SetProperty(ref _targetDate, value);
    }

    public TodayTasksViewModel(HabitService habitService)
    {
        _habitService = habitService;

        LoadCommand = new Command(async () => await LoadAsync());
        ToggleCheckCommand = new Command<TodayHabitItem>(async item => await ToggleCheckAsync(item));
    }

    /// <summary>
    /// 今日タスク一覧をサービスから取得し、TodayItems を更新する。
    /// 画面表示時およびチェックトグル後に呼ばれる。
    /// </summary>
    public async Task LoadAsync()
    {
        TodayItems.Clear();
        var items = await _habitService.GetTodayTasksAsync(TargetDate);
        foreach (var item in items)
        {
            TodayItems.Add(item);
        }
    }

    /// <summary>
    /// 指定した習慣の「今日の達成」をトグルする。
    /// サービスでチェックイン／解除を行い、その後一覧を再読み込みして表示を同期する。
    /// </summary>
    private async Task ToggleCheckAsync(TodayHabitItem? item)
    {
        if (item == null)
            return;

        await _habitService.ToggleCheckInAsync(item.Habit.Id, TargetDate);
        // ローカルのフラグを反転させて即時フィードバックしてもよいが、
        // 再読み込みで一貫した状態にしておく。
        await LoadAsync();
    }
}
