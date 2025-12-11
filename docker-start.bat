@echo off
REM ============================================================================
REM SMT Order Manager - Docker Start Script
REM
REM Einfach Doppelklick auf diese Datei!
REM ============================================================================

echo.
echo ==========================================
echo SMT Order Manager - Docker Start
echo ==========================================
echo.

REM Check if Docker is running
echo [1/5] Checking Docker...
docker info >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Docker is not running!
    echo Please start Docker Desktop and try again.
    echo.
    pause
    exit /b 1
)
echo [OK] Docker is running
echo.

REM Check if we're in the correct directory
echo [2/5] Checking project structure...
if not exist "SmtOrderManager.sln" (
    echo [ERROR] SmtOrderManager.sln not found!
    echo Please run this script from the project root directory.
    echo Current directory: %CD%
    echo.
    pause
    exit /b 1
)
echo [OK] Project structure OK
echo.

REM Build Docker image
echo [3/5] Building Docker image...
echo This may take a few minutes on first run...
docker build -t smt-order-manager:latest -f src/Presentation/SmtOrderManager.Presentation/Dockerfile .
if errorlevel 1 (
    echo [ERROR] Docker build failed!
    echo.
    pause
    exit /b 1
)
echo [OK] Docker image built successfully
echo.

REM Stop and remove existing container (if exists)
echo [4/5] Cleaning up old containers...
docker rm -f smt-order-manager-web >nul 2>&1
echo [OK] Cleanup complete
echo.

REM Run the container
echo [5/5] Starting container...
docker run -d --name smt-order-manager-web -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development smt-order-manager:latest
if errorlevel 1 (
    echo [ERROR] Failed to start container!
    echo.
    pause
    exit /b 1
)
echo [OK] Container started successfully
echo.

REM Wait a moment
echo Waiting for application to start...
timeout /t 3 /nobreak >nul

REM Show logs
echo.
echo ==========================================
echo Container Logs (last 20 lines):
echo ==========================================
docker logs --tail 20 smt-order-manager-web
echo.

REM Success message
echo ==========================================
echo SUCCESS!
echo ==========================================
echo.
echo Your application is now running!
echo.
echo Web App:  http://localhost:8080
echo.
echo Opening browser in 3 seconds...
timeout /t 3 /nobreak >nul
start http://localhost:8080
echo.
echo.
echo Useful commands:
echo   View logs:       docker logs -f smt-order-manager-web
echo   Stop container:  docker stop smt-order-manager-web
echo   Start container: docker start smt-order-manager-web
echo.
echo Press any key to close this window...
pause >nul
