# YubiKey Locker

YubiKey Locker is a lightweight Windows tray application that locks the PC when a YubiKey is unplugged. It checks Windows Plug and Play devices for Yubico's USB vendor ID (`VID_1050`) every two seconds. Once it has detected at least one YubiKey, it calls the standard Windows workstation lock command when no YubiKey remains connected.

## Features

- Runs quietly in the Windows notification area.
- Locks the workstation after the last detected YubiKey is removed.
- Shows its current monitoring state in the tray menu.
- Can start automatically when you sign in to Windows.
- Runs as a self-contained Windows x64 application; the .NET runtime does not need to be installed separately.

## Install

Download `YubiKeyLocker-Setup-1.0.0-win-x64.exe` from the repository's [GitHub Releases](../../releases) page and run it. During setup, you can choose to start YubiKey Locker automatically at sign-in. The installer also launches the app when setup finishes.

The installer is built by GitHub Actions when a version tag such as `v1.0.0` is pushed. A workflow run can also be started manually; that produces a downloadable build artifact.

## Open and use the app

YubiKey Locker runs in the Windows notification area and does not open a regular window when you double-click it. Look for the shield icon next to the clock. If it is not visible, click the `^` hidden-icons button to find it. Right-click the icon to see the current status or choose **Exit**.

The status should change to **YubiKey detected - monitoring** when a YubiKey is connected. If the app starts before a YubiKey is connected, it shows **Waiting for a YubiKey** until one is detected. After that, removing the last connected YubiKey locks the PC.

## Start automatically when you sign in

During installation, select **Start YubiKey Locker when I sign in to Windows** to enable startup. To change this later, run `scripts\Set-Startup.ps1` in PowerShell. The script adds or removes a startup entry for your Windows account:

```powershell
# Enable startup (finds the installed app or the publish build)
.\scripts\Set-Startup.ps1

# Enable startup for a specific executable
.\scripts\Set-Startup.ps1 -ExePath "C:\Path\To\YubikeyLocker.exe"

# Disable startup
.\scripts\Set-Startup.ps1 -Disable
```

## Build from source

Requirements: Windows, the .NET 10 SDK, and Inno Setup 6.

```powershell
dotnet publish YubikeyLocker.csproj -c Release -r win-x64 --self-contained true -o publish
& "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe" installer\YubiKeyLocker.iss
```

The installer is written to `artifacts\YubiKeyLocker-Setup-1.0.0-win-x64.exe`.

## How it works

The app queries Windows' `Win32_PnPEntity` inventory for a device ID containing `VID_1050`, which is Yubico's USB vendor ID. It arms after seeing a YubiKey, then locks Windows when a later check finds none. If a device query fails, the app retries and does not interpret that error as a removal. Closing the app from its tray menu stops monitoring.

## Notes

- This detects the presence of a Yubico USB device. It does not validate a key's identity, perform a challenge-response check, or replace Windows sign-in security.
- If multiple YubiKeys are connected, the PC stays unlocked while at least one remains connected.
- The app needs to be running to monitor removal. If it starts while no key is connected, it waits for one to be detected before it can lock on removal.
- For startup to work reliably, keep the app installed in a location that is available when you sign in.
- The build currently targets Windows x64.

## License

This project is provided under the MIT License. See [LICENSE](LICENSE).
