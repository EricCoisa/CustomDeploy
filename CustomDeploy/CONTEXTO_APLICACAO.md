# CustomDeploy - Contexto da Aplicação

## 📋 Visão Geral da Aplicação

A **CustomDeploy** é uma API Web ASP.NET Core 8.0 projetada para automatizar o deploy de aplicações hospedadas no GitHub com **integração nativa ao IIS (Internet Information Services)**. A aplicação oferece funcionalidades completas de clonagem de repositórios, build de projetos e publicação automatizada, utilizando o **IIS como fonte de verdade** para descoberta de sites e aplicações, com foco em segurança e monitoramento.

## 🏗️ Arquitetura e Estrutura

### Tecnologias Principais
- **.NET 8.0** - Framework principal
- **ASP.NET Core Web API** - Plataforma de desenvolvimento
- **JWT Bearer Authentication** - Sistema de autenticação
- **Swagger/OpenAPI** - Documentação da API
- **Git CLI** - Integração com repositórios
- **Process API** - Execução de comandos de build
- **IIS Management** - Integração nativa com Internet Information Services
- **PowerShell Integration** - Execução de comandos IIS via PowerShell
- **GitHub API Integration** - Validação de repositórios e autenticação explícita

### Estrutura de Pastas
```
CustomDeploy/
├── Controllers/
│   ├── AuthController.cs      # Autenticação JWT
│   ├── DeployController.cs    # Endpoint principal de deploy
│   ├── GitHubController.cs    # Validação e testes GitHub
│   ├── PublicationController.cs # Gestão de publicações (IIS-based)
│   └── WeatherForecastController.cs # Controller de exemplo
├── Models/
│   ├── DeployMetadata.cs      # Metadados de deploy
│   ├── DeployRequest.cs       # Payload de requisição
│   ├── CreateMetadataRequest.cs # Payload para criação de metadados
│   ├── UpdateMetadataRequest.cs # Payload para atualização de metadados
│   ├── GitHubSettings.cs      # Configurações de autenticação GitHub
│   ├── IISSiteModels.cs       # Modelos para sites e aplicações IIS
│   ├── IISBasedPublication.cs # Modelo de publicação baseada em IIS
│   ├── JwtSettings.cs         # Configurações JWT
│   ├── LoginRequest.cs        # Payload de login
│   ├── LoginResponse.cs       # Resposta de login
│   └── PublicationInfo.cs     # Informações de publicação (legacy)
├── Services/
│   ├── DeployService.cs       # Lógica principal de deploy
│   ├── GitHubService.cs       # Integração com GitHub API
│   ├── IISManagementService.cs # Gerenciamento do IIS
│   └── PublicationService.cs  # Gerenciamento de publicações (IIS-based)
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
- **Integração automática com IIS** (sites e aplicações)
- **Suporte à sintaxe "site/aplicacao"** para targeting específico
- Validação de segurança de caminhos
- Geração de metadados de deploy

**Payload de Exemplo:**
```json
{
  "repoUrl": "https://github.com/usuario/projeto.git",
  "branch": "main",
  "buildCommand": "npm install && npm run build",
  "buildOutput": "dist",
  "iisSiteName": "meusite",
  "targetPath": "api"
}
```

**Exemplos de Targeting IIS:**
- `iisSiteName: "Default Web Site"` - Deploy no site raiz
- `iisSiteName: "meusite", targetPath: "api"` - Deploy na aplicação /api do site
- Deploy automático detecta aplicações IIS existentes

### 2. Gerenciamento de Publicações (`PublicationController`)
**Arquitetura IIS-First:** O sistema agora usa o IIS como fonte primária de informação, descobrindo automaticamente sites e aplicações.

**Endpoints:**
- `GET /deploy/publications` - Lista todas as publicações **baseadas no IIS**
- `GET /deploy/publications/stats` - Estatísticas detalhadas (sites vs aplicações)
- `GET /deploy/publications/{name}` - Detalhes de uma publicação específica
- `DELETE /deploy/publications/{name}` - Remove publicação completa (metadados + pasta)
- `DELETE /deploy/publications/{name}/metadata-only` - Remove apenas metadados
- `GET /deploy/publications/{name}/metadata` - Obtém metadados específicos
- `PATCH /deploy/publications/{name}/metadata` - Atualiza metadados específicos
- `POST /deploy/publications/metadata` - **Cria metadados sem executar deploy**

**Recursos Avançados:**
- **Suporte a nomes com barras:** `carteira/api`, `gruppy/carteiras`
- **Correlação automática:** Vincula metadados de deploy com estrutura IIS
- **Detecção inteligente:** Diferencia sites raiz de aplicações IIS
- **Status em tempo real:** Verifica existência física de arquivos

### 3. **Integração IIS Nativa**
**Novo:** Sistema completo de integração com Internet Information Services

**Funcionalidades:**
- **Descoberta automática de sites:** Lista todos os sites configurados no IIS
- **Detecção de aplicações:** Identifica aplicações dentro de cada site
- **Targeting inteligente:** Deploy automático para caminhos físicos corretos
- **Validação IIS:** Verifica existência de sites/aplicações antes do deploy
- **Suporte a aplicações virtuais:** Gerencia aplicações IIS como `/api`, `/admin`

**Comandos IIS Suportados:**
- `Get-IISSite` - Lista sites disponíveis
- `Get-WebApplication` - Lista aplicações de um site
- Resolução automática de caminhos físicos

### 4. Segurança de Caminhos
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
  },
  "GitHubSettings": {
    "Username": "",
    "PersonalAccessToken": "",
    "UseSystemCredentials": true,
    "GitTimeoutSeconds": 300,
    "ApiBaseUrl": "https://api.github.com"
  }
}
```

