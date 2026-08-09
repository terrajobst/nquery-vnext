@echo off
setlocal

rem The differential tests replay queries against the engines in external/. They compile out when
rem those are missing, so restoring and building them here is what keeps them running: a stale
rem checkout costs coverage silently rather than loudly.
if not exist "%~dp0.gitmodules" goto :submodules_done
where git >nul 2>nul
if errorlevel 1 goto :submodules_done
git -C "%~dp0." submodule update --init external/nquery-baseline external/nquery-old
if errorlevel 1 echo Could not restore the submodules; the differential tests will compile out.
:submodules_done

rem Only the baseline is built: OldEngineDefinitionTests reads the old engine's test definitions as
rem files, and the old engine's assembly is needed by the benchmarks alone. Debug rather than
rem whatever this script was passed, as the test project references artifacts\bin\NQuery\debug_net8.0.
rem Building NQuery.Data builds NQuery with it, which is the other half of the "baseline" alias.
if not exist "%~dp0external\nquery-baseline\src\NQuery.Data\NQuery.Data.csproj" goto :baseline_done
echo.
echo Building the baseline engine...
dotnet build "%~dp0external\nquery-baseline\src\NQuery.Data" -c Debug --nologo
if errorlevel 1 exit /b 1
echo.
:baseline_done

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
