namespace MauiApp1.Presentation.Services;

/// <summary>
/// メッセージボックス表示のための抽象インターフェース。
///
/// - MVVM＋レイヤード構成では、ViewModel から OS 依存の API（user32.dll など）を
///   直接呼び出さないようにするのが重要。
/// - そこで、「メッセージを表示する」という機能だけをインターフェースとして定義し、
///   具体的な実装（Windows の P/Invoke など）は別レイヤー（インフラ／プラットフォーム層）に閉じ込める。
/// - ViewModel はこのインターフェース越しに機能を利用することで、
///   OS や P/Invoke の詳細に依存しないコードになる。
/// </summary>
public interface IMessageBoxService
{
    /// <summary>
    /// 情報メッセージを同期的に表示する。
    /// </summary>
    /// <param name="message">表示したい本文。</param>
    /// <param name="title">ダイアログのタイトル。</param>
    void ShowInfo(string message, string title);
}

