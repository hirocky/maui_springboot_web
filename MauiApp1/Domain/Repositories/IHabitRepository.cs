using MauiApp1.Domain.Entities;

namespace MauiApp1.Domain.Repositories;

/// <summary>
/// 習慣（Habit）データへのアクセスを抽象化したリポジトリのインターフェース。
///
/// ドメイン層に属する。永続化の実装詳細はインフラ層に閉じ込める。
/// </summary>
public interface IHabitRepository
{
    /// <summary>全習慣を取得。カテゴリID順・作成日順などはアプリケーション層で制御可能。</summary>
    Task<IReadOnlyList<Habit>> GetAllAsync();

    /// <summary>IDで1件取得。見つからない場合はnull。</summary>
    Task<Habit?> GetByIdAsync(int id);

    /// <summary>習慣を新規追加し、Idをセットしたエンティティを返す。</summary>
    Task<Habit> AddAsync(Habit habit);

    /// <summary>習慣を更新。</summary>
    Task UpdateAsync(Habit habit);

    /// <summary>習慣を削除。</summary>
    Task DeleteAsync(int id);
}
