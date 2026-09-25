param(
    [Parameter(Mandatory = $true)]
    [string]$WebPartExtendConfig,
    [Parameter(Mandatory = $true)]
    [string]$AssemblyName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $WebPartExtendConfig)) {
    throw "WebPartExtend.config not found: $WebPartExtendConfig"
}

# Standalone WebPartExtend_*.config files are loaded by the current Portal runtime.
# Keep this script as a cleanup tool for older deployments rather than merging again.
$content = Get-Content -LiteralPath $WebPartExtendConfig -Raw
$pattern = '(?m)^\s*<ExtendedPart\s+parentPartFullName="(?:MFG\.MO\.StartAnalysisUI|UFIDA\.U9\.MFG\.MO\.StartAnalysisUIModel\.(?:SimuDocUIFormWebPart|ManufactureSimuResultUIFormWebPart))"\s+extendedPartFullName="U9Custom\.UI\.ManufactureSimulateShortageTable\.(?:SimuDocShortageButtonPlugin|ManufactureSimulateShortageButtonPlugin)"\s+extendedPartAssemblyName="[^"]+"\s*/>\s*\r?\n?'
$updated = [System.Text.RegularExpressions.Regex]::Replace($content, $pattern, '')

if ($updated -ne $content) {
    [System.IO.File]::WriteAllText($WebPartExtendConfig, $updated, [System.Text.UTF8Encoding]::new($false))
}
