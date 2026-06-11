# Open interactive redis-cli (via Docker container)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Checking infra container (Redis)..." -ForegroundColor Cyan
$running = docker ps --filter "name=masstransit-infra" --format "{{.Names}}" 2>$null

if (-not $running) {
    Write-Host "Starting infra (docker compose)..." -ForegroundColor Yellow
    Set-Location $root
    docker compose up -d --build
    Start-Sleep -Seconds 5
}

Write-Host ""
Write-Host "Useful commands inside redis-cli:" -ForegroundColor Green
Write-Host "  KEYS *              - all saga keys"
Write-Host "  GET <guid>          - read one saga state"
Write-Host "  MONITOR             - live log of all Redis commands"
Write-Host "  exit                - close redis-cli"
Write-Host ""

docker exec -it masstransit-infra redis-cli
