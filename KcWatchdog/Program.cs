using KcWatchdog;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = Host.CreateApplicationBuilder(args);

if (OperatingSystem.IsWindows() && WindowsServiceHelpers.IsWindowsService())
{
    builder.Services.AddWindowsService(options => options.ServiceName = "KcWatchdog");
}

builder.Services.AddHostedService<WatchdogWorker>();

var host = builder.Build();
host.Run();
