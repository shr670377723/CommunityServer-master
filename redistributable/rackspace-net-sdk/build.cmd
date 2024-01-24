@echo off
setlocal

:: Parse the arguments
IF "%1"=="/?" ( GOTO :help )
IF "%1"=="help" ( GOTO :help )

:parse
IF NOT "%1"=="" (
  IF /I "%1"=="/Configuration" (
    SET ConfigArg=/p:Configuration=%2
  ) ELSE (
    SET TargetArg=/t:%1
    SHIFT
    GOTO :parse
  )
)

:: Load the environment of the most recent version of Visual Studio installed
if not defined VisualStudioVersion (
    if defined VS140COMNTOOLS (
        call "%VS140COMNTOOLS%\VsDevCmd.bat"
        goto :build
    )

    echo Error: build.cmd requires Visual Studio 2015.
    exit /b 1
)

:build
msbuild build\build.proj %TargetArg% %ConfigArg% /nologo
exit /b %ERRORLEVEL%

:help
echo build.cmd [Build^|UnitTest^|IntegrationTest^|Documentation^|Package] [/Configuration Debug^|Release]
echo.
echo Examples:
echo build.cmd
echo build.cmd UnitTest
echo build.cmd Package /Configuration Release
