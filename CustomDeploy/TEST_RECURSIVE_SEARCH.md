# 🔍 Script de Teste - Busca Recursiva de Publicações

## Funcionalidade Implementada
O método `GetPublications` agora busca recursivamente em todos os subdiretórios e inclui todos os projetos dos metadados, permitindo projetos aninhados dentro de outros projetos.

## Melhorias Implementadas
1. **Busca Recursiva**: Encontra projetos em qualquer nível de profundidade
2. **Metadados Incluídos**: Lista todos os projetos dos metadados, mesmo sem pasta física
3. **Detecção Automática**: Cria metadados para pastas encontradas sem registro
4. **Evita Duplicação**: Controle de caminhos já processados

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

## 🚀 2. Criar Estrutura de Teste Hierárquica

```powershell
Write-Host "🚀 Criando estrutura hierárquica de deploys de teste..."

# Array de deploys com diferentes níveis de aninhamento
$hierarchicalDeploys = @(
    @{
        name = "root-app"
        targetPath = "root-app"
        level = "Nível Raiz"
        description = "App no nível raiz"
    },
    @{
        name = "ecommerce-api"
        targetPath = "ecommerce/api"
        level = "Nível 1"
        description = "API dentro do projeto ecommerce"
    },
    @{
        name = "ecommerce-frontend"
        targetPath = "ecommerce/frontend"
        level = "Nível 1"
        description = "Frontend dentro do projeto ecommerce"
    },
    @{
        name = "ecommerce-admin-panel"
        targetPath = "ecommerce/admin/panel"
        level = "Nível 2"
        description = "Painel admin aninhado em 2 níveis"
    },
    @{
        name = "ecommerce-admin-reports"
        targetPath = "ecommerce/admin/reports"
        level = "Nível 2"
        description = "Relatórios aninhados em 2 níveis"
    },
    @{
        name = "microservices-auth"
        targetPath = "microservices/auth"
        level = "Nível 1"
        description = "Serviço de autenticação"
    },
    @{
        name = "microservices-payment-api"
        targetPath = "microservices/payment/api"
        level = "Nível 2"
        description = "API de pagamento aninhada"
    },
    @{
        name = "microservices-payment-webhook"
        targetPath = "microservices/payment/webhook"
        level = "Nível 2"
        description = "Webhook de pagamento aninhado"
    },
    @{
        name = "deep-nested-service"
        targetPath = "complex/project/services/api/v1"
        level = "Nível 4"
        description = "Serviço com aninhamento profundo"
    }
)

foreach ($deploy in $hierarchicalDeploys) {
    Write-Host "📦 Criando ($($deploy.level)): $($deploy.name)"
    Write-Host "   TargetPath: $($deploy.targetPath)"
    Write-Host "   Description: $($deploy.description)"
    
    $deployBody = @{
        repoUrl = "https://github.com/microsoft/vscode-website.git"
        branch = "main"
        buildCommand = "echo 'Build $($deploy.level) - $($deploy.name)'"
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
    Start-Sleep -Seconds 1
}
```

## 📋 3. Testar Busca Recursiva

```powershell
Write-Host "📋 Testando busca recursiva de publicações..."

try {
    $publicationsResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications" -Method GET -Headers $headers
    
    Write-Host "✅ Publicações obtidas com sucesso: $($publicationsResponse.count) encontradas"
    Write-Host ""
    
    # Separar por níveis de aninhamento
    $rootLevel = $publicationsResponse.publications | Where-Object { $_.parentProject -eq $null }
    $level1 = $publicationsResponse.publications | Where-Object { $_.parentProject -ne $null -and -not $_.fullPath.Contains('\') -or ($_.fullPath.Split('\').Count -eq 2) }
    $level2Plus = $publicationsResponse.publications | Where-Object { $_.parentProject -ne $null -and ($_.fullPath.Split('\').Count -gt 2) }
    
    Write-Host "🏠 NÍVEL RAIZ (sem projeto pai):"
    Write-Host "=" * 50
    foreach ($pub in $rootLevel) {
        Write-Host "📦 $($pub.name)"
        Write-Host "   Path: $($pub.fullPath)"
        Write-Host "   ParentProject: null"
        Write-Host "   Size: $($pub.sizeMB) MB"
        Write-Host "   Exists: $($pub.exists)"
        Write-Host ""
    }
    
    Write-Host "🏗️ PROJETOS COM HIERARQUIA:"
    Write-Host "=" * 50
    
    # Agrupar por projeto pai
    $groupedByParent = $publicationsResponse.publications | 
        Where-Object { $_.parentProject -ne $null } | 
        Group-Object -Property parentProject
    
    foreach ($group in $groupedByParent) {
        Write-Host "📁 Projeto Pai: $($group.Name)"
        
        # Subgrupar por profundidade
        $sortedProjects = $group.Group | Sort-Object { ($_.fullPath -split '[/\\]').Count }
        
        foreach ($project in $sortedProjects) {
            $depth = ($project.fullPath -split '[/\\]').Count
            $indent = "  " + ("  " * ($depth - 2))
            $levelIndicator = "└" + ("─" * ($depth - 1))
            
            Write-Host "$indent$levelIndicator $($project.name)"
            Write-Host "$indent   Path: $($project.fullPath)"
            Write-Host "$indent   Size: $($project.sizeMB) MB | Exists: $($project.exists)"
            Write-Host "$indent   Repository: $($project.repository)"
        }
        Write-Host ""
    }
    
} catch {
    Write-Host "❌ Erro ao obter publicações: $($_.Exception.Message)"
}
```

