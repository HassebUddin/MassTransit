# Start RabbitMQ + Redis (single Docker container) and wait until AMQP is ready
param(
    [int]$MaxWaitSeconds = 90
)

Write-Host "Starting MassTransit infra container (RabbitMQ + Redis)..." -ForegroundColor Cyan
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

docker compose up -d --build

$deadline = (Get-Date).AddSeconds($MaxWaitSeconds)
$ready = $false

while ((Get-Date) -lt $deadline) {
    try {
        docker exec masstransit-infra rabbitmq-diagnostics -q ping 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) {
            $ready = $true
            break
        }
    }
    catch { }

    Start-Sleep -Seconds 3
}

if (-not $ready) {
    Write-Host "Infra container did not become ready in time." -ForegroundColor Red
    exit 1
}

Write-Host "RabbitMQ is ready on localhost:5672" -ForegroundColor Green
Write-Host "RabbitMQ dashboard: http://localhost:15672 (guest / guest)" -ForegroundColor Green
Write-Host "Redis is ready on localhost:6379" -ForegroundColor Green
