#Requires -Version 5.1
<#
.SYNOPSIS
  Publish MauiApp1 (Release) into pep/artifacts.

.DESCRIPTION
  Default: framework-dependent (requires .NET 10 runtime on each PC).
  -SelfContained: win-x64 folder with runtime bundled.

.EXAMPLE
  .\scripts\publish-maui-windows.ps1
  .\scripts\publish-maui-windows.ps1 -SelfContained
#>
param(
    [switch]$SelfContained
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$outDir = Join-Path (Join-Path $repoRoot 'artifacts') 'MauiApp1-windows'
$tfm = 'net10.0-windows10.0.19041.0'
$proj = Join-Path (Join-Path $repoRoot 'MauiApp1') 'MauiApp1.csproj'

$publishArgs = @(
    'publish', $proj,
    '-c', 'Release',
    '-f', $tfm,
    '-o', $outDir
)
if ($SelfContained) {
    $publishArgs += @('-r', 'win-x64', '--self-contained', 'true')
}

Write-Host "Output: $outDir"
dotnet @publishArgs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Done. Double-click: $outDir\MauiApp1.exe (framework-dependent build needs .NET 10 runtime installed)."
