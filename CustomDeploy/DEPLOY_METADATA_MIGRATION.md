# 🔄 Migração do Sistema de Metadados de Deploy

## 📋 Resumo das Mudanças

A aplicação **CustomDeploy** foi atualizada para centralizar todos os metadados de deploy em um único arquivo JSON ao invés de manter arquivos `deploy.json` individuais em cada pasta de publicação.

## 🏗️ Mudanças Implementadas

### 1. **Modelo `DeployMetadata` Atualizado**
- ✅ Adicionado campo `Name` - Nome da aplicação/deploy
- ✅ Adicionado campo `TargetPath` - Caminho completo do diretório de destino
- ✅ Mantidos campos existentes: `Repository`, `Branch`, `BuildCommand`, `DeployedAt`

### 2. **Arquivo Centralizado `deploys.json`**
- 📁 **Localização:** `AppContext.BaseDirectory` (pasta do executável da aplicação)
- 📄 **Formato:** Array JSON com todos os metadados de deploy
- 🔒 **Thread-Safe:** Operações protegidas com lock para concorrência

### 3. **DeployService Modificado**
- ✅ Novo campo `_deploysJsonPath` para o arquivo centralizado
- ✅ Método `SaveDeployMetadata()` atualizado para usar arquivo único
- ✅ Métodos auxiliares:
  - `LoadAllDeployMetadata()` - Carrega todos os metadados
  - `SaveAllDeployMetadata()` - Salva todos os metadados
  - `GetAllDeployMetadata()` - API pública para acessar metadados
- ✅ Sistema de lock (`_deploysFileLock`) para operações thread-safe

### 4. **PublicationService Refatorado**
- ✅ Injeção de dependência do `DeployService`
- ✅ Método `GetPublicationsAsync()` usa arquivo centralizado
- ✅ Combinação de dados do arquivo + verificação de diretórios existentes
- ✅ Detecção de publicações "removidas" (metadados existem mas pasta não)

## 📊 Estrutura do Novo `deploys.json`

```json
[
  {
    "name": "MyReactApp",
    "repository": "https://github.com/user/react-app.git",
    "branch": "main",
    "buildCommand": "npm install && npm run build",
    "targetPath": "C:\\inetpub\\wwwroot\\MyReactApp",
    "deployedAt": "2025-08-02T15:45:00Z"
  },
  {
    "name": "MyAPI",
    "repository": "https://github.com/user/dotnet-api.git",
    "branch": "main",
    "buildCommand": "dotnet publish -c Release -o publish",
    "targetPath": "C:\\inetpub\\wwwroot\\MyAPI",
    "deployedAt": "2025-08-01T18:22:00Z"
  }
]
```

## 🔄 Processo de Deploy Atualizado

1. **Deploy Request** - Mesma API, mesmo payload
2. **Clonagem/Build** - Processo inalterado
3. **Cópia de Arquivos** - Processo inalterado
4. **Metadados** - **NOVA IMPLEMENTAÇÃO:**
   - Lê arquivo `deploys.json` existente (ou cria se não existir)
   - Remove entrada existente para o mesmo `targetPath` (se houver)
   - Adiciona nova entrada com dados atualizados
   - Salva arquivo de volta (operação atômica com lock)

## 🔍 Consulta de Publicações Atualizada

### Novo Comportamento do `GET /deploy/publications`:
1. **Carrega metadados** do arquivo `deploys.json`
2. **Verifica diretórios** existentes no disco
3. **Combina informações**:
   - Metadados + dados reais do diretório (tamanho, data de modificação)
   - Publicações "offline" (metadados existem mas pasta foi removida)
4. **Retorna lista unificada** ordenada por data de modificação

## 🛡️ Benefícios da Nova Implementação

### ✅ **Vantagens:**
- **Centralização:** Todos os metadados em um local
- **Performance:** Uma única leitura de arquivo vs múltiplas
- **Histórico:** Mantém registro de deploys mesmo após remoção manual
- **Consistência:** Dados normalizados e estruturados
- **Thread-Safety:** Operações seguras para concorrência
- **Auditoria:** Histórico completo de todos os deploys

