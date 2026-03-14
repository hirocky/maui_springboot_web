namespace MauiApp1.Application.DTOs;

/// <summary>
/// 習慣登録画面の目標頻度 Picker 用の表示用オプション。
/// Value は週あたりの回数（1～7）。
/// </summary>
public class FrequencyOption
{
    public int Value { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
