# 🔧 Script de Teste - Atualização de Metadados

## Funcionalidade Implementada
Método para atualizar metadados específicos de publicações:
- **Repository**: URL do repositório
- **Branch**: Nome da branch
- **BuildCommand**: Comando de build

## Endpoint
```
PATCH /deploy/publications/{name}/metadata
```

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

## 🚀 2. Criar Deploy de Teste (se necessário)

```powershell
# Verificar se já existe um deploy para teste
$testDeployName = "teste-atualizacao-metadata"

try {
    $existingDeploy = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/$testDeployName" -Method GET -Headers $headers
    Write-Host "✅ Deploy de teste já existe: $($existingDeploy.publication.name)"
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "🚀 Criando deploy de teste para atualização de metadados..."
        
        $deployBody = @{
            repoUrl = "https://github.com/microsoft/vscode-website.git"
            branch = "main"
            buildCommand = "npm install && npm run build"
            buildOutput = "."
            targetPath = $testDeployName
        } | ConvertTo-Json

        try {
            $deployResponse = Invoke-RestMethod -Uri "https://localhost:7071/deploy" -Method POST -Body $deployBody -Headers $headers
            
            if ($deployResponse.success) {
                Write-Host "✅ Deploy de teste criado com sucesso"
            } else {
                Write-Host "❌ Falha ao criar deploy de teste: $($deployResponse.message)"
                exit 1
            }
        } catch {
            Write-Host "❌ Erro ao criar deploy de teste: $($_.Exception.Message)"
            exit 1
        }
    } else {
        Write-Host "❌ Erro inesperado ao verificar deploy existente: $($_.Exception.Message)"
        exit 1
    }
}
```

## 📋 3. Obter Metadados Atuais

```powershell
Write-Host "📋 Obtendo metadados atuais..."

try {
    $currentMetadata = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/$testDeployName/metadata" -Method GET -Headers $headers
    
    Write-Host "✅ Metadados atuais:"
    Write-Host "  Nome: $($currentMetadata.metadata.name)"
    Write-Host "  Repository: $($currentMetadata.metadata.repository)"
    Write-Host "  Branch: $($currentMetadata.metadata.branch)"
    Write-Host "  BuildCommand: $($currentMetadata.metadata.buildCommand)"
    Write-Host "  TargetPath: $($currentMetadata.metadata.targetPath)"
    Write-Host "  DeployedAt: $($currentMetadata.metadata.deployedAt)"
    Write-Host "  Exists: $($currentMetadata.metadata.exists)"
} catch {
    Write-Host "❌ Erro ao obter metadados atuais: $($_.Exception.Message)"
    exit 1
}
```

## 🔧 4. Teste 1: Atualizar Apenas Repository

```powershell
Write-Host "🔧 Teste 1: Atualizando apenas Repository..."

$updateRepository = @{
    repository = "https://github.com/facebook/react.git"
} | ConvertTo-Json

try {
    $updateResult1 = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/$testDeployName/metadata" -Method PATCH -Body $updateRepository -Headers $headers
    
    Write-Host "✅ Repository atualizado com sucesso:"
    Write-Host "  Message: $($updateResult1.message)"
    Write-Host "  Details: $($updateResult1.details)"
    Write-Host "  Novo Repository: $($updateResult1.updatedMetadata.repository)"
    Write-Host "  Branch mantida: $($updateResult1.updatedMetadata.branch)"
    Write-Host "  BuildCommand mantido: $($updateResult1.updatedMetadata.buildCommand)"
} catch {
    Write-Host "❌ Erro ao atualizar repository: $($_.Exception.Message)"
}
```

## 🔧 5. Teste 2: Atualizar Apenas Branch

