# 🖥️ Módulo IIS Management - CustomDeploy

## 📋 Visão Geral

O módulo IIS Management permite gerenciar sites e aplicações no Internet Information Services (IIS) através da API REST do CustomDeploy.

## 🏗️ Arquitetura

### 📂 Estrutura de Arquivos

```
Controllers/
├── IISController.cs         # Endpoints REST para gerenciamento IIS

Services/
├── IISManagementService.cs  # Lógica de negócio para IIS

Models/
├── CreateSiteRequest.cs     # Modelo para criação de sites
```

## 🌐 API Endpoints

### Base URL: `/api/iis`

| Método | Endpoint | Funcionalidade |
|--------|----------|----------------|
| `POST` | `/verify-permissions` | Verifica permissões para gerenciar IIS |
| `POST` | `/create-site` | Cria um novo site no IIS |
| `GET` | `/sites` | Lista todos os sites existentes |
| `GET` | `/app-pools` | Lista todos os Application Pools |

---

## 🔐 Verificação de Permissões

### POST `/api/iis/verify-permissions`

Verifica se a aplicação possui as permissões necessárias para:
- Criar pastas no sistema
- Mover arquivos entre diretórios
- Executar comandos IIS

**Request:**
```http
POST /api/iis/verify-permissions
Authorization: Bearer <jwt_token>
```

**Response:**
```json
{
  "message": "Todas as permissões necessárias estão disponíveis",
  "permissions": {
    "canCreateFolders": true,
    "canMoveFiles": true,
    "canExecuteIISCommands": true,
    "details": [
      "✅ Pode criar pastas no wwwroot",
      "✅ Pode mover arquivos entre pastas", 
      "✅ Pode executar comandos IIS (iisreset /status)"
    ]
  },
  "timestamp": "2025-08-02T22:30:00Z"
}
```

**Response (Sem Permissões):**
```json
{
  "message": "Algumas permissões estão faltando. Verifique se a aplicação está executando com privilégios de administrador",
  "permissions": {
    "canCreateFolders": true,
    "canMoveFiles": true,
    "canExecuteIISCommands": false,
    "details": [
      "✅ Pode criar pastas no wwwroot",
      "✅ Pode mover arquivos entre pastas",
      "❌ Não pode executar comandos IIS: Access denied"
    ]
  },
  "timestamp": "2025-08-02T22:30:00Z"
}
```

---

## 🏗️ Criação de Sites

### POST `/api/iis/create-site`

Cria um novo site no IIS com Application Pool associado.

**Request:**
```json
{
  "siteName": "MeuNovoSite",
  "port": 8080,
  "physicalPath": "C:\\inetpub\\wwwroot\\meunovo-site",
  "appPool": "MeuNovoSiteAppPool"
}
```

**Response (Sucesso):**
```json
{
  "message": "Site 'MeuNovoSite' criado com sucesso na porta 8080",
  "site": {
    "siteName": "MeuNovoSite",
    "port": 8080,
    "physicalPath": "C:\\inetpub\\wwwroot\\meunovo-site",
    "appPool": "MeuNovoSiteAppPool",
    "operationLog": [
      "✅ Diretório criado: C:\\inetpub\\wwwroot\\meunovo-site",
      "✅ Application Pool criado: MeuNovoSiteAppPool",
      "✅ Site criado com sucesso: MeuNovoSite"
    ],
    "createdAt": "2025-08-02T22:30:00Z"
  },
  "timestamp": "2025-08-02T22:30:00Z"
}
```

**Response (Erro):**
```json
{
  "message": "Porta 8080 já está em uso por outro site",
  "details": {
    "operationLog": [
      "✅ Diretório já existe: C:\\inetpub\\wwwroot\\meunovo-site",
      "✅ Application Pool já existe: MeuNovoSiteAppPool",
      "❌ Porta 8080 já está em uso por outro site"
    ]
  },
  "timestamp": "2025-08-02T22:30:00Z"
}
```

**Validação (Erro):**
```json
{
  "message": "Dados inválidos",
  "errors": [
    "Nome do site é obrigatório",
    "Porta deve estar entre 1 e 65535"
  ],
  "timestamp": "2025-08-02T22:30:00Z"
}
```

---

