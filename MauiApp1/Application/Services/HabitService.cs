using MauiApp1.Application.DTOs;
using MauiApp1.Domain.Entities;
using MauiApp1.Domain.Repositories;

namespace MauiApp1.Application.Services;

/// <summary>
/// 習慣（ルーチン）機能のアプリケーションサービス。
///
/// レイヤード構成の「アプリケーション層」に位置し、
/// 「今日のタスク一覧」「チェックイン／チェックアウト」「進捗レポート」といった
/// ユースケースを、リポジトリを組み合わせて実現する。
/// UIの詳細やDBの具体的な実装には依存せず、ドメイン＋リポジトリIFのみに依存する。
/// </summary>
public class HabitService
{
    private readonly IHabitRepository _habitRepository;
    private readonly ICheckInRepository _checkInRepository;
    private readonly ICategoryRepository _categoryRepository;

    public HabitService(
        IHabitRepository habitRepository,
        ICheckInRepository checkInRepository,
        ICategoryRepository categoryRepository)
    {
        _habitRepository = habitRepository;
        _checkInRepository = checkInRepository;
        _categoryRepository = categoryRepository;
    }

    /// <summary>
    /// 今日やるべき習慣のチェックリスト用データを取得する。
    /// 全習慣と「今日すでにチェックインしているか」をまとめた DTO のリストを返す。
    /// </summary>
    /// <param name="today">「今日」の日付。通常は DateTime.Today を渡す。</param>
    public async Task<IReadOnlyList<TodayHabitItem>> GetTodayTasksAsync(DateTime today)
    {
        var habits = await _habitRepository.GetAllAsync();
        var checkedInIds = await _checkInRepository.GetCheckedInHabitIdsForDateAsync(today);

        return habits
            .Select(h => new TodayHabitItem
            {
                Habit = h,
                IsCheckedToday = checkedInIds.Contains(h.Id)
            })
            .ToList();
    }

    /// <summary>
    /// 指定した習慣について、今日の日付でチェックイン（達成）を記録する。
    /// すでに同じ日付でチェックイン済みの場合は何もしない（冪等）。
    /// </summary>
    public async Task CheckInAsync(int habitId, DateTime date)
    {
        var dateOnly = date.Date;
        var exists = await _checkInRepository.ExistsAsync(habitId, dateOnly);
        if (exists)
            return;

        var checkIn = new CheckIn
        {
            HabitId = habitId,
            CheckInDate = dateOnly,
            CreatedAt = DateTime.Now
        };
        await _checkInRepository.AddAsync(checkIn);
    }

    /// <summary>
    /// 指定した習慣について、今日の日付のチェックインを解除する（未達成に戻す）。
    /// </summary>
    public async Task UncheckAsync(int habitId, DateTime date)
    {
        await _checkInRepository.DeleteAsync(habitId, date.Date);
    }

    /// <summary>
    /// 習慣のチェック状態をトグルする。
    /// 今日すでにチェック済みなら解除、未チェックならチェックインする。
    /// </summary>
    public async Task ToggleCheckInAsync(int habitId, DateTime date)
    {
        var exists = await _checkInRepository.ExistsAsync(habitId, date.Date);
        if (exists)
            await _checkInRepository.DeleteAsync(habitId, date.Date);
        else
            await CheckInAsync(habitId, date);
    }

    /// <summary>
    /// 進捗レポート用に、各習慣の達成率と対象期間内の達成日一覧を取得する。
    /// 対象期間は「過去 N 週間」などで呼び出し元が指定する。
    /// </summary>
    /// <param name="from">集計期間の開始日（含む）。</param>
    /// <param name="to">集計期間の終了日（含む）。</param>
    public async Task<IReadOnlyList<HabitProgressItem>> GetProgressReportAsync(DateTime from, DateTime to)
    {
        var habits = await _habitRepository.GetAllAsync();
        var result = new List<HabitProgressItem>();

        foreach (var habit in habits)
        {
            var dates = await _checkInRepository.GetCheckInDatesAsync(habit.Id, from, to);
            var totalDays = (to.Date - from.Date).Days + 1;
            // 目標: 週 targetFrequencyPerWeek 回 → 期間内の週数 × targetFrequencyPerWeek が最大回数
            var weeks = Math.Max(0.0001, totalDays / 7.0);
            var targetTotal = (int)Math.Ceiling(weeks * habit.TargetFrequencyPerWeek);
            var rate = targetTotal <= 0 ? 0.0 : Math.Min(1.0, (double)dates.Count / targetTotal);

            result.Add(new HabitProgressItem
            {
                Habit = habit,
                AchievementRate = rate,
                CheckInDates = dates.ToList(),
                PeriodFrom = from,
                PeriodTo = to
            });
        }

        return result;
    }

    /// <summary>
    /// 全習慣を取得する（習慣マスタの一覧）。進捗レポートや設定画面で利用可能。
    /// </summary>
    public Task<IReadOnlyList<Habit>> GetAllHabitsAsync()
    {
        return _habitRepository.GetAllAsync();
    }

    /// <summary>
    /// 全カテゴリを取得する。習慣の登録・編集画面でドロップダウン等に利用。
    /// </summary>
    public Task<IReadOnlyList<Category>> GetAllCategoriesAsync()
    {
        return _categoryRepository.GetAllAsync();
    }

    /// <summary>
    /// 習慣を新規追加する。習慣登録画面から呼び出す。
    /// </summary>
    public async Task<Habit> AddHabitAsync(string name, int targetFrequencyPerWeek, string colorHex, int categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("習慣名は必須です。", nameof(name));
        if (targetFrequencyPerWeek < 1 || targetFrequencyPerWeek > 7)
            targetFrequencyPerWeek = 7;

        var habit = new Habit
        {
            Name = name.Trim(),
            TargetFrequencyPerWeek = targetFrequencyPerWeek,
            ColorHex = string.IsNullOrWhiteSpace(colorHex) ? "#6200EE" : colorHex.Trim(),
            CategoryId = categoryId,
            CreatedAt = DateTime.Now
        };
        return await _habitRepository.AddAsync(habit);
    }

    /// <summary>
    /// 習慣を更新する。編集画面から呼び出す。
    /// </summary>
    public async Task UpdateHabitAsync(Habit habit)
    {
        if (string.IsNullOrWhiteSpace(habit.Name))
            throw new ArgumentException("習慣名は必須です。", nameof(habit.Name));
        await _habitRepository.UpdateAsync(habit);
    }

    /// <summary>
    /// 習慣を削除する。チェックイン記録は DB の CASCADE で削除される想定。
    /// </summary>
    public Task DeleteHabitAsync(int habitId)
    {
        return _habitRepository.DeleteAsync(habitId);
    }
}
