###############################################################################
# SMT Order Manager - Docker Quick Start Script (PowerShell/Windows)
#
# This script builds and runs the SMT Order Manager application in Docker
#
# Usage:
#   .\docker-quickstart.ps1
#
# Requirements:
#   - Docker Desktop installed and running
#   - Run from project root directory (where SmtOrderManager.sln is located)
###############################################################################

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "SMT Order Manager - Docker Quick Start" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
Write-Host "[1/5] Checking Docker..." -ForegroundColor Yellow
try {
    docker info 2>&1 | Out-Null
    Write-Host "✅ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "❌ ERROR: Docker is not running!" -ForegroundColor Red
    Write-Host "   Please start Docker Desktop and try again." -ForegroundColor Red
    exit 1
}
Write-Host ""

# Check if we're in the correct directory
Write-Host "[2/5] Checking project structure..." -ForegroundColor Yellow
if (-not (Test-Path "SmtOrderManager.sln")) {
    Write-Host "❌ ERROR: SmtOrderManager.sln not found!" -ForegroundColor Red
    Write-Host "   Please run this script from the project root directory." -ForegroundColor Red
    exit 1
}
Write-Host "✅ Project structure OK" -ForegroundColor Green
Write-Host ""

# Build Docker image
Write-Host "[3/5] Building Docker image..." -ForegroundColor Yellow
Write-Host "   This may take a few minutes on first run..." -ForegroundColor Gray
docker build -t smt-order-manager:latest -f src/Presentation/SmtOrderManager.Presentation/Dockerfile .
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ ERROR: Docker build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Docker image built successfully" -ForegroundColor Green
Write-Host ""

# Stop and remove existing container (if exists)
Write-Host "[4/5] Cleaning up old containers..." -ForegroundColor Yellow
docker rm -f smt-order-manager-web 2>$null
Write-Host "✅ Cleanup complete" -ForegroundColor Green
Write-Host ""

# Run the container
Write-Host "[5/5] Starting container..." -ForegroundColor Yellow
docker run -d `
  --name smt-order-manager-web `
  -p 8080:8080 `
  -e ASPNETCORE_ENVIRONMENT=Development `
  smt-order-manager:latest

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ ERROR: Failed to start container!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Container started successfully" -ForegroundColor Green
Write-Host ""

# Wait a moment for the app to start
Write-Host "Waiting for application to start..." -ForegroundColor Gray
Start-Sleep -Seconds 3

# Show logs
Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Container Logs (last 20 lines):" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
docker logs --tail 20 smt-order-manager-web
Write-Host ""

# Success message
Write-Host "==========================================" -ForegroundColor Green
Write-Host "✅ SUCCESS!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Your application is now running!" -ForegroundColor White
Write-Host ""
Write-Host "🌐 Web App:  " -NoNewline -ForegroundColor White
Write-Host "http://localhost:8080" -ForegroundColor Cyan
Write-Host ""
Write-Host "Useful commands:" -ForegroundColor Yellow
Write-Host "  View logs:        docker logs -f smt-order-manager-web" -ForegroundColor Gray
Write-Host "  Stop container:   docker stop smt-order-manager-web" -ForegroundColor Gray
Write-Host "  Start container:  docker start smt-order-manager-web" -ForegroundColor Gray
Write-Host "  Remove container: docker rm -f smt-order-manager-web" -ForegroundColor Gray
Write-Host ""
