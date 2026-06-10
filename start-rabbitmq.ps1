# Start RabbitMQ via Docker (preferred) and wait until AMQP is fully ready
param(
    [int]$MaxWaitSeconds = 90
)

Write-Host "Starting RabbitMQ container (Docker)..." -ForegroundColor Cyan
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

docker compose up -d

$deadline = (Get-Date).AddSeconds($MaxWaitSeconds)
$ready = $false

while ((Get-Date) -lt $deadline) {
    try {
        docker exec masstransit-rabbitmq rabbitmq-diagnostics -q ping 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) {
            $ready = $true
            break
        }
    }
    catch { }

    Start-Sleep -Seconds 3
}

if (-not $ready) {
    Write-Host "RabbitMQ container did not become ready in time." -ForegroundColor Red
    exit 1
}

Write-Host "RabbitMQ is ready on localhost:5672" -ForegroundColor Green
Write-Host "Dashboard: http://localhost:15672 (guest / guest)" -ForegroundColor Green
