using System.Diagnostics;

namespace KcWatchdog;

public sealed class WatchdogWorker(
    ILogger<WatchdogWorker> logger,
    IConfiguration configuration,
    IHostEnvironment environment) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var targets = configuration.GetSection("Targets").Get<TargetApp[]>() ?? [];
        if (targets.Length == 0)
        {
            logger.LogWarning("Targets が空です。appsettings.json を確認してください。");
            return;
        }

        var poll = TimeSpan.FromSeconds(configuration.GetValue("Watchdog:PollIntervalSeconds", 10));
        using var timer = new PeriodicTimer(poll);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            foreach (var t in targets)
            {
                if (string.IsNullOrWhiteSpace(t.ProcessName) || string.IsNullOrWhiteSpace(t.ExePath))
                {
                    logger.LogWarning("無効な Target エントリをスキップしました（ProcessName / ExePath）。");
                    continue;
                }

                if (Process.GetProcessesByName(t.ProcessName).Length > 0)
                    continue;

                var fullPath = ResolveExePath(t.ExePath);
                if (!File.Exists(fullPath))
                {
                    logger.LogError("監視対象 exe が見つかりません: {Path}", fullPath);
                    continue;
                }

                logger.LogWarning("{Name} が停止しています。再起動します。", t.ProcessName);
                try
                {
                    var workDir = Path.GetDirectoryName(fullPath);
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = fullPath,
                        WorkingDirectory = string.IsNullOrEmpty(workDir) ? environment.ContentRootPath : workDir,
                        UseShellExecute = true,
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "{Name} の起動に失敗しました。", t.ProcessName);
                }
            }
        }
    }

    private string ResolveExePath(string exePath)
    {
        if (Path.IsPathFullyQualified(exePath))
            return Path.GetFullPath(exePath);

        var baseSegment = configuration.GetValue<string>("Watchdog:ExePathBase");
        var root = string.IsNullOrWhiteSpace(baseSegment)
            ? environment.ContentRootPath
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, baseSegment));

        return Path.GetFullPath(Path.Combine(root, exePath));
    }
}
