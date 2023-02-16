@echo OFF
title Install DeskBand
@setlocal enableextensions
@cd /d "%~dp0"

rem Check permissions
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Administrative permissions confirmed.
) else (
    echo Please run this script with administrator permissions.
	pause
    exit /b 0
)

taskkill /im HovyMonitor.Deskbar.Win.Updater.exe /f

IF "%CD%" == "C:\Program Files\HovyMonitorBar" (
    echo Directory is correct
) else (
    echo You should unpack files right into C:\Program Files\HovyMonitorBar directory.
    mkdir "C:\Program Files\HovyMonitorBar"
    MOVE "%CD%\*" "C:\Program Files\HovyMonitorBar"
    call "C:\Program Files\HovyMonitorBar\install_script.bat"
    exit /b 0
)

taskkill /im explorer.exe /f
start explorer.exe

if defined %PROGRAMFILES(x86)% (
   %SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\regasm.exe /nologo /u "HovyMonitor.DeskBar.Win.dll"
   %SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\regasm.exe /nologo /codebase "HovyMonitor.DeskBar.Win.dll"
) else (
   %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\regasm.exe /nologo /u "HovyMonitor.DeskBar.Win.dll"
   %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\regasm.exe /nologo /codebase "HovyMonitor.DeskBar.Win.dll"
)

taskkill /im explorer.exe /f
start explorer.exe