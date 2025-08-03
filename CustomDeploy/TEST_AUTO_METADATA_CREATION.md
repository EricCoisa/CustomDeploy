# 🧪 Script de Teste - Criação Automática de Metadados

## Pré-requisitos
1. Aplicação CustomDeploy rodando
2. Token JWT válido
3. PowerShell
4. Permissões para criar diretórios

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

## 📁 2. Criar Diretórios de Teste Manualmente

```powershell
# Configuração (ajustar conforme sua configuração)
$publicationsPath = "C:\temp\wwwroot"  # Ajustar conforme DeploySettings:PublicationsPath

# Criar diretórios de teste que simularão projetos existentes sem metadados
$testDirectories = @(
    "projeto-manual-1",
    "aplicacao-legada",
    "site-estatico-teste"
)

Write-Host "📁 Criando diretórios de teste..."

foreach ($dirName in $testDirectories) {
    $fullPath = Join-Path $publicationsPath $dirName
    
    if (-not (Test-Path $fullPath)) {
        New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
        
        # Criar alguns arquivos de exemplo para simular aplicação real
        $indexContent = @"
<!DOCTYPE html>
<html>
<head>
    <title>$dirName</title>
</head>
<body>
    <h1>Aplicação de Teste: $dirName</h1>
    <p>Criada em: $(Get-Date)</p>
    <p>Este diretório foi criado manualmente para testar a criação automática de metadados.</p>
</body>
</html>
"@
        
        $indexPath = Join-Path $fullPath "index.html"
        Set-Content -Path $indexPath -Value $indexContent -Encoding UTF8
        
        # Criar um arquivo adicional
        $readmePath = Join-Path $fullPath "README.txt"
        Set-Content -Path $readmePath -Value "Projeto de teste criado em $(Get-Date)" -Encoding UTF8
        
        Write-Host "✅ Criado: $fullPath"
    } else {
        Write-Host "⚠️ Já existe: $fullPath"
    }
}
```

## 📄 3. Verificar Estado Inicial dos Metadados

```powershell
# Verificar arquivo deploys.json antes dos testes
$deploysJsonPath = ".\bin\Debug\net8.0\deploys.json"

Write-Host "📄 Estado inicial do deploys.json:"
if (Test-Path $deploysJsonPath) {
    $initialDeploys = Get-Content $deploysJsonPath | ConvertFrom-Json
    Write-Host "Total de entradas: $($initialDeploys.Count)"
    
    foreach ($deploy in $initialDeploys) {
        Write-Host "- $($deploy.name) | Repository: $($deploy.repository)"
    }
} else {
    Write-Host "❌ Arquivo deploys.json não encontrado"
}
```

## 🔍 4. Testar GET /deploy/publications (Criação Automática)

```powershell
Write-Host "🔍 Testando GET /deploy/publications com criação automática de metadados..."

$publicationsResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications" -Method GET -Headers $headers

Write-Host "📊 Resposta da API:"
Write-Host "Total de publicações: $($publicationsResponse.count)"

# Verificar se os diretórios de teste foram descobertos e registrados
$discoveredProjects = @()

foreach ($pub in $publicationsResponse.publications) {
    Write-Host "---"
    Write-Host "Nome: $($pub.name)"
    Write-Host "Repository: $($pub.repository)"
    Write-Host "Exists: $($pub.exists)"
    Write-Host "Target Path: $($pub.fullPath)"
    
    # Verificar se é um projeto criado automaticamente
    if ($pub.repository -like "*Criado automaticamente*") {
        $discoveredProjects += $pub.name
        Write-Host "🔍 DESCOBERTO AUTOMATICAMENTE: $($pub.name)"
    }
}

Write-Host "📋 Projetos descobertos automaticamente: $($discoveredProjects.Count)"
foreach ($project in $discoveredProjects) {
    Write-Host "- $project"
}
```

## 📄 5. Verificar Arquivo deploys.json Após Descoberta

```powershell
Write-Host "📄 Verificando deploys.json após descoberta automática..."

if (Test-Path $deploysJsonPath) {
    $finalDeploys = Get-Content $deploysJsonPath | ConvertFrom-Json
    Write-Host "Total de entradas após descoberta: $($finalDeploys.Count)"
    
    # Procurar entradas criadas automaticamente
    $autoCreatedEntries = $finalDeploys | Where-Object { $_.repository -like "*Criado automaticamente*" }
    
    Write-Host "📋 Entradas criadas automaticamente: $($autoCreatedEntries.Count)"
    foreach ($entry in $autoCreatedEntries) {
        Write-Host "---"
        Write-Host "Nome: $($entry.name)"
        Write-Host "Repository: $($entry.repository)"
        Write-Host "Target Path: $($entry.targetPath)"
        Write-Host "Deployed At: $($entry.deployedAt)"
        Write-Host "Exists: $($entry.exists)"
    }
} else {
    Write-Host "❌ Arquivo deploys.json não encontrado após teste"
}
```

## 🔍 6. Testar GET /deploy/publications/{name} Individual

```powershell
Write-Host "🔍 Testando GET /deploy/publications/{name} para projeto específico..."

if ($testDirectories.Count -gt 0) {
    $testProjectName = $testDirectories[0]
    
    try {
        $projectResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/$testProjectName" -Method GET -Headers $headers
        
        Write-Host "✅ Projeto encontrado individualmente:"
        Write-Host "Nome: $($projectResponse.publication.name)"
        Write-Host "Repository: $($projectResponse.publication.repository)"
        Write-Host "Exists: $($projectResponse.publication.exists)"
        Write-Host "Size MB: $($projectResponse.publication.sizeMB)"
        
        if ($projectResponse.publication.repository -like "*Criado automaticamente*") {
            Write-Host "🔍 CONFIRMADO: Metadados criados automaticamente na consulta individual"
        }
    } catch {
        Write-Host "❌ Erro ao obter projeto individual: $($_.Exception.Message)"
    }
}
```

