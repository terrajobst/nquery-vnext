@echo off
setlocal

:: Find MSBuild to use

for /f "tokens=*" %%i in ('"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -property installationPath') do set VSPATH=%%i
set MSBUILD_PATH=%VSPATH%\MSBuild\15.0\Bin\MSBuild.exe

:: Note: We've disabled node reuse because it causes file locking issues.
::       The issue is that we extend the build with our own targets which
::       means that that rebuilding cannot successfully delete the task
::       assembly.

if not exist bin mkdir bin
"%MSBUILD_PATH%" /nologo /m /v:m /nr:false /bl:bin\msbuild.binlog %*