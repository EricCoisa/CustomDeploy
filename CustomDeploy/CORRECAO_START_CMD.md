## Correção do Problema com Comandos `start cmd`

### Problema Identificado

Quando o comando enviado era:
```bash
start cmd "nvm use 20.11.1 && npm install && npm run build:carteira"
```

O método `await process.WaitForExitAsync()` **não aguardava** a execução completa dos comandos internos (`nvm use 20.11.1`, `npm install`, `npm run build:carteira`), apenas aguardava o processo `start` retornar, que acontece imediatamente após abrir a nova janela do CMD.

### Comportamento Anterior (❌ Problemático)

1. **Comando original**: `start cmd "nvm use 20.11.1 && npm install && npm run build:carteira"`
2. **Modificação aplicada**: `start cmd /c "nvm use 20.11.1 && npm install && npm run build:carteira"`
3. **Resultado**: `start` abre uma nova janela de CMD e retorna imediatamente
4. **Problema**: `await process.WaitForExitAsync()` terminava antes dos comandos internos serem executados
5. **Consequência**: Próximas etapas do deploy executavam antes do build terminar

### Solução Implementada (✅ Corrigida)

#### Nova Lógica no `PrepareCommand()`

```csharp
// ANTES: Detectava e adicionava /c, mas mantinha o "start"
if (command.StartsWith("start cmd ") && !command.Contains("start cmd /c"))

// AGORA: Remove o "start" para aguardar execução completa
if (command.StartsWith("start cmd "))
{
    // Remove "start " do início, mantendo "cmd ..."
    command = command.Substring(6); // Remove "start "
    
    // Resultado: "cmd "nvm use 20.11.1 && npm install && npm run build:carteira""
}
```

#### Transformações de Comando

| **Entrada** | **Saída** | **Comportamento** |
|-------------|-----------|-------------------|
| `start cmd "nvm use 20.11.1 && npm install && npm run build:carteira"` | `cmd /c "nvm use 20.11.1 && npm install && npm run build:carteira"` | ✅ Aguarda execução completa |
| `start cmd echo hello` | `cmd /c echo hello` | ✅ Aguarda execução completa |
| `start cmd /c "npm install"` | `cmd /c "npm install"` | ✅ Aguarda execução completa |
| `npm install` | `npm install` | ✅ Sem alteração (funciona normalmente) |

#### Fluxo Corrigido

1. **Comando recebido**: `start cmd "nvm use 20.11.1 && npm install && npm run build:carteira"`
2. **Detecta `start cmd`**: ✅
3. **Remove `start`**: `cmd "nvm use 20.11.1 && npm install && npm run build:carteira"`
4. **Adiciona `/c`**: `cmd /c "nvm use 20.11.1 && npm install && npm run build:carteira"`
5. **Executa diretamente**: Sem nova janela, processo pai aguarda conclusão
6. **`await process.WaitForExitAsync()`**: ✅ Aguarda **todos** os comandos terminarem

### Logs Esperados

```log
info: CustomDeploy.Services.DeployService[0]
      Executando comando de build: start cmd "nvm use 20.11.1 && npm install && npm run build:carteira"
info: CustomDeploy.Services.DeployService[0]
      Detectado comando 'start cmd', removendo 'start' para aguardar execução completa
info: CustomDeploy.Services.DeployService[0]
      Adicionado /c ao comando cmd
info: CustomDeploy.Services.DeployService[0]
      Comando modificado para aguardar execução: cmd /c "nvm use 20.11.1 && npm install && npm run build:carteira"
```

### Vantagens da Solução

1. **✅ Sincronização Correta**: O deploy aguarda o build terminar antes de prosseguir
2. **✅ Suporte a NVM**: Comandos NVM funcionam corretamente
3. **✅ Logs Completos**: Todo output do build é capturado nos logs
4. **✅ Compatibilidade**: Funciona com outros tipos de comando
5. **✅ Detecção de Erros**: Falhas no build são detectadas corretamente

### Teste da Solução

Para verificar se a correção está funcionando:

1. **Envie um comando**: `start cmd "nvm use 20.11.1 && npm install && npm run build:carteira"`
2. **Verifique os logs**: Deve mostrar a remoção do `start` e modificação do comando
3. **Aguarde**: O processo deve aguardar todos os comandos terminarem
4. **Resultado**: Próximas etapas só executam após build completo

### Cenários Suportados

- ✅ `start cmd "comandos complexos com &&"`
- ✅ `start cmd comandos simples`
- ✅ `start cmd /c "comandos que já têm /c"`
- ✅ Comandos sem `start cmd` (funciona normalmente)
- ✅ NVM, NPM, Yarn, etc.
