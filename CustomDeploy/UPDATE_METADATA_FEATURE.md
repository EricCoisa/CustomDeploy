# 🔧 Funcionalidade: Atualização de Metadados de Publicações

## 📋 Visão Geral

Esta funcionalidade permite atualizar metadados específicos de publicações já deployadas, modificando apenas os campos:
- **Repository**: URL do repositório
- **Branch**: Nome da branch
- **BuildCommand**: Comando de build

## 🛠️ Implementação Técnica

### 1. Modelo de Request

**Arquivo:** `Models/UpdateMetadataRequest.cs`

```csharp
public class UpdateMetadataRequest
{
    public string? Repository { get; set; }
    public string? Branch { get; set; }
    public string? BuildCommand { get; set; }
}
```

**Características:**
- Todos os campos são opcionais (`string?`)
- Pelo menos um campo deve ser fornecido
- Valores em branco são ignorados

### 2. Método no Service

**Arquivo:** `Services/DeployService.cs`
**Método:** `UpdateDeployMetadata()`

```csharp
public (bool Success, string Message, DeployMetadata? UpdatedDeploy) UpdateDeployMetadata(
    string name, 
    string? repository = null, 
    string? branch = null, 
    string? buildCommand = null)
```

**Funcionalidades:**
- ✅ **Thread-Safe**: Protegido com locks
- ✅ **Validação de Input**: Verifica nome obrigatório e pelo menos um campo
- ✅ **Busca por Nome**: Localiza deploy usando case-insensitive comparison
- ✅ **Atualização Seletiva**: Modifica apenas campos fornecidos
- ✅ **Logging Detalhado**: Registra todas as alterações
- ✅ **Tratamento de Erros**: Retorna mensagens descritivas

**Fluxo de Execução:**
1. Validação de parâmetros
2. Carregamento de metadados existentes
3. Localização do deploy por nome
4. Backup dos valores originais
5. Aplicação das alterações
6. Salvamento dos metadados
7. Retorno com detalhes das alterações

### 3. Endpoint REST

**Arquivo:** `Controllers/PublicationController.cs`
**Endpoint:** `PATCH /deploy/publications/{name}/metadata`

```csharp
[HttpPatch("publications/{name}/metadata")]
public IActionResult UpdatePublicationMetadata(string name, [FromBody] UpdateMetadataRequest request)
```

**Características:**
- ✅ **Método HTTP**: PATCH (semântica correta para atualizações parciais)
- ✅ **Autenticação**: Requer token JWT válido
- ✅ **Validação de Input**: Verifica nome e request body
- ✅ **Códigos HTTP Apropriados**: 200, 400, 404, 500
- ✅ **Response Detalhada**: Inclui alterações realizadas

## 📡 API Reference

### Request

```http
PATCH /deploy/publications/{name}/metadata
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "repository": "https://github.com/user/repo.git",
  "branch": "develop", 
  "buildCommand": "npm install && npm run build"
}
```

### Response (200 OK)

```json
{
  "message": "Metadados atualizados com sucesso",
  "details": "Metadados do deploy 'app-name' atualizados. Alterações: Repository: 'old-repo' → 'new-repo', Branch: 'main' → 'develop'",
  "updatedMetadata": {
    "name": "app-name",
    "repository": "https://github.com/user/repo.git",
    "branch": "develop",
    "buildCommand": "npm install && npm run build", 
    "targetPath": "C:\\temp\\wwwroot\\app-name",
    "deployedAt": "2025-08-02T10:30:00Z",
    "exists": true
  },
  "timestamp": "2025-08-02T10:45:00Z"
}
```

### Response (400 Bad Request)

```json
{
  "message": "Pelo menos um campo deve ser fornecido para atualização",
  "allowedFields": ["repository", "branch", "buildCommand"]
}
```

### Response (404 Not Found)

```json
{
  "message": "Deploy 'app-name' não encontrado"
}
```

## 🔒 Validações e Segurança

### Validações de Input

1. **Nome Obrigatório**: Nome da publicação não pode ser vazio
2. **Request Body**: Pelo menos um campo deve ser fornecido
3. **Deploy Existente**: Verifica se o deploy existe nos metadados
4. **Campos Válidos**: Apenas Repository, Branch e BuildCommand são aceitos