- **WorkingDirectory:** Diretório temporário para clonagem
- **PublicationsPath:** Diretório final de publicação *(Obsoleto - agora usa caminhos IIS)*

**Configurações GitHub:**
- **Username:** Nome de usuário GitHub
- **PersonalAccessToken:** Token de acesso pessoal GitHub
- **UseSystemCredentials:** Se deve usar credenciais do sistema como fallback
- **GitTimeoutSeconds:** Timeout para operações Git
- **ApiBaseUrl:** URL base da API GitHub (para GitHub Enterprise)

**Nota:** Com a integração IIS, os caminhos de destino são automaticamente resolvidos baseados na configuração dos sites no IIS, tornando o `PublicationsPath` menos relevante.

## 🔧 Serviços Principais

### DeployService
**Responsabilidades:**
- Gerenciar clonagem/atualização de repositórios Git
- Executar comandos de build multiplataforma
- Copiar arquivos compilados para destino
- **Integração com IIS para resolução de caminhos**
- Validar segurança de caminhos
- Gerar metadados de deploy centralizados

**Métodos Principais:**
- `ExecuteDeployAsync()` - Fluxo principal de deploy
- `ValidateAndResolveTargetPath()` - Validação de segurança
- `CloneOrUpdateRepositoryAsync()` - Gestão Git
- `ExecuteBuildCommandAsync()` - Execução de builds
- `CreateMetadataOnly()` - **Novo:** Cria metadados sem deploy
- `UpdateDeployMetadata()` - Atualiza metadados existentes
- `DeletePublicationCompletely()` - Remove publicação e metadados

### IISManagementService
**Novo serviço** para integração completa com IIS

**Responsabilidades:**
- Descobrir sites configurados no IIS
- Listar aplicações de cada site
- Resolver caminhos físicos automaticamente
- Validar existência de sites/aplicações
- Executar comandos PowerShell para IIS

**Métodos Principais:**
- `GetAllSitesAsync()` - Lista todos os sites IIS
- `GetSiteApplicationsAsync()` - Lista aplicações de um site específico
- `ExecutePowerShellCommand()` - Execução segura de comandos PS

### GitHubService
**Novo serviço** para autenticação e validação GitHub

**Responsabilidades:**
- Validar repositórios GitHub via API
- Verificar existência de branches
- Gerar URLs autenticadas para clonagem
- Testar conectividade com GitHub
- Gerenciar credenciais de acesso

**Métodos Principais:**
- `ValidateRepositoryAsync()` - Valida acesso ao repositório
- `ValidateBranchAsync()` - Verifica existência de branch
- `GenerateAuthenticatedCloneUrl()` - Gera URL com credenciais
- `TestGitHubConnectivityAsync()` - Testa conectividade

### PublicationService
**Redesenhado** para usar IIS como fonte de verdade

