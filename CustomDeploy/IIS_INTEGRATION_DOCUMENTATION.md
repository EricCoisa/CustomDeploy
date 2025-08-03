# Sistema de Deploy Integrado com IIS

## Visão Geral

O sistema foi atualizado para integrar completamente com sites do IIS, substituindo o uso direto de caminhos físicos (`PublicationsPath`) por identificação de sites IIS.

## Mudanças Implementadas

### 1. **Modelo DeployRequest Atualizado**

Novo campo obrigatório:
```csharp
public string IisSiteName { get; set; } = string.Empty;
```

O campo `TargetPath` agora representa um caminho relativo dentro do site IIS.

### 2. **IISManagementService - Novos Métodos**

#### `GetSiteInfoAsync(string siteName)`
- Obtém informações detalhadas de um site IIS
- Retorna o caminho físico do site
- Valida se o site existe

#### `CheckApplicationExistsAsync(string siteName, string applicationPath)`
- Verifica se um caminho existe como aplicação IIS
- Identifica diferença entre pasta e aplicação IIS

#### `GetAllSitesAsync()`
- Lista todos os sites IIS disponíveis
- Inclui informações básicas: nome, ID, estado, caminho físico

### 3. **DeployService Completamente Refatorado**

#### Novo Fluxo de Deploy:
1. **Validação do Site IIS**: Verifica se o `iisSiteName` existe
2. **Obtenção do Caminho Físico**: Resolve automaticamente o caminho do site
3. **Verificação de Aplicação**: Identifica se o `targetPath` é uma aplicação IIS
4. **Deploy Inteligente**: Publica no caminho correto do IIS

#### Novo Método `CopyBuildOutputToIISPathAsync()`
- Especializado para deploy em caminhos IIS
- Validação de diretório pai (site físico)
- Logs específicos para operações IIS

#### Metadados Atualizados
- Salvamento com identificação completa: `{siteName}/{targetPath}`
- Armazenamento do caminho físico real

### 4. **DeployController Atualizado**

#### Validação Aprimorada:
```csharp
if (string.IsNullOrWhiteSpace(request.IisSiteName))
{
    return BadRequest(new { message = "All fields are required, including iisSiteName" });
}
```

#### Resposta Enriquecida:
```json
{
  "message": "Deploy concluído com sucesso",
  "iisSiteName": "App2Site",
  "deployDetails": {
    "siteInfo": { /* Informações do site */ },
    "applicationInfo": { /* Informações da aplicação */ },
    "finalPath": "C:\\inetpub\\wwwroot\\App2Site\\api",
    "isIISApplication": true,
    "warnings": []
  }
}
```

### 5. **IISController - Novos Endpoints**

#### `GET /api/iis/sites/{siteName}`
- Obtém informações detalhadas de um site específico
- Retorna caminho físico e configurações

#### `GET /api/iis/sites/{siteName}/applications/{applicationPath}`
- Verifica se um caminho é uma aplicação IIS
- Útil para validação antes do deploy

## Exemplos de Uso

### Payload de Deploy Atualizado:
```json
{
  "repoUrl": "https://github.com/usuario/projeto",
  "branch": "main",
  "buildCommand": "npm install && npm run build",
  "buildOutput": "dist",
  "targetPath": "api",
  "iisSiteName": "App2Site"
}
```

### Resultado do Deploy:
- **Destino Final**: `C:\inetpub\wwwroot\App2Site\api`
- **Validação**: Site "App2Site" deve existir no IIS
- **Detecção**: Identifica se "api" é uma aplicação IIS ou apenas pasta

### Cenários Suportados:

#### 1. **Site Simples**
- `iisSiteName`: "MeuSite"
- `targetPath`: ""
- **Resultado**: Deploy direto na raiz do site

#### 2. **Subprojeto como Pasta**
- `iisSiteName`: "App2Site"  
- `targetPath`: "api"
- **Resultado**: Deploy em pasta, com aviso que não é aplicação IIS

#### 3. **Subprojeto como Aplicação IIS**
- `iisSiteName`: "App2Site"
- `targetPath`: "api"
- **Resultado**: Deploy em aplicação IIS reconhecida

## Benefícios

### ✅ **Integração Completa com IIS**
- Não mais dependência de caminhos hardcoded
- Validação automática de sites
- Detecção de aplicações IIS

### ✅ **Flexibilidade**
- Suporte a qualquer site IIS configurado
- Identificação automática de estruturas
- Fallback mantido para compatibilidade

### ✅ **Transparência**
- Logs detalhados sobre estrutura IIS
- Avisos sobre diferenças entre pastas e aplicações
- Resposta rica em informações

### ✅ **Robustez**
- Validação prévia de sites
- Verificação de permissões IIS
- Tratamento de erros específicos

## Migração

### Para usar o novo sistema:

1. **Identifique seus Sites IIS**:
   ```http
   GET /api/iis/sites
   ```

2. **Atualize Payloads de Deploy**:
   - Adicione `iisSiteName`
   - Mantenha `targetPath` como relativo

3. **Verifique Aplicações IIS** (opcional):
   ```http
   GET /api/iis/sites/{siteName}/applications/{targetPath}
   ```

4. **Execute Deploy Normalmente**:
   ```http
   POST /deploy
   ```

O sistema agora oferece integração completa e transparente com IIS, mantendo flexibilidade e proporcionando feedback detalhado sobre a estrutura de deploy.
