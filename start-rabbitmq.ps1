# Start RabbitMQ, Redis, and MongoDB via Docker and wait until ready
param(
    [int]$MaxWaitSeconds = 90
)

Write-Host "Starting RabbitMQ, Redis, and MongoDB containers (Docker)..." -ForegroundColor Cyan
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

$redisReady = $false
$redisDeadline = (Get-Date).AddSeconds(30)
while ((Get-Date) -lt $redisDeadline) {
    $pong = docker exec masstransit-redis redis-cli ping 2>$null
    if ($pong -eq "PONG") {
        $redisReady = $true
        break
    }
    Start-Sleep -Seconds 2
}

if (-not $redisReady) {
    Write-Host "Redis container did not become ready in time." -ForegroundColor Red
    exit 1
}

$mongoReady = $false
$mongoDeadline = (Get-Date).AddSeconds(30)
while ((Get-Date) -lt $mongoDeadline) {
    docker exec masstransit-mongodb mongosh --quiet --eval "db.runCommand({ ping: 1 }).ok" 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) {
        $mongoReady = $true
        break
    }
    Start-Sleep -Seconds 2
}

if (-not $mongoReady) {
    Write-Host "MongoDB container did not become ready in time." -ForegroundColor Red
    exit 1
}

Write-Host "RabbitMQ is ready on localhost:5672" -ForegroundColor Green
Write-Host "Redis is ready on localhost:6379" -ForegroundColor Green
Write-Host "MongoDB is ready on localhost:27017" -ForegroundColor Green
Write-Host "Dashboard: http://localhost:15672 (guest / guest)" -ForegroundColor Green
