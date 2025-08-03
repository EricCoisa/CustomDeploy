import React, { useState, useEffect, useCallback } from 'react';
import styled from 'styled-components';
import { Modal } from '../Modal';
import { fileManagerService, type FileSystemItem, type DirectoryContents } from '../../services/fileManagerService';

// Styled Components
const FileBrowserContainer = styled.div`
  display: flex;
  flex-direction: column;
  height: 70vh;
  max-height: 600px;
  min-height: 400px;
`;

const ToolBar = styled.div`
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  border-bottom: 1px solid #e5e7eb;
  background: #f9fafb;
`;

const HelpText = styled.div`
  font-size: 0.75rem;
  color: #6b7280;
  padding: 0.5rem 1rem;
  background: #f8fafc;
  border-bottom: 1px solid #e5e7eb;
  text-align: center;
`;

const PathDisplay = styled.div`
  flex: 1;
  padding: 0.5rem 1rem;
  background: white;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  font-family: 'Courier New', monospace;
  font-size: 0.875rem;
  color: #374151;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
`;

const BackButton = styled.button`
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  background: #6b7280;
  color: white;
  border: none;
  border-radius: 0.375rem;
  cursor: pointer;
  transition: background 0.2s;
  
  &:hover:not(:disabled) {
    background: #4b5563;
  }
  
  &:disabled {
    background: #9ca3af;
    cursor: not-allowed;
  }
`;

const FileList = styled.div`
  flex: 1;
  overflow-y: auto;
  border-bottom: 1px solid #e5e7eb;
  
  &::-webkit-scrollbar {
    width: 8px;
  }
  
  &::-webkit-scrollbar-track {
    background: #f1f5f9;
  }
  
  &::-webkit-scrollbar-thumb {
    background: #cbd5e1;
    border-radius: 4px;
  }
`;

const FileItem = styled.div<{ 
  isDirectory: boolean; 
  isSelected: boolean; 
  isSelectable: boolean 
}>`
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  transition: background 0.2s;
  border-bottom: 1px solid #f3f4f6;
  opacity: ${props => props.isSelectable ? 1 : 0.6};
  user-select: none;
  position: relative;
  
  background: ${props => {
    if (props.isSelected) return '#3b82f6';
    if (props.isSelectable) return 'transparent';
    return '#f9fafb';
  }};
  
  color: ${props => props.isSelected ? 'white' : '#374151'};
  
  &:hover {
    background: ${props => {
      if (!props.isSelectable && !props.isDirectory) return '#f9fafb';
      if (props.isSelected) return '#2563eb';
      return '#f3f4f6';
    }};
  }
  
  /* Visual hint para pastas - indicação de que são clicáveis */
  ${props => props.isDirectory && props.isSelectable && `
    &:hover {
      background: ${props.isSelected ? '#2563eb' : '#e0f2fe'};
      cursor: pointer;
    }
  `}
`;

const FileIcon = styled.span`
  font-size: 1.25rem;
  min-width: 1.5rem;
  text-align: center;
`;

const NavigateArrow = styled.button`
  background: #f3f4f6;
  border: 1px solid #d1d5db;
  color: #374151;
  font-size: 0.75rem;
  font-weight: bold;
  padding: 0.375rem 0.75rem;
  border-radius: 0.5rem;
  cursor: pointer;
  transition: all 0.15s;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 2.5rem;
  height: 2rem;
  
  &:hover {
    background: #3b82f6;
    color: white;
    border-color: #3b82f6;
    transform: scale(1.05);
    box-shadow: 0 4px 6px rgba(59, 130, 246, 0.25);
  }
  
  &:active {
    transform: scale(0.95);
    box-shadow: 0 2px 4px rgba(59, 130, 246, 0.25);
  }
`;

const FileInfo = styled.div`
  flex: 1;
  min-width: 0;
`;

const FileName = styled.div`
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
`;

