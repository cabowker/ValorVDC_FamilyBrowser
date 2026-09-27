@echo off
setlocal

:: ─────────────────────────────────────────────────────────────────────────────
:: BuildOnly.bat — builds all Revit target DLLs, then packages the MSI.
::
:: Prerequisites:
::   • WiX Toolset v3.14 installed  (https://wixtoolset.org/releases/)
::   • Visual Studio 2022 or Build Tools installed (MSBuild 17)
::   • Run from the Installer\ directory (or set REPO_ROOT below)
::
:: Output:
::   Installer\bin\Release\ValorVDC_FamilyBrowser_<version>.msi
:: ─────────────────────────────────────────────────────────────────────────────

:: Locate the repo root (one level up from this script's directory)
set "SCRIPT_DIR=%~dp0"
set "REPO_ROOT=%SCRIPT_DIR%.."
pushd "%REPO_ROOT%"

:: ── 1. Locate MSBuild ───────────────────────────────────────────────────────
for /f "usebackq delims=" %%i in (
    `"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`
) do set "MSBUILD=%%i"

if not defined MSBUILD (
    echo ERROR: MSBuild not found. Install Visual Studio 2022 Build Tools.
    popd & exit /b 1
)
echo MSBuild: %MSBUILD%

:: ── 2. Locate WiX candle / light ────────────────────────────────────────────
set "WIX_BIN=%WIX%bin"
if not exist "%WIX_BIN%\candle.exe" (
    :: Try common install path if WIX env var is not set
    set "WIX_BIN=%ProgramFiles(x86)%\WiX Toolset v3.14\bin"
)
if not exist "%WIX_BIN%\candle.exe" (
    echo ERROR: WiX Toolset v3.14 not found.
    echo Install from https://wixtoolset.org/releases/ or set the WIX environment variable.
    popd & exit /b 1
)
echo WiX bin: %WIX_BIN%

:: ── 3. Restore then build each Revit version in sequence ───────────────────
::    IMPORTANT: project.assets.json is shared — restore is paired with its
::    build so a later restore doesn't overwrite the assets before R24 builds.
echo.
echo Restoring and building plugin DLLs...

for %%V in (R24 R25 R26 R27) do (
    echo.
    echo   [%%V] Restoring...
    "%MSBUILD%" ValorVDC_FamilyBrowser.csproj ^
        /p:Configuration="Release %%V" ^
        /p:Platform=AnyCPU ^
        /t:Restore ^
        /v:minimal ^
        /nologo
    if errorlevel 1 (
        echo ERROR: Restore failed for Release %%V
        popd & exit /b 1
    )

    echo   [%%V] Building...
    "%MSBUILD%" ValorVDC_FamilyBrowser.csproj ^
        /p:Configuration="Release %%V" ^
        /p:Platform=AnyCPU ^
        /t:Build ^
        /v:minimal ^
        /nologo
    if errorlevel 1 (
        echo ERROR: Build failed for Release %%V
        popd & exit /b 1
    )
)

:: ── Reset project.assets.json to R24 so Rider stays in sync after the build ──
echo.
echo   Resetting IDE project state to R24...
"%MSBUILD%" ValorVDC_FamilyBrowser.csproj ^
    /p:Configuration="Release R24" ^
    /p:Platform=AnyCPU ^
    /t:Restore ^
    /v:quiet ^
    /nologo

:: ── 5. Verify output DLLs exist ─────────────────────────────────────────────
echo.
echo Verifying output DLLs...
for %%V in (R24 R25 R26 R27) do (
    if not exist "bin\Release %%V\ValorVDC_FamilyBrowser.dll" (
        echo ERROR: bin\Release %%V\ValorVDC_FamilyBrowser.dll not found.
        popd & exit /b 1
    )
    echo   bin\Release %%V\ValorVDC_FamilyBrowser.dll  OK
)

:: ── 6. Read version from Version.props ──────────────────────────────────────
:: Parse <ProductVersion>x.y.z</ProductVersion>
:: Tokens: 1=leading spaces, 2=tag name, 3=value
for /f "tokens=3 delims=><" %%v in (
    'findstr /i "ProductVersion" Version.props'
) do set "PRODUCT_VERSION=%%v"

echo.
echo Product version: %PRODUCT_VERSION%

:: ── 7. Build the MSI ────────────────────────────────────────────────────────
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
