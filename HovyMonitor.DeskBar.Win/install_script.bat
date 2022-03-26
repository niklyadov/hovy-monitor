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

IF "%CD%" == "C:\Program Files\HovyMonitorBar" (
    echo Directory is correct
) else (
    echo You should unpack files right into C:\Program Files\HovyMonitorBar directory.
    echo "Hit any key to move all files automatically or exit and do it manually"
    pause
    mkdir "C:\Program Files\HovyMonitorBar"
    MOVE "%CD%\*" "C:\Program Files\HovyMonitorBar"
    call "C:\Program Files\HovyMonitorBar\install_script.bat"
    exit /b 0
    pause
)

taskkill /im explorer.exe /f
start explorer.exe

if defined %PROGRAMFILES(x86)% (
   %SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\regasm.exe /nologo /u "HovyMonitor.DeskBar.Win.dll"
) else (
   %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\regasm.exe /nologo /u "HovyMonitor.DeskBar.Win.dll"
)

if defined %PROGRAMFILES(x86)% (
   %SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\regasm.exe /nologo /codebase "HovyMonitor.DeskBar.Win.dll"
) else (
   %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\regasm.exe /nologo /codebase "HovyMonitor.DeskBar.Win.dll"
)
pause

taskkill /im explorer.exe /f
start explorer.exe