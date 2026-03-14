namespace MauiApp1.Domain.Entities;

/// <summary>
/// 習慣（ルーチン）を表すドメインモデル。
///
/// ドメイン層に属し、「毎日やりたいこと」の定義を表現する。
/// - 習慣名、目標頻度（週何回か）、色設定、カテゴリ を持つ。
/// </summary>
public class Habit
{
    /// <summary>主キー。</summary>
    public int Id { get; set; }

    /// <summary>習慣の名前（例: 朝のストレッチ、読書30分）。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 目標頻度：週に何回行うか。
    /// 例: 7＝毎日、3＝週3回。
    /// 達成率計算時に「週あたりの目標回数」として使う。
    /// </summary>
    public int TargetFrequencyPerWeek { get; set; }

    /// <summary>
    /// 画面表示用の色（例: #4CAF50）。
    /// ヘックス文字列で保持し、UIでそのままバインド可能にする。
    /// </summary>
    public string ColorHex { get; set; } = "#6200EE";

    /// <summary>所属カテゴリID。0の場合は未分類。</summary>
    public int CategoryId { get; set; }

    /// <summary>作成日時。インフラ層でセットされる想定。</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// ナビゲーション用：紐づくカテゴリ（リポジトリでJOINして埋める場合に使用）。
    /// 必須ではないが、一覧表示でカテゴリ名を出したいときに便利。
    /// </summary>
    public Category? Category { get; set; }
}
