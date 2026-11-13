$ErrorActionPreference = 'Stop'

$sourcePath = Join-Path $PSScriptRoot '..\NeonAudioPlayer\PlayerLogic.cs'
if (-not (Test-Path -LiteralPath $sourcePath)) {
    throw 'Production helper missing: NeonAudioPlayer/PlayerLogic.cs'
}

Add-Type -Path $sourcePath

$cases = @(
    @{ Input = [TimeSpan]::FromSeconds(0); Expected = '00:00' },
    @{ Input = [TimeSpan]::FromSeconds(65); Expected = '01:05' },
    @{ Input = [TimeSpan]::FromSeconds(3599); Expected = '59:59' },
    @{ Input = [TimeSpan]::FromSeconds(3661); Expected = '01:01:01' }
)

foreach ($case in $cases) {
    $actual = [NeonAudioPlayer.PlayerLogic]::FormatTime($case.Input)
    if ($actual -ne $case.Expected) {
        throw "FormatTime failed for $($case.Input): expected $($case.Expected), got $actual"
    }
}

if ([NeonAudioPlayer.PlayerLogic]::CanSeek($false, $true) -ne $false) {
    throw 'CanSeek must reject seeking without a source.'
}
if ([NeonAudioPlayer.PlayerLogic]::CanSeek($true, $false) -ne $false) {
    throw 'CanSeek must reject seeking without a known duration.'
}
if ([NeonAudioPlayer.PlayerLogic]::CanSeek($true, $true) -ne $true) {
    throw 'CanSeek must allow seeking with a source and known duration.'
}

Write-Host 'PlayerLogic tests passed: 7 assertions.'

