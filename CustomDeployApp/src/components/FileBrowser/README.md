# FileBrowser Component

Um componente reutilizável para navegação e seleção de arquivos/pastas do sistema de arquivos, integrado com a API FileManagerController.

## Características

- ✅ **Navegação completa**: Browse em qualquer diretório do sistema
- ✅ **Seleção flexível**: Arquivos, pastas ou ambos
- ✅ **Interface intuitiva**: Ícones diferentes para cada tipo de arquivo
- ✅ **Navegação com histórico**: Botão "Voltar" funcional
- ✅ **Tratamento de erros**: Estados de loading, erro e retry
- ✅ **Responsivo**: Layout adaptável
- ✅ **Integração modal**: Usa o componente Modal existente

## Props

| Prop | Tipo | Padrão | Descrição |
|------|------|--------|-----------|
| `isOpen` | `boolean` | - | **Obrigatório**. Controla visibilidade do browser |
| `onClose` | `() => void` | - | **Obrigatório**. Função chamada ao fechar |
| `onSelect` | `(fullPath: string) => void` | - | **Obrigatório**. Callback com caminho selecionado |
| `selectType` | `'file' \| 'folder' \| 'both'` | `'both'` | Tipo de item selecionável |
| `initialPath` | `string` | `'C:/'` | Diretório inicial |

## Exemplos de Uso

### Seleção de Qualquer Item (Padrão)

```tsx
import { FileBrowser } from '../../components';

const [showBrowser, setShowBrowser] = useState(false);
const [selectedPath, setSelectedPath] = useState('');

<FileBrowser
  isOpen={showBrowser}
  onClose={() => setShowBrowser(false)}
  onSelect={(path) => {
    setSelectedPath(path);
    console.log('Selecionado:', path);
  }}
/>
```

### Seleção Apenas de Pastas

```tsx
<FileBrowser
  isOpen={showBrowser}
  onClose={() => setShowBrowser(false)}
  onSelect={(folderPath) => setSelectedFolder(folderPath)}
  selectType="folder"
  initialPath="C:/Projects"
/>
```

### Seleção Apenas de Arquivos

```tsx
<FileBrowser
  isOpen={showBrowser}
  onClose={() => setShowBrowser(false)}
  onSelect={(filePath) => setSelectedFile(filePath)}
  selectType="file"
  initialPath="C:/Documents"
/>
```

## Funcionalidades

### Navegação
- **Clique duplo em pastas**: Navega para a pasta
- **Botão Voltar**: Retorna ao diretório anterior
- **Histórico**: Mantém histórico de navegação
- **Path display**: Mostra caminho atual completo

### Seleção
- **Seleção visual**: Item selecionado fica destacado
- **Filtro por tipo**: Apenas itens selecionáveis são destacados
- **Validação**: Só permite selecionar itens acessíveis

### Interface
- **Ícones inteligentes**: Diferentes ícones por tipo de arquivo
- **Informações do arquivo**: Tamanho e data de modificação
- **Estados visuais**: Loading, erro, pasta vazia
- **Scrollbar customizada**: Design consistente com a aplicação

## Integração com API

O componente usa o `fileManagerService` que se comunica com:

### Endpoints Utilizados
- `GET /FileManager` - Lista conteúdo do diretório
- `GET /FileManager/item` - Informações de item específico
- `GET /FileManager/drives` - Drives disponíveis
- `GET /FileManager/validate` - Validação de caminho

### Tipos de Arquivo Suportados
O serviço detecta automaticamente e aplica ícones para:
- 📁 Pastas
- 📄 Documentos de texto (.txt, .md, .log)
- 📜 Código (.js, .ts, .jsx, .tsx)
- 📋 JSON
- 🌐 HTML
- 🎨 CSS
- 🖼️ Imagens (.png, .jpg, .jpeg, .gif, .bmp)
- 🎬 Vídeos (.mp4, .avi, .mov)
- 🎵 Áudio (.mp3, .wav, .flac)
- 📦 Arquivos compactados (.zip, .rar, .7z)
- ⚙️ Executáveis (.exe, .msi)
- 📕 PDF
- 📘 Word (.doc, .docx)
- 📗 Excel (.xls, .xlsx)

## Estados do Componente

### Loading
Exibido durante carregamento de diretórios

### Erro
- Exibe mensagem de erro
- Botão "Tentar Novamente"
- Mantém funcionalidade de navegação

### Vazio
Indica quando uma pasta não contém itens

### Sucesso
Lista todos os itens com informações detalhadas

## Segurança

- ✅ **Autenticação**: Requer token JWT válido
- ✅ **Autorização**: Verifica privilégios de administrador
- ✅ **Validação**: Paths são validados no backend
- ✅ **Bloqueio**: Respeita políticas de segurança do sistema

## Responsividade

- **Modal adaptável**: 70vh de altura, mínimo 400px
- **Layout flexível**: Toolbar, lista e actions responsivos
- **Texto adaptável**: Ellipsis em nomes longos
- **Scroll inteligente**: Apenas na lista de arquivos

## Exemplos Avançados

### Seleção com Callback Personalizado

```tsx
const handleFileSelection = (path: string) => {
  // Validar extensão
  if (path.endsWith('.json')) {
    setConfigFile(path);
    setShowSuccess('Arquivo de configuração selecionado!');
  } else {
    setShowError('Selecione apenas arquivos .json');
  }
};

<FileBrowser
  isOpen={showBrowser}
  onClose={() => setShowBrowser(false)}
  onSelect={handleFileSelection}
  selectType="file"
/>
```

### Integração com Formulários

```tsx
const FormWithFilePicker = () => {
  const [formData, setFormData] = useState({
    projectPath: '',
    configFile: ''
  });

  return (
    <form>
      <input 
        value={formData.projectPath} 
        placeholder="Caminho do projeto"
        readOnly 
      />
      <button onClick={() => setShowFolderBrowser(true)}>
        Selecionar Pasta
      </button>
      
      <FileBrowser
        isOpen={showFolderBrowser}
        onClose={() => setShowFolderBrowser(false)}
        onSelect={(path) => setFormData(prev => ({
          ...prev,
          projectPath: path
        }))}
        selectType="folder"
      />
    </form>
  );
};
```

O FileBrowser está totalmente integrado e pronto para uso em qualquer parte da aplicação! 🚀
