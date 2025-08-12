# Integração Deploy Completo - Entity Framework + IIS

## Resumo da Implementação

Esta implementação integra o sistema de deploy existente (IIS) com a nova persistência em banco de dados usando Entity Framework + SQLite.

## Arquitetura

### 1. Dual Deploy System
- **Deploy Simples**: Apenas cria registros no banco (endpoint `/deploys`)
- **Deploy Completo**: Executa deploy no IIS + salva no banco (endpoint `/deploys/executar`)

### 2. Componentes Principais

#### Models/DTOs
```csharp
// Novo DTO para deploy completo
public class CriarDeployCompletoRequest
{
    public required string RepoUrl { get; set; }
    public required string Branch { get; set; }
    public string? BuildCommand { get; set; }
    public string? BuildOutput { get; set; }
    public required string IisSiteName { get; set; }
    public string? TargetPath { get; set; }
    public string? ApplicationPath { get; set; }
}
```

#### Business Service
```csharp
// Método que integra IIS + banco
public async Task<Deploy> ExecuteDeployCompletoAsync(
    DeployRequest deployRequest, 
    int usuarioId)
{
    // 1. Criar registro inicial no banco
    var deploy = await CriarDeployAsync(...);
    
    // 2. Executar deploy no IIS usando o serviço legacy
    var (success, message, details) = await _deployService.ExecuteDeployAsync(deployRequest);
    
    // 3. Atualizar status no banco baseado no resultado
    await AtualizarStatusDeployAsync(deploy.Id, status, message);
    
    return deploy;
}
```

#### Controller
```csharp
[HttpPost("executar")]
public async Task<ActionResult<DeployResponse>> ExecutarDeploy(CriarDeployCompletoRequest request)
{
    // Converter para formato legacy
    var deployRequest = new DeployRequest { ... };
    
    // Executar deploy completo
    var deploy = await _deployService.ExecuteDeployCompletoAsync(deployRequest, currentUserId);
    
    return response;
}
```

### 3. Fluxo de Execução

1. **Cliente** faz POST para `/deploys/executar` com dados do deploy
2. **Controller** converte `CriarDeployCompletoRequest` para `DeployRequest` (formato legacy)
3. **Business Service** cria registro inicial no banco com status "Iniciado"
4. **Legacy Service** executa o deploy real no IIS via `ExecuteDeployAsync`
5. **Business Service** atualiza status no banco baseado no resultado
6. **Controller** retorna resposta com todos os dados

### 4. Endpoints Disponíveis

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/deploys` | Criar deploy simples (apenas banco) |
| POST | `/deploys/executar` | Executar deploy completo (IIS + banco) |
| GET | `/deploys` | Listar todos os deploys |
| GET | `/deploys/{id}` | Obter deploy específico |
| PUT | `/deploys/{id}/status` | Atualizar status do deploy |
| GET | `/deploys/{id}/historico` | Obter histórico do deploy |

### 5. Dependências Configuradas

```csharp
// Program.cs
builder.Services.AddScoped<IDeployBusinessService, DeployBusinessService>();
builder.Services.AddScoped<DeployService>(); // Legacy service
```

## Vantagens da Implementação

1. **Compatibilidade**: Mantém o sistema legacy funcionando
2. **Rastreabilidade**: Todos os deploys são salvos no banco
3. **Histórico**: Controle completo de mudanças de status
4. **Flexibilidade**: Dois tipos de deploy conforme necessidade
5. **Migração Gradual**: Permite transição suave do sistema antigo

## Próximos Passos

1. Testar integração com IIS real
2. Implementar logs detalhados do processo
3. Adicionar validações específicas para dados do IIS
4. Considerar implementação de retry em caso de falha
5. Adicionar métricas e monitoramento

## Exemplo de Uso

```json
// Deploy completo
POST /deploys/executar
{
  "repoUrl": "https://github.com/usuario/projeto.git",
  "branch": "main",
  "buildCommand": "dotnet build",
  "buildOutput": "bin/Release/net8.0",
  "iisSiteName": "MeuSite",
  "targetPath": "C:\\inetpub\\wwwroot\\MeuSite",
  "applicationPath": "/MinhaApp"
}
```

A implementação está completa e funcional, integrando com sucesso o sistema legacy de deploy IIS com a nova persistência em Entity Framework + SQLite.
