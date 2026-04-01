namespace MauiApp1.Domain.Printing;

/// <summary>
/// フォーマット済みレシートの1行を表す値オブジェクト。
/// マーカー（[B][C][R][L][LR]）解析済みの状態を保持する。
/// </summary>
public sealed record ReceiptLine(
    string Draw,
    bool Bold,
    bool Large,
    bool Center,
    bool Right,
    bool IsLeftRight,
    string LeftText,
    string RightText
);
