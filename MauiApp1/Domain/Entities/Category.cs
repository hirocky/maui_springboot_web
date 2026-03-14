namespace MauiApp1.Domain.Entities;

/// <summary>
/// 習慣のカテゴリを表すドメインモデル。
///
/// レイヤード構成の「ドメイン層」に属し、
/// 「健康」「学習」「家事」などの分類を表現する。
/// UIやDBの詳細には依存しない純粋な業務概念。
/// </summary>
public class Category
{
    /// <summary>主キー。DBに依存しない論理的なID。</summary>
    public int Id { get; set; }

    /// <summary>カテゴリ名（例: 健康、学習、家事）。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>表示順序（小さいほど上に表示）。</summary>
    public int SortOrder { get; set; }
}