## 📊 Listagem de Recursos

### GET `/api/iis/sites`

Lista todos os sites existentes no IIS.

**Response:**
```json
{
  "message": "Sites listados com sucesso",
  "sites": "[{\"name\":\"Default Web Site\",\"id\":1,\"bindings\":[{\"protocol\":\"http\",\"bindingInformation\":\"*:80:\"}]}]",
  "timestamp": "2025-08-02T22:30:00Z"
}
```

### GET `/api/iis/app-pools`

Lista todos os Application Pools existentes no IIS.

**Response:**
```json
{
  "message": "Application Pools listados com sucesso", 
  "appPools": "[{\"name\":\"DefaultAppPool\",\"state\":\"Started\"}]",
  "timestamp": "2025-08-02T22:30:00Z"
}
```

---

## 🛠️ Funcionalidades Técnicas

### 🔧 IISManagementService

#### Métodos Principais:

**`VerifyPermissionsAsync()`**
- Testa criação de pastas temporárias
- Testa movimentação de arquivos
- Executa `iisreset /status` para verificar acesso ao IIS
- Retorna status detalhado de cada permissão

**`CreateSiteAsync(siteName, port, physicalPath, appPool)`**
- Valida parâmetros de entrada
- Cria diretório físico se não existir
- Verifica se Application Pool existe, cria se necessário
- Verifica se porta está disponível
- Verifica se site já existe
- Cria site usando PowerShell cmdlets
- Retorna log detalhado de todas as operações

#### Comandos PowerShell Utilizados:

```powershell
# Verificar/Criar Application Pool
Get-IISAppPool -Name 'AppPoolName' -ErrorAction SilentlyContinue
New-IISAppPool -Name 'AppPoolName' -Force

# Verificar porta em uso
Get-IISSite | Where-Object { $_.Bindings.bindingInformation -like '*:8080:*' }

# Verificar/Criar site
Get-IISSite -Name 'SiteName' -ErrorAction SilentlyContinue  
New-IISSite -Name 'SiteName' -PhysicalPath 'C:\path' -Port 8080 -ApplicationPool 'AppPoolName'

# Listar recursos
Get-IISSite | ConvertTo-Json
Get-IISAppPool | ConvertTo-Json
```

---

## 🔒 Segurança e Requisitos

### Permissões Necessárias:
- **Administrador**: Aplicação deve executar com privilégios de administrador
- **IIS Management**: Módulo IIS deve estar instalado
- **PowerShell**: Cmdlets do IIS devem estar disponíveis

### Validações Implementadas:
- ✅ Validação de parâmetros de entrada
- ✅ Verificação de conflitos de porta
- ✅ Verificação de sites duplicados
- ✅ Verificação de Application Pools existentes
- ✅ Criação automática de diretórios
- ✅ Log detalhado de operações

---

## 🧪 Casos de Uso

### Cenário 1: Criação de Site Simples
```bash
curl -X POST http://localhost:5000/api/iis/create-site \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "siteName": "MyWebApp",
    "port": 8080,
    "physicalPath": "C:\\inetpub\\wwwroot\\mywebapp",
    "appPool": "MyWebAppPool"
  }'
```

### Cenário 2: Verificação de Permissões
```bash
curl -X POST http://localhost:5000/api/iis/verify-permissions \
  -H "Authorization: Bearer <token>"
```

### Cenário 3: Listagem de Sites Existentes
```bash
curl -X GET http://localhost:5000/api/iis/sites \
  -H "Authorization: Bearer <token>"
```

---

## ⚠️ Considerações Importantes

### Limitações:
- Requer privilégios de administrador
- Funciona apenas em Windows com IIS instalado
- PowerShell IIS cmdlets devem estar disponíveis

### Boas Práticas:
- Sempre verificar permissões antes de criar sites
- Usar portas não padrão para evitar conflitos
- Nomear Application Pools de forma única
- Validar caminhos físicos antes da criação

### Troubleshooting:
- **"Access denied"**: Execute a aplicação como administrador
- **"Module not found"**: Instale o módulo IIS Management
- **"Port already in use"**: Use uma porta diferente ou pare o site existente

---

**🚀 Status**: Módulo completo e funcional para gerenciamento básico do IIS através da API CustomDeploy.
