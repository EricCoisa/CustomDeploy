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

$sites | ConvertTo-Json -Depth 1 -Compress
