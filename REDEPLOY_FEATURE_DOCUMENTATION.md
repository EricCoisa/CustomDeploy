# Funcionalidade de Re-Deploy para Publicações Ativas

## 📋 Resumo da Implementação

Foi implementada uma nova funcionalidade que permite realizar Re-Deploy de publicações ativas diretamente da tela de gerenciamento de publicações. Esta funcionalidade oferece uma forma rápida e conveniente de atualizar deployments existentes.

## ✨ Funcionalidades Implementadas

### 1. Botão de Re-Deploy
- **Localização**: Aparece como o primeiro botão no `ProjectCard` para publicações ativas
- **Condição**: Só aparece quando `publication.exists && publication.hasMetadata` (publicação ativa)
- **Visual**: Botão primário azul com ícone de foguete 🚀
- **Texto**: "🚀 Re-Deploy"

### 2. Modal de Re-Deploy
- **Componente**: `ReDeployModal`
- **Funcionalidade**: Reutiliza o `DeployForm` existente
- **Dados pré-preenchidos**: Carrega automaticamente os metadados da publicação
- **Editável**: Usuário pode alterar qualquer campo antes de executar

### 3. Preenchimento Automático
Os seguintes campos são preenchidos automaticamente com base nos metadados da publicação:

```typescript
{
  siteName: publication.siteName || publication.name,
  subPath: publication.subApplication || '',
  repoUrl: publication.repository || publication.repoUrl || '',
  branch: publication.branch || 'main',
  buildCommand: publication.buildCommand || 'npm install && npm run build',
  buildOutput: 'dist'
}
```

## 🏗️ Arquitetura da Implementação

### Componentes Criados/Modificados

#### 1. `ReDeployModal.tsx` (Novo)
```tsx
interface ReDeployModalProps {
  publication: Publication;
  isOpen: boolean;
  onClose: () => void;
  onSuccess?: (result: Record<string, unknown>) => void;
  onError?: (error: string) => void;
}
```

**Características:**
- Modal sobreposto com backdrop blur
- Animação de entrada suave
- Reutiliza o `DeployForm` sem preview
- Botão de fechar no canto superior direito
- Click fora do modal para fechar

#### 2. `ProjectCard.tsx` (Modificado)
- Adicionada prop `onReDeploy?: (publication: Publication) => void`
- Adicionada função `isActive()` para determinar se publicação está ativa
- Botão de Re-Deploy posicionado como primeiro botão quando ativo

#### 3. `PublicationsView/index.tsx` (Modificado)
- Estado para controlar modal: `reDeployModal`
- Handlers para abrir/fechar modal e tratar sucesso/erro
- Integração com toast notifications
- Refresh automático após deploy bem-sucedido

### Fluxo de Interação

1. **Usuário clica em "🚀 Re-Deploy"** → Abre modal com dados pré-preenchidos
2. **Modal exibe dados da publicação** → Usuário pode editar conforme necessário
3. **Usuário clica em "Executar Deploy"** → Inicia processo de deploy
4. **Deploy concluído** → Modal fecha, toast de sucesso, refresh automático dos dados

## 🎯 Benefícios da Implementação

### Para o Usuário
- ✅ **Conveniência**: Re-deploy direto da lista de publicações
- ✅ **Velocidade**: Dados pré-preenchidos economizam tempo
- ✅ **Flexibilidade**: Pode alterar configurações antes do deploy
- ✅ **Feedback**: Notificações claras de sucesso/erro

### Para o Sistema
- ✅ **Reutilização**: Aproveitamento do `DeployForm` existente
- ✅ **Consistência**: Mesmo fluxo de deploy da tela dedicada
- ✅ **Manutenibilidade**: Código modular e bem estruturado
- ✅ **Integração**: Refresh automático dos dados após deploy

## 🔍 Critérios para Exibição do Botão

O botão de Re-Deploy aparece apenas quando:

```typescript
// Publicação deve existir no IIS E ter metadados
const isActive = publication.exists && publication.hasMetadata;
```

### Estados das Publicações:

| Estado | Existe no IIS | Tem Metadados | Botão Re-Deploy |
|--------|---------------|---------------|-----------------|
| **Ativo** | ✅ | ✅ | ✅ **Visível** |
| Sem Metadados | ✅ | ❌ | ❌ Oculto |
| Não Encontrado | ❌ | ✅/❌ | ❌ Oculto |

## 🧪 Como Testar

### Cenário 1: Publicação Ativa
1. Acesse **Publicações** no menu
2. Localize uma publicação com status **"Ativo"**
3. Verifique se o botão **"🚀 Re-Deploy"** aparece como primeiro botão
4. Clique no botão e verifique se o modal abre com dados preenchidos

### Cenário 2: Execução do Re-Deploy
1. No modal, verifique se os campos estão preenchidos corretamente
2. Altere algum campo (ex: branch)
3. Clique em **"Executar Deploy"**
4. Verifique toast de sucesso e refresh automático dos dados

### Cenário 3: Publicações Não Ativas
1. Localize publicações com status **"Sem metadados"** ou **"Não encontrado"**
2. Verifique que o botão **"🚀 Re-Deploy"** NÃO aparece

## 📱 Responsividade

O modal é totalmente responsivo:
- **Desktop**: Modal centralizado com largura máxima de 800px
- **Mobile**: Modal ocupa largura total com padding lateral
- **Scroll**: Conteúdo scrollável se exceder altura da tela

## 🎨 Estilo Visual

### Modal
- Backdrop com blur e transparência
- Animação suave de entrada
- Bordas arredondadas
- Sombra elegante

### Botão Re-Deploy
- Cor primária azul (`variant="primary"`)
- Ícone de foguete 🚀
- Posicionado como destaque (primeiro botão)

## 🔧 Manutenção e Extensões Futuras

### Possíveis Melhorias:
1. **Histórico de Re-Deploys**: Exibir últimos deploys na publicação
2. **Deploy Agendado**: Agendar re-deploys para horários específicos
3. **Rollback**: Opção para reverter para deploy anterior
4. **Logs em Tempo Real**: Mostrar progresso do deploy no modal

### Pontos de Extensão:
- `onSuccess` callback pode ser expandido para mais ações
- Modal pode incluir abas para configurações avançadas
- Integração com webhooks para notificações externas

## 🎉 Conclusão

A funcionalidade de Re-Deploy foi implementada com sucesso, oferecendo:
- **UX intuitiva** com dados pré-preenchidos
- **Integração perfeita** com o sistema existente
- **Código limpo** e reutilizável
- **Design responsivo** e acessível

Esta implementação melhora significativamente a produtividade dos usuários ao permitir re-deploys rápidos e convenientes diretamente da interface de gerenciamento de publicações! 🚀
