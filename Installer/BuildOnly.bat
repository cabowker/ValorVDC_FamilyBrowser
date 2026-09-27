@echo off
setlocal

:: ─────────────────────────────────────────────────────────────────────────────
:: BuildOnly.bat — packages the MSI from already-built DLLs.
::
:: Run Build.bat first to compile the plugin DLLs for all Revit versions.
:: This script only runs candle + light — it never touches the C# project,
:: so Rider's project.assets.json is left undisturbed.
::
:: Prerequisites:
::   • WiX Toolset v3.14 installed  (https://wixtoolset.org/releases/)
::   • DLLs already built:
::       bin\Release R24\ValorVDC_FamilyBrowser.dll
::       bin\Release R25\ValorVDC_FamilyBrowser.dll
::       bin\Release R26\ValorVDC_FamilyBrowser.dll
::       bin\Release R27\ValorVDC_FamilyBrowser.dll
::
:: Output:
::   Installer\bin\Release\ValorVDC_FamilyBrowser_<version>.msi
:: ─────────────────────────────────────────────────────────────────────────────

:: Locate the repo root (one level up from this script's directory)
set "SCRIPT_DIR=%~dp0"
set "REPO_ROOT=%SCRIPT_DIR%.."
pushd "%REPO_ROOT%"

:: ── 1. Locate WiX candle / light ────────────────────────────────────────────
set "WIX_BIN=%WIX%bin"
if not exist "%WIX_BIN%\candle.exe" (
    set "WIX_BIN=%ProgramFiles(x86)%\WiX Toolset v3.14\bin"
)
if not exist "%WIX_BIN%\candle.exe" (
    echo ERROR: WiX Toolset v3.14 not found.
    echo Install from https://wixtoolset.org/releases/ or set the WIX environment variable.
    popd & exit /b 1
)
echo WiX bin: %WIX_BIN%

:: ── 2. Verify output DLLs exist ─────────────────────────────────────────────
echo.
echo Verifying output DLLs...
for %%V in (R24 R25 R26 R27) do (
    if not exist "bin\Release %%V\ValorVDC_FamilyBrowser.dll" (
        echo ERROR: bin\Release %%V\ValorVDC_FamilyBrowser.dll not found.
        echo        Run Build.bat first to compile plugin DLLs for all Revit versions.
        popd & exit /b 1
    )
    echo   bin\Release %%V\ValorVDC_FamilyBrowser.dll  OK
)

:: ── 3. Read version from Version.props ──────────────────────────────────────
for /f "tokens=3 delims=><" %%v in (
    'findstr /i "ProductVersion" Version.props'
) do set "PRODUCT_VERSION=%%v"

echo.
echo Product version: %PRODUCT_VERSION%

:: ── 4. Build the MSI ────────────────────────────────────────────────────────
echo.
echo Building MSI...

pushd "%SCRIPT_DIR%"

set "OBJ_DIR=obj\Release"
set "OUT_DIR=bin\Release"
if not exist "%OBJ_DIR%" mkdir "%OBJ_DIR%"
if not exist "%OUT_DIR%" mkdir "%OUT_DIR%"

:: Compile WiX source
"%WIX_BIN%\candle.exe" ^
    Product.wxs ^
    -dProductVersion=%PRODUCT_VERSION% ^
    -ext "%WIX_BIN%\WixUIExtension.dll" ^
    -ext "%WIX_BIN%\WixUtilExtension.dll" ^
    -out "%OBJ_DIR%\\" ^
    -nologo

if errorlevel 1 (
    echo ERROR: WiX candle failed.
    popd & popd & exit /b 1
)

:: Link
"%WIX_BIN%\light.exe" ^
    "%OBJ_DIR%\Product.wixobj" ^
    -ext "%WIX_BIN%\WixUIExtension.dll" ^
    -ext "%WIX_BIN%\WixUtilExtension.dll" ^
    -out "%OUT_DIR%\ValorVDC_FamilyBrowser_%PRODUCT_VERSION%.msi" ^
    -nologo

if errorlevel 1 (
    echo ERROR: WiX light failed.
    popd & popd & exit /b 1
)

echo.
echo ──────────────────────────────────────────────────────────────────────────
echo  SUCCESS
echo  MSI: Installer\%OUT_DIR%\ValorVDC_FamilyBrowser_%PRODUCT_VERSION%.msi
echo ──────────────────────────────────────────────────────────────────────────

popd & popd
endlocal
