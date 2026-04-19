# Göztepe Volleyball Scraper - Build & Publish Script (PowerShell)
# This script builds both frontend and backend and publishes to ./publish directory

$ErrorActionPreference = "Stop"

Write-Host "🏐 Starting Göztepe Volleyball Scraper Build Process..." -ForegroundColor Green
Write-Host "=============================================="

# Clean previous builds
Write-Host "🧹 Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path "publish") { Remove-Item -Recurse -Force "publish/*" }

# Create directories
New-Item -ItemType Directory -Force -Path "publish" | Out-Null
New-Item -ItemType Directory -Force -Path "publish/wwwroot" | Out-Null

Write-Host ""
Write-Host "🧪 Running Backend Tests..." -ForegroundColor Cyan
Write-Host "----------------------------"
Set-Location "src/backend"
try {
    dotnet test --verbosity minimal
    Write-Host "✅ Backend tests passed!" -ForegroundColor Green
}
catch {
    Write-Host "❌ Backend tests failed! Aborting build." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "🔨 Building Backend (API)..." -ForegroundColor Cyan
Write-Host "-----------------------------"
try {
    dotnet publish OlympicScraper.Api -c Release -o "../../publish" --no-restore
    Write-Host "✅ Backend build completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "❌ Backend build failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "🎨 Building Frontend..." -ForegroundColor Cyan
Write-Host "-----------------------"
Set-Location "../frontend"
try {
    npm ci
    npm run build
    Write-Host "✅ Frontend build completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "❌ Frontend build failed!" -ForegroundColor Red
    exit 1
}

# Frontend already built directly to publish/wwwroot via Vite config
Write-Host ""
Write-Host "📦 Frontend built directly to wwwroot..." -ForegroundColor Yellow
Set-Location "../../"

Write-Host ""
Write-Host "🎉 Build Process Completed Successfully!" -ForegroundColor Green
Write-Host "========================================"
Write-Host ""
Write-Host "📁 Published files located at:" -ForegroundColor White
Write-Host "   • API: ./publish/ (root)" -ForegroundColor White
Write-Host "   • Frontend: ./publish/wwwroot/" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Ready for deployment!" -ForegroundColor Green