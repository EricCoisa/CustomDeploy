# 🏗️ Funcionalidade: Campo ParentProject

## 📋 Visão Geral

Esta funcionalidade adiciona automaticamente um campo `parentProject` ao modelo `PublicationInfo`, que detecta se um deploy faz parte de um projeto pai maior baseado na estrutura da `targetPath`.

## 🧠 Lógica de Detecção

### Regras de Detecção

1. **Deploy no Nível Raiz**:
   - `targetPath` não contém separadores (`/` ou `\`)
   - `parentProject = null`
   - Exemplo: `"app1"` → `parentProject: null`

2. **Deploy em Subdiretório**:
   - `targetPath` contém separadores
   - `parentProject = primeira parte do caminho`
   - Exemplo: `"app2/api"` → `parentProject: "app2"`

3. **Deploy com Múltiplos Níveis**:
   - Sempre considera apenas o primeiro nível
   - Exemplo: `"ecommerce/admin/panel"` → `parentProject: "ecommerce"`

### Casos Especiais

- **Caminhos com barras iniciais/finais**: São normalizados automaticamente
- **Separadores mistos**: `\` e `/` são tratados uniformemente
- **Caminhos vazios ou nulos**: Retornam `parentProject: null`
- **Erros de parsing**: Logam warning e retornam `null`

## 🛠️ Implementação Técnica

### 1. Modelo Atualizado

**Arquivo:** `Models/PublicationInfo.cs`

```csharp
public class PublicationInfo
{
    // ... campos existentes ...
    
    /// <summary>
    /// Nome do projeto pai quando o deploy está em subdiretório.
    /// Null se estiver no nível raiz.
    /// Ex: "app2/api" → ParentProject = "app2", "app1" → ParentProject = null
    /// </summary>
    public string? ParentProject { get; set; }
}
```

### 2. Método de Extração

**Arquivo:** `Services/PublicationService.cs`
**Método:** `ExtractParentProject(string targetPath)`

```csharp
private string? ExtractParentProject(string targetPath)
{
    if (string.IsNullOrWhiteSpace(targetPath))
        return null;

    // Normalizar separadores de caminho para / para consistência
    var normalizedPath = targetPath.Replace('\\', '/');
    
    // Remover barras iniciais e finais
    normalizedPath = normalizedPath.Trim('/');
    
    // Se não há barras, está no nível raiz
    if (!normalizedPath.Contains('/'))
        return null;
    
    // Extrair a primeira parte (projeto pai)
    var pathParts = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
    if (pathParts.Length > 1)
        return pathParts[0];
    
    return null;
}
```

### 3. Integração nos Pontos de Criação

**Dois pontos onde `PublicationInfo` é criado:**

1. **Deploys Físicos Existentes**:
   ```csharp
   var publication = new PublicationInfo
   {
       // ... outros campos ...
       ParentProject = ExtractParentProject(metadata?.TargetPath ?? directoryInfo.Name)
   };
   ```

2. **Deploys Removidos (apenas metadata)**:
   ```csharp
   var offlinePublication = new PublicationInfo
   {
       // ... outros campos ...
       ParentProject = ExtractParentProject(metadata.TargetPath)
   };
   ```

## 📡 Exemplos de API Response

### Listagem de Publicações

```json
{
  "message": "Publicações listadas com sucesso",
  "count": 5,
  "publications": [
    {
      "name": "app1",
      "fullPath": "C:\\temp\\wwwroot\\app1",
      "repository": "https://github.com/user/app1.git",
      "parentProject": null,
      "exists": true,
      "sizeMB": 15.2
    },
    {
      "name": "app2-api",
      "fullPath": "C:\\temp\\wwwroot\\app2\\api",
      "repository": "https://github.com/user/app2.git",
      "parentProject": "app2",
      "exists": true,
      "sizeMB": 8.7
    },
    {
      "name": "app2-frontend",
      "fullPath": "C:\\temp\\wwwroot\\app2\\frontend",
      "repository": "https://github.com/user/app2-ui.git",
      "parentProject": "app2",
      "exists": true,
      "sizeMB": 12.3
    }
  ]
}
```

### Deploy Individual

```json
{
  "message": "Publicação encontrada",
  "publication": {
    "name": "microservices-auth",
    "fullPath": "C:\\temp\\wwwroot\\microservices\\auth",
    "repository": "https://github.com/company/auth-service.git",
    "branch": "main",
    "buildCommand": "npm install && npm run build",
    "parentProject": "microservices",
    "exists": true,
    "sizeMB": 25.8,
    "deployedAt": "2025-08-02T10:30:00Z"
  }
}
```

## 🎯 Casos de Uso

### 1. Microservices Architecture

```json
[
  {
    "name": "auth-service",
    "parentProject": "microservices",
    "targetPath": "microservices/auth"
  },
  {
    "name": "payment-service", 
    "parentProject": "microservices",
    "targetPath": "microservices/payment"
  },
  {
    "name": "user-service",
    "parentProject": "microservices", 
    "targetPath": "microservices/user"
  }
]
```

### 2. Frontend + Backend Projects

```json
[
  {
    "name": "api",
    "parentProject": "ecommerce",
    "targetPath": "ecommerce/api"
  },
  {
    "name": "admin-panel",
    "parentProject": "ecommerce",
    "targetPath": "ecommerce/admin"
  },
  {
    "name": "customer-app",
    "parentProject": "ecommerce",
    "targetPath": "ecommerce/customer"
  }
]
```

### 3. Multi-Environment Deployments

```json
[
  {
    "name": "production-api",
    "parentProject": "app",
    "targetPath": "app/prod"
  },
  {
    "name": "staging-api",
    "parentProject": "app", 
    "targetPath": "app/staging"
  },
  {
    "name": "development-api",
    "parentProject": "app",
    "targetPath": "app/dev"
  }
]
```

## 🔍 Análise e Agrupamento

### Agrupamento por Projeto Pai

Com o campo `parentProject`, é possível:

1. **Agrupar deploys relacionados**:
   ```javascript
   const groupedByProject = publications.reduce((groups, pub) => {
     const key = pub.parentProject || 'root';
     groups[key] = groups[key] || [];
     groups[key].push(pub);
     return groups;
   }, {});
   ```

2. **Calcular estatísticas por projeto**:
   ```javascript
   const projectStats = Object.entries(groupedByProject).map(([project, deploys]) => ({
     projectName: project,
     deployCount: deploys.length,
     totalSizeMB: deploys.reduce((sum, d) => sum + d.sizeMB, 0),
     existingDeploys: deploys.filter(d => d.exists).length
   }));
   ```

3. **Identificar projetos orfãos**:
   ```javascript
   const orphanedProjects = projectStats.filter(p => 
     p.existingDeploys === 0 && p.deployCount > 0
   );
   ```

## 📊 Exemplos de Visualização

### Dashboard Hierárquico

```
📁 Projetos (3)
├── 🏠 Nível Raiz (2 deploys)
│   ├── app1 (15.2 MB) ✅
│   └── legacy-system (8.9 MB) ✅
├── 📦 ecommerce (3 deploys)
│   ├── api (12.5 MB) ✅
│   ├── admin (18.7 MB) ✅
│   └── customer (22.3 MB) ✅
└── 🔧 microservices (4 deploys)
    ├── auth (8.2 MB) ✅
    ├── payment (15.8 MB) ✅
    ├── user (6.7 MB) ❌ (Removido)
    └── notification (9.3 MB) ✅
```

### Relatório de Status

```
📊 RELATÓRIO DE PROJETOS
========================
Total de Projetos: 3
Total de Deploys: 9
Deploys Ativos: 8
Deploys Removidos: 1

🏗️ POR PROJETO:
• ecommerce: 3 deploys (53.5 MB total)
• microservices: 4 deploys (40.0 MB total, 1 removido)
• Nível Raiz: 2 deploys (24.1 MB total)
```

## 🚀 Benefícios

### Para Desenvolvedores

1. **Organização Visual**: Fácil identificação de componentes relacionados
2. **Navegação Intuitiva**: Agrupamento lógico de serviços/aplicações
3. **Deploy em Massa**: Operações em todo um projeto
4. **Monitoramento**: Status de saúde por projeto

### Para DevOps

1. **Resource Management**: Visualizar uso de espaço por projeto
2. **Cleanup Operations**: Remover projetos completos
3. **Dependencies**: Identificar dependências entre componentes
4. **Scaling**: Planejar recursos por projeto

### Para Gestão

1. **Project Overview**: Visão geral de todos os projetos
2. **Cost Analysis**: Custo de infraestrutura por projeto  
3. **Team Alignment**: Mapear deploys para times
4. **Progress Tracking**: Acompanhar evolução de projetos

## 🔧 Implementação Sem Breaking Changes

### Compatibilidade

- ✅ **Campo Opcional**: `ParentProject` é nullable
- ✅ **Detecção Automática**: Não requer alterações em deploys existentes
- ✅ **Performance**: Cálculo durante leitura, sem overhead de storage
- ✅ **Retrocompatibilidade**: Todas as APIs existentes continuam funcionando

### Migration Path

1. **Fase 1**: Campo é adicionado e populado automaticamente
2. **Fase 2**: UIs podem começar a usar o campo para agrupamento
3. **Fase 3**: Novas funcionalidades podem aproveitar a hierarquia

## 🧪 Testing Strategy

### Unit Tests

```csharp
[Test]
public void ExtractParentProject_RootLevel_ReturnsNull()
{
    var result = ExtractParentProject("app1");
    Assert.IsNull(result);
}

[Test] 
public void ExtractParentProject_Subdirectory_ReturnsParent()
{
    var result = ExtractParentProject("app2/api");
    Assert.AreEqual("app2", result);
}

[Test]
public void ExtractParentProject_MultiLevel_ReturnsFirstLevel()
{
    var result = ExtractParentProject("ecommerce/admin/panel");
    Assert.AreEqual("ecommerce", result);
}
```

### Integration Tests

- Verificar se campo aparece nas respostas da API
- Validar agrupamento correto de deploys relacionados
- Testar casos extremos (caminhos especiais, caracteres especiais)

### End-to-End Tests

- Criar deploys com diferentes estruturas
- Verificar consistência entre deploys físicos e removidos
- Validar performance com muitos projetos

---

**📝 Nota:** Esta funcionalidade melhora significativamente a organização e visualização de deploys sem impactar funcionalidades existentes, fornecendo uma base sólida para futuras melhorias de UX.
