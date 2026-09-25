[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$PortalRoot
)

$ErrorActionPreference = 'Stop'
$packageRoot = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path -LiteralPath $PortalRoot)) { throw "Portal directory not found: $PortalRoot" }

$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$backupParent = [System.IO.Path]::GetFullPath((Join-Path $packageRoot '..\..\DeployBackup'))
$backupRoot = Join-Path $backupParent ('Production_' + $stamp)
New-Item -ItemType Directory -Force -Path $backupRoot | Out-Null

$files = @(
    @{ Source = '01_Portal\UILib\U9Custom.UI.ManufactureSimulateShortageTable.dll'; Destination = 'UILib' },
    @{ Source = '01_Portal\UILib\U9Custom.UI.ManufactureSimulateShortageTable.Independent.dll'; Destination = 'UILib' },
    @{ Source = '01_Portal\UILib\U9Custom.UI.OutsourceShortageStatistics.Independent.dll'; Destination = 'UILib' },
    @{ Source = '01_Portal\ApplicationServer\Libs\U9Custom.OutsourceShortageWritebackBP.dll'; Destination = 'ApplicationServer\Libs' },
    @{ Source = '01_Portal\ApplicationServer\Libs\U9Custom.OutsourceShortageWritebackBP.Agent.dll'; Destination = 'ApplicationServer\Libs' },
    @{ Source = '01_Portal\ApplicationServer\Libs\U9Custom.OutsourceShortageWritebackBP.Deploy.dll'; Destination = 'ApplicationServer\Libs' },
    @{ Source = '01_Portal\ApplicationServer\Libs\U9Custom.OutsourceShortageWritebackBP.ubfsvc'; Destination = 'ApplicationServer\Libs' },
    @{ Source = '01_Portal\ApplicationLib\U9Custom.OutsourceShortageWritebackBP.Agent.dll'; Destination = 'ApplicationLib' },
    @{ Source = '01_Portal\ApplicationLib\U9Custom.OutsourceShortageWritebackBP.Deploy.dll'; Destination = 'ApplicationLib' },
    @{ Source = '01_Portal\Root\WebPartExtend_ManufactureSimulateShortageTable.config'; Destination = '' }
)

foreach ($file in $files) {
    $source = Join-Path $packageRoot $file.Source
    $destinationDirectory = Join-Path $PortalRoot $file.Destination
    $destination = Join-Path $destinationDirectory (Split-Path $file.Source -Leaf)
    if (-not (Test-Path -LiteralPath $source)) { throw "Package file not found: $source" }
    New-Item -ItemType Directory -Force -Path $destinationDirectory | Out-Null
    if (Test-Path -LiteralPath $destination) { Copy-Item -LiteralPath $destination -Destination $backupRoot -Force }
    Copy-Item -LiteralPath $source -Destination $destinationDirectory -Force
}

Write-Output "Portal deployment completed. Backup: $backupRoot"
