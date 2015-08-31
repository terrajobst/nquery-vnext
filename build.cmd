@ECHO OFF

if not exist bin mkdir bin

:: Note: We've disabled node reuse because it causes file locking issues.
::       The issue is that we extend the build with our own targets which
::       means that that rebuilding cannot successfully delete the task
::       assembly.

"%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" /nologo /m /v:m /nr:false /flp:verbosity=normal;LogFile=bin\msbuild.log %*
