@echo off
setlocal

set PROJECT_FILE=%~dp0src\NQuery.Benchmarks\NQuery.Benchmarks.csproj
dotnet run --project %PROJECT_FILE% -c Release -- %*
