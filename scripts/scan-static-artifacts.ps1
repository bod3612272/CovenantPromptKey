<#
.SYNOPSIS
  Scan published static artifacts for secrets or dangerous patterns.

.DESCRIPTION
  Scans a publish directory and fails if token/secret-like patterns match.

.NOTES
  This is a client-side artefact scan; it does not replace proper secret management.
#>

[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [Alias('Path')]
  [string] $PublishRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $PublishRoot)) {
  throw "PublishRoot does not exist: $PublishRoot"
}

Write-Host "[scan-static-artifacts] PublishRoot=$PublishRoot"

function Get-MaskedValue {
  param(
    [Parameter(Mandatory = $true)]
    [string] $Value
  )

  if ($Value.Length -le 12) {
    return ('*' * $Value.Length)
  }

  return ($Value.Substring(0, 4) + '...' + $Value.Substring($Value.Length - 4))
}

# Token patterns (keep this list explicit and local to the script)
$patterns = @(
  @{ Name = 'GitHub classic PAT (ghp_)'; Regex = 'ghp_[A-Za-z0-9]{36}' },
  @{ Name = 'GitHub fine-grained PAT (github_pat_)'; Regex = 'github_pat_[A-Za-z0-9_]{20,120}' },
  @{ Name = 'Slack token (xoxb/xoxp/xoxa)'; Regex = 'xox[baprs]-[A-Za-z0-9-]{10,200}' },
  @{ Name = 'Stripe secret key (sk_live/sk_test)'; Regex = 'sk_(live|test)_[A-Za-z0-9]{16,200}' },
  @{ Name = 'Google API key (AIza)'; Regex = 'AIza[0-9A-Za-z\-_]{30,50}' },
  @{ Name = 'OAuth access token (ya29.)'; Regex = 'ya29\.[0-9A-Za-z\-_]+' },
  @{ Name = 'JWT-like token (eyJ...\..+\..+)'; Regex = 'eyJ[0-9A-Za-z\-_]+\.[0-9A-Za-z\-_]+\.[0-9A-Za-z\-_]+' },
  @{ Name = 'Private key block'; Regex = '-----BEGIN( [A-Z0-9]+)? PRIVATE KEY-----' }
)

$textExtensions = @(
  '.html', '.htm',
  '.js', '.mjs',
  '.css',
  '.json', '.map',
  '.txt', '.md',
  '.xml',
  '.yml', '.yaml',
  '.webmanifest',
  '.config'
)

$files = Get-ChildItem -LiteralPath $PublishRoot -Recurse -File |
  Where-Object { $textExtensions -contains $_.Extension.ToLowerInvariant() } |
  Sort-Object -Property FullName

Write-Host "[scan-static-artifacts] FilesToScan=$($files.Count)"

$findings = New-Object System.Collections.Generic.List[object]

foreach ($file in $files) {
  try {
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
    $content = [System.Text.Encoding]::UTF8.GetString($bytes)
  }
  catch {
    Write-Host "[scan-static-artifacts] WARN: Failed to read file: $($file.FullName) ($($_.Exception.Message))"
    continue
  }

  foreach ($pattern in $patterns) {
    $regex = [regex]::new($pattern.Regex)
    $match = $regex.Match($content)

    if ($match.Success) {
      $findings.Add([pscustomobject]@{
        File = $file.FullName
        Pattern = $pattern.Name
        Match = (Get-MaskedValue -Value $match.Value)
      })
    }
  }
}

if ($findings.Count -gt 0) {
  Write-Host "[scan-static-artifacts] FAIL: Potential secret patterns detected."
  $findings |
    Sort-Object -Property File, Pattern |
    Format-Table -AutoSize |
    Out-String |
    Write-Host

  exit 1
}

Write-Host "[scan-static-artifacts] PASS: No secret patterns detected."
exit 0
