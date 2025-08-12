# 🗃️ Entity Framework + SQLite - Documentação da Nova Estrutura

## 📋 Resumo da Implementação

A aplicação CustomDeploy foi reestruturada para usar **Entity Framework Core com SQLite**, oferecendo uma solução de persistência robusta e embutida, sem necessidade de instalação de banco de dados externo.

## 🏗️ Estrutura de Dados Implementada

### 📊 Entidades Criadas

#### 1. **Usuario** (`Models/Entities/Usuario.cs`)
- **Id** (int) - Chave primária
- **Nome** (string, max 100) - Nome do usuário
- **Email** (string, max 200) - Email único do usuário
- **Senha** (string, max 255) - Hash da senha
- **Ativo** (bool) - Status do usuário (padrão: true)
- **CriadoEm/AtualizadoEm** - Timestamps automáticos

#### 2. **AcessoNivel** (`Models/Entities/AcessoNivel.cs`)
- **Id** (int) - Chave primária
- **Nome** (string, max 50) - Nome do nível de acesso
- Valores pré-definidos:
  - `1: Administrador`
  - `2: Operador`

#### 3. **UsuarioAcesso** (`Models/Entities/UsuarioAcesso.cs`)
- **Id** (int) - Chave primária
- **UsuarioId** (FK) - Referência ao usuário
- **AcessoNivelId** (FK) - Referência ao nível de acesso
- Relacionamento 1:1 entre Usuario e UsuarioAcesso

#### 4. **Deploy** (`Models/Entities/Deploy.cs`)
- **Id** (int) - Chave primária
- **SiteName** (string, max 200) - Nome do site IIS
- **ApplicationName** (string, max 200, opcional) - Nome da aplicação
- **Data** (datetime) - Data/hora do deploy
- **UsuarioId** (FK) - Usuário que executou o deploy
- **Status** (string, max 50) - Status do deploy
- **Mensagem** (string, max 2000, opcional) - Log/retorno
- **Plataforma** (string, max 100, opcional) - Plataforma utilizada

#### 5. **DeployComando** (`Models/Entities/DeployComando.cs`)
- **Id** (int) - Chave primária
- **DeployId** (FK) - Referência ao deploy
- **Comando** (string, max 1000) - Comando executado
- **Ordem** (int) - Ordem de execução

#### 6. **DeployHistorico** (`Models/Entities/DeployHistorico.cs`)
- **Id** (int) - Chave primária
- **DeployId** (FK) - Referência ao deploy
- **Data** (datetime) - Data/hora do evento
- **Status** (string, max 50) - Status do evento
- **Mensagem** (string, max 2000, opcional) - Mensagem do evento

## 🛠️ Arquitetura Implementada

### 📁 Estrutura de Diretórios
```
Data/
├── CustomDeployDbContext.cs          # DbContext principal
├── Configurations/                   # Configurações EF
│   ├── UsuarioConfiguration.cs
│   ├── AcessoNivelConfiguration.cs
│   ├── UsuarioAcessoConfiguration.cs
│   ├── DeployConfiguration.cs
│   ├── DeployComandoConfiguration.cs
│   └── DeployHistoricoConfiguration.cs
└── Repositories/                     # Repositórios
    ├── IRepository.cs                # Interface genérica
    ├── Repository.cs                 # Implementação base
    ├── IUsuarioRepository.cs
    ├── UsuarioRepository.cs
    ├── IDeployRepository.cs
    ├── DeployRepository.cs
    ├── IAcessoNivelRepository.cs
    └── AcessoNivelRepository.cs

Services/Business/                    # Serviços de negócio
├── IUsuarioBusinessService.cs
├── UsuarioBusinessService.cs
├── IDeployBusinessService.cs
└── DeployBusinessService.cs

Models/DTOs/                         # Data Transfer Objects
├── UsuarioDTOs.cs
└── DeployDTOs.cs

Controllers/                         # Controllers da API
├── UsuariosController.cs
├── DeploysController.cs
└── AuthController.cs (atualizado)
```

### 🔧 Configurações

#### **Entity Framework + SQLite**
- **String de Conexão**: `"Data Source=customdeploy.db;Cache=Shared"`
- **Provider**: Microsoft.EntityFrameworkCore.Sqlite
- **Migrations**: Configuradas e prontas para uso

