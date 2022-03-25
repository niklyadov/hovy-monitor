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
    goto EXIT
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