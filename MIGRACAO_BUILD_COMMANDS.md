# Migração BuildCommand para BuildCommands

## Resumo da Alteração

O campo `BuildCommand` (string) foi alterado para `BuildCommands` (array de strings) em todo o sistema para permitir múltiplos comandos de build que são salvos na tabela `DeployComando`.

## Arquivos Alterados

### Backend (C#)

#### Models e DTOs
- ✅ `Models/DeployRequest.cs` - `BuildCommand` → `BuildCommands`
- ✅ `Models/DeployMetadata.cs` - `BuildCommand` → `BuildCommands`
- ✅ `Models/DTOs/DeployDTOs.cs` - `CriarDeployCompletoRequest` e `DeployResponse`

#### Controllers
- ✅ `Controllers/SystemController.cs` - Validação e resposta
- ✅ `Controllers/CustomDeployController.cs` - Validação e resposta
- ✅ `Controllers/DeploysController.cs` - Conversão entre modelos

#### Services
- ✅ `Services/DeployService.cs` - Novo método `ExecuteBuildCommandsAsync`
- ✅ `Services/PublicationService.cs` - Conversão para exibição
- ✅ `Services/Business/DeployBusinessService.cs` - Processamento da lista

### Frontend (TypeScript)

#### Services
- ✅ `services/deployService.ts` - Interfaces atualizadas

#### Arquivos de Teste
- ✅ `test-deploy-completo.http` - Exemplo com múltiplos comandos

## Como Funciona Agora

### 1. Input do Usuário
```json
{
  "buildCommands": [
    "npm install",
    "npm run build", 
    "dotnet build --configuration Release"
  ]
}
```

### 2. Processamento no Backend

#### No DeployBusinessService
```csharp
// Comandos base (git)
var comandosBase = new List<string>
{
    $"git clone {deployRequest.RepoUrl}",
    $"git checkout {deployRequest.Branch}"
};

// Adicionar comandos de build do usuário
if (deployRequest.BuildCommands != null && deployRequest.BuildCommands.Length > 0)
{
    comandosBase.AddRange(deployRequest.BuildCommands);
}

// Salvar cada comando na tabela DeployComando
for (int i = 0; i < comandosBase.Count; i++)
{
    var comando = new DeployComando
    {
        DeployId = deploy.Id,
        Comando = comandosBase[i],
        Ordem = i + 1
    };
    _context.DeployComandos.Add(comando);
}
```

#### No DeployService (Legacy)
```csharp
// Executa todos os comandos em sequência
private async Task<(bool Success, string Message)> ExecuteBuildCommandsAsync(string[] buildCommands, string workingDirectory)
{
    for (int i = 0; i < buildCommands.Length; i++)
    {
        var result = await ExecuteBuildCommandAsync(buildCommands[i], workingDirectory);
        if (!result.Success)
        {
            return (false, $"Falha no comando {i + 1} ({buildCommands[i]}): {result.Message}");
        }
    }
    return (true, $"Todos os {buildCommands.Length} comandos executados com sucesso");
}
```

### 3. Armazenamento no Banco

#### Tabela DeployComando
```sql
DeployId | Comando                              | Ordem
---------|--------------------------------------|-------
1        | git clone https://github.com/...    | 1
1        | git checkout main                    | 2  
1        | npm install                          | 3
1        | npm run build                        | 4
1        | dotnet build --configuration Release | 5
```

#### Tabela DeployMetadata (Legacy)
```csharp
// Para compatibilidade, converte array em string
BuildCommands = new string[] { "npm install", "npm run build", "dotnet build" }
```

## Vantagens da Nova Implementação

1. **Flexibilidade**: Suporte a múltiplos comandos de build
2. **Rastreabilidade**: Cada comando é salvo individualmente
3. **Ordem Garantida**: Comandos executam na ordem especificada
4. **Logs Detalhados**: Log individual de cada comando
5. **Rollback Granular**: Possível identificar qual comando falhou

## Compatibilidade

### Sistemas Legacy
- Métodos que esperam um único comando ainda funcionam (usa o primeiro comando da lista)
- PublicationService converte array para string para exibição

### APIs Existentes
- SystemController mantém compatibilidade mas agora valida array
- DeploysController usa a nova estrutura internamente

## Exemplo de Uso Completo

```http
POST /api/deploys/executar
{
  "repoUrl": "https://github.com/meu-projeto/api.git",
  "branch": "develop",
  "buildCommands": [
    "npm ci",
    "npm run test",
    "npm run build:prod",
    "dotnet build --configuration Release",
    "dotnet publish --configuration Release --output ./dist"
  ],
  "buildOutput": "dist",
  "iisSiteName": "MeuAPI",
  "targetPath": "C:\\inetpub\\wwwroot\\MeuAPI"
}
```

### Resultado no Banco
- 1 registro em `Deploy`
- 7 registros em `DeployComando` (2 git + 5 build)
- 1+ registros em `DeployHistorico`

### Execução
1. Cria deploy com status "Iniciado"
2. Salva todos os comandos na ordem
3. Executa git clone + checkout
4. Executa cada buildCommand em sequência
5. Se todos sucesso: copia arquivos para IIS
6. Atualiza status final

A migração mantém compatibilidade total com sistemas existentes enquanto adiciona a flexibilidade de múltiplos comandos de build.