### Segurança

1. **Autenticação JWT**: Endpoint protegido por autenticação
2. **Autorização**: Requer token válido
3. **Thread-Safety**: Operações thread-safe com locks
4. **Validação de Entrada**: Sanitização de inputs
5. **Logs de Auditoria**: Registro de todas as alterações

### Limitações de Segurança

- ❌ **Não valida URLs**: Repository URLs não são validadas
- ❌ **Sem validação de Branch**: Nomes de branch não são verificados
- ❌ **Comandos de Build**: Não há sanitização de comandos shell

## 🏗️ Arquitetura

### Camadas

```
Controller Layer (PublicationController)
    ↓ Validation & HTTP Handling
Service Layer (DeployService)  
    ↓ Business Logic & Thread Safety
Storage Layer (deploys.json)
    ↓ File System Persistence
```

### Fluxo de Dados

1. **HTTP Request** → Controller validation
2. **Controller** → Service method call
3. **Service** → Load metadata from file
4. **Service** → Update specific fields
5. **Service** → Save metadata to file
6. **Service** → Return result with changes
7. **Controller** → Format HTTP response

## 🧪 Cenários de Teste

### Casos de Sucesso

1. **Atualização Individual**: Um campo por vez
2. **Atualização Múltipla**: Vários campos simultaneamente
3. **Preservação de Dados**: Campos não especificados permanecem inalterados
4. **Case Insensitive**: Busca funciona independente de maiúsculas/minúsculas

### Casos de Erro

1. **Deploy Inexistente**: 404 Not Found
2. **Nome Vazio**: 400 Bad Request
3. **Body Vazio**: 400 Bad Request
4. **Sem Autenticação**: 401 Unauthorized
5. **Erro de I/O**: 500 Internal Server Error

### Testes de Carga

- **Concorrência**: Múltiplas atualizações simultâneas
- **Volume**: Atualização de muitos deploys
- **Performance**: Tempo de resposta com arquivo grande

## 📊 Logs e Monitoramento

### Logs de Informação

```
[INFO] Atualizando metadados do deploy: {Name}
[INFO] Repository atualizado: '{OldValue}' -> '{NewValue}'
[INFO] Branch atualizada: '{OldValue}' -> '{NewValue}'
[INFO] BuildCommand atualizado: '{OldValue}' -> '{NewValue}'
[INFO] Metadados do deploy '{Name}' atualizados com sucesso
```

### Logs de Erro

```
[WARNING] Deploy não encontrado para atualização: {Name}
[ERROR] Erro ao atualizar metadados do deploy: {Name}
```

### Métricas Sugeridas

- Número de atualizações por período
- Tempo médio de atualização
- Taxa de erro por tipo
- Campos mais atualizados

## 🔄 Casos de Uso Comuns

### 1. Migração de Repositório

```json
{
  "repository": "https://github.com/new-org/app.git"
}
```

### 2. Mudança de Branch de Produção

```json
{
  "branch": "release/v2.0"
}
```

### 3. Otimização do Build

```json
{
  "buildCommand": "pnpm install --frozen-lockfile && pnpm build --production"
}
```

### 4. Atualização Completa

```json
{
  "repository": "https://github.com/new-org/app.git",
  "branch": "main",
  "buildCommand": "yarn install && yarn build:prod"
}
```

## 🚀 Futuras Melhorias

### Funcionalidades

1. **Validação de URLs**: Verificar se repository URLs são válidos
2. **Histórico de Alterações**: Manter log de mudanças nos metadados
3. **Rollback**: Capacidade de reverter alterações
4. **Backup Automático**: Backup antes de alterações críticas

### Performance

1. **Cache**: Cache de metadados em memória
2. **Batch Updates**: Atualização de múltiplos deploys
3. **Async Operations**: Operações assíncronas para melhor performance

### Segurança

1. **Validação de Comandos**: Sanitização de build commands
2. **Rate Limiting**: Limitar número de atualizações por usuário
3. **Audit Trail**: Log detalhado de quem fez quais alterações
4. **Permissões Granulares**: Controle de quem pode atualizar quais campos

---

**📝 Nota:** Esta funcionalidade foi implementada seguindo as melhores práticas de desenvolvimento com foco em segurança, performance e manutenibilidade.
