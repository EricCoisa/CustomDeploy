$response1 = Invoke-RestMethod -Uri 'http://localhost:5092/api/iis/sites/carteira/applications/api' -Method Get -Headers @{'Authorization'='bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJzdWIiOiJhZG1pbiIsImp0aSI6IjdjNzM3MGJlLWQ2NDItNGY1OS05YzhkLTk5MGNiY2MxNmFjYiIsImlhdCI6MTc1NDE4NjgyMiwiZXhwIjoxNzU0MTkwNDIyLCJpc3MiOiJDdXN0b21EZXBsb3kiLCJhdWQiOiJDdXN0b21EZXBsb3kifQ.Lf4wSlD6hIohUU_FeZBUjytXRXgbgy-SE-kDkzKgB9c'}
Write-Host "CheckApplication Response (api - should not exist):"
$response1 | ConvertTo-Json -Depth 5

Write-Host "`n---`n"

$response2 = Invoke-RestMethod -Uri 'http://localhost:5092/api/iis/sites/Gruppy/applications/carteira' -Method Get -Headers @{'Authorization'='bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJzdWIiOiJhZG1pbiIsImp0aSI6IjdjNzM3MGJlLWQ2NDItNGY1OS05YzhkLTk5MGNiY2MxNmFjYiIsImlhdCI6MTc1NDE4NjgyMiwiZXhwIjoxNzU0MTkwNDIyLCJpc3MiOiJDdXN0b21EZXBsb3kiLCJhdWQiOiJDdXN0b21EZXBsb3kifQ.Lf4wSlD6hIohUU_FeZBUjytXRXgbgy-SE-kDkzKgB9c'}
Write-Host "CheckApplication Response (carteira - should exist):"
$response2 | ConvertTo-Json -Depth 5
