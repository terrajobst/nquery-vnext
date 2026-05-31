@echo off
setlocal

set SLN_FILE=%~dp0src\NQuery.slnx
dotnet test --solution %SLN_FILE%
