# 🔄 Atualização: Campo `exists` para Verificação de Existência Física

## 📋 Resumo das Novas Funcionalidades

A aplicação **CustomDeploy** foi atualizada para incluir verificação automática de existência física das aplicações deployadas, adicionando o campo `exists` aos metadados e novos endpoints para gerenciamento.

## 🏗️ Modificações Implementadas

### 1. **Modelo `DeployMetadata` Expandido**
```csharp
public class DeployMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string BuildCommand { get; set; } = string.Empty;
    public string TargetPath { get; set; } = string.Empty;
    public DateTime DeployedAt { get; set; }
    public bool Exists { get; set; } = true;  // 🆕 NOVO CAMPO
}
```

### 2. **Novos Métodos no `DeployService`**
- ✅ `GetAllDeployMetadataWithExistsCheck()` - Verifica e atualiza status de existência
- ✅ `RemoveDeployMetadata(name)` - Remove entrada dos metadados
- ✅ `GetDeployMetadata(name)` - Obtém deploy específico com status atualizado

### 3. **PublicationService Atualizado**
- ✅ Usa verificação automática de existência no `GetPublicationsAsync()`
- ✅ Identifica claramente aplicações removidas
- ✅ Persiste mudanças no arquivo `deploys.json`

### 4. **Novos Endpoints na API**
- ✅ `DELETE /deploy/publications/{name}` - Remove entrada dos metadados
- ✅ `GET /deploy/publications/{name}/metadata` - Obtém metadados específicos

## 📊 Estrutura Atualizada do `deploys.json`

```json
[
  {
    "name": "MyApp1",
    "repository": "https://github.com/user/repo.git",
    "branch": "main",
    "buildCommand": "npm run build",
    "targetPath": "C:\\inetpub\\wwwroot\\MyApp1",
    "deployedAt": "2025-08-02T15:45:00Z",
    "exists": true
  },
  {
    "name": "MyApp2",
    "repository": "https://github.com/user/another-repo.git",
    "branch": "main",
    "buildCommand": "dotnet publish",
    "targetPath": "C:\\inetpub\\wwwroot\\MyApp2",
    "deployedAt": "2025-08-01T18:22:00Z",
    "exists": false
  }
]
```

## 🔄 Processo de Verificação Automática

### **Durante `GET /deploy/publications`:**
1. **Carrega metadados** do arquivo `deploys.json`
2. **Verifica existência** de cada `targetPath` usando `Directory.Exists()`
3. **Atualiza campo `exists`** com `true`/`false` conforme necessário
4. **Persiste mudanças** no arquivo se houver alterações
5. **Combina dados** com informações reais dos diretórios
6. **Identifica aplicações removidas** como "(Removido)" na listagem

### **Lógica de Atualização:**
```csharp
foreach (var deploy in deploysList)
{
    var normalizedPath = Path.GetFullPath(deploy.TargetPath);
    var currentExists = Directory.Exists(normalizedPath);
    
    if (deploy.Exists != currentExists)
    {
        deploy.Exists = currentExists;
        hasChanges = true;
        // Log da mudança
    }
}

if (hasChanges)
{
    SaveAllDeployMetadata(deploysList);
}
```

## 🌐 Novos Endpoints da API

### 1. **DELETE /deploy/publications/{name}**
**Descrição:** Remove uma entrada de deploy dos metadados  
**Uso:** Limpeza administrativa de registros órfãos

**Exemplo:**
```bash
DELETE https://localhost:7071/deploy/publications/MyApp1
Authorization: Bearer {token}
```

**Resposta:**
```json
{
  "message": "Deploy 'MyApp1' removido com sucesso dos metadados",
  "name": "MyApp1",
  "timestamp": "2025-08-02T16:30:00Z"
}
```

### 2. **GET /deploy/publications/{name}/metadata**
**Descrição:** Obtém metadados específicos de um deploy com status atualizado  
**Uso:** Verificação detalhada de um deploy específico

**Exemplo:**
```bash
GET https://localhost:7071/deploy/publications/MyApp1/metadata
Authorization: Bearer {token}
```

**Resposta:**
```json
{
  "message": "Metadados encontrados",
  "metadata": {
    "name": "MyApp1",
    "repository": "https://github.com/user/repo.git",
    "branch": "main",
    "buildCommand": "npm run build",
    "targetPath": "C:\\inetpub\\wwwroot\\MyApp1",
    "deployedAt": "2025-08-02T15:45:00Z",
    "exists": true
  },
  "timestamp": "2025-08-02T16:30:00Z"
}
```

