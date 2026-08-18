@echo off
echo ===================================================
echo   Compiling CPU Beyond C# Application (by Diego A)
echo ===================================================

set CSC_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

if not exist "%CSC_PATH%" (
    echo [ERROR] C# Compiler csc.exe not found at %CSC_PATH%
    pause
    exit /b 1
)

"%CSC_PATH%" /nologo /target:winexe /out:cpuBeyond.exe /win32icon:app_icon.ico /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Management.dll /r:System.Core.dll Program.cs HardwareInspector.cs ReportGenerator.cs EmbeddedLogo.cs

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ===================================================
    echo   [SUCCESS] cpuBeyond.exe compiled successfully!
    echo   Custom icon embedded into executable.
    echo ===================================================
) else (
    echo.
    echo ===================================================
    echo   [ERROR] Compilation failed! Check errors above.
    echo ===================================================
)
