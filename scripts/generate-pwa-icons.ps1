param(
    [Parameter(Mandatory = $true)]
    [string]$SourcePng,

    [Parameter(Mandatory = $true)]
    [string]$Dest192Png,

    [Parameter(Mandatory = $true)]
    [string]$Dest512Png
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing

$img = [System.Drawing.Image]::FromFile($SourcePng)
try {
    $bmp192 = New-Object System.Drawing.Bitmap 192, 192
    $g192 = [System.Drawing.Graphics]::FromImage($bmp192)
    try {
        $g192.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g192.DrawImage($img, 0, 0, 192, 192)
        $bmp192.Save($Dest192Png, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $g192.Dispose()
        $bmp192.Dispose()
    }

    $bmp512 = New-Object System.Drawing.Bitmap 512, 512
    $g512 = [System.Drawing.Graphics]::FromImage($bmp512)
    try {
        $g512.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g512.DrawImage($img, 0, 0, 512, 512)
        $bmp512.Save($Dest512Png, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $g512.Dispose()
        $bmp512.Dispose()
    }
}
finally {
    $img.Dispose()
}

Write-Host "Generated: $Dest192Png"
Write-Host "Generated: $Dest512Png"
