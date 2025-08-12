# IIS Interface - Start/Stop Features Documentation

## 🎯 Novas Funcionalidades Implementadas

Esta documentação descreve as **novas funcionalidades de iniciar e parar** sites, aplicações e pools de aplicativos implementadas na interface React do sistema IIS Management.

## 🚀 Funcionalidades Adicionadas

### 1. **Sites IIS - Start/Stop**
- ✅ **Botão Iniciar Site** - Inicia sites parados
- ✅ **Botão Parar Site** - Para sites em execução  
- ✅ **Indicador Visual de Status** - Mostra se o site está rodando ou parado
- ✅ **Botões Condicionais** - Mostra apenas a ação disponível baseada no estado atual

### 2. **Aplicações - Start/Stop**
- ✅ **Botão Iniciar Aplicação** - Inicia o pool da aplicação
- ✅ **Botão Parar Aplicação** - Para o pool da aplicação
- ✅ **Controle Individual** - Cada aplicação pode ser controlada independentemente

### 3. **Application Pools - Start/Stop**
- ✅ **Botão Iniciar Pool** - Inicia pools parados
- ✅ **Botão Parar Pool** - Para pools em execução
- ✅ **Status Visual** - Badge colorido mostrando o estado atual do pool

## 🔧 Implementação Técnica

### Frontend (React/TypeScript)

#### 1. **Serviço IIS (`iisService.ts`)**
```typescript
// Novos métodos adicionados
async startSite(siteName: string)
async stopSite(siteName: string)
async startApplication(siteName: string, appPath: string)
async stopApplication(siteName: string, appPath: string)
async startAppPool(poolName: string)
async stopAppPool(poolName: string)
```

#### 2. **Redux Store (`store/iis/index.ts`)**
```typescript
// Novas actions assíncronas
export const startSite = createAsyncThunk(...)
export const stopSite = createAsyncThunk(...)
export const startApplication = createAsyncThunk(...)
export const stopApplication = createAsyncThunk(...)
export const startAppPool = createAsyncThunk(...)
export const stopAppPool = createAsyncThunk(...)
```

#### 3. **Componentes Atualizados**

**SiteCard.tsx**
- ✅ Botões condicionais Start/Stop para sites
- ✅ Botões Start/Stop para cada aplicação
- ✅ Indicador visual do status (Started/Stopped)

**AppPoolsList.tsx**
- ✅ Botões condicionais Start/Stop para pools
- ✅ Badge de status colorido

**IISView.tsx**
- ✅ Handlers para todas as operações start/stop
- ✅ Dispatch das actions Redux
- ✅ Integração com componentes filhos

## 🎨 Interface do Usuário

### Sites IIS
```
┌─────────────────────────────────────────────────────┐
│ ▼ Site Default Web Site                             │
│   Status: [Started] | Port: 80 | Pool: DefaultPool │
│   [⏹ Parar] [+ Aplicação] [Editar] [Excluir]      │
│                                                     │
│   Aplicações (2):                                   │
│   ├─ / (Root Application)                          │
│   │  [▶ Iniciar] [⏹ Parar] [Editar] [Excluir]    │
│   └─ /api (API Application)                        │
│      [▶ Iniciar] [⏹ Parar] [Editar] [Excluir]    │
└─────────────────────────────────────────────────────┘
```

### Application Pools
```
┌─────────────────────────────────────────────────────┐
│ DefaultAppPool                                      │
│ Status: [Started] | .NET: v4.0 | Pipeline: Integrated │
│ [⏹ Parar] [Editar] [Excluir]                      │
└─────────────────────────────────────────────────────┘
```

## 🔄 Comportamento dos Botões

### **Botões Condicionais**
- **Site/Pool Running** → Mostra apenas botão "⏹ Parar"
- **Site/Pool Stopped** → Mostra apenas botão "▶ Iniciar"

### **Aplicações**
- **Sempre mostra ambos** os botões "▶ Iniciar" e "⏹ Parar"
- **Controle do Pool** - As operações afetam o Application Pool da aplicação

### **Cores e Estados**
- **▶ Iniciar** - Botão azul (primary)
- **⏹ Parar** - Botão cinza (secondary)  
- **Status Started** - Badge verde
- **Status Stopped** - Badge vermelho

## 📡 API Endpoints Utilizados

### Sites
- `POST /iis/sites/{siteName}/start` - Iniciar site
- `POST /iis/sites/{siteName}/stop` - Parar site

### Applications  
- `POST /iis/sites/{siteName}/applications/start/{appPath}` - Iniciar aplicação
- `POST /iis/sites/{siteName}/applications/stop/{appPath}` - Parar aplicação

### Application Pools
- `POST /iis/app-pools/{poolName}/start` - Iniciar pool
- `POST /iis/app-pools/{poolName}/stop` - Parar pool

## 🔒 Segurança e Autenticação

- ✅ **JWT Token obrigatório** para todas as operações
- ✅ **Privilégios administrativos** necessários no backend
- ✅ **Verificação de permissões** antes das operações
- ✅ **Tratamento de erros** com mensagens amigáveis

## 🎛️ Fluxo de Operação

### **Iniciar Site**
1. Usuário clica em "▶ Iniciar" no card do site
2. Frontend dispara `startSite(siteName)` action
3. Redux faz chamada para `/iis/sites/{siteName}/start`
4. Backend inicia o site no IIS
5. State Redux é atualizado com novo status
6. Interface atualiza automaticamente (botão muda para "⏹ Parar")

### **Parar Aplicação**
1. Usuário clica em "⏹ Parar" na aplicação
2. Frontend dispara `stopApplication({ siteName, appPath })` action
3. Redux faz chamada para endpoint correspondente
4. Backend para o Application Pool da aplicação
5. Interface reflete a mudança

## 📱 Responsividade

- ✅ **Mobile-first** - Botões adaptam-se a telas pequenas
- ✅ **Flex layout** - Botões se reorganizam automaticamente
- ✅ **Touch-friendly** - Tamanhos adequados para toque
- ✅ **Texto legível** - Ícones e texto claros em qualquer resolução

## 🧪 Testing & Debugging

### **Estados para Testar**
1. ✅ Site Started → Botão "Parar" visível
2. ✅ Site Stopped → Botão "Iniciar" visível  
3. ✅ Pool Started → Botão "Parar" visível
4. ✅ Pool Stopped → Botão "Iniciar" visível
5. ✅ Aplicação com diferentes pools
6. ✅ Múltiplas aplicações por site

### **Cenários de Erro**
- ❌ **Token inválido** → Redirecionamento para login
- ❌ **Privilégios insuficientes** → Mensagem de erro clara
- ❌ **Site/Pool não encontrado** → Error handler
- ❌ **Falha na operação** → Toast notification

## 🚀 Resultado Final

### **Experiência do Usuário**
- ✅ **Interface intuitiva** com botões claramente identificados
- ✅ **Feedback visual imediato** - Botões mudam baseado no estado
- ✅ **Operações rápidas** - Um clique para start/stop
- ✅ **Status sempre visível** - Badges coloridos para identificação rápida

### **Funcionalidades Implementadas**
- ✅ **6 novos endpoints** no frontend
- ✅ **6 novas Redux actions** para start/stop
- ✅ **Botões condicionais** baseados no estado
- ✅ **Integração completa** com backend
- ✅ **Responsividade móvel**
- ✅ **Tratamento de erros robusto**

A implementação está **completa e funcional**, oferecendo uma experiência fluida para gerenciar o ciclo de vida de sites, aplicações e pools no IIS diretamente pela interface web! 🎉
