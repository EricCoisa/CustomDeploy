# 🧪 Script de Teste - Campo `exists` e Novos Endpoints

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

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "✅ Token obtido com sucesso"
```

## 🚀 2. Criar Deploy de Teste

```powershell
# Deploy de teste para validar funcionalidades
$deployBody = @{
    repoUrl = "https://github.com/microsoft/TypeScript.git"
    branch = "main"
    buildCommand = "npm install && npm run build"
    buildOutput = "lib"
    targetPath = "typescript-test"
} | ConvertTo-Json

Write-Host "🚀 Executando deploy de teste..."
$deployResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy" -Method POST -Body $deployBody -Headers $headers

Write-Host "Deploy Status: $($deployResponse.success)"
Write-Host "Deploy Message: $($deployResponse.message)"

if (-not $deployResponse.success) {
    Write-Host "❌ Deploy falhou. Interrompendo testes."
    exit 1
}
```

## 📊 3. Verificar Arquivo `deploys.json` Inicial

```powershell
# Verificar se o arquivo foi criado com o campo exists
$deploysJsonPath = ".\bin\Debug\net8.0\deploys.json"

Write-Host "📄 Verificando arquivo deploys.json..."
if (Test-Path $deploysJsonPath) {
    $deploysContent = Get-Content $deploysJsonPath | ConvertFrom-Json
    Write-Host "Total de deploys: $($deploysContent.Count)"
    
    foreach ($deploy in $deploysContent) {
        Write-Host "---"
        Write-Host "Nome: $($deploy.name)"
        Write-Host "Target Path: $($deploy.targetPath)"
        Write-Host "Exists: $($deploy.exists)"
        Write-Host "Repository: $($deploy.repository)"
    }
} else {
    Write-Host "❌ Arquivo deploys.json não encontrado"
}
```

## 🔍 4. Testar GET /deploy/publications (Com Verificação Automática)

```powershell
Write-Host "📋 Testando listagem de publicações com verificação de existência..."
$publicationsResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications" -Method GET -Headers $headers

Write-Host "Total de publicações: $($publicationsResponse.count)"
Write-Host "Publicações encontradas:"

foreach ($pub in $publicationsResponse.publications) {
    $status = if ($pub.name -like "*Removido*") { "❌ REMOVIDO" } else { "✅ ATIVO" }
    Write-Host "$status - $($pub.name) (Tamanho: $($pub.sizeMB) MB)"
}
```

## 🗑️ 5. Simular Remoção Manual de Aplicação

```powershell
# Simular remoção manual (deletar pasta da aplicação)
$publicationsPath = "C:\temp\wwwroot"  # Ajustar conforme configuração
$testAppPath = Join-Path $publicationsPath "typescript-test"

Write-Host "🗑️ Simulando remoção manual da aplicação..."
if (Test-Path $testAppPath) {
    Write-Host "Removendo diretório: $testAppPath"
    Remove-Item $testAppPath -Recurse -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Write-Host "✅ Diretório removido"
} else {
    Write-Host "⚠️ Diretório não encontrado: $testAppPath"
}
```

## 🔄 6. Verificar Atualização Automática do Campo `exists`

```powershell
Write-Host "🔄 Testando atualização automática do campo exists..."
$publicationsAfterDelete = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications" -Method GET -Headers $headers

Write-Host "Publicações após remoção manual:"
foreach ($pub in $publicationsAfterDelete.publications) {
    $status = if ($pub.name -like "*Removido*") { "❌ REMOVIDO" } else { "✅ ATIVO" }
    Write-Host "$status - $($pub.name)"
}

# Verificar se o arquivo deploys.json foi atualizado
Write-Host "📄 Verificando atualização do deploys.json..."
$deploysContentAfter = Get-Content $deploysJsonPath | ConvertFrom-Json
$testDeploy = $deploysContentAfter | Where-Object { $_.name -eq "typescript-test" }

