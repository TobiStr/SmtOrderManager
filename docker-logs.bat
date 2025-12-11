@echo off
REM ============================================================================
REM SMT Order Manager - Docker Logs Viewer
REM
REM Zeigt die Container-Logs in Echtzeit
REM ============================================================================

echo.
echo ==========================================
echo SMT Order Manager - Live Logs
echo ==========================================
echo.
echo Press CTRL+C to exit
echo.
echo.

docker logs -f smt-order-manager-web

pause
