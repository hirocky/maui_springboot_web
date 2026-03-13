namespace MauiApp1.Domain.Entities;

/// <summary>
/// TODOアイテムのドメインモデル。
/// 
/// - MVVM＋レイヤード構成においては「ドメイン層」に属するクラス。
/// - 画面や DB 固有の概念（UI 用の状態、DB 接続クラス、ORM の属性など）はここには持たせず、
///   「アプリとして管理したい情報＝業務の中心となるデータ構造」だけを表現する。
/// </summary>
public class TodoItem
{
    /// <summary>
    /// 主キー。
    /// - どの DB を使うか（MySQL など）には依存しない「論理的な ID」として扱う。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// TODOの内容（タイトル）。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 完了フラグ。
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// 作成日時。
    /// インフラ側で自動的に「現在日時」をセットする想定。
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

