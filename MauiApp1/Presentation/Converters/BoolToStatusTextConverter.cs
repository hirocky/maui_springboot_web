using System.Globalization;
using Microsoft.Maui.Controls;

namespace MauiApp1.Presentation.Converters;

/// <summary>
/// bool値（完了フラグ）を、ボタンに表示するテキストへ変換するコンバーター。
/// 
/// - MVVMでは、ViewModelのプロパティは「状態」を表現し、
///   「その状態をどう表示するか」はView側で変換することが多い。
/// - Converterはそのための小さなクラスで、プレゼンテーション層に属する。
/// </summary>
public class BoolToStatusTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool completed)
        {
            return completed ? "未完に戻す" : "完了にする";
        }

        return "完了にする";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // 今回は双方向変換の必要がないため未実装。
        throw new NotSupportedException();
    }
}

