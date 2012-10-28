@ECHO OFF
SETLOCAL

:: Ensure nuget.exe exists. If not, dowload it.

SET CACHED_NUGET=%~dp0NuGet.exe
IF EXIST %CACHED_NUGET% GOTO RESTORE
powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CACHED_NUGET%'"

:: Restore NuGet packages

:RESTORE
.\nuget.exe restore src\NQuery.sln

:: Note: We've disabled node reuse because it causes file locking issues.
::       The issue is that we extend the build with our own targets which
::       means that that rebuilding cannot successully delete the task
::       assembly.

"%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" /nologo /m /v:m /nr:false /flp:verbosity=normal %1 %2 %3 %4 %5 %6 %7 %8 %9