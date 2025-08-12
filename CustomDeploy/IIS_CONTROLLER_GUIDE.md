# Guia Completo - IIS Controller API

## Visão Geral

O IIS Controller fornece uma API REST completa para gerenciamento de Sites IIS, Aplicações e Application Pools através de operações CRUD. Esta API requer autenticação JWT e privilégios de administrador para executar operações.

## 📋 Índice

- [Autenticação](#autenticação)
- [Verificações de Permissão](#verificações-de-permissão)
- [Sites IIS](#sites-iis)
- [Aplicações IIS](#aplicações-iis)
- [Application Pools](#application-pools)
- [Códigos de Status](#códigos-de-status)
- [Modelos de Dados](#modelos-de-dados)
- [Exemplos Práticos](#exemplos-práticos)

---

## 🔐 Autenticação

Todas as operações do IIS Controller requerem autenticação JWT. Primeiro, obtenha um token de acesso:

### Login
```http
POST /auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "password"
}
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2025-08-03T16:30:00Z"
}
```

**Use o token em todas as requisições subsequentes:**
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 🛡️ Verificações de Permissão

### Status de Administrador
Verifica se o usuário atual possui privilégios de administrador.

```http
GET /iis/admin-status
Authorization: Bearer {token}
```

**Resposta:**
```json
{
  "isAdministrator": true,
  "currentUser": {
    "name": "ericv",
    "domain": "COISA",
    "fullName": "COISA\\ericv"
  },
  "canManageIIS": true,
  "instructions": [
    "✅ Aplicação executando com privilégios de administrador",
    "Todas as operações IIS estão disponíveis"
  ]
}
```

### Verificar Permissões IIS
Verifica permissões específicas para operações IIS.

```http
GET /iis/permissions
Authorization: Bearer {token}
```

**Resposta:**
```json
{
  "canCreateFolders": true,
  "canMoveFiles": true,
  "canExecuteIISCommands": true,
  "canManageIIS": true,
  "allPermissionsGranted": true,
  "instructions": [],
  "testDetails": [
    "Teste de criação de diretórios: Sucesso",
    "Teste de movimentação de arquivos: Sucesso"
  ]
}
```

---

## 🌐 Sites IIS

### Listar Todos os Sites

```http
GET /iis/sites
Authorization: Bearer {token}
```

**Resposta:**
```json
{
  "message": "Encontrados 4 sites",
  "sites": [
    {
      "name": "Default Web Site",
      "id": 1,
      "state": "Started",
      "physicalPath": "C:\\inetpub\\wwwroot",
      "bindings": [
        {
          "protocol": "http",
          "ipAddress": "0.0.0.0",
          "port": 80,
          "hostName": "",
          "bindingInformation": "0.0.0.0:80:"
        }
      ],
      "applications": [...],
      "appPoolName": "DefaultAppPool"
    }
  ],
  "timestamp": "2025-08-03T15:30:00Z"
}
```

### Obter Detalhes de um Site

```http
GET /iis/sites/{siteName}
Authorization: Bearer {token}
```

**Exemplo:**
```http
GET /iis/sites/Default Web Site
Authorization: Bearer {token}
```

### Criar Novo Site

```http
POST /iis/sites
Authorization: Bearer {token}
Content-Type: application/json

{
  "SiteName": "MeuNovoSite",
  "BindingInformation": "0.0.0.0:8080:",
  "PhysicalPath": "C:\\inetpub\\wwwroot\\meusite",
  "AppPoolName": "DefaultAppPool"
}
```

**Resposta de Sucesso:**
```json
{
  "message": "Site 'MeuNovoSite' criado com sucesso",
  "site": {
    "siteName": "MeuNovoSite",
    "id": 5,
    "physicalPath": "C:\\inetpub\\wwwroot\\meusite",
    "appPoolName": "DefaultAppPool",
    "binding": "0.0.0.0:8080:"
  },
  "timestamp": "2025-08-03T15:30:00Z"
}
```

### Atualizar Site Existente

```http
PUT /iis/sites/{siteName}
Authorization: Bearer {token}
Content-Type: application/json

{
  "PhysicalPath": "C:\\inetpub\\wwwroot\\meusite-atualizado"
}
```

**Exemplo:**
```http
PUT /iis/sites/MeuNovoSite
Authorization: Bearer {token}
Content-Type: application/json

{
  "PhysicalPath": "C:\\inetpub\\wwwroot\\meusite-atualizado"
}
```

### Deletar Site

```http
DELETE /iis/sites/{siteName}
Authorization: Bearer {token}
```

**Exemplo:**
```http
DELETE /iis/sites/MeuNovoSite
Authorization: Bearer {token}
```

**Resposta:**
```json
{
  "message": "Site 'MeuNovoSite' removido com sucesso",
  "timestamp": "2025-08-03T15:30:00Z"
}
```

---

## 📱 Aplicações IIS

### Listar Aplicações de um Site

```http
GET /iis/sites/{siteName}/applications
Authorization: Bearer {token}
```

**Exemplo:**
```http
GET /iis/sites/Default Web Site/applications
Authorization: Bearer {token}
```

**Resposta:**
```json
{
  "message": "Encontradas 3 aplicações",
  "applications": [
    {
      "name": "/app1",
      "physicalPath": "C:\\inetpub\\wwwroot\\app1",
      "enabledProtocols": "http",
      "applicationPool": "DefaultAppPool"
    },
    {
      "name": "/app2",
      "physicalPath": "C:\\inetpub\\wwwroot\\app2",
      "enabledProtocols": "http",
      "applicationPool": "MyCustomPool"
    }
  ],
  "timestamp": "2025-08-03T15:30:00Z"
}
```

### Criar Nova Aplicação

```http
POST /iis/applications
Authorization: Bearer {token}
Content-Type: application/json

{
  "SiteName": "Default Web Site",
  "AppPath": "/minhaapp",
  "PhysicalPath": "C:\\inetpub\\wwwroot\\minhaapp",
  "AppPoolName": "DefaultAppPool"
}
```

**Resposta de Sucesso:**
```json
{
  "message": "Aplicação '/minhaapp' criada com sucesso no site 'Default Web Site'",
  "application": {
    "name": "/minhaapp",
    "physicalPath": "C:\\inetpub\\wwwroot\\minhaapp",
    "enabledProtocols": "http",
    "applicationPool": "DefaultAppPool"
  },
  "timestamp": "2025-08-03T15:30:00Z"
}
```

### Atualizar Aplicação

```http
PUT /iis/sites/{siteName}/applications/{appPath}
Authorization: Bearer {token}
Content-Type: application/json

{
  "PhysicalPath": "C:\\inetpub\\wwwroot\\minhaapp-atualizada",
  "AppPoolName": "MeuNovoPool"
}
```

### Deletar Aplicação

```http
DELETE /iis/sites/{siteName}/applications/{appPath}
Authorization: Bearer {token}
```

**Exemplo:**
```http
DELETE /iis/sites/Default Web Site/applications/minhaapp
Authorization: Bearer {token}
```

**Resposta:**
```json
{
  "message": "Aplicação '/minhaapp' removida com sucesso do site 'Default Web Site'",
  "timestamp": "2025-08-03T15:30:00Z"
}
```

---

## 🏊 Application Pools

### Listar Todos os Application Pools

```http
GET /iis/app-pools
Authorization: Bearer {token}
```

**Resposta:**
```json
{
  "message": "Encontrados 3 Application Pools",
  "appPools": [
    {
      "name": "DefaultAppPool",
      "state": "Started",
      "runtimeVersion": "v4.0",
      "pipelineMode": "Integrated",
      "enable32BitAppOnWin64": false,
      "idleTimeout": 20,
      "associatedSites": ["Default Web Site"],
      "associatedApplications": []
    }
  ],
  "timestamp": "2025-08-03T15:30:00Z"
}
```

### Obter Detalhes de um Application Pool

```http
GET /iis/app-pools/{poolName}
Authorization: Bearer {token}
```

**Exemplo:**
```http
GET /iis/app-pools/DefaultAppPool
Authorization: Bearer {token}
```

### Criar Novo Application Pool

```http
POST /iis/app-pools
Authorization: Bearer {token}
Content-Type: application/json

{
  "PoolName": "MeuNovoPool",
  "RuntimeVersion": "v4.0",
  "PipelineMode": "Integrated"
}
```

**⚠️ Nota:** A criação de Application Pools pode apresentar problemas em alguns ambientes devido a limitações internas do IIS.

### Atualizar Application Pool

```http
PUT /iis/app-pools/{poolName}
Authorization: Bearer {token}
Content-Type: application/json

{
  "RuntimeVersion": "No Managed Code",
  "PipelineMode": "Classic"
}
```

### Deletar Application Pool

```http
DELETE /iis/app-pools/{poolName}
Authorization: Bearer {token}
```

### Operações de Controle

#### Iniciar Application Pool
```http
POST /iis/app-pools/{poolName}/start
Authorization: Bearer {token}
```

#### Parar Application Pool
```http
POST /iis/app-pools/{poolName}/stop
Authorization: Bearer {token}
```

#### Reciclar Application Pool
```http
POST /iis/app-pools/{poolName}/recycle
Authorization: Bearer {token}
```

---

## 📊 Códigos de Status

| Código | Descrição | Significado |
|--------|-----------|-------------|
| `200` | OK | Operação realizada com sucesso |
| `201` | Created | Recurso criado com sucesso |
| `400` | Bad Request | Dados inválidos ou obrigatórios ausentes |
| `401` | Unauthorized | Token JWT inválido ou ausente |
| `403` | Forbidden | Usuário sem privilégios de administrador |
| `404` | Not Found | Recurso não encontrado |
| `409` | Conflict | Recurso já existe (nome duplicado) |
| `500` | Internal Server Error | Erro interno do servidor ou IIS |

---

## 📋 Modelos de Dados

### CreateSiteRequest
```json
{
  "SiteName": "string (obrigatório)",
  "BindingInformation": "string (obrigatório, formato: IP:Porta:HostName)",
  "PhysicalPath": "string (obrigatório)",
  "AppPoolName": "string (obrigatório)"
}
```

### UpdateSiteRequest
```json
{
  "PhysicalPath": "string (opcional)",
  "Bindings": [
    {
      "protocol": "string",
      "ipAddress": "string",
      "port": "integer",
      "hostName": "string"
    }
  ]
}
```

### CreateApplicationRequest
```json
{
  "SiteName": "string (obrigatório)",
  "AppPath": "string (obrigatório, deve começar com '/')",
  "PhysicalPath": "string (obrigatório)",
  "AppPoolName": "string (obrigatório)"
}
```

### CreateAppPoolRequest
```json
{
  "PoolName": "string (obrigatório)",
  "RuntimeVersion": "string (v4.0, v2.0, No Managed Code)",
  "PipelineMode": "string (Integrated, Classic)"
}
```

---

## 🚀 Exemplos Práticos

### Exemplo 1: Criar um Site Completo com Aplicação

```bash
# 1. Fazer login
curl -X POST "http://localhost:5092/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password"}'

# 2. Criar o site
curl -X POST "http://localhost:5092/iis/sites" \
  -H "Authorization: Bearer {seu_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "SiteName": "MeuProjeto",
    "BindingInformation": "0.0.0.0:8080:",
    "PhysicalPath": "C:\\inetpub\\wwwroot\\meuprojeto",
    "AppPoolName": "DefaultAppPool"
  }'

# 3. Criar uma aplicação no site
curl -X POST "http://localhost:5092/iis/applications" \
  -H "Authorization: Bearer {seu_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "SiteName": "MeuProjeto",
    "AppPath": "/api",
    "PhysicalPath": "C:\\inetpub\\wwwroot\\meuprojeto\\api",
    "AppPoolName": "DefaultAppPool"
  }'
```

### Exemplo 2: Monitorar Application Pools

```bash
# Listar todos os pools
curl -X GET "http://localhost:5092/iis/app-pools" \
  -H "Authorization: Bearer {seu_token}"

# Verificar detalhes de um pool específico
curl -X GET "http://localhost:5092/iis/app-pools/DefaultAppPool" \
  -H "Authorization: Bearer {seu_token}"

# Reciclar um pool se necessário
curl -X POST "http://localhost:5092/iis/app-pools/DefaultAppPool/recycle" \
  -H "Authorization: Bearer {seu_token}"
```

### Exemplo 3: Gerenciamento de Aplicações

```bash
# Listar aplicações de um site
curl -X GET "http://localhost:5092/iis/sites/MeuProjeto/applications" \
  -H "Authorization: Bearer {seu_token}"

# Remover uma aplicação específica
curl -X DELETE "http://localhost:5092/iis/sites/MeuProjeto/applications/api" \
  -H "Authorization: Bearer {seu_token}"
```

---

## ⚠️ Considerações Importantes

1. **Privilégios de Administrador**: Todas as operações requerem que a aplicação esteja executando com privilégios de administrador.

2. **Validação de Entrada**: A API valida todos os dados de entrada e retorna erros específicos para campos obrigatórios ausentes.

3. **Gerenciamento de Estado**: Sites e Application Pools podem ter diferentes estados (Started, Stopped, etc.).

4. **Paths Físicos**: Certifique-se de que os diretórios físicos existam antes de criar sites ou aplicações.

5. **Binding Information**: Use o formato `IP:Porta:HostName` para binding information (ex: `0.0.0.0:8080:` ou `*:80:meusite.com`).

6. **Limitações Conhecidas**: A criação de Application Pools pode apresentar problemas em alguns ambientes IIS específicos.

---

## 📝 Logs e Monitoramento

A API gera logs detalhados para todas as operações. Monitore a saída do terminal para:

- ✅ Confirmações de sucesso
- ❌ Detalhes de erro
- 🔍 Validações de entrada
- 🛡️ Verificações de permissão
- 📊 Estatísticas de operação

---

## 🆘 Solução de Problemas

### Erro 401 (Unauthorized)
- Verifique se o token JWT é válido
- Confirme se o token não expirou
- Certifique-se de incluir o header `Authorization: Bearer {token}`

### Erro 403 (Forbidden)
- Execute a aplicação como administrador
- Verifique privilégios do usuário atual

### Erro 400 (Bad Request)
- Revise os campos obrigatórios
- Confirme o formato dos dados JSON
- Verifique se o BindingInformation está no formato correto

### Erro 409 (Conflict)
- Nome do site/pool já existe
- Use um nome diferente ou remova o recurso existente primeiro

### Erro 500 (Internal Server Error)
- Verifique se o IIS está funcionando corretamente
- Confirme se os paths físicos são válidos
- Consulte os logs da aplicação para detalhes específicos

---

*Documento gerado em: 03/08/2025*  
*Versão da API: 1.0*  
*Sistema: CustomDeploy IIS Management*
