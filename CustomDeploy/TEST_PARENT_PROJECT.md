# 🏗️ Script de Teste - Campo ParentProject

## Funcionalidade Implementada
Campo `parentProject` que detecta automaticamente se um deploy faz parte de um projeto pai maior, baseado na estrutura da `targetPath`.

## Lógica Implementada
- **Se `targetPath` contém `/`**: ParentProject = primeira parte do caminho
  - Ex: `app2/api` → ParentProject = `"app2"`
  - Ex: `microservices/auth/api` → ParentProject = `"microservices"`
- **Se `targetPath` não contém `/`**: ParentProject = `null`
  - Ex: `app1` → ParentProject = `null`

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
Write-Host "🚀 Criando deploys de teste para validar parentProject..."

# Array de deploys de teste com diferentes estruturas
$testDeploys = @(
    @{
        name = "app1-standalone"
        targetPath = "app1"
        expectedParent = $null
        description = "Deploy no nível raiz (sem parent)"
    },
    @{
        name = "app2-api"
        targetPath = "app2/api"
        expectedParent = "app2"
        description = "API dentro do projeto app2"
    },
    @{
        name = "app2-frontend"
        targetPath = "app2/frontend"
        expectedParent = "app2"
        description = "Frontend dentro do projeto app2"
    },
    @{
        name = "microservices-auth"
        targetPath = "microservices/auth"
        expectedParent = "microservices"
        description = "Serviço de auth dentro de microservices"
    },
    @{
        name = "microservices-payment"
        targetPath = "microservices/payment"
        expectedParent = "microservices"
        description = "Serviço de payment dentro de microservices"
    },
    @{
        name = "ecommerce-admin-panel"
        targetPath = "ecommerce/admin/panel"
        expectedParent = "ecommerce"
        description = "Painel admin com múltiplos níveis"
    }
)

foreach ($deploy in $testDeploys) {
    Write-Host "📦 Criando deploy: $($deploy.name) - $($deploy.description)"
    
    $deployBody = @{
        repoUrl = "https://github.com/microsoft/vscode-website.git"
        branch = "main"
        buildCommand = "echo 'Build para teste parentProject'"
        buildOutput = "."
        targetPath = $deploy.targetPath
    } | ConvertTo-Json

    try {
        $deployResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy" -Method POST -Body $deployBody -Headers $headers
        
        if ($deployResponse.success) {
            Write-Host "  ✅ Deploy criado: $($deploy.name)"
        } else {
            Write-Host "  ❌ Falha: $($deployResponse.message)"
        }
    } catch {
        Write-Host "  ⚠️ Erro: $($_.Exception.Message)"
    }
    
    # Aguardar um pouco entre deploys
    Start-Sleep -Seconds 2
}
```

## 📋 3. Testar Campo ParentProject

```powershell
Write-Host "📋 Testando campo parentProject na listagem de publicações..."

try {
    $publicationsResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications" -Method GET -Headers $headers
    
    Write-Host "✅ Publicações obtidas com sucesso: $($publicationsResponse.count) encontradas"
    Write-Host ""
    
    # Agrupar por parentProject para visualização
    $withParent = $publicationsResponse.publications | Where-Object { $_.parentProject -ne $null }
    $withoutParent = $publicationsResponse.publications | Where-Object { $_.parentProject -eq $null }
    
    Write-Host "🏗️ DEPLOYS COM PROJETO PAI:"
    Write-Host "=" * 50
    
    if ($withParent.Count -gt 0) {
        $groupedByParent = $withParent | Group-Object -Property parentProject
        
        foreach ($group in $groupedByParent) {
            Write-Host "📁 Projeto Pai: $($group.Name)"
            foreach ($deploy in $group.Group) {
                Write-Host "  └── $($deploy.name) (targetPath: $($deploy.fullPath -replace [regex]::Escape($publicationsPath), ''))"
                Write-Host "      Repository: $($deploy.repository)"
                Write-Host "      Exists: $($deploy.exists)"
                Write-Host "      Size: $($deploy.sizeMB) MB"
                Write-Host ""
            }
        }
    } else {
        Write-Host "❌ Nenhum deploy com projeto pai encontrado"
    }
    
    Write-Host "🏠 DEPLOYS NO NÍVEL RAIZ (SEM PROJETO PAI):"
    Write-Host "=" * 50
    
    if ($withoutParent.Count -gt 0) {
        foreach ($deploy in $withoutParent) {
            Write-Host "📦 $($deploy.name)"
            Write-Host "   ParentProject: null"
            Write-Host "   Repository: $($deploy.repository)"
            Write-Host "   Exists: $($deploy.exists)"
            Write-Host "   Size: $($deploy.sizeMB) MB"
            Write-Host ""
        }
    } else {
        Write-Host "❌ Nenhum deploy no nível raiz encontrado"
    }
    
} catch {
    Write-Host "❌ Erro ao obter publicações: $($_.Exception.Message)"
}
```

## 🧪 4. Validar Casos de Teste Específicos

```powershell
Write-Host "🧪 Validando casos de teste específicos..."

