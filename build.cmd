@echo off
setlocal

set SLN_FILE=%~dp0src\NQuery.sln
set OUT_DIR=%~dp0bin\
dotnet build %SLN_FILE% --nologo -o %OUT_DIR% -- %*
