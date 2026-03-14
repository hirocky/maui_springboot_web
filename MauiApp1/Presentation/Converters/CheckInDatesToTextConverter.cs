using System.Globalization;
using System.Collections;
using Microsoft.Maui.Controls;

namespace MauiApp1.Presentation.Converters;

/// <summary>
/// 達成日（DateTime のコレクション）を、画面用の短いテキストに変換するコンバーター。
///
/// 進捗レポート画面で「いつ達成したか」を「1/1, 1/2, 1/5 …」のような形式で表示するために使用する。
/// 多数の日付がある場合は先頭から一定数だけ表示し、残りは「他 N 日」と省略する。
/// </summary>
public class CheckInDatesToTextConverter : IValueConverter
{
    private const int MaxDatesShown = 14;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var dates = new List<DateTime>();
        if (value is IReadOnlyList<DateTime> list)
        {
            dates.AddRange(list);
        }
        else if (value is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is DateTime d)
                    dates.Add(d);
            }
        }
        else
        {
            return "達成日: なし";
        }

        return FormatDates(dates.OrderBy(d => d).ToList());
    }

    private static string FormatDates(List<DateTime> sorted)
    {
        if (sorted.Count == 0)
            return "達成日: なし";

        var take = Math.Min(sorted.Count, MaxDatesShown);
        var part = sorted.Take(take).Select(d => d.ToString("M/d", CultureInfo.CurrentCulture));
        var text = string.Join(", ", part);
        if (sorted.Count > MaxDatesShown)
            text += $" … 他 {sorted.Count - MaxDatesShown} 日";
        return "達成日: " + text;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
