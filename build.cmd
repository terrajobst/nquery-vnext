@echo off
setlocal

set SLN_FILE=%~dp0src\NQuery.slnx
dotnet build %SLN_FILE% --nologo -- %*
