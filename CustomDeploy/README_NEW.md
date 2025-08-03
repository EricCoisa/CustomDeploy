````markdown
# CustomDeploy API - Documentação

## 🚀 Visão Geral

A **CustomDeploy API** é uma solução completa para deploy automático de aplicações hospedadas no GitHub com **integração nativa ao IIS**. A API oferece clonagem de repositórios, build automatizado, publicação inteligente e gerenciamento centralizado de metadados.

### 🎯 **Principais Funcionalidades**
- ✅ **Deploy automatizado** com clonagem Git, build e publicação
- ✅ **Integração IIS nativa** - sites e aplicações como alvos de deploy
- ✅ **Metadados centralizados** em arquivo único `deploys.json`
- ✅ **Autenticação GitHub** com validação prévia de repositórios
- ✅ **Segurança robusta** com validação de caminhos anti-path traversal
- ✅ **Gerenciamento completo** com CRUD de publicações e metadados
- ✅ **Criação automática de metadados** para projetos órfãos

> 📖 **Para documentação completa e detalhada, consulte:** [`CONTEXTO_APLICACAO.md`](./CONTEXTO_APLICACAO.md)

## 🔐 Autenticação Rápida

**Endpoint:** `POST /auth/login`

**Credenciais de desenvolvimento:**
```json
{
  "username": "admin",
  "password": "password"
}
```

## 🎯 Deploy Básico

**Endpoint:** `POST /deploy`

**Request Body:**
```json
{
  "repoUrl": "https://github.com/usuario/repositorio.git",
  "branch": "main",
  "buildCommand": "npm install && npm run build",
  "buildOutput": "dist",
  "iisSiteName": "meusite",
  "targetPath": "api"
}
```

**Headers:**
```
Authorization: Bearer <seu-token-jwt>
Content-Type: application/json
```

## 📋 APIs Principais

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| **POST** | `/auth/login` | Autenticação JWT |
| **POST** | `/deploy` | Deploy automático com IIS |
| **GET** | `/deploy/publications` | Lista publicações (IIS-based) |
| **POST** | `/deploy/publications/metadata` | Cria metadados sem deploy |
| **DELETE** | `/deploy/publications/{name}` | Remove publicação completa |
| **GET** | `/github/test-connectivity` | Testa conectividade GitHub |
| **GET** | `/iis/sites` | Lista sites IIS |

## 🎯 Exemplos de Uso

### Deploy para Site Raiz
```json
{
  "repoUrl": "https://github.com/user/frontend.git",
  "branch": "main",
  "buildCommand": "npm run build",
  "buildOutput": "dist",
  "iisSiteName": "Default Web Site"
}
```

### Deploy para Aplicação IIS
```json
{
  "repoUrl": "https://github.com/user/api.git",
  "branch": "main",
  "buildCommand": "dotnet publish -c Release",
  "buildOutput": "bin/Release/net8.0/publish",
  "iisSiteName": "carteira",
  "targetPath": "api"
}
```

## 🔧 Configuração

### appsettings.json
```json
{
  "DeploySettings": {
    "WorkingDirectory": "C:\\temp\\CustomDeploy",
    "PublicationsPath": "C:\\temp\\wwwroot"
  },
  "GitHubSettings": {
    "Username": "seu-usuario",
    "PersonalAccessToken": "seu-token",
    "UseSystemCredentials": true
  },
  "Jwt": {
    "Key": "sua-chave-secreta",
    "ExpirationInMinutes": 60
  }
}
```

## 🛡️ Segurança

- **JWT Authentication** - Todos os endpoints protegidos
- **Path Validation** - Anti-path traversal robusto
- **GitHub Validation** - Verificação prévia de repositórios
- **IIS Integration** - Validação de sites e aplicações
- **Administrative Privileges** - Verificação de permissões

## 🚀 Início Rápido

1. **Configure** o `appsettings.json` com suas credenciais
2. **Execute** a aplicação: `dotnet run`
3. **Acesse** Swagger UI: `https://localhost:7071/swagger`
4. **Faça login** com `admin/password`
5. **Execute** seu primeiro deploy!

## 📖 Documentação Completa

Para informações detalhadas sobre arquitetura, funcionalidades avançadas, segurança, troubleshooting e casos de uso, consulte:

### 📄 [`CONTEXTO_APLICACAO.md`](./CONTEXTO_APLICACAO.md)

Este documento contém:
- 🏗️ Arquitetura detalhada e estrutura de serviços
- 🔧 Configurações avançadas e parâmetros
- 🛡️ Medidas de segurança e validações
- 📊 Sistema de metadados centralizados
- 🔄 Fluxos de operação completos
- 🎯 Casos de uso práticos
- 🚨 Troubleshooting e soluções
- 🔮 Pontos de extensão e melhorias futuras

---

**Versão:** 3.0 | **Framework:** .NET 8.0 | **Licença:** MIT
````
