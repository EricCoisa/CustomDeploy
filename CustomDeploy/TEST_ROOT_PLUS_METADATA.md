# 📂 Script de Teste - Busca em Nível Raiz + Metadados

## Funcionalidade Implementada
O método `GetPublications` agora:
1. **Busca apenas no nível raiz** da pasta de publicações
2. **Adiciona aos metadados** os projetos encontrados na pasta raiz sem metadados
3. **Retorna todos os projetos dos metadados** (incluindo os de subpastas registrados nos metadados)

## Lógica Nova
- **Pasta Raiz**: Detecta e adiciona aos metadados se necessário
- **Subpastas**: Aparecem apenas se estiverem registradas nos metadados (via deploys)
- **ParentProject**: Calculado baseado na `targetPath` dos metadados

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

## 🚀 2. Criar Deploys de Teste

```powershell
Write-Host "🚀 Criando deploys de teste para validar a nova lógica..."

# Array de deploys com diferentes estruturas
$testDeploys = @(
    @{
        name = "app-raiz"
        targetPath = "app-raiz"
        type = "Nível Raiz"
        expectedParent = $null
        description = "App criado diretamente no nível raiz"
    },
    @{
        name = "ecommerce-api"
        targetPath = "ecommerce/api"
        type = "Subpasta via Deploy"
        expectedParent = "ecommerce"
        description = "API registrada via deploy em subpasta"
    },
    @{
        name = "ecommerce-frontend"
        targetPath = "ecommerce/frontend"
        type = "Subpasta via Deploy"
        expectedParent = "ecommerce"
        description = "Frontend registrado via deploy em subpasta"
    },
    @{
        name = "microservices-auth"
        targetPath = "microservices/services/auth"
        type = "Subpasta Profunda"
        expectedParent = "microservices"
        description = "Serviço em subpasta profunda via deploy"
    }
)

foreach ($deploy in $testDeploys) {
    Write-Host "📦 Criando ($($deploy.type)): $($deploy.name)"
    Write-Host "   TargetPath: $($deploy.targetPath)"
    Write-Host "   Description: $($deploy.description)"
    
    $deployBody = @{
        repoUrl = "https://github.com/microsoft/vscode-website.git"
        branch = "main"
        buildCommand = "echo 'Build $($deploy.type) - $($deploy.name)'"
        buildOutput = "."
        targetPath = $deploy.targetPath
    } | ConvertTo-Json

    try {
        $deployResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy" -Method POST -Body $deployBody -Headers $headers
        
        if ($deployResponse.success) {
            Write-Host "  ✅ Deploy criado com sucesso"
        } else {
            Write-Host "  ❌ Falha: $($deployResponse.message)"
        }
    } catch {
        Write-Host "  ⚠️ Erro: $($_.Exception.Message)"
    }
    
    Write-Host ""
    Start-Sleep -Seconds 2
}
```

## 📁 3. Simular Pasta Órfã no Nível Raiz

```powershell
Write-Host "📁 Simulando criação de pasta órfã no nível raiz..."

# Para simular uma pasta órfã, você pode criar manualmente uma pasta no diretório de publicações
# Exemplo: C:\temp\wwwroot\pasta-orfa
# A aplicação deve detectar essa pasta e criar metadados automaticamente

Write-Host "ℹ️ Para testar pasta órfã:"
Write-Host "   1. Crie manualmente uma pasta em: C:\temp\wwwroot\pasta-orfa"
Write-Host "   2. Execute GetPublications - deve criar metadados automaticamente"
Write-Host "   3. A pasta aparecerá na listagem com parentProject: null"
Write-Host ""
```

## 📋 4. Testar Nova Lógica de Publicações

