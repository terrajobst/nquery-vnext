@echo off
setlocal

rem The VS Code integration tests launch a real VS Code, and the first run downloads it (about a
rem gigabyte), so they are opt-in locally. CI runs them on every push regardless.
set RUN_INTEGRATION=
if /i "%~1"=="-full" set RUN_INTEGRATION=1

set SLN_FILE=%~dp0NQuery.slnx
dotnet test --solution %SLN_FILE%
if errorlevel 1 exit /b 1

where npm >nul 2>nul
if errorlevel 1 (
    echo.
    echo Skipping the VS Code extension tests: npm was not found on PATH.
    exit /b 0
)

echo.
echo Testing the VS Code extension...

pushd "%~dp0vscode"

if not exist node_modules call npm ci
if errorlevel 1 goto :failed

call npm run test:unit
if errorlevel 1 goto :failed

if defined RUN_INTEGRATION (
    call npm run test:integration
    if errorlevel 1 goto :failed
) else (
    echo.
    echo Skipping the VS Code integration tests. Run "test.cmd -full" to include them.
)

popd
exit /b 0

:failed
popd
exit /b 1