## 🧪 4. Validar Detecção de Níveis

```powershell
Write-Host "🧪 Validando detecção de níveis hierárquicos..."

foreach ($expectedDeploy in $hierarchicalDeploys) {
    Write-Host "🔍 Validando: $($expectedDeploy.name) ($($expectedDeploy.level))"
    
    $foundPublication = $publicationsResponse.publications | Where-Object { 
        $_.name -eq $expectedDeploy.name -or $_.name -like "*$($expectedDeploy.name)*"
    }
    
    if ($foundPublication) {
        $pathParts = $expectedDeploy.targetPath -split '[/\\]'
        $expectedParent = if ($pathParts.Count -gt 1) { $pathParts[0] } else { $null }
        $actualParent = $foundPublication.parentProject
        
        if (($expectedParent -eq $null -and $actualParent -eq $null) -or 
            ($expectedParent -eq $actualParent)) {
            Write-Host "  ✅ PASSOU: ParentProject = '$actualParent'"
        } else {
            Write-Host "  ❌ FALHOU: ParentProject = '$actualParent' (esperado: '$expectedParent')"
        }
        
        Write-Host "     TargetPath: $($expectedDeploy.targetPath)"
        Write-Host "     FullPath: $($foundPublication.fullPath)"
        Write-Host "     Exists: $($foundPublication.exists)"
        Write-Host "     Level: $($expectedDeploy.level)"
    } else {
        Write-Host "  ⚠️ Deploy não encontrado na listagem"
    }
    
    Write-Host ""
}
```

## 📊 5. Estatísticas de Hierarquia

```powershell
Write-Host "📊 Estatísticas de hierarquia..."

try {
    $allPubs = $publicationsResponse.publications
    
    # Calcular profundidades
    $depthStats = @{}
    foreach ($pub in $allPubs) {
        if ($pub.fullPath) {
            $depth = ($pub.fullPath -split '[/\\]').Count - 1  # -1 para não contar o root
            if ($depthStats.ContainsKey($depth)) {
                $depthStats[$depth]++
            } else {
                $depthStats[$depth] = 1
            }
        }
    }
    
    Write-Host "📈 DISTRIBUIÇÃO POR PROFUNDIDADE:"
    foreach ($depth in ($depthStats.Keys | Sort-Object)) {
        $count = $depthStats[$depth]
        $levelName = switch ($depth) {
            0 { "Nível Raiz" }
            1 { "Nível 1" }
            2 { "Nível 2" }
            3 { "Nível 3" }
            4 { "Nível 4+" }
            default { "Nível $depth" }
        }
        Write-Host "  $levelName : $count deploy(s)"
    }
    
    # Estatísticas gerais
    $totalDeploys = $allPubs.Count
    $existingDeploys = ($allPubs | Where-Object { $_.exists }).Count
    $projectsWithChildren = ($allPubs | Where-Object { $_.parentProject -ne $null } | Select-Object -ExpandProperty parentProject -Unique).Count
    
    Write-Host ""
    Write-Host "📊 ESTATÍSTICAS GERAIS:"
    Write-Host "  Total de Deploys: $totalDeploys"
    Write-Host "  Deploys Existentes: $existingDeploys"
    Write-Host "  Deploys Removidos: $($totalDeploys - $existingDeploys)"
    Write-Host "  Projetos com Filhos: $projectsWithChildren"
    Write-Host "  Máxima Profundidade: $($depthStats.Keys | Measure-Object -Maximum | Select-Object -ExpandProperty Maximum)"
    
} catch {
    Write-Host "❌ Erro ao calcular estatísticas: $($_.Exception.Message)"
}
```