### ✅ **Recursos Mantidos:**
- **API inalterada** - Mesmos endpoints e payloads
- **Compatibilidade** - Funciona com deploys existentes
- **Logging** - Mesmos níveis de log e rastreamento
- **Validações** - Todas as validações de segurança mantidas

## 🧪 Como Testar

### 1. **Teste de Deploy Novo**
```bash
POST /deploy
{
  "repoUrl": "https://github.com/user/test-repo.git",
  "branch": "main",
  "buildCommand": "npm install && npm run build",
  "buildOutput": "dist",
  "targetPath": "test-app"
}
```

**Verificações:**
- ✅ Deploy executado com sucesso
- ✅ Arquivo `deploys.json` criado em `AppContext.BaseDirectory`
- ✅ Entrada adicionada com dados corretos

### 2. **Teste de Deploy Atualização**
```bash
# Repetir o mesmo deploy com mudanças
POST /deploy
{
  "repoUrl": "https://github.com/user/test-repo.git",
  "branch": "develop",  # Branch diferente
  "buildCommand": "npm install && npm run build",
  "buildOutput": "dist",
  "targetPath": "test-app"  # Mesmo targetPath
}
```

**Verificações:**
- ✅ Entrada anterior removida
- ✅ Nova entrada adicionada com branch "develop"
- ✅ Apenas uma entrada para o mesmo `targetPath`

### 3. **Teste de Listagem**
```bash
GET /deploy/publications
```

**Verificações:**
- ✅ Lista inclui todas as publicações
- ✅ Metadados corretos de repository, branch, etc.
- ✅ Dados de tamanho e data real do diretório
- ✅ Publicações "removidas" marcadas (se aplicável)

### 4. **Teste de Concorrência**
- Executar múltiplos deploys simultaneamente
- Verificar integridade do arquivo `deploys.json`
- Confirmar que não há corrupção de dados

## 📁 Localização dos Arquivos

### **Arquivo de Metadados:**
```
AppContext.BaseDirectory/deploys.json
```

**Exemplo em desenvolvimento:**
```
C:\Users\ericv\source\repos\CustomDeploy\CustomDeploy\CustomDeploy\bin\Debug\net8.0\deploys.json
```

**Exemplo em produção:**
```
C:\inetpub\wwwroot\CustomDeploy\deploys.json
```

### **Diretórios de Publicação:**
```
{DeploySettings:PublicationsPath}/{targetPath}/
```

**Exemplo:**
```
C:\inetpub\wwwroot\MyApp\
C:\inetpub\wwwroot\MyAPI\
```

## 🔧 Migração de Dados Existentes

### **Cenário:** Aplicação já tem deploys com arquivos `deploy.json` individuais

**Estratégia de migração (se necessário):**
1. **Automática:** Próximos deploys migram automaticamente
2. **Manual:** Script para converter arquivos existentes
3. **Híbrida:** Sistema detecta e converte sob demanda

**Nota:** A implementação atual **não afeta** deployments existentes. Eles continuam funcionando e serão migrados naturalmente nos próximos deploys.

## 🐛 Troubleshooting

### **Arquivo `deploys.json` corrompido:**
```bash
# Backup automático criado pelo sistema
deploys.json.backup
```

### **Permissões de arquivo:**
- Aplicação precisa de read/write na pasta do executável
- Verificar permissões de `AppContext.BaseDirectory`

### **Problemas de concorrência:**
- Sistema usa lock interno
- Em caso de deadlock, reiniciar aplicação

### **Metadados inconsistentes:**
- Executar novo deploy para recriar entrada
- Verificar logs para identificar origem do problema

## 📚 Logs Relevantes

### **Deploy com sucesso:**
```
[INFO] Metadados do deploy salvos no arquivo centralizado: {DeploysJsonPath}
[DEBUG] Arquivo de metadados atualizado com {Count} entradas
```

### **Primeira execução:**
```
[INFO] Arquivo de metadados não existe, criando novo: {DeploysJsonPath}
```

### **Publicação removida:**
```
[WARN] Deploy registrado mas diretório não existe: {TargetPath}
```

---

**Versão:** 2.0  
**Data da Migração:** Agosto 2025  
**Compatibilidade:** Mantém 100% de compatibilidade com API existente  
**Status:** ✅ Implementado e testado