## 📊 7. Testar GET /deploy/publications/stats (Herança da Funcionalidade)

```powershell
Write-Host "📊 Testando estatísticas com projetos descobertos automaticamente..."

try {
    $statsResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/stats" -Method GET -Headers $headers
    
    Write-Host "📈 Estatísticas atualizadas:"
    Write-Host "Total de publicações: $($statsResponse.stats.totalPublications)"
    Write-Host "Publicações existentes: $($statsResponse.stats.existingPublications)"
    Write-Host "Publicações removidas: $($statsResponse.stats.removedPublications)"
    Write-Host "Tamanho total: $($statsResponse.stats.totalSizeMB) MB"
    
    if ($statsResponse.stats.totalPublications -gt ($initialDeploys.Count)) {
        $discovered = $statsResponse.stats.totalPublications - ($initialDeploys.Count)
        Write-Host "🔍 CONFIRMADO: $discovered projeto(s) descoberto(s) automaticamente incluído(s) nas estatísticas"
    }
} catch {
    Write-Host "❌ Erro ao obter estatísticas: $($_.Exception.Message)"
}
```

## 🔄 8. Teste de Não-Duplicação

```powershell
Write-Host "🔄 Testando prevenção de duplicação de metadados..."

# Chamar novamente GET /deploy/publications para verificar se não cria duplicatas
$secondResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications" -Method GET -Headers $headers

# Contar entradas para os mesmos projetos
$duplicateCheck = @{}
foreach ($pub in $secondResponse.publications) {
    if ($duplicateCheck.ContainsKey($pub.name)) {
        Write-Host "❌ DUPLICATA DETECTADA: $($pub.name)"
        $duplicateCheck[$pub.name]++
    } else {
        $duplicateCheck[$pub.name] = 1
    }
}

$duplicates = $duplicateCheck.GetEnumerator() | Where-Object { $_.Value -gt 1 }
if ($duplicates.Count -eq 0) {
    Write-Host "✅ TESTE PASSOU: Nenhuma duplicata criada na segunda chamada"
} else {
    Write-Host "❌ TESTE FALHOU: Duplicatas encontradas:"
    foreach ($dup in $duplicates) {
        Write-Host "- $($dup.Key): $($dup.Value) ocorrências"
    }
}
```

## 📁 9. Testar com Diretório Inexistente

```powershell
Write-Host "📁 Testando comportamento com diretório inexistente..."

try {
    $nonExistentResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/diretorio-inexistente" -Method GET -Headers $headers
    Write-Host "❌ TESTE FALHOU: Retornou resposta para diretório inexistente"
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "✅ TESTE PASSOU: 404 retornado corretamente para diretório inexistente"
    } else {
        Write-Host "⚠️ Erro inesperado: $($_.Exception.Message)"
    }
}
```

## 🧹 10. Limpeza de Teste

```powershell
Write-Host "🧹 Limpeza dos diretórios de teste..."

foreach ($dirName in $testDirectories) {
    $fullPath = Join-Path $publicationsPath $dirName
    
    if (Test-Path $fullPath) {
        Remove-Item $fullPath -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "🗑️ Removido: $fullPath"
    }
}

# Opcionalmente, chamar GET novamente para ver o status "exists: false"
Write-Host "🔍 Verificando detecção de remoção..."
$cleanupResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications" -Method GET -Headers $headers

$removedProjects = $cleanupResponse.publications | Where-Object { 
    $_.name -in $testDirectories -or $_.name -like "*$($testDirectories[0])*" 
}

foreach ($removed in $removedProjects) {
    if ($removed.exists -eq $false) {
        Write-Host "✅ DETECÇÃO DE REMOÇÃO: $($removed.name) marcado como exists: false"
    } else {
        Write-Host "⚠️ Projeto ainda marcado como existente: $($removed.name)"
    }
}
```

## 📊 Resultados Esperados

### ✅ **Testes Bem-Sucedidos:**

1. **Criação Automática:** Diretórios manuais automaticamente registrados
2. **Metadados Corretos:** Campos populados com valores padrão apropriados
3. **Prevenção de Duplicatas:** Segunda chamada não cria entradas duplicadas
4. **Consulta Individual:** GET /{name} também cria metadados se necessário
5. **Estatísticas Atualizadas:** Projetos descobertos incluídos nas estatísticas
6. **Detecção de Remoção:** Sistema detecta quando projetos são removidos

### 📈 **Validações Importantes:**

- **Repository:** "N/A (Criado automaticamente)" para projetos descobertos
- **Exists:** `true` para diretórios físicos descobertos
- **DeployedAt:** Data de criação do diretório
- **Logs:** Mensagens informativas sobre criação automática
- **Performance:** Operações rápidas sem impacto significativo

### 🔍 **Indicadores de Funcionamento:**

```
[INFO] Diretório encontrado sem metadados, criando automaticamente: C:\temp\wwwroot\projeto-manual-1
[INFO] Metadados criados automaticamente para diretório existente: C:\temp\wwwroot\projeto-manual-1
[INFO] Metadados criados automaticamente: Metadados criados automaticamente para 'projeto-manual-1'
```

---

**💡 Dica:** Execute este script completo ou execute seções individualmente para testar funcionalidades específicas da criação automática de metadados.
