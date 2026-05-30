@echo off
setlocal

set SLN_FILE=%~dp0src\NQuery.sln
dotnet build %SLN_FILE% --nologo -- %*
