#!/bin/bash

# Göztepe Volleyball Scraper - Build & Publish Script
# This script builds both frontend and backend and publishes to ./publish directory

set -e  # Exit on any error

echo "🏐 Starting Göztepe Volleyball Scraper Build Process..."
echo "=============================================="

# Clean previous builds
echo "🧹 Cleaning previous builds..."
rm -rf publish/*
rm -rf build/

# Create directories
mkdir -p publish/api
mkdir -p publish/frontend

echo ""
echo "🧪 Running Backend Tests..."
echo "----------------------------"
cd src/backend
if dotnet test --verbosity minimal; then
    echo "✅ Backend tests passed!"
else
    echo "❌ Backend tests failed! Aborting build."
    exit 1
fi

echo ""
echo "🔨 Building Backend (API)..."
echo "-----------------------------"
if dotnet publish VolleyballScraper.Api -c Release -o ../../publish/api --no-restore; then
    echo "✅ Backend build completed successfully!"
else
    echo "❌ Backend build failed!"
    exit 1
fi

echo ""
echo "🎨 Building Frontend..."
echo "-----------------------"
cd ../frontend
if npm ci && npm run build; then
    echo "✅ Frontend build completed successfully!"
else
    echo "❌ Frontend build failed!"
    exit 1
fi

# Copy frontend build to publish directory
echo ""
echo "📦 Copying Frontend build..."
cd ../../
cp -r build/frontend/* publish/frontend/

echo ""
echo "🎉 Build Process Completed Successfully!"
echo "========================================"
echo ""
echo "📁 Published files located at:"
echo "   • API: ./publish/api/"
echo "   • Frontend: ./publish/frontend/"
echo ""
echo "🚀 Ready for deployment!"