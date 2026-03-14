using MauiApp1.Domain.Entities;

namespace MauiApp1.Domain.Repositories;

/// <summary>
/// チェックイン（達成記録）データへのアクセスを抽象化したリポジトリのインターフェース。
///
/// どの日付にどの習慣を完了したかの記録の永続化を担当する抽象。
/// </summary>
public interface ICheckInRepository
{
    /// <summary>習慣IDと日付でチェックインが存在するか。</summary>
    Task<bool> ExistsAsync(int habitId, DateTime date);

    /// <summary>習慣IDと日付でチェックインを1件追加。</summary>
    Task<CheckIn> AddAsync(CheckIn checkIn);

    /// <summary>指定した習慣の、指定日付のチェックインを削除。</summary>
    Task DeleteAsync(int habitId, DateTime date);

    /// <summary>ある習慣の、指定期間内のチェックイン日付一覧を取得（達成率・カレンダー用）。</summary>
    Task<IReadOnlyList<DateTime>> GetCheckInDatesAsync(int habitId, DateTime from, DateTime to);

    /// <summary>複数習慣について、指定日のチェックイン有無をまとめて取得（今日タスク画面用）。</summary>
    Task<IReadOnlySet<int>> GetCheckedInHabitIdsForDateAsync(DateTime date);
}