```powershell
Write-Host "🔧 Teste 2: Atualizando apenas Branch..."

$updateBranch = @{
    branch = "develop"
} | ConvertTo-Json

try {
    $updateResult2 = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/$testDeployName/metadata" -Method PATCH -Body $updateBranch -Headers $headers
    
    Write-Host "✅ Branch atualizada com sucesso:"
    Write-Host "  Message: $($updateResult2.message)"
    Write-Host "  Details: $($updateResult2.details)"
    Write-Host "  Repository mantido: $($updateResult2.updatedMetadata.repository)"
    Write-Host "  Nova Branch: $($updateResult2.updatedMetadata.branch)"
    Write-Host "  BuildCommand mantido: $($updateResult2.updatedMetadata.buildCommand)"
} catch {
    Write-Host "❌ Erro ao atualizar branch: $($_.Exception.Message)"
}
```

## 🔧 6. Teste 3: Atualizar Apenas BuildCommand

```powershell
Write-Host "🔧 Teste 3: Atualizando apenas BuildCommand..."

$updateBuildCommand = @{
    buildCommand = "yarn install && yarn build --production"
} | ConvertTo-Json

try {
    $updateResult3 = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/$testDeployName/metadata" -Method PATCH -Body $updateBuildCommand -Headers $headers
    
    Write-Host "✅ BuildCommand atualizado com sucesso:"
    Write-Host "  Message: $($updateResult3.message)"
    Write-Host "  Details: $($updateResult3.details)"
    Write-Host "  Repository mantido: $($updateResult3.updatedMetadata.repository)"
    Write-Host "  Branch mantida: $($updateResult3.updatedMetadata.branch)"
    Write-Host "  Novo BuildCommand: $($updateResult3.updatedMetadata.buildCommand)"
} catch {
    Write-Host "❌ Erro ao atualizar buildCommand: $($_.Exception.Message)"
}
```

## 🔧 7. Teste 4: Atualizar Múltiplos Campos

```powershell
Write-Host "🔧 Teste 4: Atualizando múltiplos campos simultaneamente..."

$updateMultiple = @{
    repository = "https://github.com/vercel/next.js.git"
    branch = "canary"
    buildCommand = "pnpm install && pnpm build"
} | ConvertTo-Json

try {
    $updateResult4 = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/$testDeployName/metadata" -Method PATCH -Body $updateMultiple -Headers $headers
    
    Write-Host "✅ Múltiplos campos atualizados com sucesso:"
    Write-Host "  Message: $($updateResult4.message)"
    Write-Host "  Details: $($updateResult4.details)"
    Write-Host "  Novo Repository: $($updateResult4.updatedMetadata.repository)"
    Write-Host "  Nova Branch: $($updateResult4.updatedMetadata.branch)"
    Write-Host "  Novo BuildCommand: $($updateResult4.updatedMetadata.buildCommand)"
} catch {
    Write-Host "❌ Erro ao atualizar múltiplos campos: $($_.Exception.Message)"
}
```

## ❌ 8. Teste 5: Validação de Erros

```powershell
Write-Host "❌ Teste 5: Validando tratamento de erros..."

# Teste 5.1: Body vazio
Write-Host "🔍 Teste 5.1: Request sem dados..."
try {
    $emptyResult = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/$testDeployName/metadata" -Method PATCH -Body "{}" -Headers $headers
    Write-Host "❌ TESTE FALHOU: Deveria rejeitar body vazio"
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "✅ TESTE PASSOU: Body vazio rejeitado corretamente (400)"
    } else {
        Write-Host "⚠️ Erro inesperado: $($_.Exception.Message)"
    }
}

# Teste 5.2: Deploy inexistente
Write-Host "🔍 Teste 5.2: Deploy inexistente..."
$updateNonExistent = @{
    repository = "https://github.com/test/test.git"
} | ConvertTo-Json

try {
    $nonExistentResult = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/deploy-inexistente/metadata" -Method PATCH -Body $updateNonExistent -Headers $headers
    Write-Host "❌ TESTE FALHOU: Deveria retornar 404 para deploy inexistente"
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "✅ TESTE PASSOU: Deploy inexistente retorna 404 corretamente"
    } else {
        Write-Host "⚠️ Erro inesperado: $($_.Exception.Message)"
    }
}

# Teste 5.3: Nome inválido
Write-Host "🔍 Teste 5.3: Nome vazio..."
try {
    $invalidNameResult = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/ /metadata" -Method PATCH -Body $updateNonExistent -Headers $headers
    Write-Host "❌ TESTE FALHOU: Deveria rejeitar nome vazio"
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "✅ TESTE PASSOU: Nome vazio rejeitado corretamente (400)"
    } else {
        Write-Host "⚠️ Erro inesperado: $($_.Exception.Message)"
    }
}
```

