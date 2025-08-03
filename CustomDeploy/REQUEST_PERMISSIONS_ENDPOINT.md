# 🔐 Endpoint de Verificação de Permissões - IIS Management

## 📋 Endpoint Adicionado

### GET `/api/iis/request-permissions`

Este endpoint verifica se a aplicação possui todas as permissões necessárias para gerenciar o IIS e retorna instruções detalhadas para resolver problemas.

---

## 🧪 Teste do Endpoint

### Request
```http
GET /api/iis/request-permissions
Authorization: Bearer <jwt_token>
```

### Response (Todas as Permissões OK)
```json
{
  "success": true,
  "message": "Todas as permissões necessárias estão disponíveis",
  "permissions": {
    "canCreateFolders": true,
    "canMoveFiles": true,
    "canExecuteIISCommands": true,
    "allPermissionsGranted": true
  },
  "testDetails": [
    "✅ Pode criar diretórios na pasta da aplicação",
    "✅ Pode mover arquivos entre diretórios",
    "✅ Pode executar comandos IIS (iisreset /status)"
  ],
  "instructions": [
    "✅ Todas as permissões necessárias estão disponíveis!",
    "A aplicação pode gerenciar sites IIS sem problemas."
  ],
  "timestamp": "2025-08-02T22:45:00Z"
}
```

### Response (Permissões Faltando)
```json
{
  "success": false,
  "message": "Algumas permissões estão faltando",
  "permissions": {
    "canCreateFolders": true,
    "canMoveFiles": true,
    "canExecuteIISCommands": false,
    "allPermissionsGranted": false
  },
  "testDetails": [
    "✅ Pode criar diretórios na pasta da aplicação",
    "✅ Pode mover arquivos entre diretórios",
    "❌ Comando IIS falhou (código: 5)"
  ],
  "instructions": [
    "• Execute a aplicação como Administrador para gerenciar IIS",
    "• Verifique se o IIS está instalado e em execução",
    "",
    "📋 Instruções Gerais:",
    "• Feche a aplicação completamente",
    "• Clique com botão direito no executável > 'Executar como administrador'",
    "• Verifique se o IIS está instalado (Control Panel > Programs > Turn Windows features on/off > IIS)",
    "• Desabilite temporariamente o antivírus se necessário"
  ],
  "timestamp": "2025-08-02T22:45:00Z"
}
```

---

## 🛠️ Funcionalidades Implementadas

### Modelo `PermissionCheckResult`
```csharp
public class PermissionCheckResult
{
    public bool CanCreateFolders { get; set; }
    public bool CanMoveFiles { get; set; }
    public bool CanExecuteIISCommands { get; set; }
    public List<string> Instructions { get; set; }
    public bool AllPermissionsGranted { get; }
    public List<string> TestDetails { get; set; }
}
```

### Método `RequestPermissionsAsync()`

#### Testes Realizados:

1. **Criação de Diretórios**
   - Local: Pasta da aplicação (`AppContext.BaseDirectory`)
   - Teste: Criar pasta `temp_permission_test`
   - Cleanup: Remove pasta após teste

2. **Movimentação de Arquivos**
   - Local: Pasta da aplicação
   - Teste: Criar arquivo, mover entre diretórios
   - Cleanup: Remove diretórios e arquivos de teste

3. **Comandos IIS**
   - Comando: `cmd.exe /c iisreset /status`
   - Verifica: Exit code = 0
   - Detecta: Erros de acesso vs. IIS não instalado

#### Instruções Personalizadas:

**Para Acesso Negado:**
- Execute como Administrador
- Verifique permissões de pasta
- Desabilite antivírus temporariamente

**Para IIS não Instalado:**
- Instale o IIS via Windows Features
- Habilite IIS Management Console
- Verifique se serviço está rodando

**Para Problemas Gerais:**
- Instruções passo-a-passo para execução como admin
- Links para documentação do IIS
- Contato de suporte técnico

---

## 🔍 Diferenças vs. Método Anterior

| Aspecto | `VerifyPermissionsAsync()` | `RequestPermissionsAsync()` |
|---------|---------------------------|----------------------------|
| **Retorno** | Objeto anônimo | Classe tipada `PermissionCheckResult` |
| **Instruções** | Lista simples | Instruções contextuais e detalhadas |
| **Local de Teste** | `wwwroot` + temp paths | Apenas pasta da aplicação |
| **Cleanup** | Básico | Robusto com try/finally |
| **Detecção de Erros** | Genérica | Específica (UnauthorizedAccess vs outras) |
| **Endpoint** | `POST /verify-permissions` | `GET /request-permissions` |

---

## 📊 Casos de Uso

### Caso 1: Aplicação sem Privilégios
```bash
curl -X GET http://localhost:5000/api/iis/request-permissions \
  -H "Authorization: Bearer <token>"
```

**Resultado**: Instruções para executar como administrador

### Caso 2: IIS não Instalado
```bash
# Mesmo comando acima
```

**Resultado**: Instruções para instalar IIS via Windows Features

### Caso 3: Antivírus Bloqueando
```bash
# Mesmo comando acima
```

**Resultado**: Instruções para configurar exceções no antivírus

### Caso 4: Todas as Permissões OK
```bash
# Mesmo comando acima
```

**Resultado**: Confirmação de que pode prosseguir com operações IIS

---

## 🎯 Benefícios

### Para Desenvolvedores:
- **Diagnóstico Rápido**: Identifica exatamente qual permissão está faltando
- **Instruções Precisas**: Passos específicos para resolver cada problema
- **Teste Isolado**: Não interfere com arquivos de produção

### Para Usuários Finais:
- **Autodiagnóstico**: Podem verificar permissões sem suporte técnico
- **Instruções Claras**: Passos em português, fáceis de seguir
- **Resolução Guiada**: Diferentes soluções para diferentes problemas

### Para DevOps:
- **Automação**: Pode ser usado em scripts de implantação
- **Validação**: Verifica ambiente antes de deploy
- **Troubleshooting**: Facilita suporte remoto

---

**✅ Status**: Endpoint funcional e pronto para uso em diagnósticos de permissão IIS!
