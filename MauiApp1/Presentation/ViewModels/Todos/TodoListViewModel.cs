using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MauiApp1.Application.Services;
using MauiApp1.Domain.Entities;
using MauiApp1.Presentation.Services;
using MauiApp1.Presentation.ViewModels;

namespace MauiApp1.Presentation.ViewModels.Todos;

/// <summary>
/// TODOリスト画面用のViewModel。
///
/// - 「プレゼンテーション層」に位置し、画面ロジックを担当する。
/// - DBやWebViewなどの具体的な実装詳細には直接触れず、
///   アプリケーションサービス（TodoService）を通じてドメインの操作を行う。
/// - View（XAML）は、このクラスのプロパティとコマンドにバインドするだけにすることで、
///   UIとロジックの分離（MVVM）を実現する。
/// </summary>
public class TodoListViewModel : BaseViewModel
{
    private readonly TodoService _todoService;
    private readonly IMessageBoxService? _messageBoxService;

    public ObservableCollection<TodoItem> Todos { get; } = new();

    private string _newTitle = string.Empty;

    /// <summary>
    /// 画面の入力欄にバインドされる新規TODOタイトル。
    /// </summary>
    public string NewTitle
    {
        get => _newTitle;
        set
        {
            // 入力値を更新して画面に通知する。
            SetProperty(ref _newTitle, value);
            // もし Command の実行可否に入力値を使う場合は、
            // ここで ChangeCanExecute を呼び出すのが定石。
            // （現在は CanExecute を使っていないが、MVVM の定石として例を残しておく）
            if (AddCommand is Command addCmd)
            {
                addCmd.ChangeCanExecute();
            }
        }
    }

    /// <summary>
    /// 画面から呼ばれるコマンド。
    /// </summary>
    public ICommand LoadCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand ToggleCompletedCommand { get; }
    public ICommand DeleteCommand { get; }

    public TodoListViewModel(
        TodoService todoService,
        IMessageBoxService? messageBoxService = null)
    {
        _todoService = todoService;
        // IMessageBoxService は「OS 依存の表示ロジック」を隠蔽するための抽象。
        // - Windows では P/Invoke を使った実装が DI コンテナに登録される。
        // - 他プラットフォームやテスト環境では null のままでも動くようにしておく。
        _messageBoxService = messageBoxService;

        // MAUI標準のCommandを利用して、UIからの操作をViewModelのメソッドにひも付ける。
        LoadCommand = new Command(async () => await LoadAsync());

        // 追加ボタンの有効/無効制御は、AddAsync 内で NewTitle を検証するだけにして、
        // Button 側は常に押せるようにしておく（学習しやすさ・挙動の分かりやすさを優先）。
        AddCommand = new Command(async () => await AddAsync());
        ToggleCompletedCommand = new Command(async (object? p) => await ToggleCompletedAsync(p as TodoItem));
        DeleteCommand = new Command(async (object? p) => await DeleteAsync(p as TodoItem));
    }

    /// <summary>
    /// 画面表示時などにTODO一覧を読み込む処理。
    /// </summary>
    public async Task LoadAsync()
    {
        Todos.Clear();

        var items = await _todoService.GetTodosAsync();
        foreach (var item in items)
        {
            Todos.Add(item);
        }
    }

    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTitle))
        {
            return;
        }

        var item = await _todoService.AddTodoAsync(NewTitle);
        // 新しいアイテムは先頭に追加して、最近追加したものが上に来るようにする。
        Todos.Insert(0, item);

        // MVVM＋レイヤード構成的な観点では、
        // - 「OS 標準のメッセージボックスを出す」という具体的な実装詳細はインフラ／プラットフォーム層に置き、
        // - ViewModel からは IMessageBoxService を通じて利用する。
        //
        // こうすることで、ViewModel 自体は user32.dll や P/Invoke の存在を知らずに済み、
        // 将来「別の UI フレームワーク」や「別 OS」に移植するときも差し替えが容易になる。
        _messageBoxService?.ShowInfo($"タスクを追加しました: {item.Title}", "TODO 追加");

        NewTitle = string.Empty;
    }

    private async Task ToggleCompletedAsync(TodoItem item)
    {
        if (item == null)
        {
            return;
        }

        await _todoService.ToggleCompletedAsync(item);

        // 操作後の一覧再読込（シンプルさ重視）。
        await LoadAsync();
    }

    private async Task DeleteAsync(TodoItem item)
    {
        if (item == null)
        {
            return;
        }

        await _todoService.DeleteAsync(item);

        Todos.Remove(item);
    }
}
