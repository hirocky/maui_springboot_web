namespace MauiApp1.Domain.Printing;

/// <summary>
/// 印字対象のレシート全体を表す値オブジェクト。
/// </summary>
public sealed record ReceiptDocument(
    IReadOnlyList<ReceiptLine> Lines,
    string? LogoPath = null
);
