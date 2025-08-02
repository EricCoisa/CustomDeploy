# CustomDeploy API - Documentação

## 🚀 Visão Geral

A **CustomDeploy API** é uma solução para deploy automático de aplicações hospedadas no GitHub. A API clona repositórios, executa comandos de build e copia os arquivos gerados para um diretório de destino.

## 🔐 Autenticação

A API utiliza **JWT (JSON Web Token)** para autenticação.

### Login
**Endpoint:** `POST /auth/login`

**Credenciais fixas para desenvolvimento:**
- **Username:** `admin`
- **Password:** `password`

**Request:**
```json
{
  "username": "admin",
  "password": "password"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2025-08-02T15:30:00Z"
}
```

## 🎯 Deploy

**Endpoint:** `POST /deploy`

**Headers:**
```
Authorization: Bearer <seu-token-jwt>
Content-Type: application/json
```

**Request Body:**
```json
{
  "repoUrl": "https://github.com/usuario/repositorio.git",
  "branch": "main",
  "buildCommand": "npm install && npm run build",
  "buildOutput": "dist",
  "targetPath": "minha-aplicacao"
}
```

### Parâmetros

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `repoUrl` | string | URL do repositório Git (HTTPS) |
| `branch` | string | Branch a ser clonada/atualizada |
| `buildCommand` | string | Comando para executar o build |
| `buildOutput` | string | Pasta onde ficam os arquivos após o build |
| `targetPath` | string | **Caminho relativo** dentro do diretório de publicações |

### ⚠️ **Importante: TargetPath**

O `targetPath` deve ser **sempre relativo** ao diretório de publicações configurado. Por exemplo:

✅ **Válidos:**
- `"minha-app"` → `{PublicationsPath}/minha-app`
- `"projetos/frontend"` → `{PublicationsPath}/projetos/frontend`
- `"v2/sistema"` → `{PublicationsPath}/v2/sistema`

❌ **Inválidos:**
- `"C:\\Windows\\System32"` (caminho absoluto)
- `"../../../etc/passwd"` (path traversal)
- `"/var/www/html"` (caminho absoluto Unix)
- `"..\\..\\malware"` (tentativa de escape)

### Exemplos de buildCommand

**Node.js:**
```bash
npm install && npm run build
```

**React:**
```bash
yarn install && yarn build
```

**.NET:**
```bash
dotnet publish -c Release -o ./publish
```

**Vue.js:**
```bash
npm ci && npm run build
```

## 📋 Processo de Deploy

1. **Clone/Update:** A API clona o repositório (se não existir) ou faz `git pull` na branch especificada
2. **Build:** Executa o comando de build no diretório do repositório
3. **Deploy:** Limpa o diretório de destino e copia os arquivos do `buildOutput`

## ⚙️ Configurações

As configurações ficam no `appsettings.json`:

```json
{
  "DeploySettings": {
    "WorkingDirectory": "C:\\temp\\CustomDeploy"
  },
  "Jwt": {
    "Issuer": "CustomDeploy",
    "Audience": "CustomDeploy", 
    "Key": "MyVerySecretKeyForCustomDeployJwtAuthentication2025!",
    "ExpirationInMinutes": 60
  }
}
```

## 📝 Logs

A aplicação registra logs detalhados de todas as operações:
- Informações de clone/pull do Git
- Output dos comandos de build
- Operações de cópia de arquivos
- Erros e exceções

## 🔧 Pré-requisitos

- **Git** instalado e acessível via linha de comando
- **Node.js** (se for fazer build de projetos JavaScript)
- **.NET SDK** (se for fazer build de projetos .NET)
- Permissões de escrita no diretório de destino

## 🚨 Segurança

⚠️ **IMPORTANTE:** Esta versão é para desenvolvimento. Para produção:

1. Substituir credenciais fixas por autenticação real
2. Validar e sanitizar caminhos de arquivo
3. Implementar rate limiting
4. Usar HTTPS
5. Validar URLs de repositório
6. Implementar logs de auditoria

## 📊 Códigos de Resposta

| Código | Descrição |
|--------|-----------|
| 200 | Deploy realizado com sucesso |
| 400 | Dados de entrada inválidos |
| 401 | Token JWT inválido ou expirado |
| 500 | Erro interno (Git, build ou cópia) |

## 🎯 Exemplo Completo

```bash
# 1. Login
curl -X POST http://localhost:5092/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password"}'

# 2. Deploy (use o token da resposta anterior)
curl -X POST http://localhost:5092/deploy \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -d '{
    "repoUrl": "https://github.com/octocat/Hello-World.git",
    "branch": "master", 
    "buildCommand": "echo Build && mkdir dist && echo Hello > dist/index.txt",
    "buildOutput": "dist",
    "targetPath": "hello-world"
  }'
```