**Responsabilidades:**
- **Descobrir publicações via IIS** (não mais via sistema de arquivos)
- Correlacionar metadados de deploy com estrutura IIS
- Calcular estatísticas baseadas em dados reais
- Gerenciar informações de sites e aplicações
- Suporte a nomes compostos (site/aplicacao)

**Métodos Principais:**
- `GetPublicationsAsync()` - Lista publicações baseadas no IIS
- `GetPublicationByNameAsync()` - Busca específica por nome
- `CreateMetadataLookup()` - Correlação inteligente de metadados

## 📊 Sistema de Metadados

### Arquivo deploys.json (Centralizado)
**Mudança importante:** Agora usa um arquivo centralizado `deploys.json` em vez de arquivos individuais por publicação.

Estrutura do arquivo centralizado:
```json
[
  {
    "name": "campanha",
    "repository": "https://github.com/usuario/campanha.git",
    "branch": "main", 
    "buildCommand": "npm install && npm run build",
    "targetPath": "C:\\inetpub\\wwwroot\\campanha",
    "deployedAt": "2025-08-03T10:30:00Z",
    "exists": true
  },
  {
    "name": "carteira/api",
    "repository": "https://github.com/usuario/carteira-api.git",
    "branch": "main",
    "buildCommand": "dotnet publish -c Release",
    "targetPath": "C:\\inetpub\\wwwroot\\carteira\\api",
    "deployedAt": "0001-01-01T00:00:00",
    "exists": false
  }
]
```

### Benefícios dos Metadados Centralizados
- **Gestão unificada:** Todos os metadados em um local
- **Correlação IIS:** Vinculação automática com sites/aplicações
- **Busca eficiente:** Queries rápidas por nome ou caminho
- **Suporte a aplicações:** Nomes como "site/aplicacao"
- **Status em tempo real:** Campo `exists` dinâmico
- **Deployment tracking:** Campo `deployedAt` para histórico

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

### Processo Completo (Atualizado)
1. **Autenticação** - Validação do token JWT
2. **Validação** - Verificação dos parâmetros de entrada
3. **Descoberta IIS** - Validação do site/aplicação no IIS
4. **Resolução de Caminho** - Determinação automática do caminho físico
5. **Segurança** - Validação do targetPath resolvido
6. **Clonagem** - Git clone ou git pull do repositório
7. **Build** - Execução do comando de build
8. **Cópia** - Transferência dos arquivos compilados
9. **Metadados** - Atualização do arquivo `deploys.json` centralizado
10. **Resposta** - Retorno do status da operação com detalhes IIS

### Fluxo de Criação de Metadados (Novo)
**Endpoint:** `POST /deploy/publications/metadata`

1. **Validação de entrada** - Campos obrigatórios
2. **Verificação IIS** - Confirmação de site existente
3. **Resolução de aplicação** - Detecção de aplicação IIS (se aplicável)
4. **Construção de caminho** - Determinação do targetPath final
5. **Verificação de duplicatas** - Por nome e caminho
6. **Persistência** - Adição ao `deploys.json` centralizado
7. **Resposta** - Confirmação com detalhes completos

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
| POST | `/deploy` | Execução de deploy automático com IIS |

### Publicações (IIS-Based)
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/deploy/publications` | Lista todas as publicações baseadas no IIS |
| GET | `/deploy/publications/stats` | Estatísticas detalhadas (sites vs apps) |
| GET | `/deploy/publications/{name}` | Detalhes de publicação (suporte a "site/app") |
| DELETE | `/deploy/publications/{name}` | Remove publicação completa |
| DELETE | `/deploy/publications/{name}/metadata-only` | Remove apenas metadados |

### Gerenciamento de Metadados (Novo)
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/deploy/publications/{name}/metadata` | Obtém metadados específicos |
| PATCH | `/deploy/publications/{name}/metadata` | Atualiza metadados (repo, branch, build) |
| **POST** | **`/deploy/publications/metadata`** | **Cria metadados sem executar deploy** |

