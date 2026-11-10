$ErrorActionPreference = 'Stop'
$blenderExe = Join-Path $PSScriptRoot 'Blender/blender-4.5.9-windows-x64/blender.exe'
$env:DISABLE_TELEMETRY = 'true'
Start-Process -FilePath $blenderExe -ArgumentList @('--python', ('"' + (Join-Path $PSScriptRoot 'start-blender.py') + '"')) -WorkingDirectory $PSScriptRoot -WindowStyle Hidden