```powershell
Write-Host "📋 Testando nova lógica de publicações..."

try {
    $publicationsResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications" -Method GET -Headers $headers
    
    Write-Host "✅ Publicações obtidas com sucesso: $($publicationsResponse.count) encontradas"
    Write-Host ""
    
    # Separar por origem
    $rootLevel = $publicationsResponse.publications | Where-Object { $_.parentProject -eq $null }
    $subfolderProjects = $publicationsResponse.publications | Where-Object { $_.parentProject -ne $null }
    
    Write-Host "🏠 PROJETOS NO NÍVEL RAIZ (parentProject: null):"
    Write-Host "=" * 60
    foreach ($pub in $rootLevel) {
        Write-Host "📦 $($pub.name)"
        Write-Host "   FullPath: $($pub.fullPath)"
        Write-Host "   Size: $($pub.sizeMB) MB"
        Write-Host "   Exists: $($pub.exists)"
        Write-Host "   Repository: $($pub.repository)"
        Write-Host "   Origem: $(if ($pub.repository) { 'Deploy' } else { 'Pasta Órfã Detectada' })"
        Write-Host ""
    }
    
    Write-Host "🏗️ PROJETOS EM SUBPASTAS (via metadados):"
    Write-Host "=" * 60
    
    if ($subfolderProjects.Count -gt 0) {
        # Agrupar por projeto pai
        $groupedByParent = $subfolderProjects | Group-Object -Property parentProject
        
        foreach ($group in $groupedByParent) {
            Write-Host "📁 Projeto Pai: $($group.Name)"
            foreach ($project in $group.Group) {
                Write-Host "  └── $($project.name)"
                Write-Host "      Path: $($project.fullPath)"
                Write-Host "      Size: $($project.sizeMB) MB | Exists: $($project.exists)"
                Write-Host "      Repository: $($project.repository)"
            }
            Write-Host ""
        }
    } else {
        Write-Host "❌ Nenhum projeto em subpasta encontrado"
    }
    
} catch {
    Write-Host "❌ Erro ao obter publicações: $($_.Exception.Message)"
}
```

## 🧪 5. Validar Comportamento Esperado

```powershell
Write-Host "🧪 Validando comportamento esperado..."

foreach ($deploy in $testDeploys) {
    Write-Host "🔍 Validando: $($deploy.name) ($($deploy.type))"
    
    $foundPublication = $publicationsResponse.publications | Where-Object { 
        $_.name -eq $deploy.name -or $_.name -like "*$($deploy.name)*"
    }
    
    if ($foundPublication) {
        $actualParent = $foundPublication.parentProject
        $expectedParent = $deploy.expectedParent
        
        if (($actualParent -eq $null -and $expectedParent -eq $null) -or 
            ($actualParent -eq $expectedParent)) {
            Write-Host "  ✅ PASSOU: ParentProject = '$actualParent'"
        } else {
            Write-Host "  ❌ FALHOU: ParentProject = '$actualParent' (esperado: '$expectedParent')"
        }
        
        Write-Host "     TargetPath: $($deploy.targetPath)"
        Write-Host "     FullPath: $($foundPublication.fullPath)"
        Write-Host "     Exists: $($foundPublication.exists)"
        Write-Host "     Type: $($deploy.type)"
        
        # Validar se subpastas foram registradas corretamente
        if ($deploy.type -ne "Nível Raiz") {
            if ($foundPublication.repository) {
                Write-Host "  ✅ Subpasta registrada via deploy (tem repository)"
            } else {
                Write-Host "  ❌ Subpasta sem metadados de deploy"
            }
        }
    } else {
        Write-Host "  ⚠️ Deploy não encontrado na listagem"
    }
    
    Write-Host ""
}
```

## 📊 6. Verificar Origem dos Dados

```powershell
Write-Host "📊 Verificando origem dos dados..."

try {
    $allPubs = $publicationsResponse.publications
    
    # Classificar por origem
    $fromDeploys = ($allPubs | Where-Object { $_.repository -ne $null }).Count
    $fromOrphanFolders = ($allPubs | Where-Object { $_.repository -eq $null -and $_.exists -eq $true }).Count
    $removedProjects = ($allPubs | Where-Object { $_.exists -eq $false }).Count
    
    Write-Host "📈 ORIGEM DOS DADOS:"
    Write-Host "  Projetos via Deploy: $fromDeploys"
    Write-Host "  Pastas Órfãs Detectadas: $fromOrphanFolders"
    Write-Host "  Projetos Removidos: $removedProjects"
    Write-Host "  Total: $($allPubs.Count)"
    Write-Host ""
    
    # Verificar distribuição de ParentProject
    $withParent = ($allPubs | Where-Object { $_.parentProject -ne $null }).Count
    $withoutParent = ($allPubs | Where-Object { $_.parentProject -eq $null }).Count
    
    Write-Host "📊 DISTRIBUIÇÃO HIERÁRQUICA:"
    Write-Host "  Nível Raiz (parentProject: null): $withoutParent"
    Write-Host "  Em Subpastas (com parentProject): $withParent"
    
    if ($withParent -gt 0) {
        $uniqueParents = $allPubs | Where-Object { $_.parentProject -ne $null } | 
                         Select-Object -ExpandProperty parentProject -Unique
        Write-Host "  Projetos Pai Únicos: $($uniqueParents.Count)"
        foreach ($parent in $uniqueParents) {
            $childCount = ($allPubs | Where-Object { $_.parentProject -eq $parent }).Count
            Write-Host "    📁 $parent: $childCount projeto(s)"
        }
    }
    
} catch {
    Write-Host "❌ Erro ao analisar origem dos dados: $($_.Exception.Message)"
}
```

