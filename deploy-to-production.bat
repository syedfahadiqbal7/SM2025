@echo off
echo ========================================
echo    SmartCenter CI/CD Deployment Tool
echo ========================================
echo.

REM Set paths
set "MAIN_PROJECT=D:\webapplication_codes2022preview\SmartCenter-main\AFFZ_11012025"
set "GIT_REPO=D:\webapplication_codes2022preview\SmartCenter-main\AFFZ_11012025\SM2025"
set "TIMESTAMP=%date:~-4,4%-%date:~-10,2%-%date:~-7,2%_%time:~0,2%-%time:~3,2%-%time:~6,2%"
set "TIMESTAMP=%TIMESTAMP: =0%"

echo 📁 Main Project: %MAIN_PROJECT%
echo 📁 Git Repository: %GIT_REPO%
echo 🕐 Timestamp: %TIMESTAMP%
echo.

REM Check if main project exists
if not exist "%MAIN_PROJECT%" (
    echo ❌ ERROR: Main project folder not found!
    echo    Expected: %MAIN_PROJECT%
    pause
    exit /b 1
)

REM Check if git repo exists
if not exist "%GIT_REPO%" (
    echo ❌ ERROR: Git repository folder not found!
    echo    Expected: %GIT_REPO%
    pause
    exit /b 1
)

echo ✅ Paths verified successfully!
echo.

REM Navigate to git repository
cd /d "%GIT_REPO%"

REM Check git status
echo 🔍 Checking git status...
git status
echo.

REM Ask user for commit message
set /p COMMIT_MSG="📝 Enter commit message (or press Enter for default): "
if "%COMMIT_MSG%"=="" set "COMMIT_MSG=Auto-deploy: %TIMESTAMP% - Updates from main project"

echo.
echo 📋 Commit message: %COMMIT_MSG%
echo.

REM Copy updated files from main project to git repo
echo 📂 Copying updated files from main project...
echo.

REM Copy AFFZ_API
if exist "%MAIN_PROJECT%\AFFZ_API" (
    echo 📁 Copying AFFZ_API...
    xcopy "%MAIN_PROJECT%\AFFZ_API" "AFFZ_API" /E /Y /Q >nul 2>&1
    if errorlevel 1 (
        echo ⚠️  Warning: Some files in AFFZ_API may not have copied
    ) else (
        echo ✅ AFFZ_API copied successfully
    )
)

REM Copy AFFZ_Admin
if exist "%MAIN_PROJECT%\AFFZ_Admin" (
    echo 📁 Copying AFFZ_Admin...
    xcopy "%MAIN_PROJECT%\AFFZ_Admin" "AFFZ_Admin" /E /Y /Q >nul 2>&1
    if errorlevel 1 (
        echo ⚠️  Warning: Some files in AFFZ_Admin may not have copied
    ) else (
        echo ✅ AFFZ_Admin copied successfully
    )
)

REM Copy AFFZ_MVC (Customer)
if exist "%MAIN_PROJECT%\AFFZ_MVC" (
    echo 📁 Copying AFFZ_MVC...
    xcopy "%MAIN_PROJECT%\AFFZ_MVC" "AFFZ_MVC" /E /Y /Q >nul 2>&1
    if errorlevel 1 (
        echo ⚠️  Warning: Some files in AFFZ_MVC may not have copied
    ) else (
        echo ✅ AFFZ_MVC copied successfully
    )
)

REM Copy AFFZ_Provider
if exist "%MAIN_PROJECT%\AFFZ_Provider" (
    echo 📁 Copying AFFZ_Provider...
    xcopy "%MAIN_PROJECT%\AFFZ_Provider" "AFFZ_Provider" /E /Y /Q >nul 2>&1
    if errorlevel 1 (
        echo ⚠️  Warning: Some files in AFFZ_Provider may not have copied
    ) else (
        echo ✅ AFFZ_Provider copied successfully
    )
)

REM Copy AspireHost
if exist "%MAIN_PROJECT%\AspireHost" (
    echo 📁 Copying AspireHost...
    xcopy "%MAIN_PROJECT%\AspireHost" "AspireHost" /E /Y /Q >nul 2>&1
    if errorlevel 1 (
        echo ⚠️  Warning: Some files in AspireHost may not have copied
    ) else (
        echo ✅ AspireHost copied successfully
    )
)

REM Copy SCAPI.ServiceDefaults
if exist "%MAIN_PROJECT%\SCAPI.ServiceDefaults" (
    echo 📁 Copying SCAPI.ServiceDefaults...
    xcopy "%MAIN_PROJECT%\SCAPI.ServiceDefaults" "SCAPI.ServiceDefaults" /E /Y /Q >nul 2>&1
    if errorlevel 1 (
        echo ⚠️  Warning: Some files in SCAPI.ServiceDefaults may not have copied
    ) else (
        echo ✅ SCAPI.ServiceDefaults copied successfully
    )
)

REM Copy solution file
if exist "%MAIN_PROJECT%\AFFZ_11012025.sln" (
    echo 📁 Copying solution file...
    copy "%MAIN_PROJECT%\AFFZ_11012025.sln" "AFFZ_11012025.sln" >nul 2>&1
    echo ✅ Solution file copied successfully
)

echo.
echo 📋 Files copied successfully!
echo.

REM Add all changes to git
echo 🔄 Adding changes to git...
git add .
if errorlevel 1 (
    echo ❌ ERROR: Failed to add files to git
    pause
    exit /b 1
)

REM Commit changes
echo 💾 Committing changes...
git commit -m "%COMMIT_MSG%"
if errorlevel 1 (
    echo ❌ ERROR: Failed to commit changes
    pause
    exit /b 1
)

REM Push to main branch
echo 🚀 Pushing to main branch...
git push origin main
if errorlevel 1 (
    echo ❌ ERROR: Failed to push to main branch
    pause
    exit /b 1
)

echo.
echo ========================================
echo    🎉 DEPLOYMENT TRIGGERED SUCCESSFULLY!
echo ========================================
echo.
echo 📋 What happens next:
echo    1. ✅ Build process starts (6 projects)
echo    2. 🚀 SIT deployment begins
echo    3. 🚀 UAT deployment (after SIT success)
echo    4. 🚀 Production deployment (after UAT success)
echo.
echo 🚀 Projects included:
echo    - AFFZ_API
echo    - AFFZ_Admin
echo    - AFFZ_Customer
echo    - AFFZ_Provider
echo    - AspireHost
echo    - SCAPI.ServiceDefaults
echo.
echo 🌐 Check progress at: https://github.com/syedfahadiqbal7/SM2025/actions
echo.
echo ⏳ Deployment will take approximately 10-15 minutes
echo.
pause
