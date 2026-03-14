using MauiApp1.Domain.Entities;

namespace MauiApp1.Application.DTOs.Habits;

/// <summary>
/// 「今日タスク」画面の1行分のデータを表す DTO。
///
/// アプリケーション層で、習慣（Habit）と「今日すでにチェックインしているか」を
/// まとめて返すために使用する。ViewModel はこの型をそのままバインド可能な
/// コレクションとして利用できる。
/// </summary>
public class TodayHabitItem
{
    /// <summary>習慣のドメインモデル。</summary>
    public Habit Habit { get; set; } = null!;

    /// <summary>今日すでに達成済み（チェックイン済み）かどうか。</summary>
    public bool IsCheckedToday { get; set; }
}
