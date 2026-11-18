using System.Runtime.InteropServices;

namespace YubikeyLocker;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon trayIcon;
    private readonly ToolStripMenuItem statusItem;
    private readonly YubikeyDeviceMonitor monitor;
    private readonly Control uiDispatcher;

    public TrayApplicationContext()
    {
        statusItem = new ToolStripMenuItem("Starting device monitor...") { Enabled = false };
        var menu = new ContextMenuStrip();
        menu.Items.Add(statusItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitThread());

        trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Shield,
            Text = "YubiKey Locker",
            ContextMenuStrip = menu,
            Visible = true
        };

        uiDispatcher = new Control();
        _ = uiDispatcher.Handle;

        monitor = new YubikeyDeviceMonitor();
        monitor.StatusChanged += UpdateStatus;
        monitor.YubikeyRemoved += LockWorkstation;
        monitor.Start();
    }

    private void UpdateStatus(string status)
    {
        if (uiDispatcher.InvokeRequired)
        {
            uiDispatcher.BeginInvoke(new Action(() => UpdateStatus(status)));
            return;
        }

        statusItem.Text = status.Length > 60 ? status[..57] + "..." : status;
        var tooltip = $"YubiKey Locker: {status}";
        trayIcon.Text = tooltip.Length > 63 ? tooltip[..60] + "..." : tooltip;
    }

    private void LockWorkstation()
    {
        if (uiDispatcher.InvokeRequired)
        {
            uiDispatcher.BeginInvoke(new Action(LockWorkstation));
            return;
        }

        if (!LockWorkStation())
        {
            trayIcon.ShowBalloonTip(3000, "YubiKey Locker", "Windows could not lock the workstation.", ToolTipIcon.Warning);
        }
    }

    protected override void ExitThreadCore()
    {
        monitor.Dispose();
        uiDispatcher.Dispose();
        trayIcon.Visible = false;
        trayIcon.Dispose();
        base.ExitThreadCore();
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool LockWorkStation();
}
