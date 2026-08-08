@echo off
setlocal

set SLN_FILE=%~dp0NQuery.slnx
dotnet build %SLN_FILE% -t build -t pack --nologo %*
if errorlevel 1 exit /b 1

rem The VS Code extension is a separate toolchain, so it is skipped rather than failed when npm
rem is absent -- working on the query engine alone should not require Node.
where npm >nul 2>nul
if errorlevel 1 (
    echo.
    echo Skipping the VS Code extension: npm was not found on PATH.
    exit /b 0
)

echo.
echo Building the VS Code extension...

pushd "%~dp0vscode"

rem Only on a cold clone: restoring on every build would dominate an otherwise incremental one.
if not exist node_modules call npm ci
if errorlevel 1 goto :failed

call npm run compile
if errorlevel 1 goto :failed

popd
exit /b 0

:failed
popd
exit /b 1
