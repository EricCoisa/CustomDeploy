# 🧪 Script de Teste - Exclusão Completa de Publicações

## Pré-requisitos
1. Aplicação CustomDeploy rodando
2. Token JWT válido
3. PowerShell
4. Permissões para criar/deletar diretórios

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
# Deploy de teste para validar exclusão completa
$deployBody = @{
    repoUrl = "https://github.com/microsoft/vscode-website.git"
    branch = "main"
    buildCommand = "echo 'Build simulado'"
    buildOutput = "."
    targetPath = "teste-exclusao-completa"
} | ConvertTo-Json

Write-Host "🚀 Criando deploy de teste para exclusão..."
try {
    $deployResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy" -Method POST -Body $deployBody -Headers $headers
    
    if ($deployResponse.success) {
        Write-Host "✅ Deploy criado com sucesso: $($deployResponse.message)"
    } else {
        Write-Host "❌ Deploy falhou: $($deployResponse.message)"
        exit 1
    }
} catch {
    Write-Host "❌ Erro no deploy: $($_.Exception.Message)"
    exit 1
}
```

## 📁 3. Verificar Criação Física do Projeto

```powershell
# Configuração (ajustar conforme sua configuração)
$publicationsPath = "C:\temp\wwwroot"  # Ajustar conforme DeploySettings:PublicationsPath
$testProjectPath = Join-Path $publicationsPath "teste-exclusao-completa"

Write-Host "📁 Verificando criação física do projeto..."

if (Test-Path $testProjectPath) {
    Write-Host "✅ Pasta física encontrada: $testProjectPath"
    
    # Verificar conteúdo da pasta
    $files = Get-ChildItem $testProjectPath -File | Select-Object -First 5
    Write-Host "📄 Arquivos na pasta ($($files.Count) de $($(Get-ChildItem $testProjectPath -File).Count)):"
    foreach ($file in $files) {
        Write-Host "  - $($file.Name) ($([math]::Round($file.Length / 1KB, 2)) KB)"
    }
    
    # Calcular tamanho total
    $totalSize = (Get-ChildItem $testProjectPath -Recurse -File | Measure-Object -Property Length -Sum).Sum
    $totalSizeMB = [math]::Round($totalSize / 1MB, 2)
    Write-Host "📊 Tamanho total da pasta: $totalSizeMB MB"
} else {
    Write-Host "❌ Pasta física não encontrada: $testProjectPath"
    Write-Host "⚠️ Deploy pode não ter sido concluído corretamente"
}
```

## 📋 4. Verificar Metadados Criados

```powershell
Write-Host "📋 Verificando metadados criados..."

# Verificar via API
try {
    $publicationResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/teste-exclusao-completa" -Method GET -Headers $headers
    
    Write-Host "✅ Metadados encontrados via API:"
    Write-Host "  Nome: $($publicationResponse.publication.name)"
    Write-Host "  Repository: $($publicationResponse.publication.repository)"
    Write-Host "  Exists: $($publicationResponse.publication.exists)"
    Write-Host "  Size MB: $($publicationResponse.publication.sizeMB)"
    Write-Host "  Target Path: $($publicationResponse.publication.fullPath)"
} catch {
    Write-Host "❌ Erro ao obter metadados via API: $($_.Exception.Message)"
}

# Verificar arquivo deploys.json
$deploysJsonPath = ".\bin\Debug\net8.0\deploys.json"
if (Test-Path $deploysJsonPath) {
    $deploysContent = Get-Content $deploysJsonPath | ConvertFrom-Json
    $testDeploy = $deploysContent | Where-Object { $_.name -eq "teste-exclusao-completa" }
    
    if ($testDeploy) {
        Write-Host "✅ Entrada encontrada no deploys.json:"
        Write-Host "  Target Path: $($testDeploy.targetPath)"
        Write-Host "  Deployed At: $($testDeploy.deployedAt)"
        Write-Host "  Exists: $($testDeploy.exists)"
    } else {
        Write-Host "❌ Entrada não encontrada no deploys.json"
    }
}
```

## 🗑️ 5. Testar Exclusão Completa

```powershell
Write-Host "🗑️ Testando exclusão completa (metadados + pasta física)..."

