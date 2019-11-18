@echo off
setlocal

:: Use vswhere to find an install of VS

set VSWHERE="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist %VSWHERE% goto error
for /f "tokens=*" %%i in ('%VSWHERE% -property installationPath -prerelease') do set VSINSTALLDIR=%%i

set MSBUILD="%VSINSTALLDIR%\MSBuild\Current\Bin\MSBuild.exe"
if exist %MSBUILD% goto build

:error

echo ERROR: You need Visual Studio 2019 to build.
exit /B -1

:build

:: Note: We've disabled node reuse because it causes file locking issues.
::       The issue is that we extend the build with our own targets which
::       means that that rebuilding cannot successfully delete the task
::       assembly.

if not exist bin mkdir bin
%MSBUILD% /nologo /m /v:m /nr:false /bl:msbuild.binlog %*