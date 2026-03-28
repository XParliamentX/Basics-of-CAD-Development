$ErrorActionPreference = "Stop"

$dllPath = "C:\Program Files\ASCON\KOMPAS-3D v24 Study\Bin\KompasAppsHelper.dll"

if (-not (Test-Path $dllPath)) {
  throw "DLL not found: $dllPath"
}

$asm = [Reflection.Assembly]::LoadFrom($dllPath)
$types = $asm.GetTypes() | Sort-Object FullName

$types |
  Select-Object -First 150 FullName |
  ForEach-Object { $_.FullName }