#### **Dependency Injection** (`Program.cs`)
```csharp
// Entity Framework
builder.Services.AddDbContext<CustomDeployDbContext>(options =>
    options.UseSqlite(connectionString));

// Repositórios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAcessoNivelRepository, AcessoNivelRepository>();
builder.Services.AddScoped<IDeployRepository, DeployRepository>();

// Serviços de Negócio
builder.Services.AddScoped<IUsuarioBusinessService, UsuarioBusinessService>();
builder.Services.AddScoped<IDeployBusinessService, DeployBusinessService>();
```

## 🚀 Como Usar

### 1. **Inicialização do Banco**
O banco é criado automaticamente na primeira execução com dados de seed:
- **Usuário Admin**: `admin@customdeploy.com` / `admin123`
- **Níveis de Acesso**: Administrador e Operador

### 2. **Autenticação Atualizada**
```http
POST /auth/login
Content-Type: application/json

{
  "username": "admin@customdeploy.com",
  "password": "admin123"
}
```

### 3. **Gerenciamento de Usuários**

#### Criar Usuário
```http
POST /api/usuarios
Authorization: Bearer {token}
Content-Type: application/json

{
  "nome": "João Silva",
  "email": "joao@empresa.com",
  "senha": "senha123",
  "acessoNivelId": 2
}
```

#### Listar Usuários
```http
GET /api/usuarios
Authorization: Bearer {token}
```

#### Obter Usuário por ID
```http
GET /api/usuarios/{id}
Authorization: Bearer {token}
```

#### Atualizar Usuário
```http
PUT /api/usuarios/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "nome": "João Santos",
  "ativo": true
}
```

### 4. **Gerenciamento de Deploys**

#### Criar Deploy
```http
POST /api/deploys
Authorization: Bearer {token}
Content-Type: application/json

{
  "siteName": "MeuSite",
  "applicationName": "MinhaApp",
  "comandos": [
    "git clone https://github.com/user/repo.git",
    "npm install",
    "npm run build"
  ],
  "plataforma": "Node.js"
}
```

#### Listar Deploys
```http
GET /api/deploys
Authorization: Bearer {token}
```

#### Obter Deploy Completo
```http
GET /api/deploys/{id}
Authorization: Bearer {token}
```

#### Atualizar Status do Deploy
```http
PUT /api/deploys/{id}/status
Authorization: Bearer {token}
Content-Type: application/json

{
  "status": "Sucesso",
  "mensagem": "Deploy realizado com sucesso"
}
```

## 🔐 Controle de Permissões

### **Administrador**
- ✅ Criar, editar, excluir usuários
- ✅ Visualizar todos os deploys
- ✅ Gerenciar sistema completo

### **Operador**
- ✅ Criar deploys
- ✅ Visualizar próprios deploys
- ✅ Atualizar status dos próprios deploys
- ❌ Gerenciar usuários

## 📊 Recursos Implementados

### **Auditoria Completa**
- Timestamps automáticos (CriadoEm/AtualizadoEm)
- Histórico detalhado de deploys
- Rastreamento de comandos executados
- Log de usuários responsáveis

### **Segurança**
- Hash SHA256 para senhas
- JWT com roles (Administrador/Operador)
- Validação de permissões por endpoint
- Prevenção de acesso não autorizado

### **Performance**
- Lazy loading configurado
- Índices otimizados
- Queries eficientes com Include()
- Cache compartilhado SQLite

## 🗄️ Comandos Entity Framework

### **Migrations**
```bash
# Criar nova migration
dotnet ef migrations add NomeDaMigration

# Aplicar migrations
dotnet ef database update

# Remover última migration
dotnet ef migrations remove

# Ver histórico
dotnet ef migrations list
```

### **Banco de Dados**
```bash
# Recriar banco
dotnet ef database drop
dotnet ef database update

# Scripts SQL
dotnet ef migrations script
```

## 📝 Próximos Passos

1. **Integração com o Frontend**
   - Atualizar chamadas da API
   - Implementar telas de usuários
   - Dashboard de deploys

2. **Melhorias de Funcionalidade**
   - Filtros avançados de deploy
   - Relatórios de auditoria
   - Notificações de status

3. **Monitoramento**
   - Logs estruturados
   - Métricas de performance
   - Health checks

---

🎉 **A estrutura está completa e pronta para uso!** O banco SQLite será criado automaticamente na primeira execução, e o usuário administrador estará disponível para começar a usar o sistema.
