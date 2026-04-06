using MauiApp1.Presentation.Services;

namespace MauiApp1.Infrastructure.Platform;

/// <summary>
/// Windows 以外ではカスタマーディスプレイを使わない。
/// </summary>
public sealed class NullCustomerDisplayService : ICustomerDisplayService
{
    public Task SendTwoLinesAsync(
        string line1,
        string line2,
        CustomerDisplaySendOptions options,
        CancellationToken cancellationToken = default)
        => Task.FromException(
            new PlatformNotSupportedException(
                "カスタマーディスプレイのサンプルは Windows（net10.0-windows）でのみ利用できます。"));
}
