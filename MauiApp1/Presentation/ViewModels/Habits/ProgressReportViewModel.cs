using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp1.Application.DTOs.Habits;
using MauiApp1.Application.Services;
using MauiApp1.Presentation.ViewModels;

namespace MauiApp1.Presentation.ViewModels.Habits;

/// <summary>
/// 「進捗レポート」画面用の ViewModel。
///
/// プレゼンテーション層に属し、各習慣の達成率と対象期間内の達成日一覧を保持する。
/// HabitService.GetProgressReportAsync を呼び出し、結果を HabitProgressItem のリストとして
/// View に提供する。カレンダー表示や達成率の表示は View がこのデータをどう描画するかに委ねる。
/// </summary>
public class ProgressReportViewModel : BaseViewModel
{
    private readonly HabitService _habitService;

    /// <summary>
    /// 習慣ごとの進捗（達成率・達成日一覧）を保持するコレクション。
    /// 進捗レポート画面のメインのリストにバインドする。
    /// </summary>
    public ObservableCollection<HabitProgressItem> ProgressItems { get; } = new();

    /// <summary>
    /// 画面表示時や期間変更時に進捗データを再取得するコマンド。
    /// </summary>
    public ICommand LoadCommand { get; }

    /// <summary>
    /// 集計期間の開始日。デフォルトは過去4週間の開始日。
    /// 将来、ピッカーで変更可能にする場合はこれを TwoWay バインドする。
    /// </summary>
    private DateTime _periodFrom = DateTime.Today.AddDays(-28);

    public DateTime PeriodFrom
    {
        get => _periodFrom;
        set => SetProperty(ref _periodFrom, value);
    }

    /// <summary>
    /// 集計期間の終了日。デフォルトは今日。
    /// </summary>
    private DateTime _periodTo = DateTime.Today;

    public DateTime PeriodTo
    {
        get => _periodTo;
        set => SetProperty(ref _periodTo, value);
    }

    public ProgressReportViewModel(HabitService habitService)
    {
        _habitService = habitService;
        LoadCommand = new Command(async () => await LoadAsync());
    }

    /// <summary>
    /// 進捗レポート用データをサービスから取得し、ProgressItems を更新する。
    /// </summary>
    public async Task LoadAsync()
    {
        ProgressItems.Clear();
        var from = PeriodFrom.Date;
        var to = PeriodTo.Date;
        if (from > to)
        {
            var t = from;
            from = to;
            to = t;
        }

        var items = await _habitService.GetProgressReportAsync(from, to);
        foreach (var item in items)
        {
            ProgressItems.Add(item);
        }
    }
}