if ($testDeploy) {
    Write-Host "Campo exists atualizado para: $($testDeploy.exists)"
    if ($testDeploy.exists -eq $false) {
        Write-Host "✅ Campo exists atualizado corretamente para false"
    } else {
        Write-Host "❌ Campo exists deveria ser false"
    }
} else {
    Write-Host "❌ Deploy typescript-test não encontrado nos metadados"
}
```

## 🔍 7. Testar Novo Endpoint GET /deploy/publications/{name}/metadata

```powershell
Write-Host "🔍 Testando endpoint de metadados específicos..."
try {
    $metadataResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/typescript-test/metadata" -Method GET -Headers $headers
    
    Write-Host "✅ Metadados obtidos:"
    Write-Host "Nome: $($metadataResponse.metadata.name)"
    Write-Host "Exists: $($metadataResponse.metadata.exists)"
    Write-Host "Repository: $($metadataResponse.metadata.repository)"
    Write-Host "Target Path: $($metadataResponse.metadata.targetPath)"
    Write-Host "Deployed At: $($metadataResponse.metadata.deployedAt)"
    
    if ($metadataResponse.metadata.exists -eq $false) {
        Write-Host "✅ Status exists correto nos metadados específicos"
    } else {
        Write-Host "❌ Status exists incorreto nos metadados específicos"
    }
} catch {
    Write-Host "❌ Erro ao obter metadados: $($_.Exception.Message)"
}
```

## 🗑️ 8. Testar Endpoint DELETE /deploy/publications/{name}

```powershell
Write-Host "🗑️ Testando remoção de entrada dos metadados..."
try {
    $deleteResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/typescript-test" -Method DELETE -Headers $headers
    
    Write-Host "✅ Resposta da remoção:"
    Write-Host "Message: $($deleteResponse.message)"
    Write-Host "Name: $($deleteResponse.name)"
    
    # Verificar se foi removido do arquivo
    Start-Sleep -Seconds 1
    $deploysAfterDelete = Get-Content $deploysJsonPath | ConvertFrom-Json
    $deletedDeploy = $deploysAfterDelete | Where-Object { $_.name -eq "typescript-test" }
    
    if (-not $deletedDeploy) {
        Write-Host "✅ Deploy removido com sucesso dos metadados"
    } else {
        Write-Host "❌ Deploy ainda existe nos metadados"
    }
} catch {
    Write-Host "❌ Erro ao remover deploy: $($_.Exception.Message)"
}
```

## 📋 9. Verificar Estado Final

```powershell
Write-Host "📋 Verificando estado final..."

# Listar publicações finais
$finalPublications = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications" -Method GET -Headers $headers
Write-Host "Publicações finais: $($finalPublications.count)"

# Verificar arquivo final
$finalDeploys = Get-Content $deploysJsonPath | ConvertFrom-Json
Write-Host "Entradas no deploys.json: $($finalDeploys.Count)"

# Tentar obter metadados do deploy removido (deve dar 404)
Write-Host "🔍 Tentando obter metadados do deploy removido (esperado 404)..."
try {
    $removedMetadata = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/typescript-test/metadata" -Method GET -Headers $headers
    Write-Host "❌ Deploy ainda encontrado (não deveria)"
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "✅ Deploy não encontrado (comportamento esperado)"
    } else {
        Write-Host "❌ Erro inesperado: $($_.Exception.Message)"
    }
}
```

## 🔄 10. Teste de Re-deploy (Opcional)

```powershell
Write-Host "🔄 Testando re-deploy após remoção..."
$redeployBody = @{
    repoUrl = "https://github.com/microsoft/TypeScript.git"
    branch = "main"
    buildCommand = "npm install && npm run build"
    buildOutput = "lib"
    targetPath = "typescript-test-v2"
} | ConvertTo-Json

$redeployResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy" -Method POST -Body $redeployBody -Headers $headers

if ($redeployResponse.success) {
    Write-Host "✅ Re-deploy executado com sucesso"
    
    # Verificar se o novo deploy tem exists: true
    Start-Sleep -Seconds 2
    $newDeploys = Get-Content $deploysJsonPath | ConvertFrom-Json
    $newDeploy = $newDeploys | Where-Object { $_.name -eq "typescript-test-v2" }
    
    if ($newDeploy -and $newDeploy.exists -eq $true) {
        Write-Host "✅ Novo deploy criado com exists: true"
    } else {
        Write-Host "❌ Problema com novo deploy"
    }
} else {
    Write-Host "❌ Re-deploy falhou: $($redeployResponse.message)"
}
```

## 🧹 11. Limpeza Final

```powershell
Write-Host "🧹 Limpeza final..."

# Remover aplicações de teste
$testPaths = @("typescript-test", "typescript-test-v2")
foreach ($testPath in $testPaths) {
    $fullTestPath = Join-Path $publicationsPath $testPath
    if (Test-Path $fullTestPath) {
        Remove-Item $fullTestPath -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "Removido: $fullTestPath"
    }
}

# Listar estado final
$cleanupPublications = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications" -Method GET -Headers $headers
Write-Host "📊 Estado final: $($cleanupPublications.count) publicações"
```

## 📊 Resultados Esperados

### ✅ **Testes Bem-Sucedidos:**
1. **Deploy inicial:** Campo `exists: true` criado automaticamente
2. **Remoção manual:** Campo `exists` atualizado para `false` automaticamente
3. **Listagem atualizada:** Aplicação marcada como "(Removido)"
4. **Endpoint metadata:** Retorna status correto com `exists: false`
5. **Endpoint DELETE:** Remove entrada dos metadados com sucesso
6. **Re-deploy:** Novo deploy criado com `exists: true`

### 📈 **Métricas de Validação:**
- **Precisão:** 100% de detecção de aplicações removidas
- **Performance:** Verificação rápida em lote
- **Consistência:** Arquivo sempre sincronizado com disco
- **Usabilidade:** Endpoints intuitivos para gestão

---

**💡 Dica:** Execute este script completo ou execute seções individualmente para testar funcionalidades específicas.
