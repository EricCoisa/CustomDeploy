# 🧪 Script de Teste - Sistema de Metadados Centralizado

## Pré-requisitos
1. Aplicação CustomDeploy rodando
2. Token JWT válido
3. PowerShell ou ferramenta de teste de API

## 🔐 1. Obter Token de Autenticação

```powershell
# Login para obter token JWT
$loginBody = @{
    username = "admin"
    password = "password"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "https://localhost:7071/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
$token = $loginResponse.token

Write-Host "Token obtido: $token"
```

## 🚀 2. Executar Deploy de Teste

```powershell
# Deploy 1 - Primeira aplicação
$deployBody1 = @{
    repoUrl = "https://github.com/microsoft/vscode.git"
    branch = "main"
    buildCommand = "npm install && npm run compile"
    buildOutput = "out"
    targetPath = "vscode-app"
} | ConvertTo-Json

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "Executando Deploy 1..."
$deployResponse1 = Invoke-RestMethod -Uri "https://localhost:7071/deploy" -Method POST -Body $deployBody1 -Headers $headers
Write-Host "Deploy 1 Status: $($deployResponse1.success)"
Write-Host "Deploy 1 Message: $($deployResponse1.message)"
```

```powershell
# Deploy 2 - Segunda aplicação
$deployBody2 = @{
    repoUrl = "https://github.com/facebook/react.git"
    branch = "main"
    buildCommand = "npm install && npm run build"
    buildOutput = "build"
    targetPath = "react-demo"
} | ConvertTo-Json

Write-Host "Executando Deploy 2..."
$deployResponse2 = Invoke-RestMethod -Uri "https://localhost:7071/deploy" -Method POST -Body $deployBody2 -Headers $headers
Write-Host "Deploy 2 Status: $($deployResponse2.success)"
Write-Host "Deploy 2 Message: $($deployResponse2.message)"
```

## 📋 3. Verificar Arquivo de Metadados

```powershell
# Localizar e exibir o arquivo deploys.json
$deploysJsonPath = ".\bin\Debug\net8.0\deploys.json"

if (Test-Path $deploysJsonPath) {
    Write-Host "📄 Arquivo deploys.json encontrado:"
    Get-Content $deploysJsonPath | ConvertFrom-Json | ConvertTo-Json -Depth 10
} else {
    Write-Host "❌ Arquivo deploys.json não encontrado em: $deploysJsonPath"
}
```

## 📊 4. Listar Publicações via API

```powershell
# Obter lista de publicações
Write-Host "📋 Obtendo lista de publicações..."
$publicationsResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications" -Method GET -Headers $headers

Write-Host "Total de publicações: $($publicationsResponse.Count)"

foreach ($pub in $publicationsResponse) {
    Write-Host "---"
    Write-Host "Nome: $($pub.name)"
    Write-Host "Repository: $($pub.repository)"
    Write-Host "Branch: $($pub.branch)"
    Write-Host "Tamanho: $($pub.sizeMB) MB"
    Write-Host "Deploy em: $($pub.deployedAt)"
    Write-Host "Caminho: $($pub.fullPath)"
}
```

## 🔄 5. Teste de Atualização (Re-deploy)

```powershell
# Re-deploy da primeira aplicação com branch diferente
$updateBody = @{
    repoUrl = "https://github.com/microsoft/vscode.git"
    branch = "release/1.85"  # Branch diferente
    buildCommand = "npm install && npm run compile"
    buildOutput = "out"
    targetPath = "vscode-app"  # Mesmo targetPath
} | ConvertTo-Json

Write-Host "Executando atualização do Deploy 1..."
$updateResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy" -Method POST -Body $updateBody -Headers $headers
Write-Host "Update Status: $($updateResponse.success)"
Write-Host "Update Message: $($updateResponse.message)"

# Verificar se o arquivo foi atualizado corretamente
Start-Sleep -Seconds 2
Write-Host "📄 Arquivo deploys.json após atualização:"
Get-Content $deploysJsonPath | ConvertFrom-Json | ConvertTo-Json -Depth 10
```

