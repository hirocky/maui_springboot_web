using MauiApp1.Domain.Entities;

namespace MauiApp1.Application.DTOs;

/// <summary>
/// 進捗レポート画面用の1習慣あたりの集計結果を表す DTO。
///
/// 習慣ごとの達成率と、対象期間内の達成日一覧を持ち、
/// カレンダー表示や達成率表示に利用する。
/// </summary>
public class HabitProgressItem
{
    /// <summary>習慣のドメインモデル。</summary>
    public Habit Habit { get; set; } = null!;

    /// <summary>
    /// 対象期間内の達成率（0.0 ～ 1.0）。
    /// 目標が「週7回」なら、期間の週数×7 を分母に、実際のチェックイン回数を分子にして算出。
    /// </summary>
    public double AchievementRate { get; set; }

    /// <summary>対象期間内にチェックインした日付の一覧（カレンダー表示用）。</summary>
    public IReadOnlyList<DateTime> CheckInDates { get; set; } = Array.Empty<DateTime>();

    /// <summary>対象期間の開始日（表示ラベル用）。</summary>
    public DateTime PeriodFrom { get; set; }

    /// <summary>対象期間の終了日（表示ラベル用）。</summary>
    public DateTime PeriodTo { get; set; }
}