try {
    $deleteResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/teste-exclusao-completa" -Method DELETE -Headers $headers
    
    Write-Host "✅ Resposta da exclusão:"
    Write-Host "  Message: $($deleteResponse.message)"
    Write-Host "  Name: $($deleteResponse.name)"
    Write-Host "  Timestamp: $($deleteResponse.timestamp)"
    
    # Verificar se a mensagem indica exclusão completa
    if ($deleteResponse.message -like "*removido completamente*") {
        Write-Host "🎯 EXCLUSÃO COMPLETA confirmada na resposta"
    } elseif ($deleteResponse.message -like "*pasta física não existe*") {
        Write-Host "⚠️ Pasta física já havia sido removida"
    } else {
        Write-Host "⚠️ Resposta inesperada: $($deleteResponse.message)"
    }
} catch {
    Write-Host "❌ Erro na exclusão: $($_.Exception.Message)"
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "❌ Deploy não encontrado (404)"
    }
}
```

## 🔍 6. Verificar Remoção dos Metadados

```powershell
Write-Host "🔍 Verificando remoção dos metadados..."

# Verificar via API (deve retornar 404)
try {
    $checkResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/teste-exclusao-completa" -Method GET -Headers $headers
    Write-Host "❌ TESTE FALHOU: Deploy ainda encontrado via API"
    Write-Host "  Nome: $($checkResponse.publication.name)"
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "✅ TESTE PASSOU: Deploy não encontrado via API (404 esperado)"
    } else {
        Write-Host "⚠️ Erro inesperado: $($_.Exception.Message)"
    }
}

# Verificar arquivo deploys.json
if (Test-Path $deploysJsonPath) {
    $deploysContentAfter = Get-Content $deploysJsonPath | ConvertFrom-Json
    $testDeployAfter = $deploysContentAfter | Where-Object { $_.name -eq "teste-exclusao-completa" }
    
    if (-not $testDeployAfter) {
        Write-Host "✅ TESTE PASSOU: Entrada removida do deploys.json"
    } else {
        Write-Host "❌ TESTE FALHOU: Entrada ainda existe no deploys.json"
        Write-Host "  Exists: $($testDeployAfter.exists)"
    }
}
```

## 📁 7. Verificar Remoção da Pasta Física

```powershell
Write-Host "📁 Verificando remoção da pasta física..."

if (-not (Test-Path $testProjectPath)) {
    Write-Host "✅ TESTE PASSOU: Pasta física removida com sucesso"
} else {
    Write-Host "❌ TESTE FALHOU: Pasta física ainda existe"
    
    # Tentar verificar conteúdo
    try {
        $remainingFiles = Get-ChildItem $testProjectPath -ErrorAction Stop
        Write-Host "⚠️ Pasta contém $($remainingFiles.Count) itens"
    } catch {
        Write-Host "⚠️ Pasta existe mas não é acessível: $($_.Exception.Message)"
    }
}
```

## 📋 8. Verificar Lista Geral de Publicações

```powershell
Write-Host "📋 Verificando lista geral após exclusão..."

try {
    $allPublicationsResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications" -Method GET -Headers $headers
    
    $testStillExists = $allPublicationsResponse.publications | Where-Object { 
        $_.name -eq "teste-exclusao-completa" -or $_.name -like "*teste-exclusao-completa*" 
    }
    
    if (-not $testStillExists) {
        Write-Host "✅ TESTE PASSOU: Projeto não aparece na lista geral"
    } else {
        Write-Host "❌ TESTE FALHOU: Projeto ainda aparece na lista:"
        foreach ($project in $testStillExists) {
            Write-Host "  - $($project.name) | Exists: $($project.exists)"
        }
    }
    
    Write-Host "📊 Total de publicações restantes: $($allPublicationsResponse.count)"
} catch {
    Write-Host "❌ Erro ao obter lista de publicações: $($_.Exception.Message)"
}
```

## 🧪 9. Testar Endpoint de Exclusão Apenas de Metadados

```powershell
Write-Host "🧪 Testando endpoint de exclusão apenas de metadados..."

