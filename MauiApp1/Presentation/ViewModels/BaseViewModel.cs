using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiApp1.Presentation.ViewModels;

/// <summary>
/// MVVMパターンにおけるViewModelの共通基底クラス。
/// 
/// - 画面にバインドされるプロパティが変更されたときに通知するため、
///   INotifyPropertyChangedを実装している。
/// - ここでは「UIに近いが、UIコントロールそのものは知らない」層＝プレゼンテーション層のクラス。
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value))
        {
            return;
        }

        backingField = value;
        OnPropertyChanged(propertyName);
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

