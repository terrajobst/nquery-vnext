@echo off
setlocal

set SLN_FILE=%~dp0src\NQueryViewer\NQueryViewer.csproj
dotnet run --project %SLN_FILE% --nologo -- %*