foreach ($deploy in $testDeploys) {
    Write-Host "🔍 Validando: $($deploy.name)"
    
    try {
        $publication = $publicationsResponse.publications | Where-Object { 
            $_.name -eq $deploy.name -or $_.name -like "*$($deploy.name)*"
        }
        
        if ($publication) {
            $actualParent = $publication.parentProject
            $expectedParent = $deploy.expectedParent
            
            if (($actualParent -eq $null -and $expectedParent -eq $null) -or 
                ($actualParent -eq $expectedParent)) {
                Write-Host "  ✅ PASSOU: ParentProject = '$actualParent' (esperado: '$expectedParent')"
            } else {
                Write-Host "  ❌ FALHOU: ParentProject = '$actualParent' (esperado: '$expectedParent')"
            }
            
            Write-Host "     TargetPath: $($deploy.targetPath)"
            Write-Host "     Description: $($deploy.description)"
        } else {
            Write-Host "  ⚠️ Deploy não encontrado na listagem"
        }
    } catch {
        Write-Host "  ❌ Erro na validação: $($_.Exception.Message)"
    }
    
    Write-Host ""
}
```

## 📊 5. Estatísticas de Projeto Pai

```powershell
Write-Host "📊 Estatísticas de projetos pai..."

try {
    $allPublications = $publicationsResponse.publications
    
    # Calcular estatísticas
    $totalDeploys = $allPublications.Count
    $deploysWithParent = ($allPublications | Where-Object { $_.parentProject -ne $null }).Count
    $deploysWithoutParent = $totalDeploys - $deploysWithParent
    
    # Contar projetos pai únicos
    $uniqueParents = $allPublications | Where-Object { $_.parentProject -ne $null } | 
                     Select-Object -ExpandProperty parentProject -Unique
    
    Write-Host "📈 ESTATÍSTICAS:"
    Write-Host "  Total de Deploys: $totalDeploys"
    Write-Host "  Deploys com Projeto Pai: $deploysWithParent"
    Write-Host "  Deploys no Nível Raiz: $deploysWithoutParent"
    Write-Host "  Projetos Pai Únicos: $($uniqueParents.Count)"
    Write-Host ""
    
    if ($uniqueParents.Count -gt 0) {
        Write-Host "🏗️ PROJETOS PAI IDENTIFICADOS:"
        foreach ($parent in $uniqueParents) {
            $childCount = ($allPublications | Where-Object { $_.parentProject -eq $parent }).Count
            Write-Host "  📁 $parent ($childCount deploy(s))"
        }
    }
    
} catch {
    Write-Host "❌ Erro ao calcular estatísticas: $($_.Exception.Message)"
}
```

## 🔍 6. Testar Deploy Individual

```powershell
Write-Host "🔍 Testando obtenção de deploy individual..."

$testDeployName = "app2-api"
try {
    $individualDeploy = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/$testDeployName" -Method GET -Headers $headers
    
    Write-Host "✅ Deploy individual obtido:"
    Write-Host "  Nome: $($individualDeploy.publication.name)"
    Write-Host "  ParentProject: $($individualDeploy.publication.parentProject)"
    Write-Host "  Repository: $($individualDeploy.publication.repository)"
    Write-Host "  FullPath: $($individualDeploy.publication.fullPath)"
    Write-Host "  Exists: $($individualDeploy.publication.exists)"
    
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "⚠️ Deploy $testDeployName não encontrado (esperado se não foi criado)"
    } else {
        Write-Host "❌ Erro: $($_.Exception.Message)"
    }
}
```

## 🧹 7. Limpeza (Opcional)

```powershell
Write-Host "🧹 Limpeza opcional dos deploys de teste..."

$cleanup = Read-Host "Deseja remover todos os deploys de teste? (s/N)"
if ($cleanup -eq 's' -or $cleanup -eq 'S') {
    foreach ($deploy in $testDeploys) {
        Write-Host "🗑️ Removendo: $($deploy.name)"
        
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

### ✅ **Estruturas de Teste:**

```json
[
  {
    "name": "app1-standalone",
    "parentProject": null,
    "fullPath": "C:\\temp\\wwwroot\\app1"
  },
  {
    "name": "app2-api", 
    "parentProject": "app2",
    "fullPath": "C:\\temp\\wwwroot\\app2\\api"
  },
  {
    "name": "app2-frontend",
    "parentProject": "app2", 
    "fullPath": "C:\\temp\\wwwroot\\app2\\frontend"
  },
  {
    "name": "microservices-auth",
    "parentProject": "microservices",
    "fullPath": "C:\\temp\\wwwroot\\microservices\\auth"
  },
  {
    "name": "ecommerce-admin-panel",
    "parentProject": "ecommerce",
    "fullPath": "C:\\temp\\wwwroot\\ecommerce\\admin\\panel"
  }
]
```

### 📊 **Agrupamento Esperado:**

```
🏗️ DEPLOYS COM PROJETO PAI:
📁 Projeto Pai: app2
  └── app2-api (targetPath: app2/api)
  └── app2-frontend (targetPath: app2/frontend)

📁 Projeto Pai: microservices  
  └── microservices-auth (targetPath: microservices/auth)
  └── microservices-payment (targetPath: microservices/payment)

📁 Projeto Pai: ecommerce
  └── ecommerce-admin-panel (targetPath: ecommerce/admin/panel)

🏠 DEPLOYS NO NÍVEL RAIZ:
📦 app1-standalone (parentProject: null)
```

---

**💡 Dica:** Use este script para validar completamente a funcionalidade de detecção de projetos pai, incluindo casos complexos com múltiplos níveis de diretórios.
