#!/usr/bin/env pwsh

<#
.SYNOPSIS
	Tags a release for the SideroLabs.Omni.Api project after running all unit tests.

.DESCRIPTION
	This script runs all unit tests in the SideroLabs.Omni.Api.Tests project and, if successful,
	creates a git tag for the current version using NerdBank GitVersioning. Optionally publishes
	the NuGet package and symbols if requested and nuget_key.txt file is available.

.PARAMETER Force
	Forces the tagging even if there are uncommitted changes.

.PARAMETER SkipTests
	Skips running unit tests (not recommended).

.PARAMETER Publish
	Automatically publishes to NuGet without prompting.

.PARAMETER SkipSymbols
	Skips publishing symbols package (not recommended).

.EXAMPLE
	.\Tag.ps1
	Runs tests and tags the current version.

.EXAMPLE
	.\Tag.ps1 -Force
	Runs tests and tags the current version, ignoring uncommitted changes.

.EXAMPLE
	.\Tag.ps1 -Publish
	Runs tests, tags the current version, and publishes to NuGet with symbols.

.EXAMPLE
	.\Tag.ps1 -Publish -SkipSymbols
	Runs tests, tags the current version, and publishes to NuGet without symbols.
#>

[CmdletBinding()]
param(
	[switch]$Force,
	[switch]$SkipTests,
	[switch]$Publish,
	[switch]$SkipSymbols
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Ensure information messages are visible
$InformationPreference = "Continue"

Write-Information "[*] SideroLabs.Omni.Api Release Tagging Script" -InformationAction Continue
Write-Information "=============================================" -InformationAction Continue

try {
	# Check if we're in a git repository
	if (-not (Test-Path ".git")) {
		Write-Error "[!] This script must be run from the root of a git repository."
		exit 1
	}

	# Check for uncommitted changes (unless forced)
	if (-not $Force) {
		$gitStatus = git status --porcelain 2>$null
		if ($LASTEXITCODE -eq 0 -and $gitStatus) {
			Write-Warning "[!] There are uncommitted changes:"
			Write-Output $gitStatus
			Write-Output ""
			Write-Error "Please commit your changes first, or use -Force to ignore this check."
			exit 1
		}
	}

	# Get the current version from NerdBank GitVersioning
	Write-Information "[*] Getting current version..." -InformationAction Continue
	
	# Check if nbgv tool is available - try different approaches
	$nbgvVersion = $null
	$nbgvFound = $false
	$version = $null
	
	# First try: dotnet nbgv (global tool)
	try {
		$nbgvVersion = dotnet nbgv get-version --format json 2>$null
		if ($LASTEXITCODE -eq 0) {
			$nbgvFound = $true
			Write-Information "   Using global dotnet nbgv tool" -InformationAction Continue
		}
	} catch {
		# Intentionally ignore errors and try next approach
		Write-Verbose "Global nbgv tool not available, trying next approach"
	}
	
	# Second try: dotnet tool run nbgv (local tool)
	if (-not $nbgvFound) {
		try {
			$nbgvVersion = dotnet tool run nbgv get-version --format json 2>$null
			if ($LASTEXITCODE -eq 0) {
				$nbgvFound = $true
				Write-Information "   Using local dotnet tool nbgv" -InformationAction Continue
			}
		} catch {
			# Intentionally ignore errors and try next approach
			Write-Verbose "Local nbgv tool not available, trying next approach"
		}
	}
	
	# Third try: Check if it's installed as a local tool and restore if needed
	if (-not $nbgvFound) {
		Write-Information "   NerdBank GitVersioning tool not found, attempting to install..." -InformationAction Continue
		
		# Try to restore local tools first
		if (Test-Path ".config/dotnet-tools.json") {
			Write-Information "   Restoring local tools..." -InformationAction Continue
			dotnet tool restore 2>$null
			if ($LASTEXITCODE -eq 0) {
				$nbgvVersion = dotnet tool run nbgv get-version --format json 2>$null
				if ($LASTEXITCODE -eq 0) {
					$nbgvFound = $true
					Write-Information "   Using restored local nbgv tool" -InformationAction Continue
				}
			}
		}
		
		# If still not found, try to install it locally
		if (-not $nbgvFound) {
			Write-Information "   Installing NerdBank GitVersioning as local tool..." -InformationAction Continue
			dotnet new tool-manifest --force 2>$null
			dotnet tool install nbgv 2>$null
			if ($LASTEXITCODE -eq 0) {
				$nbgvVersion = dotnet tool run nbgv get-version --format json 2>$null
				if ($LASTEXITCODE -eq 0) {
					$nbgvFound = $true
					Write-Information "   Successfully installed and using local nbgv tool" -InformationAction Continue
				}
			}
		}
	}
	
	# Fourth try: Fallback to reading version.json directly
	if (-not $nbgvFound) {
		Write-Information "   NerdBank GitVersioning tool unavailable, using fallback method..." -InformationAction Continue
		
		if (Test-Path "version.json") {
			try {
				$versionJson = Get-Content "version.json" -Raw | ConvertFrom-Json
				$baseVersion = $versionJson.version
				
				# Simple fallback versioning - just use the base version with build number
				$buildNumber = (Get-Date).ToString("yyyyMMdd")
				$version = "$baseVersion-build$buildNumber"
				
				Write-Information "   Using fallback version: $version" -InformationAction Continue
				Write-Information "   Note: This is a simplified version. For production releases, please install nbgv." -InformationAction Continue
			} catch {
				Write-Error "[!] Could not read version.json file."
				exit 1
			}
		} else {
			Write-Error "[!] version.json file not found and nbgv tool unavailable."
			exit 1
		}
	} else {
		# Parse the nbgv output
		$versionObject = $nbgvVersion | ConvertFrom-Json
		$version = $versionObject.NuGetPackageVersion
	}
	
	Write-Information "   Current version: $version" -InformationAction Continue

	# Check if tag already exists
	$existingTag = git tag -l "v$version" 2>$null
	if ($existingTag) {
		Write-Warning "[!] Tag 'v$version' already exists."
		$response = Read-Host "Do you want to delete the existing tag and create a new one? (y/N)"
		if ($response -ne 'y' -and $response -ne 'Y') {
			Write-Error "[!] Tagging cancelled."
			exit 1
		}
		
		Write-Information "[*] Deleting existing tag..." -InformationAction Continue
		git tag -d "v$version" 2>$null
		git push origin ":refs/tags/v$version" 2>$null
	}

	# Restore packages
	Write-Information "[*] Restoring NuGet packages..." -InformationAction Continue
	dotnet restore
	if ($LASTEXITCODE -ne 0) {
		throw "Failed to restore packages"
	}

	# Build the solution
	Write-Information "[*] Building solution..." -InformationAction Continue
	dotnet build --configuration Release --no-restore
	if ($LASTEXITCODE -ne 0) {
		throw "Build failed"
	}

	# Run unit tests (unless skipped)
	if (-not $SkipTests) {
		Write-Information "[*] Running unit tests..." -InformationAction Continue
		
		# Check if test project exists
		$testProject = "SideroLabs.Omni.Api.Tests/SideroLabs.Omni.Api.Tests.csproj"
		if (-not (Test-Path $testProject)) {
			Write-Warning "[!] Test project not found at $testProject. Skipping tests."
		} else {
			# Create TestResults directory if it doesn't exist
			if (-not (Test-Path "TestResults")) {
				New-Item -ItemType Directory -Path "TestResults" | Out-Null
			}

			# Run tests with detailed output
			Write-Information "   Running tests in $testProject..." -InformationAction Continue
			dotnet test $testProject `
				--configuration Release `
				--no-build `
				--verbosity normal `
				--logger "console;verbosity=detailed" `
				--logger "trx;LogFileName=TestResults.trx" `
				--collect:"XPlat Code Coverage" `
				--results-directory "TestResults"

			if ($LASTEXITCODE -ne 0) {
				Write-Error "[!] Unit tests failed! Cannot proceed with tagging."
				Write-Output "   Please fix the failing tests before creating a release."
				exit 1
			}

			Write-Information "[+] All tests passed!" -InformationAction Continue
		}
	} else {
		Write-Warning "[!] Skipping unit tests as requested."
	}

	# Create and push the tag
	Write-Information "[*] Creating tag 'v$version'..." -InformationAction Continue
	git tag -a "v$version" -m "Release version $version"
	
	if ($LASTEXITCODE -ne 0) {
		throw "Failed to create git tag"
	}
	
	Write-Information "[*] Pushing tag to origin..." -InformationAction Continue
	git push origin "v$version"
	
	if ($LASTEXITCODE -ne 0) {
		Write-Warning "[!] Failed to push tag to origin. You may need to push it manually:"
		Write-Output "   git push origin v$version"
	}

	Write-Information "[+] Successfully tagged version $version!" -InformationAction Continue

	# Check for NuGet publishing
	$shouldPublish = $Publish
	if (-not $shouldPublish) {
		$response = Read-Host "Do you want to publish this package to NuGet? (y/N)"
		$shouldPublish = ($response -eq 'y' -or $response -eq 'Y')
	}

	if ($shouldPublish) {
		Write-Information "[*] Preparing to publish NuGet package..." -InformationAction Continue
		
		# Look for API key file
		$apiKeyFile = "nuget_key.txt"
		if (-not (Test-Path $apiKeyFile)) {
			Write-Warning "[!] NuGet API key file '$apiKeyFile' not found."
			Write-Output "   Please create a file named 'nuget_key.txt' containing your NuGet API key."
			Write-Output "   You can get an API key from: https://www.nuget.org/account/apikeys"
			Write-Output "   Skipping NuGet publishing."
			return
		}

		# Read API key
		$apiKey = Get-Content $apiKeyFile -Raw -ErrorAction SilentlyContinue
		if ([string]::IsNullOrWhiteSpace($apiKey)) {
			Write-Error "[!] NuGet API key file is empty or contains only whitespace."
			Write-Output "   Skipping NuGet publishing."
			return
		}
		$apiKey = $apiKey.Trim()

		Write-Information "[+] Found NuGet API key file." -InformationAction Continue

		# Create nupkg directory if it doesn't exist
		if (-not (Test-Path "nupkg")) {
			New-Item -ItemType Directory -Path "nupkg" | Out-Null
		}

		# Build package with symbols
		Write-Information "[*] Building NuGet package with symbols..." -InformationAction Continue
		dotnet pack SideroLabs.Omni.Api/SideroLabs.Omni.Api.csproj `
			--configuration Release `
			--no-build `
			--output "nupkg" `
			--include-symbols `
			--include-source

		if ($LASTEXITCODE -ne 0) {
			throw "Failed to create NuGet package"
		}

		# Find the package files
		$packageFile = Get-ChildItem "nupkg" -Filter "SideroLabs.Omni.Api.$version.nupkg" -ErrorAction SilentlyContinue | Select-Object -First 1
		$symbolsFile = Get-ChildItem "nupkg" -Filter "SideroLabs.Omni.Api.$version.snupkg" -ErrorAction SilentlyContinue | Select-Object -First 1
		
		if (-not $packageFile) {
			Write-Error "[!] Could not find package file for version $version in nupkg directory"
			Write-Output "   Available files:"
			Get-ChildItem "nupkg" -Filter "*.nupkg" | ForEach-Object { Write-Output "   - $($_.Name)" }
			return
		}

		# Publish main package
		Write-Information "[*] Publishing package to NuGet..." -InformationAction Continue
		Write-Information "   Package: $($packageFile.Name)" -InformationAction Continue
		
		dotnet nuget push $packageFile.FullName `
			--api-key $apiKey `
			--source https://api.nuget.org/v3/index.json `
			--skip-duplicate

		$packagePushSuccess = ($LASTEXITCODE -eq 0)
		
		if (-not $packagePushSuccess) {
			Write-Warning "[!] Failed to publish main package. This might be due to:"
			Write-Output "   - Package version already exists"
			Write-Output "   - Invalid API key"
			Write-Output "   - Network issues"
			Write-Output "   You can manually publish later using:"
			Write-Output "   dotnet nuget push $($packageFile.FullName) --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json"
		} else {
			Write-Information "[+] Main package published successfully!" -InformationAction Continue
		}

		# Publish symbols package if available and not skipped
		if ($symbolsFile -and -not $SkipSymbols) {
			Write-Information "[*] Publishing symbols package to NuGet..." -InformationAction Continue
			Write-Information "   Symbols: $($symbolsFile.Name)" -InformationAction Continue
			
			dotnet nuget push $symbolsFile.FullName `
				--api-key $apiKey `
				--source https://api.nuget.org/v3/index.json `
				--skip-duplicate

			if ($LASTEXITCODE -ne 0) {
				Write-Warning "[!] Failed to publish symbols package. This might be due to:"
				Write-Output "   - Symbols package version already exists"
				Write-Output "   - Invalid API key"
				Write-Output "   - Network issues"
				Write-Output "   You can manually publish symbols later using:"
				Write-Output "   dotnet nuget push $($symbolsFile.FullName) --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json"
			} else {
				Write-Information "[+] Symbols package published successfully!" -InformationAction Continue
			}
		} elseif (-not $symbolsFile) {
			Write-Warning "[!] No symbols package found for version $version"
			Write-Information "   Expected file: SideroLabs.Omni.Api.$version.snupkg" -InformationAction Continue
		} elseif ($SkipSymbols) {
			Write-Information "[*] Skipping symbols package publishing as requested." -InformationAction Continue
		}

		if ($packagePushSuccess) {
			Write-Information "[+] Package publishing completed!" -InformationAction Continue
			Write-Information "   Package will be available at: https://www.nuget.org/packages/SideroLabs.Omni.Api/$version" -InformationAction Continue
			if ($symbolsFile -and -not $SkipSymbols) {
				Write-Information "   Symbols will be available for debugging and source stepping" -InformationAction Continue
			}
		}
	}

	Write-Output ""
	Write-Information "[+] Release process completed successfully!" -InformationAction Continue
	Write-Information "   Tag: v$version" -InformationAction Continue
	Write-Information "   You can now create a release on GitHub: https://github.com/panoramicdata/SideroLabs.Omni.Api/releases/new?tag=v$version" -InformationAction Continue

} catch {
	Write-Error "[!] Error during release process: $_"
	Write-Output "   Error details: $($_.Exception.Message)"
	
	# Clean up failed tag if it was created
	if ($version) {
		$tagExists = git tag -l "v$version" 2>$null
		if ($tagExists -and $LASTEXITCODE -eq 0) {
			Write-Information "[*] Cleaning up failed tag..." -InformationAction Continue
			git tag -d "v$version" 2>$null
		}
	}
	
	exit 1
}

Write-Output ""
Write-Information "[*] Next steps:" -InformationAction Continue
Write-Output "   1. Create a GitHub release: https://github.com/panoramicdata/SideroLabs.Omni.Api/releases/new?tag=v$version"
if ($shouldPublish) {
	Write-Output "   2. Monitor NuGet package: https://www.nuget.org/packages/SideroLabs.Omni.Api/"
	Write-Output "   3. Update dependent projects to use the new version"
	if (-not $SkipSymbols) {
		Write-Output "   4. Symbols are available for enhanced debugging experience"
	}
} else {
	Write-Output "   2. Publish to NuGet manually if needed"
}
Write-Information "   [*] Don't forget to add release notes!" -InformationAction Continue
