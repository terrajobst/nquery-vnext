@echo off
setlocal

set SLN_FILE=%~dp0src\NQuery.slnx
dotnet test %SLN_FILE% --nologo -- %*