## 🔍 Comportamento da Listagem Atualizada

### **GET /deploy/publications** - Resposta Expandida:
```json
{
  "message": "Publicações listadas com sucesso",
  "count": 3,
  "publications": [
    {
      "name": "ActiveApp",
      "fullPath": "C:\\inetpub\\wwwroot\\ActiveApp",
      "lastModified": "2025-08-02T15:45:00Z",
      "sizeMB": 15.7,
      "repository": "https://github.com/user/active-app.git",
      "branch": "main",
      "buildCommand": "npm run build",
      "deployedAt": "2025-08-02T15:45:00Z"
    },
    {
      "name": "RemovedApp (Removido)",
      "fullPath": "C:\\inetpub\\wwwroot\\RemovedApp",
      "lastModified": "2025-08-01T18:22:00Z",
      "sizeMB": 0,
      "repository": "https://github.com/user/removed-app.git",
      "branch": "main",
      "buildCommand": "dotnet publish",
      "deployedAt": "2025-08-01T18:22:00Z"
    }
  ],
  "timestamp": "2025-08-02T16:30:00Z"
}
```

## 🛡️ Recursos de Segurança e Robustez

### ✅ **Thread-Safety Mantida**
- Operações protegidas com lock
- Operações atômicas de leitura/escrita
- Sem race conditions em verificações concorrentes

### ✅ **Validação de Caminhos**
- Normalização com `Path.GetFullPath()`
- Verificação segura de existência
- Prevenção de path traversal

### ✅ **Logging Detalhado**
```
[INFO] Status de existência atualizado para MyApp1: False
[INFO] Arquivo de metadados atualizado com status de existência
[WARN] Deploy registrado mas diretório não existe: C:\inetpub\wwwroot\MyApp2
[INFO] Encontradas 3 publicações (2 ativas, 1 removidas)
```

## 🧪 Cenários de Teste

### **Teste 1: Aplicação Removida Manualmente**
1. Deploy uma aplicação normalmente
2. Deletar manualmente a pasta da aplicação
3. Chamar `GET /deploy/publications`
4. ✅ **Resultado:** Campo `exists: false` atualizado automaticamente

### **Teste 2: Limpeza Administrativa**
1. Verificar aplicações marcadas como "(Removido)"
2. Chamar `DELETE /deploy/publications/{name}`
3. ✅ **Resultado:** Entrada removida dos metadados

### **Teste 3: Verificação de Metadados**
1. Chamar `GET /deploy/publications/{name}/metadata`
2. ✅ **Resultado:** Status `exists` refletindo estado atual do disco

### **Teste 4: Re-deploy Após Remoção**
1. Aplicação marcada como `exists: false`
2. Fazer novo deploy para o mesmo `targetPath`
3. ✅ **Resultado:** Campo `exists` volta para `true`

## 📈 Benefícios das Novas Funcionalidades

### 🎯 **Para Desenvolvedores:**
- **Visibilidade clara** do status de cada aplicação
- **Limpeza fácil** de registros órfãos
- **Histórico preservado** mesmo após remoção manual

### 🎯 **Para Administradores:**
- **Monitoramento automático** de integridade
- **Gestão centralizada** de metadados
- **Auditoria completa** de deploys

### 🎯 **Para o Sistema:**
- **Sincronização automática** entre metadados e disco
- **Performance otimizada** com verificações em lote
- **Consistência garantida** em operações concorrentes

## 🔮 Próximos Passos (Sugestões)

### **Funcionalidades Futuras:**
- 🔄 **Auto-cleanup**: Remover automaticamente entradas antigas com `exists: false`
- 📊 **Dashboard**: Interface web para visualizar status das aplicações
- 🔔 **Alertas**: Notificações quando aplicações ficam indisponíveis
- 📧 **Relatórios**: Relatórios periódicos de status das aplicações

### **Melhorias de Performance:**
- ⚡ **Cache**: Cache de status de existência com TTL
- 🔄 **Background Jobs**: Verificação periódica em background
- 📊 **Métricas**: Coleta de métricas de uso e performance

---

**✅ Status:** Implementado e testado  
**🔧 Compatibilidade:** 100% compatível com API existente  
**📅 Data:** Agosto 2025  
**🎯 Impacto:** Melhoria significativa na gestão e monitoramento de deploys
