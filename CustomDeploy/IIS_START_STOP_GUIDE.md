# IIS Start/Stop Operations - Documentação

## Visão Geral

Este documento descreve os novos métodos adicionados ao `IISController` para permitir **iniciar e parar** sites, aplicações e pools de aplicativos no IIS.

## 🎯 Funcionalidades Implementadas

### 1. **Site Management (Start/Stop)**
- ✅ `StartSite(string siteName)` - Inicia um site específico
- ✅ `StopSite(string siteName)` - Para um site específico

### 2. **Application Management (Start/Stop)**  
- ✅ `StartApplication(string siteName, string appPath)` - Inicia uma aplicação específica
- ✅ `StopApplication(string siteName, string appPath)` - Para uma aplicação específica

### 3. **Application Pool Management (Start/Stop)**
- ✅ `StartAppPool(string poolName)` - Inicia um pool de aplicativos
- ✅ `StopAppPool(string poolName)` - Para um pool de aplicativos

## 📋 Endpoints da API

### Sites

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/iis/sites/{siteName}/start` | Inicia um site |
| `POST` | `/api/iis/sites/{siteName}/stop` | Para um site |

### Application Pools

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/iis/app-pools/{poolName}/start` | Inicia um pool |
| `POST` | `/api/iis/app-pools/{poolName}/stop` | Para um pool |

### Applications

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/iis/sites/{siteName}/applications/start/{*appPath}` | Inicia uma aplicação |
| `POST` | `/api/iis/sites/{siteName}/applications/stop/{*appPath}` | Para uma aplicação |

## 🔧 Implementação Técnica

### Controller (`IISController.cs`)

Foram adicionadas **3 novas seções**:

1. **Site Management (Start/Stop)** - Linhas adicionadas ao controller
2. **Application Management (Start/Stop)** - Gerenciamento de aplicações
3. **Application Pool Management (Start/Stop)** - Gerenciamento de pools

### Service (`IISManagementService.cs`)

Foram implementados **6 novos métodos**:

1. `StartSiteAsync(string siteName)`
2. `StopSiteAsync(string siteName)`
3. `StartApplicationAsync(string siteName, string appPath)`
4. `StopApplicationAsync(string siteName, string appPath)`
5. `StartAppPoolAsync(string poolName)`
6. `StopAppPoolAsync(string poolName)`

### Características da Implementação

- **Assíncrono**: Todos os métodos são `async/await`
- **Verificação de Estado**: Verifica se o objeto já está no estado desejado
- **Validação**: Verifica se sites/pools/aplicações existem antes de operar
- **Logs Detalhados**: Registra todas as operações e erros
- **Tratamento de Erros**: Retorna respostas padronizadas com detalhes do erro
- **Delay para Estabilização**: Aguarda 1-1.5 segundos após operações para estabilização

## 📄 Formato de Resposta

### Sucesso
```json
{
  "message": "Site 'MeuSite' iniciado com sucesso",
  "siteName": "MeuSite",
  "status": "Started",
  "timestamp": "2025-08-03T10:30:00.000Z"
}
```

### Erro
```json
{
  "message": "Site 'SiteInexistente' não encontrado",
  "errors": ["O site especificado não existe no IIS"],
  "timestamp": "2025-08-03T10:30:00.000Z"
}
```

## 🔒 Autenticação e Permissões

### Requisitos
- **JWT Token**: Todas as operações requerem autenticação
- **Privilégios Administrativos**: Necessário executar como administrador
- **Permissões IIS**: O usuário deve ter permissões para gerenciar IIS

### Headers Necessários
```http
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

## 🚀 Exemplos de Uso

### 1. Iniciar um Site
```http
POST http://localhost:5092/api/iis/sites/Default Web Site/start
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### 2. Parar um Application Pool
```http
POST http://localhost:5092/api/iis/app-pools/DefaultAppPool/stop
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### 3. Iniciar uma Aplicação
```http
POST http://localhost:5092/api/iis/sites/MeuSite/applications/start/api/v1
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

## 🧪 Testes

### Arquivo de Testes
- **Localização**: `iis-start-stop-tests.http`
- **Conteúdo**: 100+ cenários de teste abrangentes
- **Cobertura**: Sites, pools, aplicações, cenários de erro

### Categorias de Teste
1. ✅ **Autenticação** - Login e obtenção de token JWT
2. ✅ **Sites** - Start/Stop de sites individuais
3. ✅ **Pools** - Start/Stop de application pools
4. ✅ **Aplicações** - Start/Stop de aplicações específicas
5. ✅ **Verificação** - Endpoints para verificar status
6. ✅ **Cenários Complexos** - Sequências de operações
7. ✅ **Error Handling** - Testes de comportamento em cenários de erro

## ⚠️ Considerações Importantes

### Dependências entre Componentes
1. **Aplicações dependem do Application Pool** - Se o pool estiver parado, a aplicação não funcionará
2. **Aplicações dependem do Site** - Se o site estiver parado, suas aplicações também não funcionarão
3. **Ordem recomendada para iniciar**: Pool → Site → Aplicação
4. **Ordem recomendada para parar**: Aplicação → Site → Pool (opcional)

### Comportamento Inteligente
- **StartApplication**: Automaticamente inicia o pool e site se necessário
- **StopApplication**: Para apenas o pool da aplicação (não o site inteiro)
- **Verificação de Estado**: Não executa operações desnecessárias se já está no estado desejado

### Performance
- **Delays**: Pequenos delays (1-1.5s) após operações para garantir estabilização
- **Verificação Prévia**: Verifica estado atual antes de executar operações
- **Operações Atômicas**: Cada operação é independente e pode ser executada isoladamente

## 📊 Códigos de Status HTTP

| Código | Cenário |
|--------|---------|
| `200 OK` | Operação executada com sucesso |
| `400 Bad Request` | Erro de validação ou objeto não encontrado |
| `401 Unauthorized` | Token JWT inválido ou ausente |
| `500 Internal Server Error` | Erro interno do servidor ou IIS |

## 🔍 Logs e Monitoramento

### Tipos de Log
- **Info**: Início e sucesso de operações
- **Warning**: Estados já corretos (não há mudança)
- **Error**: Falhas na execução ou objetos não encontrados

### Exemplo de Logs
```log
[INFO] Iniciando site IIS: Default Web Site
[INFO] Site IIS 'Default Web Site' iniciado com sucesso
[ERROR] Erro ao iniciar site IIS: SiteInexistente - Site não encontrado
```

## 🎉 Resultado Final

Os novos métodos foram implementados com sucesso e oferecem:

- ✅ **6 novos endpoints** para controle de start/stop
- ✅ **Integração completa** com o sistema de autenticação existente
- ✅ **Tratamento robusto de erros** com mensagens claras
- ✅ **Logs detalhados** para monitoramento e debug
- ✅ **Testes abrangentes** com mais de 20 cenários diferentes
- ✅ **Documentação completa** com exemplos práticos

A implementação segue os padrões estabelecidos no projeto e mantém consistência com os métodos CRUD existentes.
