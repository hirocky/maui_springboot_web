using MauiApp1.Domain.Entities;
using MauiApp1.Domain.Repositories;

namespace MauiApp1.Application.Services;

/// <summary>
/// TODO機能のアプリケーションサービス。
/// 
/// - レイヤード構成における「アプリケーション層」に相当する。
/// - 画面からの操作を、ドメインモデル＋リポジトリを使って「ユースケース」としてまとめる役割。
/// - 「どのように見せるか（UI）」や「どこに保存するか（DB）」には依存しない。
/// </summary>
public class TodoService
{
    private readonly ITodoRepository _repository;

    public TodoService(ITodoRepository repository)
    {
        // コンストラクタインジェクションで、インフラ実装に依存しないようにする。
        _repository = repository;
    }

    /// <summary>
    /// TODO一覧を作成日時の降順で取得するユースケース。
    /// </summary>
    public async Task<IReadOnlyList<TodoItem>> GetTodosAsync()
    {
        var items = await _repository.GetAllAsync();
        // 「並び順をどうするか」はUIではなくアプリケーション層が決める責務とする。
        return items
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    /// <summary>
    /// 新しいTODOを追加するユースケース。
    /// </summary>
    public async Task<TodoItem> AddTodoAsync(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("タイトルは必須です。", nameof(title));
        }

        var item = new TodoItem
        {
            Title = title.Trim(),
            IsCompleted = false,
            CreatedAt = DateTime.Now
        };

        return await _repository.AddAsync(item);
    }

    /// <summary>
    /// 完了フラグをトグル（ON/OFF）するユースケース。
    /// </summary>
    public async Task ToggleCompletedAsync(TodoItem item)
    {
        item.IsCompleted = !item.IsCompleted;
        await _repository.UpdateAsync(item);
    }

    /// <summary>
    /// TODOを削除するユースケース。
    /// </summary>
    public Task DeleteAsync(TodoItem item)
    {
        return _repository.DeleteAsync(item.Id);
    }
}