## 🔍 6. Testar Casos Especiais

```powershell
Write-Host "🔍 Testando casos especiais..."

# Teste 1: Deploy individual aninhado
Write-Host "🧪 Teste 1: Deploy individual aninhado"
try {
    $nestedDeploy = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/ecommerce-admin-panel" -Method GET -Headers $headers
    
    Write-Host "  ✅ Deploy aninhado encontrado:"
    Write-Host "    Nome: $($nestedDeploy.publication.name)"
    Write-Host "    ParentProject: $($nestedDeploy.publication.parentProject)"
    Write-Host "    FullPath: $($nestedDeploy.publication.fullPath)"
    
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "  ⚠️ Deploy aninhado não encontrado"
    } else {
        Write-Host "  ❌ Erro: $($_.Exception.Message)"
    }
}

# Teste 2: Estatísticas incluindo aninhados
Write-Host ""
Write-Host "🧪 Teste 2: Estatísticas incluindo deploys aninhados"
try {
    $statsResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/stats" -Method GET -Headers $headers
    
    Write-Host "  ✅ Estatísticas obtidas:"
    Write-Host "    Total: $($statsResponse.stats.totalPublications)"
    Write-Host "    Existentes: $($statsResponse.stats.existingPublications)"
    Write-Host "    Removidos: $($statsResponse.stats.removedPublications)"
    Write-Host "    Tamanho Total: $($statsResponse.stats.totalSizeMB) MB"
    
} catch {
    Write-Host "  ❌ Erro nas estatísticas: $($_.Exception.Message)"
}
```

## 🧹 7. Limpeza (Opcional)

```powershell
Write-Host "🧹 Limpeza opcional dos deploys hierárquicos..."

$cleanup = Read-Host "Deseja remover todos os deploys de teste hierárquicos? (s/N)"
if ($cleanup -eq 's' -or $cleanup -eq 'S') {
    
    # Remover em ordem reversa (mais profundos primeiro)
    $deploysToRemove = $hierarchicalDeploys | Sort-Object { ($_.targetPath -split '[/\\]').Count } -Descending
    
    foreach ($deploy in $deploysToRemove) {
        Write-Host "🗑️ Removendo: $($deploy.name) ($($deploy.level))"
        
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
    Write-Host "ℹ️ Deploys hierárquicos mantidos para futuras validações"
}
```

## 📋 Resultados Esperados

### ✅ **Estrutura Hierárquica Detectada:**

```
🏠 NÍVEL RAIZ:
📦 root-app (parentProject: null)

🏗️ PROJETOS COM HIERARQUIA:
📁 Projeto Pai: ecommerce
  └─ ecommerce-api
  └─ ecommerce-frontend  
    └── ecommerce-admin-panel
    └── ecommerce-admin-reports

📁 Projeto Pai: microservices
  └─ microservices-auth
    └── microservices-payment-api
    └── microservices-payment-webhook

📁 Projeto Pai: complex
    └──── deep-nested-service
```

### 📊 **Distribuição por Profundidade:**

```
📈 DISTRIBUIÇÃO POR PROFUNDIDADE:
  Nível Raiz : 1 deploy(s)
  Nível 1 : 3 deploy(s)  
  Nível 2 : 4 deploy(s)
  Nível 4+ : 1 deploy(s)
```

### 🔍 **Benefícios da Busca Recursiva:**

1. **Detecção Completa**: Encontra todos os projetos independente da profundidade
2. **Organização Hierárquica**: Mantém relação pai-filho claramente definida
3. **Flexibilidade**: Suporta estruturas complexas de microserviços
4. **Consistência**: Inclui tanto projetos físicos quanto apenas metadata
5. **Performance**: Evita duplicação com controle de caminhos processados

---

**💡 Dica:** Use este script para validar completamente a funcionalidade de busca recursiva, testando desde estruturas simples até hierarquias complexas com múltiplos níveis de aninhamento.
