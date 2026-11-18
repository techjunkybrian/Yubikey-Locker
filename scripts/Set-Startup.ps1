param(
    [string]$ExePath,
    [switch]$Disable
)

$runKeyPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
$valueName = 'YubiKeyLocker'

if ($Disable) {
    Remove-ItemProperty -Path $runKeyPath -Name $valueName -ErrorAction SilentlyContinue
    Write-Host 'YubiKey Locker will no longer start when you sign in.'
    exit 0
}

if ([string]::IsNullOrWhiteSpace($ExePath)) {
    $candidatePaths = @(
        (Join-Path $env:ProgramFiles 'YubiKey Locker\YubikeyLocker.exe'),
        (Join-Path $env:LOCALAPPDATA 'Programs\YubiKey Locker\YubikeyLocker.exe'),
        (Join-Path $PSScriptRoot '..\publish\YubikeyLocker.exe')
    )

    $ExePath = $candidatePaths | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
}

if ([string]::IsNullOrWhiteSpace($ExePath) -or -not (Test-Path -LiteralPath $ExePath -PathType Leaf)) {
    throw 'Could not find YubikeyLocker.exe. Pass its path with -ExePath.'
}

$resolvedExePath = (Resolve-Path -LiteralPath $ExePath).Path
$command = '"{0}"' -f $resolvedExePath
New-Item -Path $runKeyPath -Force | Out-Null
Set-ItemProperty -Path $runKeyPath -Name $valueName -Value $command
Write-Host "YubiKey Locker will start when you sign in: $resolvedExePath"
