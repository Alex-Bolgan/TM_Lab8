$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'NeonAudioPlayer\NeonAudioPlayer.csproj'
$window = Join-Path $root 'NeonAudioPlayer\MainWindow.xaml'
$app = Join-Path $root 'NeonAudioPlayer\App.xaml'

foreach ($path in @($project, $window, $app)) {
    if (-not (Test-Path -LiteralPath $path)) { throw "Missing required artifact: $path" }
}

[xml]$projectXml = Get-Content -Raw -LiteralPath $project
$properties = $projectXml.Project.PropertyGroup
if ($properties.TargetFramework -ne 'net8.0-windows') { throw 'TargetFramework must be net8.0-windows.' }
if ($properties.UseWPF -ne 'true') { throw 'UseWPF must be true.' }

$xaml = Get-Content -Raw -LiteralPath $window
$requiredTokens = @(
    'WindowStyle="None"', 'AllowsTransparency="True"', 'x:Name="AudioEngine"',
    'x:Name="SeekSlider"', 'x:Name="VolumeSlider"', 'OpenFile_Click',
    'Play_Click', 'PauseResume_Click', 'Stop_Click', 'SkinButton_Click',
    'Cyan', 'Magenta', 'Ultraviolet', 'MediaElement'
)
foreach ($token in $requiredTokens) {
    if (-not $xaml.Contains($token)) { throw "MainWindow.xaml is missing contract token: $token" }
}

Write-Host "Project contract tests passed: $($requiredTokens.Count + 5) assertions."

