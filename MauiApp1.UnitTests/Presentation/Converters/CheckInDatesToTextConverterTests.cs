using System.Globalization;
using MauiApp1.Presentation.Converters;
using Xunit;

namespace MauiApp1.UnitTests.Presentation.Converters;

/// <summary>
/// <see cref="CheckInDatesToTextConverter"/> の単体テスト。
/// Convert / ConvertBack の挙動を検証する。
/// </summary>
public class CheckInDatesToTextConverterTests
{
    private readonly CheckInDatesToTextConverter _converter;
    private readonly CultureInfo _culture = CultureInfo.CurrentCulture;

    public CheckInDatesToTextConverterTests()
    {
        _converter = new CheckInDatesToTextConverter();
    }

    [Fact]
    public void Convert_null_達成日なし()
    {
        var result = _converter.Convert(null, typeof(string), null, _culture);

        Assert.Equal("達成日: なし", result);
    }

    [Fact]
    public void Convert_空リスト_達成日なし()
    {
        var result = _converter.Convert(new List<DateTime>(), typeof(string), null, _culture);

        Assert.Equal("達成日: なし", result);
    }

    [Fact]
    public void Convert_日付が1件_フォーマットされた文字列()
    {
        var dates = new List<DateTime> { new(2025, 3, 14) };

        var result = _converter.Convert(dates, typeof(string), null, _culture);

        Assert.StartsWith("達成日: ", result?.ToString());
        Assert.Contains("3/14", result?.ToString());
    }

    [Fact]
    public void Convert_日付が複数_昇順で並びカンマ区切り()
    {
        var dates = new List<DateTime>
        {
            new(2025, 3, 10),
            new(2025, 3, 12),
            new(2025, 3, 14)
        };

        var result = _converter.Convert(dates, typeof(string), null, _culture)?.ToString() ?? "";

        Assert.StartsWith("達成日: ", result);
        Assert.Contains("3/10", result);
        Assert.Contains("3/12", result);
        Assert.Contains("3/14", result);
    }

    [Fact]
    public void ConvertBack_NotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            _converter.ConvertBack("達成日: 3/14", typeof(object), null, _culture));
    }
}
