@echo off
setlocal

set SLN_FILE=%~dp0src\NQuery.sln
dotnet test %SLN_FILE% --nologo -- %*
