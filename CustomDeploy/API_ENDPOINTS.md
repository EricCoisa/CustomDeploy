# 🌐 API Endpoints - CustomDeploy

## 📊 Visão Geral da API

O CustomDeploy oferece uma API REST completa para gerenciamento de deployments e metadados de aplicações publicadas.

### 🔐 Autenticação

Todos os endpoints (exceto login) requerem JWT Bearer Token no header:
```
Authorization: Bearer <jwt_token>
```

---

## 🔑 Autenticação

### POST `/auth/login`
Autentica usuário e retorna JWT token.

**Request:**
```json
{
  "username": "admin",
  "password": "sua_senha"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2025-08-02T16:30:00Z"
}
```

---

## 🚀 Deploy Operations

### POST `/deploy`
Realiza deploy de uma aplicação com metadados completos.

**Request:**
```json
{
  "name": "minha-api",
  "targetPath": "microservices/auth",
  "repository": "https://github.com/user/auth-api.git",
  "branch": "main",
  "buildCommand": "dotnet build --configuration Release"
}
```

**Response:**
```json
{
  "message": "Deploy realizado com sucesso para microservices/auth",
  "deployPath": "C:\\temp\\wwwroot\\microservices\\auth",
  "metadata": {
    "name": "minha-api",
    "targetPath": "microservices/auth",
    "repository": "https://github.com/user/auth-api.git",
    "branch": "main",
    "buildCommand": "dotnet build --configuration Release",
    "deployedAt": "2025-08-02T15:30:00Z",
    "exists": true
  }
}
```

---

## 📋 Publication Management

### GET `/publications`
Lista todas as publicações (físicas + metadados).

**Response:**
```json
{
  "message": "Publicações listadas com sucesso",
  "count": 4,
  "publications": [
    {
      "name": "main-app",
      "fullPath": "C:\\temp\\wwwroot\\main-app",
      "parentProject": null,
      "exists": true,
      "sizeMB": 45.7,
      "repository": "https://github.com/user/main-app.git",
      "branch": "main",
      "buildCommand": "npm run build",
      "lastModified": "2025-08-02T15:30:00Z"
    },
    {
      "name": "auth-service",
      "fullPath": "C:\\temp\\wwwroot\\services\\auth",
      "parentProject": "services",
      "exists": true,
      "sizeMB": 12.3,
      "repository": "https://github.com/user/auth-service.git",
      "branch": "develop",
      "buildCommand": "dotnet publish",
      "lastModified": "2025-08-02T14:20:00Z"
    }
  ]
}
```

### PATCH `/publications/{name}/metadata`
Atualiza metadados específicos de uma publicação.

**Campos Atualizáveis:** `repository`, `branch`, `buildCommand`

**Request:**
```json
{
  "repository": "https://github.com/user/new-repo.git",
  "branch": "feature/new-version",
  "buildCommand": "npm run build:prod"
}
```

**Response:**
```json
{
  "message": "Metadados atualizados com sucesso",
  "updatedFields": {
    "repository": "https://github.com/user/new-repo.git",
    "branch": "feature/new-version",
    "buildCommand": "npm run build:prod"
  }
}
```

### DELETE `/publications/{name}/complete`
Remove completamente uma publicação (metadados + pasta física).

**Response:**
```json
{
  "message": "Publicação 'minha-api' removida completamente (metadados e pasta física)",
  "removedPath": "C:\\temp\\wwwroot\\microservices\\auth"
}
```

**Errors:**
```json
{
  "message": "Publicação 'inexistente' não encontrada nos metadados"
}
```

### DELETE `/publications/{name}/metadata`
Remove apenas os metadados de uma publicação (mantém pasta física).

**Response:**
```json
{
  "message": "Metadados da publicação 'minha-api' removidos (pasta física mantida)",
  "preservedPath": "C:\\temp\\wwwroot\\microservices\\auth"
}
```

---

## 🔍 Funcionalidades Especiais

### 📂 Detecção Automática de Pastas
- **Busca Root-Level**: Detecta automaticamente pastas no primeiro nível
- **Auto-Metadata**: Cria metadados para pastas órfãs encontradas
- **Estrutura Híbrida**: Combina pastas físicas + metadados centralizados

### 🏗️ Organização Hierárquica
- **ParentProject**: Detectado automaticamente baseado no `targetPath`
- **Exemplos**:
  - `"app1"` → `parentProject: null`
  - `"services/auth"` → `parentProject: "services"`
  - `"ecommerce/frontend/admin"` → `parentProject: "ecommerce"`

### 🗃️ Metadados Centralizados
- **Arquivo**: `deploys.json` no diretório da aplicação
- **Thread-Safe**: Operações com lock para concorrência
- **Verificação exists**: Validação automática da existência física

---

## 📊 Status Codes

| Código | Descrição | Uso |
|--------|-----------|-----|
| `200` | OK | Operações de consulta e atualização bem-sucedidas |
| `201` | Created | Deploy realizado com sucesso |
| `400` | Bad Request | Dados inválidos ou campos obrigatórios faltando |
| `401` | Unauthorized | Token JWT inválido ou expirado |
| `404` | Not Found | Publicação não encontrada |
| `409` | Conflict | Tentativa de deploy em local já ocupado |
| `500` | Internal Server Error | Erro interno do servidor |

---

## 🧪 Exemplos de Uso

### Cenário 1: Deploy de Nova Aplicação

```bash
# 1. Autenticar
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "sua_senha"}'

# 2. Fazer deploy
curl -X POST http://localhost:5000/deploy \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "nova-api",
    "targetPath": "apis/nova",
    "repository": "https://github.com/user/nova-api.git",
    "branch": "main",
    "buildCommand": "dotnet publish -c Release"
  }'

# 3. Verificar listagem
curl -X GET http://localhost:5000/publications \
  -H "Authorization: Bearer <token>"
```

### Cenário 2: Atualização de Metadados

```bash
# Atualizar repository e branch
curl -X PATCH http://localhost:5000/publications/nova-api/metadata \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "repository": "https://github.com/user/nova-api-v2.git",
    "branch": "v2.0"
  }'
```

### Cenário 3: Remoção Controlada

```bash
# Remover apenas metadados (manter pasta)
curl -X DELETE http://localhost:5000/publications/nova-api/metadata \
  -H "Authorization: Bearer <token>"

# Ou remover tudo (metadados + pasta)
curl -X DELETE http://localhost:5000/publications/nova-api/complete \
  -H "Authorization: Bearer <token>"
```

---

## 🔧 Configuração

### appsettings.json
```json
{
  "JwtSettings": {
    "SecretKey": "sua_chave_secreta_muito_segura_com_32_chars",
    "Issuer": "CustomDeploy",
    "Audience": "CustomDeployUsers",
    "ExpirationMinutes": 60
  },
  "PublicationsPath": "C:\\temp\\wwwroot"
}
```

### Credenciais Padrão
- **Username**: admin
- **Password**: Configurada no `JwtSettings:SecretKey`

---

## 📝 Notas Importantes

### Segurança
- Todos os endpoints (exceto login) requerem autenticação JWT
- Tokens expiram conforme configurado em `ExpirationMinutes`
- Operações de escrita são protegidas com autenticação

### Performance
- Busca não-recursiva para melhor performance
- Cache de metadados em memória
- Operações thread-safe para concorrência

### Flexibilidade
- Suporte a estruturas hierárquicas via `targetPath`
- Detecção automática de pastas órfãs
- Atualização seletiva de metadados

---

**🔄 Versioning**: API versão 1.0 - CustomDeploy ASP.NET Core 8.0