## 📋 9. Verificar Estado Final

```powershell
Write-Host "📋 Verificando estado final dos metadados..."

try {
    $finalMetadata = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/$testDeployName/metadata" -Method GET -Headers $headers
    
    Write-Host "✅ Metadados finais:"
    Write-Host "  Nome: $($finalMetadata.metadata.name)"
    Write-Host "  Repository: $($finalMetadata.metadata.repository)"
    Write-Host "  Branch: $($finalMetadata.metadata.branch)"
    Write-Host "  BuildCommand: $($finalMetadata.metadata.buildCommand)"
    Write-Host "  TargetPath: $($finalMetadata.metadata.targetPath)"
    Write-Host "  DeployedAt: $($finalMetadata.metadata.deployedAt)"
    Write-Host "  Exists: $($finalMetadata.metadata.exists)"
} catch {
    Write-Host "❌ Erro ao obter metadados finais: $($_.Exception.Message)"
}
```

## 🧹 10. Limpeza (Opcional)

```powershell
Write-Host "🧹 Limpeza opcional do deploy de teste..."

# Pergunta se deseja remover o deploy de teste
$cleanup = Read-Host "Deseja remover o deploy de teste? (s/N)"
if ($cleanup -eq 's' -or $cleanup -eq 'S') {
    try {
        $deleteResult = Invoke-RestMethod -Uri "https://localhost:7071/deploy/publications/$testDeployName" -Method DELETE -Headers $headers
        Write-Host "✅ Deploy de teste removido: $($deleteResult.message)"
    } catch {
        Write-Host "⚠️ Erro ao remover deploy de teste: $($_.Exception.Message)"
    }
} else {
    Write-Host "ℹ️ Deploy de teste mantido para futuras validações"
}
```

## 📊 Resultados Esperados

### ✅ **Operações Bem-Sucedidas:**

1. **Atualização Individual:** Cada campo pode ser atualizado separadamente
2. **Atualização Múltipla:** Vários campos podem ser atualizados simultaneamente
3. **Preservação de Dados:** Campos não especificados permanecem inalterados
4. **Resposta Detalhada:** Log das alterações realizadas
5. **Validação de Input:** Rejeita requests inválidos com códigos HTTP apropriados

### 📈 **Logs Esperados no Servidor:**

```
[INFO] Atualizando metadados do deploy: teste-atualizacao-metadata
[INFO] Repository atualizado: 'https://github.com/microsoft/vscode-website.git' -> 'https://github.com/facebook/react.git'
[INFO] Branch atualizada: 'main' -> 'develop'  
[INFO] BuildCommand atualizado: 'npm install && npm run build' -> 'yarn install && yarn build --production'
[INFO] Metadados do deploy 'teste-atualizacao-metadata' atualizados com sucesso
```

### 🔍 **Exemplos de Response:**

```json
{
  "message": "Metadados atualizados com sucesso",
  "details": "Metadados do deploy 'teste-atualizacao-metadata' atualizados. Alterações: Repository: 'old-repo' → 'new-repo', Branch: 'old-branch' → 'new-branch'",
  "updatedMetadata": {
    "name": "teste-atualizacao-metadata",
    "repository": "https://github.com/vercel/next.js.git",
    "branch": "canary", 
    "buildCommand": "pnpm install && pnpm build",
    "targetPath": "C:\\temp\\wwwroot\\teste-atualizacao-metadata",
    "deployedAt": "2025-08-02T10:30:00Z",
    "exists": true
  },
  "timestamp": "2025-08-02T10:45:00Z"
}
```

---

**💡 Dica:** Use este script para validar todas as funcionalidades de atualização de metadados, incluindo casos de sucesso e tratamento de erros.
