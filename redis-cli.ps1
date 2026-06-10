# Open interactive redis-cli (via Docker container)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Checking Redis container..." -ForegroundColor Cyan
$running = docker ps --filter "name=masstransit-redis" --format "{{.Names}}" 2>$null

if (-not $running) {
    Write-Host "Starting Redis (docker compose)..." -ForegroundColor Yellow
    Set-Location $root
    docker compose up -d redis
    Start-Sleep -Seconds 3
}

Write-Host ""
Write-Host "Useful commands inside redis-cli:" -ForegroundColor Green
Write-Host "  KEYS *              - all saga keys"
Write-Host "  GET <guid>          - read one saga state"
Write-Host "  MONITOR             - live log of all Redis commands"
Write-Host "  exit                - close redis-cli"
Write-Host ""

docker exec -it masstransit-redis redis-cli
