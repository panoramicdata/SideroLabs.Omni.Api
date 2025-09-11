$ErrorActionPreference = "Stop"

try {
    Write-Host "Fetching main.go from GitHub API..."
    $response = Invoke-RestMethod -Uri "https://api.github.com/repos/rothgar/siderolabs-omni-client/contents/main.go"
    
    if ($response.content) {
        Write-Host "Content found, decoding..."
        $content = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($response.content))
        
        # Write to a temporary file for inspection
        $content | Out-File -FilePath "temp-main.go" -Encoding UTF8
        Write-Host "Content saved to temp-main.go"
        
        # Display first part of content
        Write-Host "First 2000 characters:"
        Write-Host $content.Substring(0, [Math]::Min(2000, $content.Length))
    } else {
        Write-Host "No content found in response"
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)"
}
