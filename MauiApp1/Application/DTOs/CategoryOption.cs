namespace MauiApp1.Application.DTOs;

/// <summary>
/// 習慣登録画面のカテゴリ Picker 用の表示用オプション。
/// Id=0 は「未分類」を表す。
/// </summary>
public class CategoryOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