## 🔍 7. Testar Casos Específicos

```powershell
Write-Host "🔍 Testando casos específicos..."

# Teste 1: Deploy individual de subpasta
Write-Host "🧪 Teste 1: Deploy individual de subpasta"
try {
    $subfolderDeploy = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/ecommerce-api" -Method GET -Headers $headers
    
    Write-Host "  ✅ Deploy de subpasta encontrado individualmente:"
    Write-Host "    Nome: $($subfolderDeploy.publication.name)"
    Write-Host "    ParentProject: $($subfolderDeploy.publication.parentProject)"
    Write-Host "    FullPath: $($subfolderDeploy.publication.fullPath)"
    Write-Host "    Repository: $($subfolderDeploy.publication.repository)"
    
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "  ⚠️ Deploy de subpasta não encontrado individualmente"
    } else {
        Write-Host "  ❌ Erro: $($_.Exception.Message)"
    }
}

# Teste 2: Verificar metadados diretos
Write-Host ""
Write-Host "🧪 Teste 2: Verificar metadados diretos"
try {
    $metadataResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/ecommerce-api/metadata" -Method GET -Headers $headers
    
    Write-Host "  ✅ Metadados acessíveis diretamente:"
    Write-Host "    Nome: $($metadataResponse.metadata.name)"
    Write-Host "    TargetPath: $($metadataResponse.metadata.targetPath)"
    Write-Host "    Repository: $($metadataResponse.metadata.repository)"
    Write-Host "    Exists: $($metadataResponse.metadata.exists)"
    
} catch {
    Write-Host "  ❌ Erro ao acessar metadados: $($_.Exception.Message)"
}
```

## 🧹 8. Limpeza (Opcional)

```powershell
Write-Host "🧹 Limpeza opcional dos deploys de teste..."

$cleanup = Read-Host "Deseja remover todos os deploys de teste? (s/N)"
if ($cleanup -eq 's' -or $cleanup -eq 'S') {
    foreach ($deploy in $testDeploys) {
        Write-Host "🗑️ Removendo: $($deploy.name) ($($deploy.type))"
        
        try {
            $deleteResult = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/$($deploy.name)" -Method DELETE -Headers $headers
            Write-Host "  ✅ Removido: $($deleteResult.message)"
        } catch {
            if ($_.Exception.Response.StatusCode -eq 404) {
                Write-Host "  ⚠️ Já removido ou não encontrado"
            } else {
                Write-Host "  ❌ Erro: $($_.Exception.Message)"
            }
        }
    }
} else {
    Write-Host "ℹ️ Deploys de teste mantidos para futuras validações"
}
```

## 📋 Resultados Esperados

### ✅ **Comportamento da Nova Lógica:**

1. **Pasta Raiz**:
   - Apps criados diretamente: `app-raiz` (parentProject: null)
   - Pastas órfãs detectadas: Metadados criados automaticamente

2. **Subpastas**:
   - Apenas aparecem se registradas via deploy nos metadados
   - `ecommerce-api` (parentProject: "ecommerce")
   - `microservices-auth` (parentProject: "microservices")

3. **Projetos Removidos**:
   - Aparecem como "(Removido)" se existem nos metadados mas não fisicamente

### 📊 **Estrutura Resultante:**

```json
{
  "publications": [
    {
      "name": "app-raiz",
      "parentProject": null,
      "exists": true,
      "repository": "https://github.com/..."
    },
    {
      "name": "ecommerce-api",
      "parentProject": "ecommerce", 
      "exists": true,
      "repository": "https://github.com/..."
    },
    {
      "name": "microservices-auth",
      "parentProject": "microservices",
      "exists": true,
      "repository": "https://github.com/..."
    }
  ]
}
```

### 🔍 **Benefícios da Nova Abordagem:**

1. **Performance**: Não recursivo, mais rápido
2. **Controle**: Subpastas só aparecem se registradas via deploy
3. **Flexibilidade**: Suporta estruturas complexas via metadados
4. **Detecção**: Pasta órfãs no raiz são detectadas automaticamente
5. **Consistência**: Fonte única de verdade nos metadados

---

**💡 Dica:** Use este script para validar que a nova lógica funciona corretamente, detectando projetos no nível raiz e listando todos os projetos registrados nos metadados, incluindo os em subpastas.
