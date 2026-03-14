namespace MauiApp1.Domain.Entities;

/// <summary>
/// 習慣の達成記録（チェックイン）を表すドメインモデル。
///
/// 「どの日付に、どの習慣を完了したか」を1行で表現する。
/// 進捗レポートの達成率計算やカレンダー表示の元データとなる。
/// </summary>
public class CheckIn
{
    /// <summary>主キー。</summary>
    public int Id { get; set; }

    /// <summary>どの習慣を達成したかを示す習慣ID。</summary>
    public int HabitId { get; set; }

    /// <summary>
    /// 達成した日付（日付のみ。時刻は持たない想定）。
    /// 同一習慣・同一日付で重複チェックインしない運用を想定。
    /// </summary>
    public DateTime CheckInDate { get; set; }

    /// <summary>記録を作成した日時。</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>ナビゲーション用：紐づく習慣（必要に応じてリポジトリで読み込む）。</summary>
    public Habit? Habit { get; set; }
}
