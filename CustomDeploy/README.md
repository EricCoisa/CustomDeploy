# 🚀 CustomDeploy API

Automatize deploys de projetos do GitHub direto para o IIS, com segurança e praticidade.

## Principais Recursos

- Deploy automático: clone, build e publicação em poucos passos
- Integração nativa com IIS (sites e aplicações)
- Metadados centralizados em `deploys.json`
- Autenticação JWT e validação de repositórios GitHub
- Segurança contra path traversal
- Gerenciamento fácil de publicações e metadados

## Como Usar

1. Configure o `appsettings.json` com suas credenciais
2. Execute: `dotnet run`
3. Acesse: `https://localhost:7071/swagger`
4. Modifique o usuário inicial caso queira em : ([`Program.cs`](./Program.cs))
5. Faça login:  
6. Realize seu deploy!

## Exemplos de Deploy

Deploy para site IIS:
```json
{
  "repoUrl": "https://github.com/user/frontend.git",
  "branch": "main",
  "buildCommand": "npm run build",
  "buildOutput": "dist",
  "iisSiteName": "Default Web Site"
}
```

Deploy para aplicação IIS:
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

## Endpoints Úteis

| Método | Endpoint                | Função                       |
|--------|-------------------------|------------------------------|
| POST   | `/auth/login`           | Autenticação JWT             |
| POST   | `/deploy`               | Deploy automático            |
| GET    | `/deploy/publications`  | Listar publicações           |
| GET    | `/iis/sites`            | Listar sites IIS             |

## Segurança

- Autenticação JWT
- Validação de caminhos e permissões
- Verificação de repositórios GitHub

## Documentação Completa

Veja detalhes, exemplos avançados e dicas em [`CONTEXTO_APLICACAO.md`](./CONTEXTO_APLICACAO.md).

---

**Versão:** 3.0 | **.NET:** 8.0 | **Licença:** MIT

Desenvolvido por [EricCoisa](https://github.com/EricCoisa)

---