# IIS Controller - Referência Rápida de Endpoints

## 🔐 Autenticação

```http
POST /auth/login
{
  "username": "admin",
  "password": "password"
}
```

## 🌐 Sites IIS

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/iis/sites` | Listar todos os sites |
| `GET` | `/iis/sites/{siteName}` | Obter detalhes de um site |
| `POST` | `/iis/sites` | Criar novo site |
| `PUT` | `/iis/sites/{siteName}` | Atualizar site |
| `DELETE` | `/iis/sites/{siteName}` | Deletar site |

### Criar Site
```json
{
  "SiteName": "MeuSite",
  "BindingInformation": "0.0.0.0:8080:",
  "PhysicalPath": "C:\\inetpub\\wwwroot\\meusite",
  "AppPoolName": "DefaultAppPool"
}
```

## 📱 Aplicações IIS

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/iis/sites/{siteName}/applications` | Listar aplicações do site |
| `POST` | `/iis/applications` | Criar nova aplicação |
| `PUT` | `/iis/sites/{siteName}/applications/{appPath}` | Atualizar aplicação |
| `DELETE` | `/iis/sites/{siteName}/applications/{appPath}` | Deletar aplicação |

### Criar Aplicação
```json
{
  "SiteName": "MeuSite",
  "AppPath": "/api",
  "PhysicalPath": "C:\\inetpub\\wwwroot\\meusite\\api",
  "AppPoolName": "DefaultAppPool"
}
```

## 🏊 Application Pools

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/iis/app-pools` | Listar todos os pools |
| `GET` | `/iis/app-pools/{poolName}` | Obter detalhes de um pool |
| `POST` | `/iis/app-pools` | Criar novo pool |
| `PUT` | `/iis/app-pools/{poolName}` | Atualizar pool |
| `DELETE` | `/iis/app-pools/{poolName}` | Deletar pool |
| `POST` | `/iis/app-pools/{poolName}/start` | Iniciar pool |
| `POST` | `/iis/app-pools/{poolName}/stop` | Parar pool |
| `POST` | `/iis/app-pools/{poolName}/recycle` | Reciclar pool |

### Criar Application Pool
```json
{
  "PoolName": "MeuPool",
  "RuntimeVersion": "v4.0",
  "PipelineMode": "Integrated"
}
```

## 🛡️ Permissões

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/iis/admin-status` | Status de administrador |
| `GET` | `/iis/permissions` | Verificar permissões IIS |

## 📊 Códigos de Status

- `200` - OK
- `201` - Created
- `400` - Bad Request (dados inválidos)
- `401` - Unauthorized (token inválido)
- `403` - Forbidden (sem privilégios admin)
- `404` - Not Found
- `409` - Conflict (nome já existe)
- `500` - Internal Server Error

## 🔑 Headers Obrigatórios

```http
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

## 🚀 Exemplo Completo

```bash
# 1. Login
curl -X POST "http://localhost:5092/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password"}'

# 2. Criar Site
curl -X POST "http://localhost:5092/iis/sites" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "SiteName": "TestSite",
    "BindingInformation": "0.0.0.0:8080:",
    "PhysicalPath": "C:\\inetpub\\wwwroot\\testsite",
    "AppPoolName": "DefaultAppPool"
  }'

# 3. Criar Aplicação
curl -X POST "http://localhost:5092/iis/applications" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "SiteName": "TestSite",
    "AppPath": "/api",
    "PhysicalPath": "C:\\inetpub\\wwwroot\\testsite\\api",
    "AppPoolName": "DefaultAppPool"
  }'

# 4. Listar Sites
curl -X GET "http://localhost:5092/iis/sites" \
  -H "Authorization: Bearer {token}"
```

---
*Versão: 1.0 | Data: 03/08/2025*
