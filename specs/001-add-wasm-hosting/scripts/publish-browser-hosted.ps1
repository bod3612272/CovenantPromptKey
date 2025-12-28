<#
.SYNOPSIS
  Publish browser-hosted (Blazor WebAssembly) output for free static hosting targets.

.DESCRIPTION
  Skeleton script for US2. This will be completed when the WASM host exists.
  It should produce deterministic validation output (paths, hashes, base href).

.NOTES
  Keep minimal-change; do not modify publish output manually.
#>

[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [ValidateSet('github-pages', 'azure-swa')]
  [string] $Platform,

  [string] $BasePath,

  [ValidateSet('Debug', 'Release')]
  [string] $Configuration = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Normalize-BasePath {
  param([Parameter(Mandatory = $true)][string] $Value)

  $v = $Value.Trim()
  if (-not $v.StartsWith('/')) { $v = '/' + $v }
  if (-not $v.EndsWith('/')) { $v = $v + '/' }
  return $v
}

function Replace-BaseHrefInFile {
  param(
    [Parameter(Mandatory = $true)][string] $FilePath,
    [Parameter(Mandatory = $true)][string] $NewBaseHref
  )

  if (-not (Test-Path -LiteralPath $FilePath)) {
    return
  }

  $content = Get-Content -LiteralPath $FilePath -Raw
  $replacement = '<base href="' + $NewBaseHref + '" />'
  $updated = $content -replace '<base\s+href="[^"]*"\s*/>', $replacement
  Set-Content -LiteralPath $FilePath -Value $updated -NoNewline
}

function Upsert-CspMetaInFile {
  param(
    [Parameter(Mandatory = $true)][string] $FilePath,
    [Parameter(Mandatory = $true)][string] $Csp
  )

  if (-not (Test-Path -LiteralPath $FilePath)) {
    return
  }

  $content = Get-Content -LiteralPath $FilePath -Raw

  $metaRegex = '<meta\s+http-equiv="Content-Security-Policy"\s+content="[^"]*"\s*/?>'
  $metaReplacement = '<meta http-equiv="Content-Security-Policy" content="' + $Csp + '" />'

  if ($content -match $metaRegex) {
    $updated = $content -replace $metaRegex, $metaReplacement
    Set-Content -LiteralPath $FilePath -Value $updated -NoNewline
    return
  }

  # Insert CSP meta before the first <base ...> (or as first element inside <head> if <base> not found).
  if ($content -match '<base\s+href=') {
    $updated = $content -replace '(<base\s+href=)', ($metaReplacement + "`r`n    " + '$1')
    Set-Content -LiteralPath $FilePath -Value $updated -NoNewline
    return
  }

  if ($content -match '<head\s*>') {
    $updated = $content -replace '(<head\s*>)', ('$1' + "`r`n    " + $metaReplacement)
    Set-Content -LiteralPath $FilePath -Value $updated -NoNewline
    return
  }
}

Write-Host "[publish-browser-hosted] Platform=$Platform Configuration=$Configuration"

$repoRoot = Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')
$wasmProject = Join-Path $repoRoot 'CovenantPromptKeyWebAssembly\CovenantPromptKeyWebAssembly.csproj'

if (-not (Test-Path -LiteralPath $wasmProject)) {
  throw "WASM project not found: $wasmProject"
}

$outputDir = Join-Path $repoRoot ("ReleaseDownload\browser-hosted\$Platform\$Configuration")
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

Write-Host "[publish-browser-hosted] Publishing: $wasmProject"
dotnet publish $wasmProject -c $Configuration -o $outputDir | Out-Host

$wwwrootDir = Join-Path $outputDir 'wwwroot'
if (-not (Test-Path -LiteralPath $wwwrootDir)) {
  throw "Publish output missing wwwroot: $wwwrootDir"
}

$cspBaseline = "default-src 'self'; base-uri 'self'; object-src 'none'; script-src 'self' 'wasm-unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self';"

Upsert-CspMetaInFile -FilePath (Join-Path $wwwrootDir 'index.html') -Csp $cspBaseline
Upsert-CspMetaInFile -FilePath (Join-Path $wwwrootDir '404.html') -Csp $cspBaseline

if ($Platform -eq 'github-pages') {
  if ([string]::IsNullOrWhiteSpace($BasePath)) {
    throw "BasePath is required for github-pages (e.g. '/CovenantPromptKey/')"
  }

  $baseHref = Normalize-BasePath -Value $BasePath
  Replace-BaseHrefInFile -FilePath (Join-Path $wwwrootDir 'index.html') -NewBaseHref $baseHref
  Replace-BaseHrefInFile -FilePath (Join-Path $wwwrootDir '404.html') -NewBaseHref $baseHref
}

if ($Platform -eq 'azure-swa') {
  $swaConfigSrc = Join-Path $repoRoot 'CovenantPromptKeyWebAssembly\staticwebapp.config.json'
  $swaConfigDst = Join-Path $wwwrootDir 'staticwebapp.config.json'
  Copy-Item -LiteralPath $swaConfigSrc -Destination $swaConfigDst -Force
}

$indexPath = Join-Path $wwwrootDir 'index.html'
$swAssetsPath = Join-Path $wwwrootDir 'service-worker-assets.js'

$indexHash = if (Test-Path -LiteralPath $indexPath) { (Get-FileHash -Algorithm SHA256 -LiteralPath $indexPath).Hash } else { '' }
$swAssetsHash = if (Test-Path -LiteralPath $swAssetsPath) { (Get-FileHash -Algorithm SHA256 -LiteralPath $swAssetsPath).Hash } else { '' }

Write-Host "[publish-browser-hosted] OutputDir=$outputDir"
Write-Host "[publish-browser-hosted] WwwrootDir=$wwwrootDir"
Write-Host "[publish-browser-hosted] IndexHtmlSha256=$indexHash"
Write-Host "[publish-browser-hosted] ServiceWorkerAssetsSha256=$swAssetsHash"

exit 0