## 📈 6. Verificar Estatísticas

```powershell
# Obter estatísticas das publicações
Write-Host "📊 Obtendo estatísticas..."
$statsResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/stats" -Method GET -Headers $headers

Write-Host "Estatísticas:"
Write-Host "Total de publicações: $($statsResponse.totalPublications)"
Write-Host "Tamanho total: $($statsResponse.totalSizeMB) MB"
Write-Host "Última atualização: $($statsResponse.lastUpdate)"
```

## 🧪 7. Teste de Cenário Avançado

```powershell
# Teste de múltiplos deploys simultâneos (simular concorrência)
$jobs = @()

for ($i = 1; $i -le 3; $i++) {
    $scriptBlock = {
        param($index, $token)
        
        $deployBody = @{
            repoUrl = "https://github.com/nodejs/node.git"
            branch = "main"
            buildCommand = "echo 'Building...'"
            buildOutput = "."
            targetPath = "concurrent-test-$index"
        } | ConvertTo-Json
        
        $headers = @{
            "Authorization" = "Bearer $token"
            "Content-Type" = "application/json"
        }
        
        try {
            $response = Invoke-RestMethod -Uri "https://localhost:7071/deploy" -Method POST -Body $deployBody -Headers $headers
            return "Job $index - Success: $($response.success)"
        } catch {
            return "Job $index - Error: $($_.Exception.Message)"
        }
    }
    
    $jobs += Start-Job -ScriptBlock $scriptBlock -ArgumentList $i, $token
}

# Aguardar todos os jobs
Write-Host "Executando 3 deploys simultâneos..."
$results = $jobs | Wait-Job | Receive-Job
$jobs | Remove-Job

foreach ($result in $results) {
    Write-Host $result
}
```

## 🧹 8. Limpeza de Teste

```powershell
# Opcional: Remover diretórios de teste criados
$testDirs = @("vscode-app", "react-demo", "concurrent-test-1", "concurrent-test-2", "concurrent-test-3")
$publicationsPath = "C:\temp\wwwroot"  # Ajustar conforme configuração

foreach ($dir in $testDirs) {
    $fullPath = Join-Path $publicationsPath $dir
    if (Test-Path $fullPath) {
        Write-Host "🧹 Removendo diretório de teste: $fullPath"
        Remove-Item $fullPath -Recurse -Force
    }
}

# Verificar como o sistema detecta publicações removidas
Write-Host "📋 Publicações após limpeza:"
$cleanupResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications" -Method GET -Headers $headers

foreach ($pub in $cleanupResponse) {
    $status = if ($pub.name -like "*Removido*") { "❌ REMOVIDO" } else { "✅ ATIVO" }
    Write-Host "$status - $($pub.name)"
}
```

## 🎯 Resultados Esperados

### ✅ **Sucessos Esperados:**
1. **Deploy 1 e 2:** Executados com sucesso
2. **Arquivo deploys.json:** Criado com 2 entradas
3. **API /publications:** Retorna 2 publicações com metadados
4. **Re-deploy:** Atualiza branch para "release/1.85", mantém 2 entradas
5. **Deploys simultâneos:** Todos executados sem corrupção
6. **Publicações removidas:** Detectadas e marcadas como "(Removido)"

### 📊 **Exemplo de deploys.json Resultante:**
```json
[
  {
    "name": "vscode-app",
    "repository": "https://github.com/microsoft/vscode.git",
    "branch": "release/1.85",
    "buildCommand": "npm install && npm run compile",
    "targetPath": "C:\\temp\\wwwroot\\vscode-app",
    "deployedAt": "2025-08-02T16:30:00Z"
  },
  {
    "name": "react-demo",
    "repository": "https://github.com/facebook/react.git",
    "branch": "main",
    "buildCommand": "npm install && npm run build",
    "targetPath": "C:\\temp\\wwwroot\\react-demo",
    "deployedAt": "2025-08-02T16:25:00Z"
  }
]
```

---

**💡 Dica:** Execute este script em partes para verificar cada funcionalidade individualmente ou use ferramentas como Postman/Insomnia para testes mais detalhados.