# Primeiro, criar outro deploy de teste
$metadataOnlyTestBody = @{
    repoUrl = "https://github.com/facebook/react.git"
    branch = "main"
    buildCommand = "echo 'Build para teste metadata-only'"
    buildOutput = "."
    targetPath = "teste-metadata-only"
} | ConvertTo-Json

Write-Host "🚀 Criando segundo deploy para teste metadata-only..."
try {
    $deployResponse2 = Invoke-RestMethod -Uri "https://localhost:7071/deploy" -Method POST -Body $metadataOnlyTestBody -Headers $headers
    
    if ($deployResponse2.success) {
        Write-Host "✅ Segundo deploy criado com sucesso"
        
        # Aguardar um pouco para garantir criação
        Start-Sleep -Seconds 3
        
        # Testar exclusão apenas de metadados
        Write-Host "🗑️ Testando exclusão apenas de metadados..."
        $metadataDeleteResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/teste-metadata-only/metadata-only" -Method DELETE -Headers $headers
        
        Write-Host "✅ Resposta da exclusão de metadados:"
        Write-Host "  Message: $($metadataDeleteResponse.message)"
        
        # Verificar se pasta física ainda existe
        $metadataTestPath = Join-Path $publicationsPath "teste-metadata-only"
        if (Test-Path $metadataTestPath) {
            Write-Host "✅ TESTE PASSOU: Pasta física mantida após exclusão de metadados"
        } else {
            Write-Host "❌ TESTE FALHOU: Pasta física foi removida (não deveria)"
        }
        
        # Limpar pasta manualmente
        if (Test-Path $metadataTestPath) {
            Remove-Item $metadataTestPath -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "🧹 Pasta de teste removida manualmente"
        }
    }
} catch {
    Write-Host "⚠️ Erro no teste metadata-only: $($_.Exception.Message)"
}
```

## 🔄 10. Teste de Exclusão de Deploy Inexistente

```powershell
Write-Host "🔄 Testando exclusão de deploy inexistente..."

try {
    $nonExistentDeleteResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/deploy-que-nao-existe" -Method DELETE -Headers $headers
    Write-Host "❌ TESTE FALHOU: Retornou resposta para deploy inexistente"
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "✅ TESTE PASSOU: 404 retornado corretamente para deploy inexistente"
    } else {
        Write-Host "⚠️ Erro inesperado: $($_.Exception.Message)"
    }
}
```

## 📊 Resultados Esperados

### ✅ **Testes Bem-Sucedidos:**

1. **Deploy de Teste:** Criado com sucesso com pasta física e metadados
2. **Exclusão Completa:** Remove tanto metadados quanto pasta física
3. **Verificação API:** Deploy não encontrado após exclusão (404)
4. **Verificação Arquivo:** Entrada removida do `deploys.json`
5. **Verificação Física:** Pasta completamente removida do disco
6. **Lista Geral:** Deploy não aparece mais na listagem
7. **Metadata-Only:** Remove apenas metadados mantendo pasta física
8. **Deploy Inexistente:** 404 retornado corretamente

### 📈 **Indicadores de Funcionamento:**

```
✅ Deploy criado com sucesso
✅ Pasta física encontrada: C:\temp\wwwroot\teste-exclusao-completa
✅ Metadados encontrados via API
✅ EXCLUSÃO COMPLETA confirmada na resposta
✅ TESTE PASSOU: Deploy não encontrado via API (404 esperado)
✅ TESTE PASSOU: Entrada removida do deploys.json
✅ TESTE PASSOU: Pasta física removida com sucesso
✅ TESTE PASSOU: Projeto não aparece na lista geral
```

### 🔍 **Logs Esperados no Servidor:**

```
[INFO] Solicitação para remover publicação completamente: teste-exclusao-completa
[INFO] Deletando pasta física: C:\temp\wwwroot\teste-exclusao-completa
[INFO] Pasta física deletada: C:\temp\wwwroot\teste-exclusao-completa
[INFO] Deploy removido dos metadados: teste-exclusao-completa
```

---

**💡 Dica:** Execute este script completo para validar todas as funcionalidades de exclusão completa, ou execute seções específicas para testar cenários individuais.
