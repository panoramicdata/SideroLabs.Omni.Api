@echo off
REM Simple batch wrapper for Tag.ps1 script
REM This bypasses PowerShell execution policy issues

echo Running SideroLabs.Omni.Api Tagging Script...
echo.

if "%1"=="--help" (
    echo Usage: tag.bat [options]
    echo.
    echo Options:
    echo   --skip-tests    Skip running unit tests
    echo   --force         Force tagging even with uncommitted changes
    echo   --publish       Automatically publish to NuGet
    echo   --help          Show this help message
    echo.
    echo Examples:
    echo   tag.bat
    echo   tag.bat --skip-tests
    echo   tag.bat --force --publish
    goto :end
)

set POWERSHELL_ARGS=-ExecutionPolicy Bypass -File "Tag.ps1"

if "%1"=="--skip-tests" set POWERSHELL_ARGS=%POWERSHELL_ARGS% -SkipTests
if "%1"=="--force" set POWERSHELL_ARGS=%POWERSHELL_ARGS% -Force
if "%1"=="--publish" set POWERSHELL_ARGS=%POWERSHELL_ARGS% -Publish

if "%2"=="--skip-tests" set POWERSHELL_ARGS=%POWERSHELL_ARGS% -SkipTests
if "%2"=="--force" set POWERSHELL_ARGS=%POWERSHELL_ARGS% -Force
if "%2"=="--publish" set POWERSHELL_ARGS=%POWERSHELL_ARGS% -Publish

if "%3"=="--skip-tests" set POWERSHELL_ARGS=%POWERSHELL_ARGS% -SkipTests
if "%3"=="--force" set POWERSHELL_ARGS=%POWERSHELL_ARGS% -Force
if "%3"=="--publish" set POWERSHELL_ARGS=%POWERSHELL_ARGS% -Publish

powershell %POWERSHELL_ARGS%

:end