const FileDetails = styled.div`
  font-size: 0.75rem;
  opacity: 0.8;
  margin-top: 0.25rem;
`;

const LoadingState = styled.div`
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  color: #6b7280;
`;

const ErrorState = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  color: #ef4444;
  text-align: center;
  
  button {
    margin-top: 1rem;
    padding: 0.5rem 1rem;
    background: #ef4444;
    color: white;
    border: none;
    border-radius: 0.375rem;
    cursor: pointer;
    
    &:hover {
      background: #dc2626;
    }
  }
`;

const ActionBar = styled.div`
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding: 1rem;
  background: #f9fafb;
`;

const ActionButton = styled.button<{ variant?: 'primary' | 'secondary' }>`
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 0.375rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s;
  
  ${props => props.variant === 'primary' ? `
    background: #3b82f6;
    color: white;
    
    &:hover:not(:disabled) {
      background: #2563eb;
    }
    
    &:disabled {
      background: #9ca3af;
      cursor: not-allowed;
    }
  ` : `
    background: #6b7280;
    color: white;
    
    &:hover {
      background: #4b5563;
    }
  `}
`;

// Props do componente
interface FileBrowserProps {
  isOpen: boolean;
  onClose: () => void;
  onSelect: (fullPath: string) => void;
  selectType?: 'file' | 'directory' | 'both';
  initialPath?: string;
}

export const FileBrowser: React.FC<FileBrowserProps> = ({
  isOpen,
  onClose,
  onSelect,
  selectType = 'both',
  initialPath = 'C:/',
}) => {
  const [currentPath, setCurrentPath] = useState<string>(initialPath);
  const [directoryContents, setDirectoryContents] = useState<DirectoryContents | null>(null);
  const [selectedItem, setSelectedItem] = useState<FileSystemItem | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [history, setHistory] = useState<string[]>([]);

  // Carregar conteúdo do diretório
  const loadDirectory = async (path: string) => {
    setLoading(true);
    setError(null);
    setSelectedItem(null);

    try {
      const response = await fileManagerService.getDirectoryContents(path);
      
      if (response.success) {
        console.log('Conteúdo do diretório carregado:', response.data); 
        setDirectoryContents(response.data);
        setCurrentPath(response.data.currentPath);
      } else {
        setError(response.message || 'Erro ao carregar diretório');
      }
    } catch {
      setError('Erro de conexão com o servidor');
    } finally {
      setLoading(false);
    }
  };

  // Navegar para um diretório
  const navigateToDirectory = (path: string) => {
    setHistory(prev => [...prev, currentPath]);
    loadDirectory(path);
  };

  // Voltar ao diretório anterior
  const navigateBack = useCallback(() => {
    if (history.length > 0) {
      const previousPath = history[history.length - 1];
      setHistory(prev => prev.slice(0, -1));
      loadDirectory(previousPath);
    }
  }, [history]);

  // Verificar se um item é selecionável
  const isItemSelectable = (item: FileSystemItem): boolean => {
    if (!item.isAccessible) return false;
    
    switch (selectType) {
      case 'file':
        return !item.isDirectory;
      case 'directory':
        return item.isDirectory;
      case 'both':
      default:
        return true;
    }
  };

  // Lidar com clique simples no item (apenas seleção)
  const handleItemClick = (item: FileSystemItem) => {
    if (isItemSelectable(item)) {
      setSelectedItem(item);
    }
  };

  // Lidar com clique duplo no item (navegação para pastas)
  const handleItemDoubleClick = (item: FileSystemItem) => {
    console.log('Item double clicked:', item);
    if (item.isDirectory && item.isAccessible) {
      // Se é uma pasta acessível, navegar para ela
      navigateToDirectory(item.fullPath);
    }
  };

  // Confirmar seleção
  const handleConfirmSelection = () => {
    if (selectedItem) {
      onSelect(selectedItem.fullPath);
      onClose();
    }
  };

  // Carregar diretório inicial quando modal abre
  useEffect(() => {
    if (isOpen) {
      setCurrentPath(initialPath);
      setHistory([]);
      loadDirectory(initialPath);
    }
  }, [isOpen, initialPath]);

  // Adicionar suporte a teclas (como Backspace para voltar)
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (!isOpen) return;
      
      if (e.key === 'Backspace' && history.length > 0) {
        e.preventDefault();
        navigateBack();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, history.length, navigateBack]);

  // Renderizar item da lista
  const renderFileItem = (item: FileSystemItem) => {
    const isSelectable = isItemSelectable(item);
    const isSelected = selectedItem?.fullPath === item.fullPath;
    
    const handleNavigateClick = (e: React.MouseEvent) => {
      e.stopPropagation();
      e.preventDefault();
      if (item.isDirectory && item.isAccessible) {
        navigateToDirectory(item.fullPath);
      }
    };
    
    return (
      <FileItem
        key={item.fullPath}
        isDirectory={item.isDirectory}
        isSelected={isSelected}
        isSelectable={isSelectable}
        onClick={() => handleItemClick(item)}
        onDoubleClick={(e) => {
          e.preventDefault();
          e.stopPropagation();
          handleItemDoubleClick(item);
        }}
        style={{
          cursor: item.isDirectory && item.isAccessible ? 'pointer' : 
                  isSelectable ? 'pointer' : 'default'
        }}
      >
        <FileIcon>{fileManagerService.getFileIcon(item)}</FileIcon>
        <FileInfo>
          <FileName>{item.name}</FileName>
          <FileDetails>
            {item.isDirectory ? 
              (item.isAccessible ? 'Pasta • Clique duplo ou seta para abrir' : 'Pasta • Não acessível') : 
              fileManagerService.formatFileSize(item.size)
            }
            {item.lastModified && ` • ${new Date(item.lastModified).toLocaleDateString()}`}
          </FileDetails>
        </FileInfo>
        {item.isDirectory && item.isAccessible && (
          <NavigateArrow
            onClick={handleNavigateClick}
            title={`Abrir pasta: ${item.name}`}
          >
            Abrir
          </NavigateArrow>
        )}
      </FileItem>
    );
  };

  return (
    <Modal 
      isOpen={isOpen} 
      onClose={onClose} 
      title="Selecionar Arquivo/Pasta"
      closeOnOverlayClick={false}
    >
      <FileBrowserContainer>
        <HelpText>
          💡 Clique para selecionar • Clique duplo ou botão "Abrir" para navegar em pastas • Backspace para voltar
        </HelpText>
        <ToolBar>
          <BackButton 
            onClick={navigateBack} 
            disabled={history.length === 0}
            title={history.length > 0 ? 
              `Voltar para: ${history[history.length - 1]}` : 
              'Nenhum histórico de navegação'
            }
          >
            ← Voltar
          </BackButton>
          <PathDisplay title={currentPath}>
            {currentPath}
          </PathDisplay>
        </ToolBar>

        <FileList>
          {loading && (
            <LoadingState>
              Carregando...
            </LoadingState>
          )}

          {error && (
            <ErrorState>
              <div>❌ {error}</div>
              <button onClick={() => loadDirectory(currentPath)}>
                Tentar Novamente
              </button>
            </ErrorState>
          )}

          {!loading && !error && directoryContents && (
            <>
              {directoryContents.items.map(renderFileItem)}
              {directoryContents.items.length === 0 && (
                <LoadingState>
                  📂 Pasta vazia
                </LoadingState>
              )}
            </>
          )}
        </FileList>

        <ActionBar>
          <ActionButton onClick={onClose}>
            Cancelar
          </ActionButton>
          <ActionButton 
            variant="primary" 
            onClick={handleConfirmSelection}
            disabled={!selectedItem}
          >
            Selecionar
          </ActionButton>
        </ActionBar>
      </FileBrowserContainer>
    </Modal>
  );
};
