using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp1.Application.DTOs.Habits;
using MauiApp1.Application.Services;
using MauiApp1.Domain.Entities;
using MauiApp1.Presentation.Services;
using MauiApp1.Presentation.ViewModels;

namespace MauiApp1.Presentation.ViewModels.Habits;

/// <summary>
/// 習慣登録画面用の ViewModel。
/// 習慣一覧の表示・新規追加・削除を HabitService 経由で行う。
/// カテゴリ・色・目標頻度は Picker 用のオプションリストを用意し、フォームから選択させる。
/// </summary>
public class HabitListViewModel : BaseViewModel
{
    private readonly HabitService _habitService;
    private readonly IMessageBoxService? _messageBoxService;

    public ObservableCollection<Habit> Habits { get; } = new();

    /// <summary>新規追加フォーム: 習慣名。</summary>
    private string _newHabitName = string.Empty;
    public string NewHabitName
    {
        get => _newHabitName;
        set => SetProperty(ref _newHabitName, value);
    }

    /// <summary>カテゴリ Picker 用（未分類 + DB のカテゴリ一覧）。</summary>
    public ObservableCollection<CategoryOption> CategoryOptions { get; } = new();

    /// <summary>色 Picker 用（固定のプリセット）。</summary>
    public ObservableCollection<ColorOption> ColorOptions { get; } = new();

    /// <summary>目標頻度 Picker 用（週1回～毎日）。</summary>
    public ObservableCollection<FrequencyOption> FrequencyOptions { get; } = new();

    /// <summary>フォームで選択中のカテゴリ。未選択時は未分類とする。</summary>
    private CategoryOption? _selectedCategoryOption;
    public CategoryOption? SelectedCategoryOption
    {
        get => _selectedCategoryOption;
        set => SetProperty(ref _selectedCategoryOption, value);
    }

    /// <summary>フォームで選択中の色。</summary>
    private ColorOption? _selectedColorOption;
    public ColorOption? SelectedColorOption
    {
        get => _selectedColorOption;
        set => SetProperty(ref _selectedColorOption, value);
    }

    /// <summary>フォームで選択中の目標頻度。</summary>
    private FrequencyOption? _selectedFrequencyOption;
    public FrequencyOption? SelectedFrequencyOption
    {
        get => _selectedFrequencyOption;
        set => SetProperty(ref _selectedFrequencyOption, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }

    public HabitListViewModel(HabitService habitService, IMessageBoxService? messageBoxService = null)
    {
        _habitService = habitService;
        _messageBoxService = messageBoxService;
#pragma warning disable CA1416 // Command のプラットフォーム注釈に対する誤検知を局所抑制
        LoadCommand = new Command(async () => await LoadAsync());
        AddCommand = new Command(async () => await AddAsync());
        DeleteCommand = new Command(async (object? p) => await DeleteAsync(p as Habit));
#pragma warning restore CA1416

        // 色プリセット（Picker の表示名とヘックス値）
        ColorOptions.Add(new ColorOption { Name = "紫", Hex = "#6200EE" });
        ColorOptions.Add(new ColorOption { Name = "緑", Hex = "#4CAF50" });
        ColorOptions.Add(new ColorOption { Name = "青", Hex = "#2196F3" });
        ColorOptions.Add(new ColorOption { Name = "オレンジ", Hex = "#FF9800" });
        ColorOptions.Add(new ColorOption { Name = "ピンク", Hex = "#E91E63" });
        ColorOptions.Add(new ColorOption { Name = "濃い紫", Hex = "#9C27B0" });

        // 目標頻度（週1回～毎日）
        FrequencyOptions.Add(new FrequencyOption { Value = 7, DisplayName = "毎日" });
        FrequencyOptions.Add(new FrequencyOption { Value = 6, DisplayName = "週6回" });
        FrequencyOptions.Add(new FrequencyOption { Value = 5, DisplayName = "週5回" });
        FrequencyOptions.Add(new FrequencyOption { Value = 4, DisplayName = "週4回" });
        FrequencyOptions.Add(new FrequencyOption { Value = 3, DisplayName = "週3回" });
        FrequencyOptions.Add(new FrequencyOption { Value = 2, DisplayName = "週2回" });
        FrequencyOptions.Add(new FrequencyOption { Value = 1, DisplayName = "週1回" });
    }

    /// <summary>
    /// 習慣一覧とカテゴリ一覧を読み込む。Picker の初期選択もここで行う。
    /// </summary>
    public async Task LoadAsync()
    {
        Habits.Clear();
        var habits = await _habitService.GetAllHabitsAsync();
        foreach (var h in habits)
            Habits.Add(h);

        // カテゴリオプション（未分類 + DB のカテゴリ）
        CategoryOptions.Clear();
        CategoryOptions.Add(new CategoryOption { Id = 0, Name = "未分類" });
        var categories = await _habitService.GetAllCategoriesAsync();
        foreach (var c in categories)
            CategoryOptions.Add(new CategoryOption { Id = c.Id, Name = c.Name });

        // 初期選択
        if (SelectedCategoryOption == null && CategoryOptions.Count > 0)
            SelectedCategoryOption = CategoryOptions[0];
        if (SelectedColorOption == null && ColorOptions.Count > 0)
            SelectedColorOption = ColorOptions[0];
        if (SelectedFrequencyOption == null && FrequencyOptions.Count > 0)
            SelectedFrequencyOption = FrequencyOptions[0];
    }

    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(NewHabitName))
            return;

        var categoryId = SelectedCategoryOption?.Id ?? 0;
        var colorHex = SelectedColorOption?.Hex ?? "#6200EE";
        var frequency = SelectedFrequencyOption?.Value ?? 7;

        var habit = await _habitService.AddHabitAsync(NewHabitName, frequency, colorHex, categoryId);
        Habits.Insert(0, habit);
        NewHabitName = string.Empty;

        // 追加完了をユーザーに通知。IMessageBoxService は Windows では DI 登録済み、他プラットフォームでは null のため ?. で呼ぶ。
        _messageBoxService?.ShowInfo($"習慣を追加しました: {habit.Name}", "習慣登録");
    }

    private async Task DeleteAsync(Habit? habit)
    {
        if (habit == null) return;
        await _habitService.DeleteHabitAsync(habit.Id);
        Habits.Remove(habit);
    }
}
