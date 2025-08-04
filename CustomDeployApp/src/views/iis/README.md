# Interface de Gerenciamento IIS

Esta interface permite gerenciar sites, aplicações e pools de aplicativos do IIS através de uma interface web moderna e intuitiva.

## Funcionalidades

### 📊 Status de Permissões
- Verifica se a aplicação tem privilégios de administrador
- Mostra quais permissões estão disponíveis para gerenciar o IIS
- Permite solicitar elevação de privilégios

### 🌐 Gerenciamento de Sites
- **Listar Sites**: Visualiza todos os sites configurados no IIS
- **Criar Site**: Cria novos sites com configurações personalizadas
- **Editar Site**: Modifica configurações de sites existentes (em desenvolvimento)
- **Excluir Site**: Remove sites do IIS
- **Expandir/Recolher**: Visualiza aplicações dentro de cada site

### 📱 Gerenciamento de Aplicações
- **Listar Aplicações**: Mostra todas as aplicações dentro de um site
- **Criar Aplicação**: Adiciona novas aplicações a um site existente
- **Editar Aplicação**: Modifica configurações de aplicações (em desenvolvimento)
- **Excluir Aplicação**: Remove aplicações de um site

### 🏊 Gerenciamento de Application Pools
- **Listar Pools**: Visualiza todos os application pools disponíveis
- **Criar Pool**: Cria novos application pools com configurações avançadas
- **Editar Pool**: Modifica configurações de pools existentes (em desenvolvimento)
- **Excluir Pool**: Remove application pools (apenas se não estiverem em uso)

## Como Usar

### Acesso
1. Faça login na aplicação
2. No dashboard, clique no botão "🖥️ Gerenciar IIS"
3. Ou acesse diretamente: `/iis`

### Verificação de Permissões
1. A interface automaticamente verifica as permissões ao carregar
2. Use o botão "🔄 Verificar Permissões" para atualizar o status
3. Se necessário, clique em "🔐 Solicitar Admin" para elevar privilégios

### Gerenciando Sites
1. Na aba "Sites IIS", clique em "+ Novo Site"
2. Preencha os campos obrigatórios:
   - **Nome do Site**: Nome único para identificar o site
   - **Binding Information**: Formato `IP:Porta:Host` (ex: `*:80:` ou `localhost:8080`)
   - **Caminho Físico**: Diretório onde estão os arquivos do site
   - **Application Pool**: Pool que executará o site
3. Clique em "Criar Site"

### Gerenciando Aplicações
1. Expanda um site clicando na seta ou no nome
2. Clique em "+ Aplicação" no cabeçalho do site
3. Preencha os dados:
   - **Caminho da Aplicação**: Deve começar com `/` (ex: `/api`, `/admin`)
   - **Caminho Físico**: Diretório da aplicação
   - **Application Pool**: Pool para a aplicação
4. Clique em "Criar Aplicação"

### Gerenciando Application Pools
1. Vá para a aba "Application Pools"
2. Clique em "+ Novo Application Pool"
3. Configure as opções:
   - **Nome**: Nome único do pool
   - **Versão .NET**: v4.0, v2.0 ou "No Managed Code"
   - **Pipeline Mode**: Integrated ou Classic
   - **Identity Type**: Tipo de identidade para execução
   - **Configurações avançadas**: Timeouts, limites de memória, etc.
4. Clique em "Criar Application Pool"

## Dicas Importantes

### ⚠️ Permissões
- A aplicação precisa executar como **Administrador** para gerenciar o IIS
- Certifique-se de que o IIS está instalado e configurado no servidor
- Algumas operações podem falhar se o usuário não tiver permissões suficientes

### 🔄 Atualização Automática
- A interface atualiza automaticamente após operações de CRUD
- Use os botões de atualização se necessário
- Mensagens de erro são exibidas em caso de problemas

### 🏗️ Melhores Práticas
- **Sites**: Use bindings únicos para evitar conflitos
- **Aplicações**: Organize em subdiretórios lógicos (ex: `/api`, `/admin`)
- **App Pools**: Use pools dedicados para aplicações críticas
- **Segurança**: Utilize identidades apropriadas baseadas nos requisitos de segurança

## Estrutura de Arquivos

```
src/
├── components/iis/          ← Componentes reutilizáveis
│   ├── SiteFormModal.tsx    ← Modal para criar sites
│   ├── ApplicationFormModal.tsx ← Modal para criar aplicações
│   ├── AppPoolFormModal.tsx ← Modal para criar app pools
│   ├── SiteCard.tsx         ← Card expansível para sites
│   ├── AppPoolsList.tsx     ← Lista de application pools
│   ├── PermissionsStatus.tsx ← Status de permissões
│   └── ConfirmationModal.tsx ← Modal de confirmação
├── services/iisService.ts   ← Serviço para API calls
├── store/iis/              ← Redux state management
│   ├── types.ts            ← Interfaces TypeScript
│   └── index.ts            ← Actions e Reducers
└── views/iis/              ← View principal
    └── IISView.tsx         ← Interface principal do IIS
```

## API Backend

A interface consome os endpoints do `IISController`:

- `GET /api/iis/request-permissions` - Verificar permissões
- `GET /api/iis/admin-status` - Status de administrador
- `POST /api/iis/request-admin` - Solicitar privilégios
- `GET /api/iis/sites` - Listar sites
- `POST /api/iis/sites` - Criar site
- `PUT /api/iis/sites/{siteName}` - Atualizar site
- `DELETE /api/iis/sites/{siteName}` - Excluir site
- `GET /api/iis/sites/{siteName}/applications` - Listar aplicações
- `POST /api/iis/applications` - Criar aplicação
- `DELETE /api/iis/sites/{siteName}/applications/{appPath}` - Excluir aplicação
- `GET /api/iis/app-pools` - Listar app pools
- `POST /api/iis/app-pools` - Criar app pool
- `DELETE /api/iis/app-pools/{poolName}` - Excluir app pool
