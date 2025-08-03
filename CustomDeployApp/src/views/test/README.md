# 🧪 TestView - Página de Testes de Componentes

## Visão Geral

A **TestView** é uma página temporária criada especificamente para facilitar o desenvolvimento e teste manual de componentes durante o processo de desenvolvimento da aplicação.

## Objetivo

Esta página serve como um ambiente isolado onde desenvolvedores podem:

- ✅ **Testar componentes individualmente** sem afetar outras partes da aplicação
- 🔍 **Verificar o funcionamento** de componentes novos ou modificados
- 🎨 **Visualizar diferentes estados** e configurações dos componentes
- 🐛 **Debug e troubleshooting** de problemas específicos

## Acesso

A página está disponível em: `/test`

**Navegação:**
- Dashboard: `http://localhost:5173/dashboard`
- Página de Testes: `http://localhost:5173/test`

## Componentes Testáveis

### 🪟 Modal
- **Modal Informativo**: Teste básico de abertura/fechamento e conteúdo
- **Modal de Confirmação**: Teste de modal com ações (confirmar/cancelar)

**Características testadas:**
- Abertura e fechamento
- Título personalizado
- Conteúdo HTML complexo
- Botão de fechar (X)
- Fechamento com tecla ESC
- Fechamento clicando fora do modal
- Animações de entrada e saída

### 📁 FileBrowser
- **Arquivos e Pastas**: Navegação completa no sistema de arquivos
- **Apenas Arquivos**: Seleção restrita a arquivos
- **Apenas Pastas**: Seleção restrita a diretórios

**Características testadas:**
- Navegação no sistema de arquivos
- Histórico de navegação (voltar/avançar)
- Seleção de arquivos/pastas
- Diferentes tipos de visualização
- Integração com API backend

### 🔘 Outros Componentes
- **Button**: Diferentes variações e estados
- **Input**: Campos de entrada com validação
- **Layout**: Estrutura de páginas protegidas

## Como Usar

### 1. Acessar a Página
```
http://localhost:5173/test
```

### 2. Testar Componentes
- Clique nos botões correspondentes para abrir/testar cada componente
- Observe o comportamento e funcionalidades
- Verifique os resultados na seção "Resultados dos Testes"

### 3. Analisar Resultados
- Os resultados das interações são exibidos na parte inferior da página
- Use o botão "Limpar Resultados" para resetar

## Adicionando Novos Testes

Para adicionar testes de novos componentes:

### 1. Importar o Componente
```tsx
import { NovoComponente } from '../../components';
```

### 2. Adicionar Estado
```tsx
const [showNovoComponente, setShowNovoComponente] = useState(false);
```

### 3. Criar Botão de Teste
```tsx
<TestButton onClick={() => setShowNovoComponente(true)}>
  Testar Novo Componente
</TestButton>
```

### 4. Implementar o Componente
```tsx
<NovoComponente 
  isOpen={showNovoComponente}
  onClose={() => setShowNovoComponente(false)}
  // outras props...
/>
```

## Estrutura da Página

```
TestView/
├── 📋 Seção de Informações
├── 🪟 Testes de Modal
├── 📁 Testes de FileBrowser  
├── 📊 Resultados dos Testes
└── 🛠️ Notas de Desenvolvimento
```

## Características Técnicas

- **Framework**: React 19 + TypeScript
- **Styling**: Styled-Components
- **Layout**: ProtectedLayout (reutilizado)
- **Estado**: React useState hooks
- **Responsivo**: Design adaptável para mobile

## Remoção

⚠️ **Esta página é temporária** e deve ser removida antes do deploy de produção.

Para remover:
1. Deletar pasta `src/views/test/`
2. Remover import e rota em `src/infra/routes.tsx`
3. Remover links de navegação no Header (opcional)
4. Remover export em `src/views/index.ts`

## Benefícios

- 🚀 **Desenvolvimento Ágil**: Teste rápido sem setup complexo
- 🔒 **Ambiente Isolado**: Não afeta funcionalidades em produção
- 📱 **Responsive Testing**: Teste em diferentes tamanhos de tela
- 🎯 **Foco no Componente**: Concentração específica no que está sendo desenvolvido
- 📝 **Documentação Viva**: Exemplos práticos de uso dos componentes
