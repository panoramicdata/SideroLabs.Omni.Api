#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Debug script to test the Tag.ps1 functionality step by step.

.DESCRIPTION
    This script tests each component of the Tag.ps1 script individually to help troubleshoot issues.
#>

Write-Host "🔍 SideroLabs.Omni.Api Debug Script" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan

try {
    # Test 1: Check git repository
    Write-Host "1. Checking git repository..." -ForegroundColor Blue
    if (Test-Path ".git") {
        Write-Host "   ✅ Git repository found" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Not a git repository" -ForegroundColor Red
        exit 1
    }

    # Test 2: Check git status
    Write-Host "2. Checking git status..." -ForegroundColor Blue
    $gitStatus = git status --porcelain 2>$null
    if ($LASTEXITCODE -eq 0) {
        if ($gitStatus) {
            Write-Host "   ⚠️  Uncommitted changes found:" -ForegroundColor Yellow
            $gitStatus | ForEach-Object { Write-Host "      $_" -ForegroundColor Yellow }
        } else {
            Write-Host "   ✅ Working directory clean" -ForegroundColor Green
        }
    } else {
        Write-Host "   ❌ Git status command failed" -ForegroundColor Red
    }

    # Test 3: Check NerdBank GitVersioning
    Write-Host "3. Testing NerdBank GitVersioning..." -ForegroundColor Blue
    $nbgvOutput = dotnet nbgv get-version --format json 2>$null
    if ($LASTEXITCODE -eq 0) {
        $versionObject = $nbgvOutput | ConvertFrom-Json
        $version = $versionObject.NuGetPackageVersion
        Write-Host "   ✅ NerdBank GitVersioning working" -ForegroundColor Green
        Write-Host "   📋 Current version: $version" -ForegroundColor Cyan
    } else {
        Write-Host "   ❌ NerdBank GitVersioning failed" -ForegroundColor Red
        Write-Host "   Error: $nbgvOutput" -ForegroundColor Red
    }

    # Test 4: Check dotnet restore
    Write-Host "4. Testing dotnet restore..." -ForegroundColor Blue
    dotnet restore --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✅ Package restore successful" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Package restore failed" -ForegroundColor Red
    }

    # Test 5: Check dotnet build
    Write-Host "5. Testing dotnet build..." -ForegroundColor Blue
    dotnet build --configuration Release --no-restore --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✅ Build successful" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Build failed" -ForegroundColor Red
    }

    # Test 6: Check test project
    Write-Host "6. Checking test project..." -ForegroundColor Blue
    $testProject = "SideroLabs.Omni.Api.Tests/SideroLabs.Omni.Api.Tests.csproj"
    if (Test-Path $testProject) {
        Write-Host "   ✅ Test project found" -ForegroundColor Green
        
        # Test 7: Run tests
        Write-Host "7. Testing unit tests..." -ForegroundColor Blue
        dotnet test $testProject --configuration Release --no-build --verbosity quiet
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ Tests passed" -ForegroundColor Green
        } else {
            Write-Host "   ❌ Tests failed" -ForegroundColor Red
        }
    } else {
        Write-Host "   ❌ Test project not found at $testProject" -ForegroundColor Red
    }

    # Test 8: Check package creation
    Write-Host "8. Testing package creation..." -ForegroundColor Blue
    if (-not (Test-Path "nupkg")) {
        New-Item -ItemType Directory -Path "nupkg" | Out-Null
    }
    
    dotnet pack SideroLabs.Omni.Api/SideroLabs.Omni.Api.csproj --configuration Release --no-build --output "nupkg" --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✅ Package creation successful" -ForegroundColor Green
        $packages = Get-ChildItem "nupkg" -Filter "*.nupkg"
        $packages | ForEach-Object { Write-Host "      📦 $($_.Name)" -ForegroundColor Cyan }
    } else {
        Write-Host "   ❌ Package creation failed" -ForegroundColor Red
    }

    # Test 9: Check PowerShell execution policy
    Write-Host "9. Checking PowerShell execution policy..." -ForegroundColor Blue
    $executionPolicy = Get-ExecutionPolicy
    Write-Host "   📋 Current execution policy: $executionPolicy" -ForegroundColor Cyan
    if ($executionPolicy -in @("Restricted", "AllSigned")) {
        Write-Host "   ⚠️  Execution policy may prevent script execution" -ForegroundColor Yellow
        Write-Host "   💡 Try running with: powershell -ExecutionPolicy Bypass -File ./Tag.ps1" -ForegroundColor Cyan
    } else {
        Write-Host "   ✅ Execution policy allows script execution" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "🎉 Debug check completed!" -ForegroundColor Green
    Write-Host "   If all tests passed, Tag.ps1 should work correctly." -ForegroundColor Cyan
    Write-Host "   To run the tagging script:" -ForegroundColor Cyan
    Write-Host "   • With tests: .\Tag.ps1" -ForegroundColor White
    Write-Host "   • Skip tests: .\Tag.ps1 -SkipTests" -ForegroundColor White
    Write-Host "   • Force run: .\Tag.ps1 -Force" -ForegroundColor White

} catch {
    Write-Error "❌ Debug check failed: $_"
    Write-Host "   Error details: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
