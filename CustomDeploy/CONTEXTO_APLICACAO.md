# CustomDeploy - Contexto da Aplicação

## 📋 Visão Geral da Aplicação

A **CustomDeploy** é uma API Web ASP.NET Core 8.0 projetada para automatizar o deploy de aplicações hospedadas no GitHub. A aplicação oferece funcionalidades completas de clonagem de repositórios, build de projetos e publicação automatizada, com foco em segurança e monitoramento.

## 🏗️ Arquitetura e Estrutura

### Tecnologias Principais
- **.NET 8.0** - Framework principal
- **ASP.NET Core Web API** - Plataforma de desenvolvimento
- **JWT Bearer Authentication** - Sistema de autenticação
- **Swagger/OpenAPI** - Documentação da API
- **Git CLI** - Integração com repositórios
- **Process API** - Execução de comandos de build

### Estrutura de Pastas
```
CustomDeploy/
├── Controllers/
│   ├── AuthController.cs      # Autenticação JWT
│   ├── DeployController.cs    # Endpoint principal de deploy
│   ├── PublicationController.cs # Gestão de publicações
│   └── WeatherForecastController.cs # Controller de exemplo
├── Models/
│   ├── DeployMetadata.cs      # Metadados de deploy
│   ├── DeployRequest.cs       # Payload de requisição
│   ├── JwtSettings.cs         # Configurações JWT
│   ├── LoginRequest.cs        # Payload de login
│   ├── LoginResponse.cs       # Resposta de login
│   └── PublicationInfo.cs     # Informações de publicação
├── Services/
│   ├── DeployService.cs       # Lógica principal de deploy
│   └── PublicationService.cs  # Gerenciamento de publicações
├── Program.cs                 # Configuração da aplicação
├── appsettings.json          # Configurações
└── README.md                 # Documentação
```

## 🔐 Sistema de Autenticação

### Configuração JWT
```json
{
  "Jwt": {
    "Issuer": "CustomDeploy",
    "Audience": "CustomDeploy",
    "Key": "MyVerySecretKeyForCustomDeployJwtAuthentication2025!",
    "ExpirationInMinutes": 60
  }
}
```

### Credenciais de Desenvolvimento
- **Username:** `admin`
- **Password:** `password`

### Fluxo de Autenticação
1. **POST /auth/login** - Obtém token JWT
2. **Headers com Bearer Token** - Protege endpoints sensíveis
3. **Expiração configurável** - 60 minutos por padrão

## 🚀 Funcionalidades Principais

### 1. Deploy Automático (`DeployController`)
**Endpoint:** `POST /deploy`

**Funcionalidades:**
- Clonagem ou atualização de repositórios Git
- Execução de comandos de build personalizados
- Cópia de arquivos para diretório de destino
- Validação de segurança de caminhos
- Geração de metadados de deploy

**Payload de Exemplo:**
```json
{
  "repoUrl": "https://github.com/usuario/projeto.git",
  "branch": "main",
  "buildCommand": "npm install && npm run build",
  "buildOutput": "dist",
  "targetPath": "minha-aplicacao"
}
```

### 2. Gerenciamento de Publicações (`PublicationController`)
**Endpoints:**
- `GET /deploy/publications` - Lista todas as publicações
- `GET /deploy/publications/stats` - Estatísticas gerais
- `GET /deploy/publications/{name}` - Detalhes de uma publicação

### 3. Segurança de Caminhos
- Validação anti-path traversal
- Restrição a diretório de publicações configurado
- Resolução de caminhos relativos para absolutos

## ⚙️ Configurações

### DeploySettings
```json
{
  "DeploySettings": {
    "WorkingDirectory": "C:\\teste\\CustomDeploy",
    "PublicationsPath": "C:\\temp\\wwwroot"
  }
}
```

- **WorkingDirectory:** Diretório temporário para clonagem
- **PublicationsPath:** Diretório final de publicação

## 🔧 Serviços Principais

### DeployService
**Responsabilidades:**
- Gerenciar clonagem/atualização de repositórios Git
- Executar comandos de build multiplataforma
- Copiar arquivos compilados para destino
- Validar segurança de caminhos
- Gerar metadados de deploy

**Métodos Principais:**
- `ExecuteDeployAsync()` - Fluxo principal de deploy
- `ValidateAndResolveTargetPath()` - Validação de segurança
- `CloneOrUpdateRepositoryAsync()` - Gestão Git
- `ExecuteBuildCommandAsync()` - Execução de builds
- `SaveDeployMetadataAsync()` - Persistência de metadados

### PublicationService
**Responsabilidades:**
- Listar publicações disponíveis
- Carregar metadados de deploy
- Calcular estatísticas de uso
- Gerenciar informações de diretórios

## 📊 Sistema de Metadados

