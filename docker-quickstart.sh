#!/bin/bash

###############################################################################
# SMT Order Manager - Docker Quick Start Script
#
# This script builds and runs the SMT Order Manager application in Docker
#
# Usage:
#   ./docker-quickstart.sh
#
# Requirements:
#   - Docker Desktop installed and running
#   - Run from project root directory (where SmtOrderManager.sln is located)
###############################################################################

set -e  # Exit on error

echo "=========================================="
echo "SMT Order Manager - Docker Quick Start"
echo "=========================================="
echo ""

# Check if Docker is running
echo "[1/5] Checking Docker..."
if ! docker info > /dev/null 2>&1; then
    echo "❌ ERROR: Docker is not running!"
    echo "   Please start Docker Desktop and try again."
    exit 1
fi
echo "✅ Docker is running"
echo ""

# Check if we're in the correct directory
echo "[2/5] Checking project structure..."
if [ ! -f "SmtOrderManager.sln" ]; then
    echo "❌ ERROR: SmtOrderManager.sln not found!"
    echo "   Please run this script from the project root directory."
    exit 1
fi
echo "✅ Project structure OK"
echo ""

# Build Docker image
echo "[3/5] Building Docker image..."
echo "   This may take a few minutes on first run..."
docker build -t smt-order-manager:latest -f src/Presentation/SmtOrderManager.Presentation/Dockerfile .
echo "✅ Docker image built successfully"
echo ""

# Stop and remove existing container (if exists)
echo "[4/5] Cleaning up old containers..."
docker rm -f smt-order-manager-web 2>/dev/null || true
echo "✅ Cleanup complete"
echo ""

# Run the container
echo "[5/5] Starting container..."
docker run -d \
  --name smt-order-manager-web \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  smt-order-manager:latest

echo "✅ Container started successfully"
echo ""

# Wait a moment for the app to start
echo "Waiting for application to start..."
sleep 3

# Show logs
echo ""
echo "=========================================="
echo "Container Logs (last 20 lines):"
echo "=========================================="
docker logs --tail 20 smt-order-manager-web
echo ""

# Success message
echo "=========================================="
echo "✅ SUCCESS!"
echo "=========================================="
echo ""
echo "Your application is now running!"
echo ""
echo "🌐 Web App:  http://localhost:8080"
echo ""
echo "Useful commands:"
echo "  View logs:       docker logs -f smt-order-manager-web"
echo "  Stop container:  docker stop smt-order-manager-web"
echo "  Start container: docker start smt-order-manager-web"
echo "  Remove container: docker rm -f smt-order-manager-web"
echo ""
