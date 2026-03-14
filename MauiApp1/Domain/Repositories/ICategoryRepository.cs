using MauiApp1.Domain.Entities;

namespace MauiApp1.Domain.Repositories;

/// <summary>
/// カテゴリデータへのアクセスを抽象化したリポジトリのインターフェース。
///
/// ドメイン層の一部。アプリケーション層やViewModelはこのIFのみを参照し、
/// 実際の永続化（MySQL等）には依存しないようにする。
/// </summary>
public interface ICategoryRepository
{
    /// <summary>全カテゴリを取得。SortOrderの昇順を想定。</summary>
    Task<IReadOnlyList<Category>> GetAllAsync();

    /// <summary>IDで1件取得。見つからない場合はnull。</summary>
    Task<Category?> GetByIdAsync(int id);

    /// <summary>カテゴリを新規追加し、Idをセットしたエンティティを返す。</summary>
    Task<Category> AddAsync(Category category);

    /// <summary>カテゴリを更新。</summary>
    Task UpdateAsync(Category category);

    /// <summary>カテゴリを削除。</summary>
    Task DeleteAsync(int id);
}
