#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Tags a release for the SideroLabs.Omni.Api project after running all unit tests.

.DESCRIPTION
    This script runs all unit tests in the SideroLabs.Omni.Api.Tests project and, if successful,
    creates a git tag for the current version using NerdBank GitVersioning. Optionally publishes
    the NuGet package if requested and nuget_key.txt file is available.

.PARAMETER Force
    Forces the tagging even if there are uncommitted changes.

.PARAMETER SkipTests
    Skips running unit tests (not recommended).

.PARAMETER Publish
    Automatically publishes to NuGet without prompting.

.EXAMPLE
    .\Tag.ps1
    Runs tests and tags the current version.

.EXAMPLE
    .\Tag.ps1 -Force
    Runs tests and tags the current version, ignoring uncommitted changes.

.EXAMPLE
    .\Tag.ps1 -Publish
    Runs tests, tags the current version, and publishes to NuGet.
#>

[CmdletBinding()]
param(
    [switch]$Force,
    [switch]$SkipTests,
    [switch]$Publish
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "🚀 SideroLabs.Omni.Api Release Tagging Script" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

try {
    # Check if we're in a git repository
    if (-not (Test-Path ".git")) {
        Write-Error "❌ This script must be run from the root of a git repository."
        exit 1
    }

    # Check for uncommitted changes (unless forced)
    if (-not $Force) {
        $gitStatus = git status --porcelain 2>$null
        if ($LASTEXITCODE -eq 0 -and $gitStatus) {
            Write-Warning "⚠️  There are uncommitted changes:"
            Write-Host $gitStatus -ForegroundColor Yellow
            Write-Host ""
            Write-Host "Please commit your changes first, or use -Force to ignore this check." -ForegroundColor Red
            exit 1
        }
    }

    # Get the current version from NerdBank GitVersioning
    Write-Host "📋 Getting current version..." -ForegroundColor Blue
    
    # Check if nbgv tool is available
    $nbgvVersion = dotnet nbgv get-version --format json 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ NerdBank GitVersioning tool not found. Make sure it's installed and the project is configured correctly."
        exit 1
    }
    
    $versionObject = $nbgvVersion | ConvertFrom-Json
    $version = $versionObject.NuGetPackageVersion
    Write-Host "   Current version: $version" -ForegroundColor Green

    # Check if tag already exists
    $existingTag = git tag -l "v$version" 2>$null
    if ($existingTag) {
        Write-Warning "⚠️  Tag 'v$version' already exists."
        $response = Read-Host "Do you want to delete the existing tag and create a new one? (y/N)"
        if ($response -ne 'y' -and $response -ne 'Y') {
            Write-Host "❌ Tagging cancelled." -ForegroundColor Red
            exit 1
        }
        
        Write-Host "🗑️  Deleting existing tag..." -ForegroundColor Yellow
        git tag -d "v$version" 2>$null
        git push origin ":refs/tags/v$version" 2>$null
    }

    # Restore packages
    Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Blue
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to restore packages"
    }

    # Build the solution
    Write-Host "🔨 Building solution..." -ForegroundColor Blue
    dotnet build --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }

    # Run unit tests (unless skipped)
    if (-not $SkipTests) {
        Write-Host "🧪 Running unit tests..." -ForegroundColor Blue
        
        # Check if test project exists
        $testProject = "SideroLabs.Omni.Api.Tests/SideroLabs.Omni.Api.Tests.csproj"
        if (-not (Test-Path $testProject)) {
            Write-Warning "⚠️  Test project not found at $testProject. Skipping tests."
        } else {
            # Create TestResults directory if it doesn't exist
            if (-not (Test-Path "TestResults")) {
                New-Item -ItemType Directory -Path "TestResults" | Out-Null
            }

            # Run tests with detailed output
            Write-Host "   Running tests in $testProject..." -ForegroundColor Cyan
            dotnet test $testProject `
                --configuration Release `
                --no-build `
                --verbosity normal `
                --logger "console;verbosity=detailed" `
                --logger "trx;LogFileName=TestResults.trx" `
                --collect:"XPlat Code Coverage" `
                --results-directory "TestResults"

            if ($LASTEXITCODE -ne 0) {
                Write-Error "❌ Unit tests failed! Cannot proceed with tagging."
                Write-Host "   Please fix the failing tests before creating a release." -ForegroundColor Red
                exit 1
            }

            Write-Host "✅ All tests passed!" -ForegroundColor Green
        }
    } else {
        Write-Warning "⚠️  Skipping unit tests as requested."
    }

    # Create and push the tag
    Write-Host "🏷️  Creating tag 'v$version'..." -ForegroundColor Blue
    git tag -a "v$version" -m "Release version $version"
    
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create git tag"
    }
    
    Write-Host "📤 Pushing tag to origin..." -ForegroundColor Blue
    git push origin "v$version"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "⚠️  Failed to push tag to origin. You may need to push it manually:"
        Write-Host "   git push origin v$version" -ForegroundColor Cyan
    }

    Write-Host "🎉 Successfully tagged version $version!" -ForegroundColor Green

    # Check for NuGet publishing
    $shouldPublish = $Publish
    if (-not $shouldPublish) {
        $response = Read-Host "Do you want to publish this package to NuGet? (y/N)"
        $shouldPublish = ($response -eq 'y' -or $response -eq 'Y')
    }

    if ($shouldPublish) {
        Write-Host "📦 Preparing to publish NuGet package..." -ForegroundColor Blue
        
        # Look for API key file
        $apiKeyFile = "nuget_key.txt"
        if (-not (Test-Path $apiKeyFile)) {
            Write-Warning "⚠️  NuGet API key file '$apiKeyFile' not found."
            Write-Host "   Please create a file named 'nuget_key.txt' containing your NuGet API key." -ForegroundColor Yellow
            Write-Host "   You can get an API key from: https://www.nuget.org/account/apikeys" -ForegroundColor Yellow
            Write-Host "   Skipping NuGet publishing." -ForegroundColor Yellow
            return
        }

        # Read API key
        $apiKey = Get-Content $apiKeyFile -Raw -ErrorAction SilentlyContinue
        if ([string]::IsNullOrWhiteSpace($apiKey)) {
            Write-Error "❌ NuGet API key file is empty or contains only whitespace."
            Write-Host "   Skipping NuGet publishing." -ForegroundColor Yellow
            return
        }
        $apiKey = $apiKey.Trim()

        Write-Host "🔑 Found NuGet API key file." -ForegroundColor Green

        # Create nupkg directory if it doesn't exist
        if (-not (Test-Path "nupkg")) {
            New-Item -ItemType Directory -Path "nupkg" | Out-Null
        }

        # Build package
        Write-Host "📦 Building NuGet package..." -ForegroundColor Blue
        dotnet pack SideroLabs.Omni.Api/SideroLabs.Omni.Api.csproj `
            --configuration Release `
            --no-build `
            --output "nupkg"

        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create NuGet package"
        }

        # Find the package file
        $packageFile = Get-ChildItem "nupkg" -Filter "SideroLabs.Omni.Api.$version.nupkg" -ErrorAction SilentlyContinue | Select-Object -First 1
        if (-not $packageFile) {
            Write-Error "❌ Could not find package file for version $version in nupkg directory"
            Write-Host "   Available files:" -ForegroundColor Yellow
            Get-ChildItem "nupkg" -Filter "*.nupkg" | ForEach-Object { Write-Host "   - $($_.Name)" -ForegroundColor Yellow }
            return
        }

        Write-Host "📤 Publishing package to NuGet..." -ForegroundColor Blue
        Write-Host "   Package: $($packageFile.Name)" -ForegroundColor Cyan
        
        dotnet nuget push $packageFile.FullName `
            --api-key $apiKey `
            --source https://api.nuget.org/v3/index.json `
            --skip-duplicate

        if ($LASTEXITCODE -ne 0) {
            Write-Warning "⚠️  Failed to publish package. This might be due to:"
            Write-Host "   - Package version already exists" -ForegroundColor Yellow
            Write-Host "   - Invalid API key" -ForegroundColor Yellow
            Write-Host "   - Network issues" -ForegroundColor Yellow
            Write-Host "   You can manually publish later using:" -ForegroundColor Cyan
            Write-Host "   dotnet nuget push $($packageFile.FullName) --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json" -ForegroundColor Cyan
        } else {
            Write-Host "🎉 Package published successfully!" -ForegroundColor Green
            Write-Host "   Package will be available at: https://www.nuget.org/packages/SideroLabs.Omni.Api/$version" -ForegroundColor Cyan
        }
    }

    Write-Host ""
    Write-Host "✨ Release process completed successfully!" -ForegroundColor Green
    Write-Host "   Tag: v$version" -ForegroundColor Cyan
    Write-Host "   You can now create a release on GitHub: https://github.com/panoramicdata/SideroLabs.Omni.Api/releases/new?tag=v$version" -ForegroundColor Cyan

} catch {
    Write-Error "❌ Error during release process: $_"
    Write-Host "   Error details: $($_.Exception.Message)" -ForegroundColor Red
    
    # Clean up failed tag if it was created
    if ($version) {
        $tagExists = git tag -l "v$version" 2>$null
        if ($tagExists -and $LASTEXITCODE -eq 0) {
            Write-Host "🧹 Cleaning up failed tag..." -ForegroundColor Yellow
            git tag -d "v$version" 2>$null
        }
    }
    
    exit 1
}

Write-Host ""
Write-Host "🎯 Next steps:" -ForegroundColor Cyan
Write-Host "   1. Create a GitHub release: https://github.com/panoramicdata/SideroLabs.Omni.Api/releases/new?tag=v$version" -ForegroundColor White
if ($shouldPublish) {
    Write-Host "   2. Monitor NuGet package: https://www.nuget.org/packages/SideroLabs.Omni.Api/" -ForegroundColor White
    Write-Host "   3. Update dependent projects to use the new version" -ForegroundColor White
} else {
    Write-Host "   2. Publish to NuGet manually if needed" -ForegroundColor White
}
Write-Host "   📋 Don't forget to add release notes!" -ForegroundColor Yellow
