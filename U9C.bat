@echo off
setlocal

set "U9_HOME=E:\yonyou\U9CE"
set "PKG=%~dp0"
set "DLL=U9Custom.UI.ManufactureSimulateShortageTable.dll"
set "INDEPENDENT_DLL=U9Custom.UI.OutsourceShortageStatistics.Independent.dll"
set "PDB=U9Custom.UI.ManufactureSimulateShortageTable.pdb"
set "CFG=WebPartExtend_ManufactureSimulateShortageTable.config"

set "SRC_DLL=%PKG%bin\Release\%DLL%"
if not exist "%SRC_DLL%" set "SRC_DLL=%PKG%%DLL%"
set "SRC_INDEPENDENT_DLL=%PKG%独立委外欠料统计\bin\Release\%INDEPENDENT_DLL%"
if not exist "%SRC_INDEPENDENT_DLL%" set "SRC_INDEPENDENT_DLL=%PKG%%INDEPENDENT_DLL%"

set "SRC_PDB=%PKG%bin\Release\%PDB%"
if not exist "%SRC_PDB%" set "SRC_PDB=%PKG%%PDB%"

set "SRC_CFG=%PKG%%CFG%"
set "DST_UILIB=%U9_HOME%\Portal\UILib"
set "DST_PORTAL=%U9_HOME%\Portal"

echo Deploying ManufactureSimulateShortageTable to %U9_HOME%
echo.

if not exist "%SRC_DLL%" goto :missing_source
if not exist "%SRC_INDEPENDENT_DLL%" goto :missing_source
if not exist "%SRC_CFG%" goto :missing_source
if not exist "%DST_UILIB%" goto :missing_target
if not exist "%DST_PORTAL%" goto :missing_target

copy /Y "%SRC_DLL%" "%DST_UILIB%\" >nul
if errorlevel 1 goto :copy_failed

copy /Y "%SRC_INDEPENDENT_DLL%" "%DST_UILIB%\" >nul
if errorlevel 1 goto :copy_failed

if exist "%SRC_PDB%" (
    copy /Y "%SRC_PDB%" "%DST_UILIB%\" >nul
    if errorlevel 1 goto :copy_failed
)

copy /Y "%SRC_CFG%" "%DST_PORTAL%\" >nul
if errorlevel 1 goto :copy_failed

powershell -NoProfile -ExecutionPolicy Bypass -Command "(Get-Item '%DST_PORTAL%\web.config').LastWriteTime = Get-Date"

echo.
echo Deploy completed.
pause
exit /b 0

:missing_source
echo ERROR: Missing build output or config file. Please build Release first and keep package files complete.
pause
exit /b 1

:missing_target
echo ERROR: U9 target folder not found. Please check U9_HOME in this BAT.
pause
exit /b 1

:copy_failed
echo ERROR: Copy failed. Please run this BAT as Administrator or stop related Portal process first.
pause
exit /b 1
