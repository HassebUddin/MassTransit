# Start RabbitMQ (Docker) then Sample.Service
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "=== Step 1: Starting RabbitMQ + Redis (Docker) ===" -ForegroundColor Cyan
& "$root\start-rabbitmq.ps1"
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "RabbitMQ: http://localhost:15672 | MongoDB: localhost:27017 (orders db)" -ForegroundColor Green

Get-Process Sample.Service -ErrorAction SilentlyContinue | Stop-Process -Force

Write-Host ""
Write-Host "=== Step 2: Starting Sample.Service ===" -ForegroundColor Cyan
Set-Location "$root\Sample.Service"
dotnet run -- --console