### GitHub Validation & Security (Novo)
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/github/test-connectivity` | Testa conectividade com GitHub API |
| POST | `/github/validate-repository` | Valida acesso a repositório específico |
| POST | `/github/validate-branch` | Valida existência de branch |

### IIS Management (Interno)
Endpoints internos para descoberta de estrutura IIS:
- Sites discovery via `Get-IISSite`
- Applications discovery via `Get-WebApplication`
- Physical path resolution automática

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

### 1. Deploy de Aplicação React para IIS
```json
{
  "repoUrl": "https://github.com/user/react-app.git",
  "branch": "main",
  "buildCommand": "npm install && npm run build",
  "buildOutput": "build",
  "iisSiteName": "Default Web Site",
  "targetPath": "react-app"
}
```

### 2. Deploy de API .NET para Aplicação IIS
```json
{
  "repoUrl": "https://github.com/user/dotnet-api.git", 
  "branch": "main",
  "buildCommand": "dotnet publish -c Release -o publish",
  "buildOutput": "publish",
  "iisSiteName": "carteira",
  "targetPath": "api"
}
```
**Resultado:** Deploy para aplicação IIS `/api` do site `carteira`

### 3. Deploy de Site Static para Site Raiz
```json
{
  "repoUrl": "https://github.com/user/static-site.git",
  "branch": "main", 
  "buildCommand": "npm install && npm run generate",
  "buildOutput": "dist",
  "iisSiteName": "meusite"
}
```
**Resultado:** Deploy direto para a raiz do site IIS

### 4. Criação de Metadados sem Deploy
```json
POST /deploy/publications/metadata
{
  "iisSiteName": "campanha",
  "subPath": "admin",
  "repoUrl": "https://github.com/user/admin-panel.git",
  "branch": "develop",
  "buildCommand": "npm run build:prod",
  "buildOutput": "dist"
}
```
**Resultado:** Metadados criados para `campanha/admin` sem executar deploy

## 🔮 Extensibilidade

### Pontos de Extensão
- **Novos provedores Git** (GitLab, Bitbucket)
- **Sistemas de build adicionais** (Docker, Maven)
- **Integração com outros servidores web** (Apache, Nginx)
- **Notificações** (Slack, Teams, Email)
- **Webhooks** para deploy automático
- **Interface web** para gerenciamento
- **Backup automático** antes de deploys
- **Rollback inteligente** para versões anteriores

### Arquitetura Preparada
- Injeção de dependência configurada
- Interfaces bem definidas para IIS
- Logging estruturado
- Configuração externa
- **Serviços modulares** (IIS, Deploy, Publication)
- **Metadados centralizados** para extensibilidade

## 🆕 Principais Mudanças e Melhorias

### Versão Atual (Agosto 2025)

#### 🎯 **Integração IIS Nativa**
- **IIS como fonte de verdade:** Sistema completamente redesenhado
- **Descoberta automática:** Sites e aplicações detectados via PowerShell
- **Targeting inteligente:** Resolução automática de caminhos físicos
- **Suporte a aplicações virtuais:** Deploy em `/api`, `/admin`, etc.

#### 📊 **Sistema de Metadados Centralizado**
- **Arquivo único:** `deploys.json` substitui arquivos individuais
- **Busca eficiente:** Queries por nome ou caminho
- **Correlação automática:** Vincula metadados com estrutura IIS
- **Status dinâmico:** Campo `exists` atualizado em tempo real

#### 🔧 **Novos Endpoints de Gestão**
- **CreateMetadados:** Cria metadados sem executar deploy
- **UpdateMetadata:** Atualiza repo, branch, buildCommand
- **DeleteMetadataOnly:** Remove apenas metadados
- **Estatísticas avançadas:** Sites vs aplicações, com/sem metadados

#### 🔐 **Autenticação GitHub Explícita**
- **GitHubService:** Serviço dedicado para integração GitHub API
- **Validação de repositórios:** Verifica acesso antes do deploy
- **URL autenticada:** Gera URLs com credenciais embutidas
- **Fallback inteligente:** Usa credenciais sistema se não configurado
- **Segurança aprimorada:** Controle explícito de acesso a repos privados

#### 🏗️ **Arquitetura Modular**
- **IISManagementService:** Serviço dedicado para IIS
- **PublicationService redesenhado:** Baseado em dados IIS
- **DeployService estendido:** Novos métodos para gestão de metadados

---

**Versão do Documento:** 2.0  
**Data de Criação:** Agosto 2025  
**Última Atualização:** Agosto 2025  
**Autor:** Sistema CustomDeploy  
**Principais Mudanças:** Integração IIS nativa, metadados centralizados, endpoint CreateMetadados
