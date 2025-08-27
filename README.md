# 🚀 CustomDeploy

Automatize deploys de projetos do GitHub direto para o IIS, com segurança, praticidade e interface moderna.

## Visão Geral

CustomDeploy é uma solução completa para automação de deploys de aplicações web e APIs hospedadas no IIS, integrando backend robusto em .NET 8 e frontend moderno em React + TypeScript. O sistema permite gerenciar publicações, autenticação, permissões centralizada.

## Principais Recursos

- Deploy automático: clone, build e publicação em poucos passos
- Integração nativa com IIS (sites e aplicações)
- Interface web intuitiva para gerenciamento de deploys e publicações
- Autenticação JWT e validação de repositórios GitHub
- Segurança contra path traversal e validação de permissões
- Dados centralizados e histórico de deploys


# 🚀 CustomDeploy

Automatize deploys de projetos do GitHub direto para o IIS, com segurança, praticidade e interface moderna.





📖 **Visão Geral**

CustomDeploy é uma solução completa para automação de deploys de aplicações web e APIs hospedadas no IIS, integrando backend robusto em .NET 8 e frontend moderno em React + TypeScript.

O sistema permite gerenciar publicações, autenticação, permissões centralizadas e histórico de deploys.

✨ **Principais Recursos**

- Deploy automático: clone, build e publicação em poucos passos
- Integração nativa com IIS (sites e aplicações)
- Interface web intuitiva para gerenciamento de deploys e publicações
- Autenticação JWT e controle de permissões por nível de acesso
- Segurança contra path traversal e validação de repositórios GitHub
- Histórico de deploys centralizado

📂 **Estrutura do Projeto**
```
CustomDeploy/
├── CustomDeploy/        # Backend .NET 8 (API REST)
│   ├── Controllers/     # Endpoints principais
│   ├── Data/            # DbContext, configurações e repositórios
│   ├── Models/          # Modelos de dados e DTOs
│   ├── Services/        # Lógica de negócio e integrações
│   ├── Utils/           # Utilitários de autenticação e segurança
│   ├── Migrations/      # Migrations do Entity Framework
│   ├── appsettings.*    # Configurações do sistema
│   └── Program.cs       # Ponto de entrada da API
├── CustomDeployApp/     # Frontend React + TypeScript
│   ├── src/             # Código-fonte principal
│   ├── public/          # Arquivos estáticos
│   ├── package.json     # Dependências e scripts
│   └── vite.config.ts   # Configuração do Vite
└── CustomDeploy.sln     # Solução principal
```

🛠 **Tecnologias Utilizadas**

- Backend: .NET 8, Entity Framework Core, JWT, IIS Integration, SQLite
- Frontend: React 19+, TypeScript, Vite, Redux Toolkit, Styled-components, Axios, React Router, DnD Kit, React Toastify

✅ **Pré-requisitos**

Antes de executar, certifique-se de ter instalado:

- .NET 8 SDK
- Node.js 20+
- npm (ou yarn)
- IIS habilitado na máquina (com suporte a sites e aplicações)

▶️ **Como Executar**

**Backend (.NET API)**

1. Configure o `appsettings.Development.json` com suas credenciais e parâmetros.
   Exemplo de configuração SQLite:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Data Source=customdeploy.db"
   }
   ```
2. Execute as migrations para criar o banco:
   ```sh
   dotnet ef database update
   ```
3. Rode a API:
   ```sh
   dotnet run --project CustomDeploy
   ```
4. Acesse a documentação Swagger:
   `https://localhost:7071/swagger`

**Frontend (Web App)**

1. Instale as dependências:
   ```sh
   npm install
   ```
2. Inicie o servidor de desenvolvimento:
   ```sh
   npm run dev
   ```
3. Acesse no navegador:
   `http://localhost:5173`

📦 **Exemplos de Deploy**

Deploy para site IIS
```json
{
  "repoUrl": "https://github.com/user/frontend.git",
  "branch": "main",
  "buildCommands": ["npm install", "npm run build"],
  "buildOutput": "dist",
  "iisSiteName": "Default Web Site"
}
```

Deploy para aplicação IIS
```json
{
  "repoUrl": "https://github.com/user/api.git",
  "branch": "main",
  "buildCommands": ["dotnet restore", "dotnet publish -c Release"],
  "buildOutput": "bin/Release/net8.0/publish",
  "iisSiteName": "carteira",
  "targetPath": "api"
}
```

🔗 **Endpoints Úteis**

| Método | Endpoint                | Função                  |
|--------|-------------------------|-------------------------|
| POST   | `/auth/login`           | Autenticação JWT        |
| POST   | `/deploy`               | Executar deploy         |
| GET    | `/deploy/publications`  | Listar publicações      |
| GET    | `/iis/sites`            | Listar sites no IIS     |

🔒 **Segurança**

- Autenticação via JWT
- Controle de permissões por nível de acesso
- Validação de caminhos e permissões
- Verificação de repositórios GitHub antes do deploy

📚 **Documentação**

- [API - README](./CustomDeploy/README.md)
- [Frontend - README](./CustomDeployApp/README.md)

---

Versão: 3.0 | .NET: 8.0 | React: 19+ | Licença: MIT

Desenvolvido por [EricCoisa](https://github.com/EricCoisa)

---
