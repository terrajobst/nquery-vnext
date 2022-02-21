@echo off
setlocal

set SLN_FILE=%~dp0src\NQueryViewer\NQueryViewer.csproj
set OUT_DIR=%~dp0bin\
dotnet run --project %SLN_FILE% --nologo -o %OUT_DIR% -- %*
