# Modal Component

Um componente Modal reutilizável construído com styled-components e React.

## Características

- ✅ **Portal**: Renderizado fora da árvore de componentes usando `ReactDOM.createPortal`
- ✅ **Animações**: Transições suaves de entrada e saída
- ✅ **Acessibilidade**: Suporte a ESC para fechar e foco adequado
- ✅ **Responsivo**: Adapta-se a diferentes tamanhos de tela
- ✅ **Flexível**: Aceita qualquer conteúdo via children
- ✅ **Customizável**: Props para controlar comportamento

## Props

| Prop | Tipo | Padrão | Descrição |
|------|------|--------|-----------|
| `isOpen` | `boolean` | - | **Obrigatório**. Controla se o modal está visível |
| `onClose` | `() => void` | - | **Obrigatório**. Função chamada ao fechar o modal |
| `title` | `string` | - | Opcional. Título exibido no header do modal |
| `children` | `ReactNode` | - | **Obrigatório**. Conteúdo interno do modal |
| `closeOnOverlayClick` | `boolean` | `true` | Se deve fechar ao clicar fora do modal |
| `closeOnEsc` | `boolean` | `true` | Se deve fechar ao pressionar ESC |

## Exemplos de Uso

### Modal Básico com Título

```tsx
import { Modal, Button } from '../../components';

const [showModal, setShowModal] = useState(false);

<Modal 
  isOpen={showModal} 
  onClose={() => setShowModal(false)} 
  title="Título do Modal"
>
  <p>Conteúdo do modal aqui...</p>
  <Button onClick={() => setShowModal(false)}>
    Fechar
  </Button>
</Modal>
```

### Modal de Confirmação

```tsx
<Modal 
  isOpen={showConfirm} 
  onClose={() => setShowConfirm(false)} 
  title="Confirmar Ação"
>
  <p>Tem certeza que deseja prosseguir?</p>
  <div style={{ display: 'flex', gap: '1rem', justifyContent: 'flex-end' }}>
    <Button onClick={() => setShowConfirm(false)}>Cancelar</Button>
    <Button onClick={handleConfirm}>Confirmar</Button>
  </div>
</Modal>
```

### Modal sem Título (com botão flutuante)

```tsx
<Modal 
  isOpen={showModal} 
  onClose={() => setShowModal(false)}
>
  <h3>Título customizado</h3>
  <p>Quando não há prop title, o botão de fechar fica flutuante.</p>
</Modal>
```

### Modal que não fecha ao clicar fora

```tsx
<Modal 
  isOpen={showModal} 
  onClose={() => setShowModal(false)} 
  title="Modal Restrito"
  closeOnOverlayClick={false}
  closeOnEsc={false}
>
  <p>Este modal só fecha pelos botões internos.</p>
  <Button onClick={() => setShowModal(false)}>Fechar</Button>
</Modal>
```

## Funcionalidades

### Animações
- **Fade in**: O overlay aparece com uma transição suave
- **Scale in**: O modal "cresce" do centro com efeito scale
- **Backdrop blur**: Efeito de desfoque no fundo

### Acessibilidade
- **ESC key**: Fecha o modal (pode ser desabilitado)
- **Focus trap**: Mantém o foco dentro do modal
- **ARIA labels**: Botão de fechar tem label apropriado
- **Portal**: Evita problemas de z-index

### Responsividade
- **Max width**: 90vw em telas pequenas
- **Max height**: 90vh com scroll interno se necessário
- **Min width**: 320px mínimo
- **Scroll customizado**: Scrollbar estilizada

### Comportamentos
- **Body scroll**: Previne scroll da página quando modal está aberto
- **Overlay click**: Fecha ao clicar fora (configurável)
- **Multiple modals**: Suporta múltiplos modais (z-index adequado)

## Estilos

O modal usa um sistema de cores consistente:
- **Background**: Branco com sombra pronunciada
- **Header**: Fundo cinza claro com border
- **Close button**: Hover suave e focus ring
- **Scrollbar**: Personalizada e discreta

## Integração

O componente está disponível no barrel export:

```tsx
import { Modal } from '../../components';
```

E pode ser usado em qualquer parte da aplicação que precise de confirmações, formulários, ou exibição de conteúdo modal.
