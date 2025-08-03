# Script para testar os dados do IIS que chegam no C#
Write-Host "=== Testando dados do IIS ===" -ForegroundColor Green

# Comando igual ao que está no IISManagementService
$sites = @()
Get-IISSite | ForEach-Object {
    $site = $_
    $physicalPath = 'Path Not Available'
    
    try {
        $rootApp = $site.Applications | Where-Object { $_.Path -eq '/' } | Select-Object -First 1
        if ($rootApp) {
            $rootVDir = $rootApp.VirtualDirectories | Where-Object { $_.Path -eq '/' } | Select-Object -First 1
            if ($rootVDir -and $rootVDir.PhysicalPath) {
                $physicalPath = $rootVDir.PhysicalPath
            }
        }
    } catch {
        # Keep default value
    }
    
    $siteInfo = [PSCustomObject]@{
        Name = $site.Name
        Id = [int]$site.Id
        State = [int]$site.State
        PhysicalPath = $physicalPath
    }
    $sites += $siteInfo
}

Write-Host "Sites encontrados: $($sites.Count)" -ForegroundColor Yellow

foreach ($site in $sites) {
    Write-Host "Site: $($site.Name), ID: $($site.Id), State: $($site.State), Path: $($site.PhysicalPath)" -ForegroundColor Cyan
}

Write-Host "`n=== JSON para deserialização ===" -ForegroundColor Green
$json = $sites | ConvertTo-Json -Depth 1 -Compress
Write-Host $json -ForegroundColor White

Write-Host "`n=== Testando deserialization manual ===" -ForegroundColor Green
$parsed = $json | ConvertFrom-Json
foreach ($p in $parsed) {
    Write-Host "Parsed - Name: '$($p.Name)', ID: $($p.Id), State: $($p.State), Path: '$($p.PhysicalPath)'" -ForegroundColor Magenta
}
