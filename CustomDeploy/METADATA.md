# CustomDeploy API - Metadados de Deploy

## 🎯 Funcionalidade de Metadados

A partir desta versão, o CustomDeploy automaticamente salva metadados de cada deploy realizado e os inclui nas consultas de publicações.

## 📋 Como Funciona

### 1. Durante o Deploy

Quando um deploy é realizado via `POST /deploy`, o sistema:

1. **Clone/Update** do repositório
2. **Execução** do comando de build  
3. **Cópia** dos arquivos para o destino
4. **🆕 Salvamento** de metadados em `deploy.json`

O arquivo `deploy.json` é criado automaticamente na pasta de destino com:

```json
{
  "repository": "https://github.com/user/repo.git",
  "branch": "main", 
  "buildCommand": "npm run build",
  "deployedAt": "2025-08-02T15:32:00Z"
}
```

### 2. Durante as Consultas

Quando você consulta `GET /deploy/publications`, o sistema:

1. **Lista** todos os diretórios de publicação
2. **Verifica** se existe `deploy.json` em cada diretório
3. **Carrega** e inclui os metadados na resposta

## 📊 Resposta Atualizada

### `GET /deploy/publications`

```json
{
  "message": "Publicações listadas com sucesso",
  "count": 3,
  "publications": [
    {
      "name": "carteira",
      "fullPath": "C:\\temp\\wwwroot\\carteira",
      "lastModified": "2025-08-02T16:12:34",
      "sizeMB": 84.3,
      "repository": "https://github.com/LinkedFarma/Gruppy.Solution.Front",
      "branch": "dev",
      "buildCommand": "npm run build:carteira", 
      "deployedAt": "2025-08-02T15:45:00Z"
    },
    {
      "name": "app1",
      "fullPath": "C:\\temp\\wwwroot\\app1",
      "lastModified": "2025-08-02T14:30:00",
      "sizeMB": 12.5,
      "repository": "https://github.com/exemplo/app1.git",
      "branch": "main",
      "buildCommand": "npm run build",
      "deployedAt": "2025-08-02T14:30:00Z"
    },
    {
      "name": "app2",
      "fullPath": "C:\\temp\\wwwroot\\app2", 
      "lastModified": "2025-08-02T13:15:00",
      "sizeMB": 6.8,
      "repository": null,
      "branch": null,
      "buildCommand": null,
      "deployedAt": null
    }
  ]
}
```

### Campos de Metadados

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `repository` | string? | URL do repositório Git |
| `branch` | string? | Branch utilizada no deploy |
| `buildCommand` | string? | Comando de build executado |
| `deployedAt` | DateTime? | Data/hora do deploy em UTC |

> **Nota:** Se o arquivo `deploy.json` não existir, os campos de metadados serão `null`.

## 🔧 Estrutura de Arquivos

Após um deploy bem-sucedido:

```
C:\inetpub\wwwroot\MinhaApp\
├── index.html          ← Arquivos da aplicação
├── assets/
│   ├── main.js
│   └── style.css
└── deploy.json         ← 🆕 Metadados do deploy
```

## ⚡ Benefícios

1. **📊 Rastreabilidade** - Saiba de onde veio cada aplicação
2. **🔄 Versionamento** - Controle de branches e commits
3. **📅 Histórico** - Quando cada aplicação foi deployada
4. **🛠️ Reprodução** - Como reconstruir a aplicação

## 🚨 Tratamento de Erros

- ✅ Se `deploy.json` não existir → metadados ficam `null`
- ✅ Se `deploy.json` estiver corrompido → logga warning, metadados ficam `null`
- ✅ Se não houver permissão de leitura → logga warning, metadados ficam `null`
- ✅ A listagem de publicações **nunca falha** por causa dos metadados

## 🧪 Exemplo de Teste Completo

### 1. Fazer Deploy
```http
POST /deploy
Authorization: Bearer <token>
{
  "repoUrl": "https://github.com/user/myapp.git",
  "branch": "production",
  "buildCommand": "npm install && npm run build",
  "buildOutput": "dist",
  "targetPath": "C:\\inetpub\\wwwroot\\myapp"
}
```

### 2. Verificar Metadados
```http
GET /deploy/publications/myapp
Authorization: Bearer <token>
```

**Resposta:**
```json
{
  "message": "Publicação encontrada",
  "publication": {
    "name": "myapp",
    "fullPath": "C:\\inetpub\\wwwroot\\myapp",
    "lastModified": "2025-08-02T16:20:00", 
    "sizeMB": 15.2,
    "repository": "https://github.com/user/myapp.git",
    "branch": "production",
    "buildCommand": "npm install && npm run build",
    "deployedAt": "2025-08-02T16:19:45Z"
  }
}
```

## 🎉 **Agora você tem rastreabilidade completa dos seus deploys!** 🚀
