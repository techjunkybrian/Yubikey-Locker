using System.Management;

namespace YubikeyLocker;

internal sealed class YubikeyDeviceMonitor : IDisposable
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
    private readonly CancellationTokenSource cancellation = new();
    private Task monitoringTask = Task.CompletedTask;
    private bool started;
    private bool hasSeenYubikey;

    public event Action<string>? StatusChanged;
    public event Action? YubikeyRemoved;

    public void Start()
    {
        if (started)
        {
            throw new InvalidOperationException("The device monitor has already been started.");
        }

        started = true;
        monitoringTask = Task.Run(MonitorAsync);
    }

    private async Task MonitorAsync()
    {
        using var timer = new PeriodicTimer(PollInterval);
        while (!cancellation.IsCancellationRequested)
        {
            try
            {
                var present = IsYubikeyPresent();
                if (present)
                {
                    hasSeenYubikey = true;
                    StatusChanged?.Invoke("YubiKey detected - monitoring");
                }
                else if (hasSeenYubikey)
                {
                    hasSeenYubikey = false;
                    StatusChanged?.Invoke("YubiKey removed - locking PC");
                    YubikeyRemoved?.Invoke();
                }
                else
                {
                    StatusChanged?.Invoke("Waiting for a YubiKey");
                }
            }
            catch (Exception ex)
            {
                // A failed query must not be treated as a removal. Keep retrying.
                StatusChanged?.Invoke($"Device check failed; retrying ({ex.Message})");
            }

            try
            {
                if (!await timer.WaitForNextTickAsync(cancellation.Token))
                {
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private static bool IsYubikeyPresent()
    {
        using var searcher = new ManagementObjectSearcher(
            "SELECT PNPDeviceID FROM Win32_PnPEntity WHERE PNPDeviceID LIKE '%VID_1050%'");
        using var devices = searcher.Get();
        return devices.Count > 0;
    }

    public void Dispose()
    {
        cancellation.Cancel();
        try
        {
            monitoringTask.Wait(TimeSpan.FromSeconds(1));
        }
        catch (AggregateException)
        {
            // The monitor is stopping; no further action is needed.
        }
        cancellation.Dispose();
    }
}
