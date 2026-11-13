$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$projectRoot = Join-Path $root 'NeonAudioPlayer'

$xamlLines = Get-ChildItem -LiteralPath $projectRoot -Recurse -File -Filter '*.xaml' |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
    ForEach-Object { (Get-Content -LiteralPath $_.FullName | Where-Object { $_.Trim().Length -gt 0 }).Count } |
    Measure-Object -Sum

$csharpLines = Get-ChildItem -LiteralPath $projectRoot -Recurse -File -Filter '*.cs' |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
    ForEach-Object { (Get-Content -LiteralPath $_.FullName | Where-Object { $_.Trim().Length -gt 0 }).Count } |
    Measure-Object -Sum

$xaml = [double]$xamlLines.Sum
$csharp = [double]$csharpLines.Sum
$ratio = if (($xaml + $csharp) -eq 0) { 0 } else { 100 * $xaml / ($xaml + $csharp) }

Write-Host "XAML non-empty lines: $xaml"
Write-Host "C# non-empty lines:   $csharp"
Write-Host ("XAML ratio:           {0:N2}%" -f $ratio)

if ($ratio -lt 90) {
    throw ("XAML must be at least 90%; actual ratio is {0:N2}%." -f $ratio)
}
