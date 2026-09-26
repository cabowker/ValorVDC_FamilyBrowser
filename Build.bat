@echo off
setlocal

:: ─────────────────────────────────────────────────────────────────────────────
:: Build.bat — clean + restore + build all Revit Release configurations.
::
:: Usage (from repo root or any subdirectory):
::   Build.bat          — clean then build R24, R25, R26, R27
::   Build.bat R24      — clean then build only R24
::   Build.bat R25 R26  — clean then build R25 and R26
::
:: Output DLLs land in:
::   bin\Release R24\ValorVDC_FamilyBrowser.dll   (net48)
::   bin\Release R25\ValorVDC_FamilyBrowser.dll   (net8.0-windows)
::   bin\Release R26\ValorVDC_FamilyBrowser.dll   (net8.0-windows)
::   bin\Release R27\ValorVDC_FamilyBrowser.dll   (net10.0-windows)
:: ─────────────────────────────────────────────────────────────────────────────

:: Always run from the repo root (where the .csproj lives)
set "REPO_ROOT=%~dp0"
pushd "%REPO_ROOT%"

:: ── Locate MSBuild ───────────────────────────────────────────────────────────
for /f "usebackq delims=" %%i in (
    `"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`
) do set "MSBUILD=%%i"

if not defined MSBUILD (
    echo ERROR: MSBuild not found. Install Visual Studio 2022 Build Tools.
    popd & exit /b 1
)

:: ── Determine which versions to build ───────────────────────────────────────
if "%~1"=="" (
    set "VERSIONS=R24 R25 R26 R27"
) else (
    set "VERSIONS=%*"
)

:: ── Clean + Restore + Build each version ────────────────────────────────────
echo.
echo Building: %VERSIONS%
echo.

for %%V in (%VERSIONS%) do (
    echo ── %%V ─────────────────────────────────────────────────────────────────
    echo   Restoring...
    "%MSBUILD%" ValorVDC_FamilyBrowser.csproj ^
        /p:Configuration="Release %%V" ^
        /p:Platform=AnyCPU ^
        /t:Restore ^
        /v:minimal /nologo
    if errorlevel 1 ( echo ERROR: Restore failed for %%V & popd & exit /b 1 )

    echo   Cleaning...
    "%MSBUILD%" ValorVDC_FamilyBrowser.csproj ^
        /p:Configuration="Release %%V" ^
        /p:Platform=AnyCPU ^
        /t:Clean ^
        /v:minimal /nologo
    if errorlevel 1 ( echo ERROR: Clean failed for %%V & popd & exit /b 1 )

    echo   Building...
    "%MSBUILD%" ValorVDC_FamilyBrowser.csproj ^
        /p:Configuration="Release %%V" ^
        /p:Platform=AnyCPU ^
        /t:Build ^
        /v:minimal /nologo
    if errorlevel 1 ( echo ERROR: Build failed for %%V & popd & exit /b 1 )

    echo   OK: bin\Release %%V\ValorVDC_FamilyBrowser.dll
    echo.
)

echo ─────────────────────────────────────────────────────────────────────────
echo  Done.
echo ─────────────────────────────────────────────────────────────────────────
popd
endlocal
