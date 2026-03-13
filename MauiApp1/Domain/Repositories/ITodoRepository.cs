using MauiApp1.Domain.Entities;

namespace MauiApp1.Domain.Repositories;

/// <summary>
/// TODOデータへのアクセス方法を抽象化したリポジトリのインターフェース。
/// 
/// - 「ドメイン層」の一部だが、インフラ（DB）の詳細はここに書かない。
/// - アプリケーション層やViewModelは、このインターフェースだけを参照し、
///   実際の実装（MySQLやその他のDB）には依存しないようにすることで、
///   「DBを変えても上位レイヤーのコードを極力変更しない」ことを目指す。
/// </summary>
public interface ITodoRepository
{
    Task<IReadOnlyList<TodoItem>> GetAllAsync();

    Task<TodoItem> AddAsync(TodoItem item);

    Task UpdateAsync(TodoItem item);

    Task DeleteAsync(int id);
}