### Arquivo deploy.json
Cada publicação gera um arquivo `deploy.json` com:
```json
{
  "repository": "https://github.com/usuario/projeto.git",
  "branch": "main",
  "buildCommand": "npm install && npm run build",
  "deployedAt": "2025-08-02T10:30:00Z"
}
```

### Benefícios dos Metadados
- Rastreabilidade de deploys
- Histórico de mudanças
- Informações de origem
- Auditoria de operações

## 🛡️ Recursos de Segurança

### 1. Autenticação JWT
- Tokens com expiração
- Validação de issuer/audience
- Chave secreta configurável

### 2. Validação de Caminhos
- Prevenção de path traversal (../, ..\)
- Restrição a diretório autorizado
- Normalização de caminhos

### 3. Logging de Segurança
- Log de tentativas de acesso inválido
- Monitoramento de operações sensíveis
- Rastreamento de atividades

## 🔄 Fluxo de Deploy

### Processo Completo
1. **Autenticação** - Validação do token JWT
2. **Validação** - Verificação dos parâmetros de entrada
3. **Segurança** - Validação do targetPath
4. **Clonagem** - Git clone ou git pull do repositório
5. **Build** - Execução do comando de build
6. **Cópia** - Transferência dos arquivos compilados
7. **Metadados** - Geração do arquivo deploy.json
8. **Resposta** - Retorno do status da operação

### Tratamento de Erros
- Logs detalhados em cada etapa
- Mensagens de erro informativas
- Rollback automático em caso de falha
- Validação prévia de recursos

## 🐳 Considerações de Deploy

### Ambiente de Desenvolvimento
- Swagger UI habilitado
- CORS permissivo
- Logs verbosos
- Credenciais fixas

### Ambiente de Produção
- HTTPS obrigatório
- Logs otimizados
- Credenciais seguras
- Validações rígidas

## 📋 APIs Disponíveis

### Autenticação
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/auth/login` | Autenticação e obtenção de token |

### Deploy
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/deploy` | Execução de deploy automático |

### Publicações
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/deploy/publications` | Lista todas as publicações |
| GET | `/deploy/publications/stats` | Estatísticas gerais |
| GET | `/deploy/publications/{name}` | Detalhes de uma publicação |

## 🔍 Monitoramento e Logs

### Níveis de Log
- **Information:** Operações normais
- **Warning:** Situações atípicas
- **Error:** Falhas de processo
- **Critical:** Falhas de segurança

### Eventos Monitorados
- Tentativas de autenticação
- Operações de deploy
- Acessos a arquivos
- Validações de segurança

## 🚨 Troubleshooting

### Problemas Comuns

#### 1. Falha de Autenticação
- Verificar credenciais (admin/password)
- Validar configuração JWT
- Confirmar expiração do token

#### 2. Erro de Build
- Verificar comando de build
- Confirmar dependências instaladas
- Verificar permissões de diretório

#### 3. Problemas de Path
- Validar targetPath relativo
- Confirmar PublicationsPath configurado
- Verificar permissões de escrita

#### 4. Falha de Git
- Verificar conectividade de rede
- Confirmar URL do repositório
- Validar permissões de acesso

## 📚 Dependências do Projeto

### NuGet Packages
```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.1.2" />
```

### Dependências do Sistema
- **Git CLI** - Para operações de repositório
- **Node.js/npm** - Para builds de projetos JavaScript
- **.NET CLI** - Para builds de projetos .NET

## 🎯 Casos de Uso

### 1. Deploy de Aplicação React
```json
{
  "repoUrl": "https://github.com/user/react-app.git",
  "branch": "main",
  "buildCommand": "npm install && npm run build",
  "buildOutput": "build",
  "targetPath": "minha-react-app"
}
```

### 2. Deploy de API .NET
```json
{
  "repoUrl": "https://github.com/user/dotnet-api.git",
  "branch": "main",
  "buildCommand": "dotnet publish -c Release -o publish",
  "buildOutput": "publish",
  "targetPath": "minha-api"
}
```

### 3. Deploy de Site Static
```json
{
  "repoUrl": "https://github.com/user/static-site.git",
  "branch": "main",
  "buildCommand": "npm install && npm run generate",
  "buildOutput": "dist",
  "targetPath": "meu-site"
}
```

## 🔮 Extensibilidade

### Pontos de Extensão
- **Novos provedores Git** (GitLab, Bitbucket)
- **Sistemas de build adicionais** (Docker, Maven)
- **Notificações** (Slack, Teams, Email)
- **Webhooks** para deploy automático
- **Interface web** para gerenciamento

### Arquitetura Preparada
- Injeção de dependência configurada
- Interfaces bem definidas
- Logging estruturado
- Configuração externa

---

**Versão do Documento:** 1.0  
**Data de Criação:** Agosto 2025  
**Última Atualização:** Agosto 2025  
**Autor:** Sistema CustomDeploy
