using System.Runtime.InteropServices;
using MauiApp1.Presentation.Services;

namespace MauiApp1.Infrastructure.Platform;

/// <summary>
/// Windows の OS 標準メッセージボックス（user32.dll / MessageBoxW）を使って
/// メッセージを表示する実装クラス。
///
/// - レイヤード構成では「インフラ／プラットフォーム層」に属する。
/// - P/Invoke によるネイティブ API 呼び出しはこのクラスに閉じ込め、
///   上位レイヤー（ViewModel など）は IMessageBoxService という抽象だけを見る。
/// - #if WINDOWS でガードすることで、Windows 以外のプラットフォームをビルド対象に含めても
///   コンパイルエラーにならないようにしている。
/// </summary>
#if WINDOWS
public class WindowsMessageBoxService : IMessageBoxService
{
    // user32.dll の MessageBoxW 関数への P/Invoke 宣言。
    // - CharSet.Unicode を指定することで、.NET の string を UTF-16 として扱う。
    // - UI スレッド上で呼び出される前提の、同期 API として利用する。
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(
        nint hWnd,
        string text,
        string caption,
        uint type);

    /// <summary>
    /// 情報メッセージを OS 標準のメッセージボックスで表示する。
    /// </summary>
    public void ShowInfo(string message, string title)
    {
        const uint MB_OK = 0x00000000;
        const uint MB_ICONINFORMATION = 0x00000040;

        // hWnd は MAUI から簡単には取得しにくいので、サンプルでは 0（オーナーなし）にしている。
        _ = MessageBoxW(0, message, title, MB_OK | MB_ICONINFORMATION);
    }
}
#endif

